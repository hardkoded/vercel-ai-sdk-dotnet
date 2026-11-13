// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIChatLanguageModelTests;

/// <summary>Port of <c>openai-chat-language-model.test.ts</c> &gt; <c>doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/openai/src/chat/openai-chat-language-model.test.ts::doStream::";

    // "undefined" stands for a stream that ends without any finish_reason chunk; null is a JSON null finish_reason.
    [Theory]
    [InlineData("undefined", FinishReason.Interrupted)]
    [InlineData(null, FinishReason.Interrupted)]
    [InlineData("stop", FinishReason.Stop)]
    [InlineData("length", FinishReason.Length)]
    [InlineData("tool_calls", FinishReason.ToolCalls)]
    [InlineData("unknown_reason", FinishReason.Other)]
    [UpstreamTest(Prefix + "maps a stream ending with finish_reason %j to %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_a_stream_ending_with_finish_reason_to(string? raw, FinishReason unified)
    {
        var chunks = new List<JsonNode>
        {
            new JsonObject
            {
                ["id"] = "chat-1",
                ["object"] = "chat.completion.chunk",
                ["created"] = 1,
                ["model"] = "gpt-4o",
                ["choices"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["index"] = 0,
                        ["delta"] = new JsonObject { ["content"] = "Partial answer" },
                        ["finish_reason"] = null,
                    },
                },
            },
        };
        if (raw != "undefined")
        {
            chunks.Add(new JsonObject
            {
                ["choices"] = new JsonArray
                {
                    new JsonObject { ["index"] = 0, ["delta"] = new JsonObject(), ["finish_reason"] = raw },
                },
            });
        }

        var body = string.Concat(chunks.Select(chunk => "data: " + chunk.ToJsonString() + "\n\n"))
            + (raw == "undefined" ? string.Empty : "data: [DONE]\n\n");
        var capture = new OpenAICapture { ServerSentEvents = body };

        var parts = await OpenAIUpstream.Read(
            await OpenAIUpstream.Provider(capture).ChatModel("gpt-4o").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));

        Assert.Contains(parts, part => part is TextDeltaStreamPart { Id: "0", Delta: "Partial answer" });
        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(unified, finish.FinishReason);
        Assert.Equal(raw == "undefined" ? null : raw, finish.RawFinishReason);
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(params string[] chunks)
    {
        var capture = new OpenAICapture { ServerSentEvents = UpstreamChat.Sse(chunks) + "data: [DONE]\n\n" };
        return await OpenAIUpstream.Read(
            await OpenAIUpstream.Provider(capture).ChatModel("gpt-4o-2024-08-06").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
    }

    private static string Chunk(string id, string delta, string? finishReason, long created = 1)
    {
        return "{\"id\":\"" + id + "\",\"object\":\"chat.completion.chunk\",\"created\":" + created
            + ",\"model\":\"gpt-4o-2024-08-06\",\"choices\":[{\"index\":0,\"delta\":" + delta
            + ",\"finish_reason\":" + (finishReason == null ? "null" : "\"" + finishReason + "\"") + "}]}";
    }

    private static void AssertRefusalStream(List<LanguageModelStreamPart> parts, string id, DateTimeOffset timestamp, string[] deltas)
    {
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal(id, metadata.Id);
        Assert.Equal("gpt-4o-2024-08-06", metadata.ModelId);
        Assert.Equal(timestamp, metadata.Timestamp);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        for (var i = 0; i < deltas.Length; i++)
        {
            var delta = Assert.IsType<TextDeltaStreamPart>(parts[3 + i]);
            Assert.Equal("0", delta.Id);
            Assert.Equal(deltas[i], delta.Delta);
        }

        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[3 + deltas.Length]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[4 + deltas.Length]);
        Assert.Equal(FinishReason.ContentFilter, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        Assert.Equal(5 + deltas.Length, parts.Count);
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream a refusal as text deltas", Coverage = UpstreamCoverage.Covered, Commit = "f6dbf04b5dd06561ce8c6cf223e86a287cd7c4d3")]
    public async Task Should_stream_a_refusal_as_text_deltas()
    {
        var parts = await Stream(
            Chunk("chatcmpl-refusal", "{\"role\":\"assistant\",\"content\":null,\"refusal\":\"\"}", null),
            Chunk("chatcmpl-refusal", "{\"refusal\":\"I'm\"}", null),
            Chunk("chatcmpl-refusal", "{\"refusal\":\" sorry, but I can't assist with that request.\"}", null),
            Chunk("chatcmpl-refusal", "{}", "stop"));

        AssertRefusalStream(
            parts,
            "chatcmpl-refusal",
            DateTimeOffset.FromUnixTimeSeconds(1),
            new[] { "I'm", " sorry, but I can't assist with that request." });
        Assert.Equal("{\"openai\":{}}", parts.OfType<FinishStreamPart>().Single().ProviderMetadata!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream a refusal from a real response as text deltas", Coverage = UpstreamCoverage.Covered, Commit = "f6dbf04b5dd06561ce8c6cf223e86a287cd7c4d3")]
    public async Task Should_stream_a_refusal_from_a_real_response_as_text_deltas()
    {
        var chunks = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "openai-chat-refusal.chunks.txt"))
            .Where(line => line.Trim().Length > 0)
            .ToArray();
        var parts = await Stream(chunks);

        AssertRefusalStream(
            parts,
            "chatcmpl-Egi9csIJjpCxGlUk8ywkdX7vmz6Si",
            DateTimeOffset.Parse("2026-11-05T03:10:12Z"),
            new[] { "I'm", " very", " sorry", ",", " but", " I", " can't", " assist", " with", " that", "." });
        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal("{\"openai\":{\"acceptedPredictionTokens\":0,\"rejectedPredictionTokens\":0}}", finish.ProviderMetadata!.Value.GetRawText());
        Assert.Equal(74, finish.Usage.InputTokens);
        Assert.Equal(0, finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Equal(12, finish.Usage.OutputTokens);
        Assert.Equal(12, finish.Usage.TextTokens);
        Assert.Equal(0, finish.Usage.ReasoningTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should append refusal deltas to streamed content", Coverage = UpstreamCoverage.Covered, Commit = "f6dbf04b5dd06561ce8c6cf223e86a287cd7c4d3")]
    public async Task Should_append_refusal_deltas_to_streamed_content()
    {
        var parts = await Stream(
            Chunk("chatcmpl-content-and-refusal", "{\"role\":\"assistant\",\"content\":\"Here is the first part.\"}", null),
            Chunk("chatcmpl-content-and-refusal", "{\"refusal\":\" I cannot continue.\"}", null),
            Chunk("chatcmpl-content-and-refusal", "{}", "stop"));

        AssertRefusalStream(
            parts,
            "chatcmpl-content-and-refusal",
            DateTimeOffset.FromUnixTimeSeconds(1),
            new[] { "Here is the first part.", " I cannot continue." });
        Assert.Equal("{\"openai\":{}}", parts.OfType<FinishStreamPart>().Single().ProviderMetadata!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should forward error stream parts after a refusal has started", Coverage = UpstreamCoverage.Partial, Note = "The error part carries the message string. Upstream wraps a structured error object.", Commit = "f6dbf04b5dd06561ce8c6cf223e86a287cd7c4d3")]
    public async Task Should_forward_error_stream_parts_after_a_refusal_has_started()
    {
        var parts = await Stream(
            Chunk("chatcmpl-refusal-error", "{\"role\":\"assistant\",\"content\":null,\"refusal\":\"I'm sorry\"}", null, 1702657020),
            "{\"error\":{\"message\":\"stream failed after refusal\",\"type\":\"server_error\",\"param\":null,\"code\":null}}");

        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("chatcmpl-refusal-error", metadata.Id);
        Assert.Equal("gpt-4o-2024-08-06", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2023-12-15T16:17:00Z"), metadata.Timestamp);
        Assert.Equal("0", Assert.IsType<TextStartStreamPart>(parts[2]).Id);
        Assert.Equal("I'm sorry", Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("stream failed after refusal", Assert.IsType<ErrorStreamPart>(parts[4]).Message);
        Assert.Equal("0", Assert.IsType<TextEndStreamPart>(parts[5]).Id);
        var finish = Assert.IsType<FinishStreamPart>(parts[6]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        Assert.Equal(7, parts.Count);
    }
}
