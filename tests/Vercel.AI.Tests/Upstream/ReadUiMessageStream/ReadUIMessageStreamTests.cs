// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ReadUiMessageStream;

/// <summary>Port of <c>read-ui-message-stream.test.ts</c> &gt; <c>readUIMessageStream</c>.</summary>
public sealed class ReadUIMessageStreamTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/read-ui-message-stream.test.ts::readUIMessageStream::";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(Prefix + "resumes raw text input after persistence (dynamic: %s)", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Resumes_raw_text_input_after_persistence_dynamic(bool dynamic)
    {
        var dynamicField = dynamic ? "true" : "false";
        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"message-1"}""",
                "{\"type\":\"tool-input-start\",\"toolCallId\":\"call-1\",\"toolName\":\"exec\",\"inputEncoding\":\"text\",\"dynamic\":" + dynamicField + "}",
                """{"type":"tool-input-delta","toolCallId":"call-1","inputTextDelta":"text(\""}""")));
        var validated = await Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions
        {
            Messages = [messages[messages.Count - 1]],
        });
        var persisted = validated[0];
        var persistedPart = Assert.IsType<ToolUIPart>(persisted.Parts[0]);
        Assert.Equal(ToolUIPartState.InputStreaming, persistedPart.State);
        Assert.Equal(ToolInputEncoding.Text, persistedPart.InputEncoding);
        Assert.Equal("text(\"", persistedPart.Input!.Value.GetString());
        Assert.Equal("text(\"", persistedPart.RawInput!.Value.GetString());

        var resumed = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"tool-input-delta","toolCallId":"call-1","inputTextDelta":"probe\");\n"}"""))
            {
                Message = persisted,
                TerminateOnError = true,
            });

        var resumedPart = Assert.IsType<ToolUIPart>(resumed[resumed.Count - 1].Parts[0]);
        Assert.Equal(ToolUIPartState.InputStreaming, resumedPart.State);
        Assert.Equal(ToolInputEncoding.Text, resumedPart.InputEncoding);
        Assert.Equal("text(\"probe\");\n", resumedPart.Input!.Value.GetString());
        Assert.Equal("text(\"probe\");\n", resumedPart.RawInput!.Value.GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should discard a seeded assistant message when the stream starts with a different message id", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_discard_a_seeded_assistant_message_when_the_stream_starts_with_a_different_message_id()
    {
        var seeded = new UIMessage(UIMessageRole.Assistant, [new TextUIPart("Previous response")])
        {
            Id = "previous-response",
            Metadata = JsonDocument.Parse("""{"source":"previous"}""").RootElement.Clone(),
        };

        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"resumed-response"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Resumed response"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish"}"""))
            {
                Message = seeded,
            });

        var last = messages[messages.Count - 1];
        Assert.Equal("resumed-response", last.Id);
        Assert.Equal(UIMessageRole.Assistant, last.Role);
        Assert.Null(last.Metadata);
        var part = Assert.IsType<TextUIPart>(Assert.Single(last.Parts));
        Assert.Equal("Resumed response", part.Text);
        Assert.Equal("done", part.State);
        Assert.Null(part.ProviderMetadata);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("json")]
    [UpstreamTest(Prefix + "keeps JSON parsing for interleaved calls with encoding %s", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Keeps_JSON_parsing_for_interleaved_calls_with_encoding(string? inputEncoding)
    {
        var encodingField = inputEncoding == null ? string.Empty : ",\"inputEncoding\":\"" + inputEncoding + "\"";
        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"message-1"}""",
                """{"type":"tool-input-start","toolCallId":"text","toolName":"exec","inputEncoding":"text"}""",
                "{\"type\":\"tool-input-start\",\"toolCallId\":\"json\",\"toolName\":\"weather\"" + encodingField + "}",
                "{\"type\":\"tool-input-start\",\"toolCallId\":\"encoded-string\",\"toolName\":\"legacy\"" + encodingField + "}",
                """{"type":"tool-input-delta","toolCallId":"text","inputTextDelta":"\"quoted"}""",
                """{"type":"tool-input-delta","toolCallId":"json","inputTextDelta":"{\"city\":\"Paris\"}"}""",
                """{"type":"tool-input-delta","toolCallId":"encoded-string","inputTextDelta":"\"already encoded\""}""",
                """{"type":"tool-input-delta","toolCallId":"text","inputTextDelta":"\""}"""))
            {
                TerminateOnError = true,
            });

        var parts = messages[messages.Count - 1].Parts.OfType<ToolUIPart>().ToList();
        Assert.Equal(3, parts.Count);
        Assert.Equal("text", parts[0].ToolCallId);
        Assert.Equal("\"quoted\"", parts[0].Input!.Value.GetString());
        Assert.Equal("json", parts[1].ToolCallId);
        Assert.Equal("Paris", parts[1].Input!.Value.GetProperty("city").GetString());
        Assert.Equal("encoded-string", parts[2].ToolCallId);
        Assert.Equal("already encoded", parts[2].Input!.Value.GetString());
    }

    [Theory]
    [InlineData("the same message id", true)]
    [InlineData("no message id", false)]
    [UpstreamTest(Prefix + "should preserve a seeded assistant message with $name", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_preserve_a_seeded_assistant_message_with_name(string name, bool hasStartChunk)
    {
        Assert.NotEmpty(name);
        var seeded = new UIMessage(UIMessageRole.Assistant, [new TextUIPart("Partial response")])
        {
            Id = "resumed-response",
            Metadata = JsonDocument.Parse("""{"source":"seed"}""").RootElement.Clone(),
        };

        var chunks = new List<string>();
        if (hasStartChunk)
        {
            chunks.Add("""{"type":"start","messageId":"resumed-response"}""");
        }

        chunks.Add("""{"type":"text-start","id":"text-1"}""");
        chunks.Add("""{"type":"text-delta","id":"text-1","delta":" continued"}""");
        chunks.Add("""{"type":"text-end","id":"text-1"}""");
        chunks.Add("""{"type":"finish"}""");

        var messages = await Read(new ReadUIMessageStreamOptions(CreateUIMessageStream(chunks.ToArray())) { Message = seeded });

        var last = messages[messages.Count - 1];
        Assert.Equal("resumed-response", last.Id);
        Assert.Equal(UIMessageRole.Assistant, last.Role);
        Assert.Equal("seed", last.Metadata!.Value.GetProperty("source").GetString());
        Assert.Equal(2, last.Parts.Count);
        var first = Assert.IsType<TextUIPart>(last.Parts[0]);
        Assert.Equal("Partial response", first.Text);
        var second = Assert.IsType<TextUIPart>(last.Parts[1]);
        Assert.Equal(" continued", second.Text);
        Assert.Equal("done", second.State);
        Assert.Null(second.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "should continue a hydrated partial static tool call", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_continue_a_hydrated_partial_static_tool_call()
    {
        await AssertHydratedPartialToolCall(JsonDocument.Parse("\"{\\\"title\\\":\\\"Hel\"").RootElement.Clone());
    }

    [Fact]
    [UpstreamTest(Prefix + "should accept a hydrated partial static tool call without raw input", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_accept_a_hydrated_partial_static_tool_call_without_raw_input()
    {
        await AssertHydratedPartialToolCall(null);
    }

    private static async Task AssertHydratedPartialToolCall(JsonElement? rawInput)
    {
        var message = new UIMessage(
            UIMessageRole.Assistant,
            [
                new ToolUIPart("createDocument", "tool-1", ToolUIPartState.InputStreaming)
                {
                    Input = JsonDocument.Parse("""{"title":"Hel"}""").RootElement.Clone(),
                    RawInput = rawInput,
                },
            ])
        {
            Id = "msg-123",
        };

        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"tool-input-delta","toolCallId":"tool-1","inputTextDelta":"lo\"}"}""",
                """{"type":"tool-input-available","toolCallId":"tool-1","toolName":"createDocument","input":{"title":"Hello"}}"""))
            {
                Message = message,
                TerminateOnError = true,
            });

        var part = Assert.IsType<ToolUIPart>(Assert.Single(messages[messages.Count - 1].Parts));
        Assert.Equal("tool-createDocument", part.Type);
        Assert.Equal("tool-1", part.ToolCallId);
        Assert.Equal(ToolUIPartState.InputAvailable, part.State);
        Assert.Equal("Hello", part.Input!.Value.GetProperty("title").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should return a ui message object stream for a basic input stream", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_return_a_ui_message_object_stream_for_a_basic_input_stream()
    {
        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"msg-123"}""",
                """{"type":"start-step"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello, "}""",
                """{"type":"text-delta","id":"text-1","delta":"world!"}""",
                """{"type":"text-end","id":"text-1"}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}""")));

        Assert.Equal(5, messages.Count);
        Assert.All(messages, message =>
        {
            Assert.Equal("msg-123", message.Id);
            Assert.Equal(UIMessageRole.Assistant, message.Role);
            Assert.Null(message.Metadata);
        });

        Assert.Empty(messages[0].Parts);

        AssertStepAndText(messages[1], string.Empty, "streaming");
        AssertStepAndText(messages[2], "Hello, ", "streaming");
        AssertStepAndText(messages[3], "Hello, world!", "streaming");
        AssertStepAndText(messages[4], "Hello, world!", "done");
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve tool parts when tool call ids repeat across steps", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_preserve_tool_parts_when_tool_call_ids_repeat_across_steps()
    {
        var messages = await Read(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"msg-123"}""",
                """{"type":"start-step"}""",
                """{"type":"tool-input-available","toolCallId":"call-0","toolName":"recordStep","input":{"step":1},"providerMetadata":{"openai":{"itemId":"fc-step-1"}}}""",
                """{"type":"tool-output-available","toolCallId":"call-0","output":{"recorded":1}}""",
                """{"type":"finish-step"}""",
                """{"type":"start-step"}""",
                """{"type":"tool-input-available","toolCallId":"call-0","toolName":"recordStep","input":{"step":2},"providerMetadata":{"openai":{"itemId":"fc-step-2"}}}""",
                """{"type":"tool-output-available","toolCallId":"call-0","output":{"recorded":2}}""",
                """{"type":"finish-step"}""",
                """{"type":"finish"}""")));

        var toolParts = messages[messages.Count - 1].Parts.OfType<ToolUIPart>().ToList();

        Assert.Equal(2, toolParts.Count);
        AssertToolPart(toolParts[0], 1, "fc-step-1");
        AssertToolPart(toolParts[1], 2, "fc-step-2");
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw an error when encountering an error UI stream part", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_throw_an_error_when_encountering_an_error_UI_stream_part()
    {
        var uiMessages = UIMessageStreamConverter.ReadUIMessageStream(
            new ReadUIMessageStreamOptions(CreateUIMessageStream(
                """{"type":"start","messageId":"msg-123"}""",
                """{"type":"text-start","id":"text-1"}""",
                """{"type":"text-delta","id":"text-1","delta":"Hello"}""",
                """{"type":"error","errorText":"Test error message"}"""))
            {
                TerminateOnError = true,
            });

        var error = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await foreach (var unused in uiMessages)
            {
            }
        });
        Assert.Contains("Test error message", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "should cancel the input stream when iteration exits early", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Should_cancel_the_input_stream_when_iteration_exits_early()
    {
        var cancelCount = 0;

        async IAsyncEnumerable<JsonObject> Source()
        {
            try
            {
                yield return Chunk("""{"type":"start","messageId":"msg-123"}""");
                yield return Chunk("""{"type":"text-start","id":"text-1"}""");
                yield return Chunk("""{"type":"text-delta","id":"text-1","delta":"Hello"}""");
                await Task.Delay(Timeout.Infinite);
            }
            finally
            {
                Interlocked.Increment(ref cancelCount);
            }
        }

        await foreach (var message in UIMessageStreamConverter.ReadUIMessageStream(new ReadUIMessageStreamOptions(Source())))
        {
            if (message.Parts.Any(part => part is TextUIPart { Text.Length: > 0 }))
            {
                break;
            }
        }

        Assert.Equal(1, Volatile.Read(ref cancelCount));
    }

    private static void AssertStepAndText(UIMessage message, string text, string state)
    {
        Assert.Equal(2, message.Parts.Count);
        Assert.IsType<StepStartUIPart>(message.Parts[0]);
        var part = Assert.IsType<TextUIPart>(message.Parts[1]);
        Assert.Equal(text, part.Text);
        Assert.Equal(state, part.State);
        Assert.Null(part.ProviderMetadata);
    }

    private static void AssertToolPart(ToolUIPart part, int step, string itemId)
    {
        Assert.Equal("tool-recordStep", part.Type);
        Assert.Equal("call-0", part.ToolCallId);
        Assert.Equal(ToolUIPartState.OutputAvailable, part.State);
        Assert.Equal("{\"step\":" + step + "}", part.Input!.Value.GetRawText());
        Assert.Equal("{\"recorded\":" + step + "}", part.Output!.Value.GetRawText());
        Assert.Equal(itemId, part.CallProviderMetadata!["openai"].GetProperty("itemId").GetString());
    }

    private static JsonObject Chunk(string json) => (JsonObject)JsonNode.Parse(json)!;

    private static async IAsyncEnumerable<JsonObject> CreateUIMessageStream(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return Chunk(chunk);
            await Task.Yield();
        }
    }

    private static async Task<List<UIMessage>> Read(ReadUIMessageStreamOptions options)
    {
        var messages = new List<UIMessage>();
        await foreach (var message in UIMessageStreamConverter.ReadUIMessageStream(options))
        {
            messages.Add(message);
        }

        return messages;
    }
}
