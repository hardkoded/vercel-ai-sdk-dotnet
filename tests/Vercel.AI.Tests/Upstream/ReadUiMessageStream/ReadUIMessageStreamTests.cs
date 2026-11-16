// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ReadUiMessageStream;

/// <summary>Port of <c>read-ui-message-stream.test.ts</c> &gt; <c>readUIMessageStream</c>.</summary>
public sealed class ReadUIMessageStreamTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/read-ui-message-stream.test.ts::readUIMessageStream::";

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
