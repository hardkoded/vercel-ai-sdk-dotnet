// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GroqChatLanguageModelTests.GroqChatTestSupport;

namespace Vercel.AI.Tests.GroqChatLanguageModelTests;

/// <summary>Helpers that read an upstream <c>*.chunks.txt</c> fixture the way the snapshots expect.</summary>
internal static class StreamFixture
{
    internal static List<JsonObject> Chunks(string name)
    {
        return File.ReadAllLines(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".chunks.txt")
            )
            .Where(line => line.Trim().Length > 0)
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToList();
    }

    internal static string Joined(string name, string field)
    {
        var builder = new StringBuilder();
        foreach (var chunk in Chunks(name))
        {
            var delta = chunk["choices"]?.AsArray().FirstOrDefault()?["delta"];
            if (delta?[field] is JsonValue value && value.TryGetValue<string>(out var text))
            {
                builder.Append(text);
            }
        }

        return builder.ToString();
    }
}

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doStream &gt; text</c>.</summary>
public sealed class DoStreamTextTests
{
    [Fact]
    [UpstreamTest(
        TestFile + "::doStream > text::should stream text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_text()
    {
        var parts = await Stream(Model(ChunksFixture("groq-text")));

        Assert.IsType<StreamStartStreamPart>(parts[0]);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-7eb08824-fb8d-47af-a1f0-3aa786f2d1f3", metadata.Id);
        Assert.Equal("llama-3.3-70b-versatile", metadata.ModelId);
        Assert.Equal(new DateTimeOffset(2026, 2, 11, 0, 47, 19, TimeSpan.Zero), metadata.Timestamp);
        var start = Assert.Single(parts.OfType<TextStartStreamPart>());
        Assert.Equal("txt-0", start.Id);
        Assert.Equal(
            StreamFixture.Joined("groq-text", "content"),
            string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta))
        );
        Assert.Equal("txt-0", Assert.Single(parts.OfType<TextEndStreamPart>()).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(
            StreamFixture.Chunks("groq-text").Last(chunk => chunk["x_groq"]?["usage"] != null)[
                "x_groq"
            ]!["usage"]!["prompt_tokens"]!.GetValue<int>(),
            finish.Usage.InputTokens
        );
    }
}

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doStream &gt; tool call</c>.</summary>
public sealed class DoStreamToolCallTests
{
    [Fact]
    [UpstreamTest(
        TestFile + "::doStream > tool call::should stream tool call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_tool_call()
    {
        var parts = await Stream(Model(ChunksFixture("groq-tool-call")));

        Assert.Collection(
            parts,
            part => Assert.IsType<StreamStartStreamPart>(part),
            part =>
            {
                var metadata = Assert.IsType<ResponseMetadataStreamPart>(part);
                Assert.Equal("chatcmpl-b610d559-f156-4aca-8827-24b4fe6af54f", metadata.Id);
                Assert.Equal("llama-3.3-70b-versatile", metadata.ModelId);
                Assert.Equal(
                    new DateTimeOffset(2026, 2, 11, 0, 47, 23, TimeSpan.Zero),
                    metadata.Timestamp
                );
            },
            part =>
            {
                var start = Assert.IsType<ToolInputStartStreamPart>(part);
                Assert.Equal("tk85n1k4m", start.Id);
                Assert.Equal("weather", start.ToolName);
            },
            part => Assert.Equal("{}", Assert.IsType<ToolInputDeltaStreamPart>(part).Delta),
            part => Assert.Equal("tk85n1k4m", Assert.IsType<ToolInputEndStreamPart>(part).Id),
            part =>
            {
                var call = Assert.IsType<ToolCallStreamPart>(part);
                Assert.Equal("tk85n1k4m", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("{}", call.ArgumentsJson);
            },
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
                Assert.Equal("tool_calls", finish.RawFinishReason);
                Assert.Equal(210, finish.Usage.InputTokens);
                Assert.Equal(15, finish.Usage.OutputTokens);
            }
        );
    }
}

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doStream &gt; reasoning</c>.</summary>
public sealed class DoStreamReasoningTests
{
    private const string Prefix = TestFile + "::doStream > reasoning::";

    [Fact]
    [UpstreamTest(
        Prefix + "should keep reasoning active when deltas include empty tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_reasoning_active_when_deltas_include_empty_tool_calls()
    {
        const string head =
            """data: {"id":"chatcmpl-test","object":"chat.completion.chunk","created":1,"model":"test-model","choices":[{"index":0,"delta":""";
        var handler = new SseStreamHandler([
            head
                + """{"role":"assistant","content":"","reasoning":"Think ","tool_calls":[]},"finish_reason":null}]}"""
                + "\n\n",
            head
                + """{"content":"","reasoning":"more...","tool_calls":[]},"finish_reason":null}]}"""
                + "\n\n",
            head
                + """{"content":"Hello","reasoning":"","tool_calls":[]},"finish_reason":"stop"}]}"""
                + "\n\n",
            "data: [DONE]\n\n",
        ]);

        var parts = await Stream(Model(handler));

        Assert.Collection(
            parts.Where(part => part.Type.StartsWith("reasoning-", StringComparison.Ordinal)),
            part => Assert.Equal("reasoning-0", Assert.IsType<ReasoningStartStreamPart>(part).Id),
            part =>
            {
                var delta = Assert.IsType<ReasoningDeltaStreamPart>(part);
                Assert.Equal("reasoning-0", delta.Id);
                Assert.Equal("Think ", delta.Delta);
            },
            part =>
            {
                var delta = Assert.IsType<ReasoningDeltaStreamPart>(part);
                Assert.Equal("reasoning-0", delta.Id);
                Assert.Equal("more...", delta.Delta);
            },
            part => Assert.Equal("reasoning-0", Assert.IsType<ReasoningEndStreamPart>(part).Id)
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream reasoning", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_reasoning()
    {
        var parts = await Stream(Model(ChunksFixture("groq-reasoning")));

        Assert.Equal("reasoning-0", Assert.Single(parts.OfType<ReasoningStartStreamPart>()).Id);
        Assert.Equal(
            StreamFixture.Joined("groq-reasoning", "reasoning"),
            string.Concat(parts.OfType<ReasoningDeltaStreamPart>().Select(part => part.Delta))
        );
        Assert.Equal("reasoning-0", Assert.Single(parts.OfType<ReasoningEndStreamPart>()).Id);
        Assert.Equal(
            StreamFixture.Joined("groq-reasoning", "content"),
            string.Concat(parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta))
        );
        Assert.True(
            parts.FindIndex(part => part is ReasoningEndStreamPart)
                < parts.FindIndex(part => part is TextStartStreamPart)
        );
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(963, finish.Usage.ReasoningTokens);
    }
}

/// <summary>Port of <c>groq-chat-language-model.test.ts</c> &gt; <c>doStream with raw chunks</c>.</summary>
public sealed class DoStreamWithRawChunksTests
{
    [Fact]
    [UpstreamTest(
        TestFile
            + "::doStream with raw chunks::should stream raw chunks when includeRawChunks is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_raw_chunks_when_includeRawChunks_is_true()
    {
        string[] chunks =
        [
            """{"id":"chatcmpl-123","object":"chat.completion.chunk","created":1234567890,"model":"gemma2-9b-it","choices":[{"index":0,"delta":{"content":"Hello"},"finish_reason":null}]}""",
            """{"id":"chatcmpl-456","object":"chat.completion.chunk","created":1234567890,"model":"gemma2-9b-it","choices":[{"index":0,"delta":{"content":" world"},"finish_reason":null}]}""",
            """{"id":"chatcmpl-789","object":"chat.completion.chunk","created":1234567890,"model":"gemma2-9b-it","choices":[{"index":0,"delta":{},"finish_reason":"stop"}],"x_groq":{"usage":{"prompt_tokens":10,"completion_tokens":5,"total_tokens":15}}}""",
        ];
        var handler = new SseStreamHandler(
            chunks.Select(chunk => "data: " + chunk + "\n\n").Append("data: [DONE]\n\n")
        );
        var options = TestPrompt();
        options.IncludeRawChunks = true;

        var parts = await Stream(Model(handler), options);

        Assert.Collection(
            parts,
            part => Assert.IsType<StreamStartStreamPart>(part),
            part => AssertRaw(chunks[0], part),
            part =>
                Assert.Equal("chatcmpl-123", Assert.IsType<ResponseMetadataStreamPart>(part).Id),
            part => Assert.Equal("txt-0", Assert.IsType<TextStartStreamPart>(part).Id),
            part => Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => AssertRaw(chunks[1], part),
            part => Assert.Equal(" world", Assert.IsType<TextDeltaStreamPart>(part).Delta),
            part => AssertRaw(chunks[2], part),
            part => Assert.Equal("txt-0", Assert.IsType<TextEndStreamPart>(part).Id),
            part =>
            {
                var finish = Assert.IsType<FinishStreamPart>(part);
                Assert.Equal(FinishReason.Stop, finish.FinishReason);
                Assert.Equal("stop", finish.RawFinishReason);
                AssertUsage(
                    finish.Usage,
                    10,
                    10,
                    null,
                    5,
                    5,
                    null,
                    """{"prompt_tokens":10,"completion_tokens":5,"total_tokens":15}"""
                );
            }
        );
    }

    private static void AssertRaw(string expected, LanguageModelStreamPart part)
    {
        AssertJson(expected, JsonNode.Parse(Assert.IsType<RawStreamPart>(part).RawJson));
    }
}
