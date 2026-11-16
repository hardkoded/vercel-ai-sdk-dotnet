// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToUiMessageStream;

/// <summary>Port of <c>to-ui-message-stream.test.ts</c> &gt; <c>toUIMessageStream</c>.</summary>
public sealed class ToUIMessageStreamTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream::";

    [Fact]
    [UpstreamTest(Prefix + "routes error parts through onError", Coverage = UpstreamCoverage.Covered)]
    public async Task Routes_error_parts_through_onError()
    {
        var chunks = await Convert(
            [new ErrorPart("boom") { Error = new JsError("boom") }],
            new ToUIMessageChunkOptions { OnError = error => $"handled: {error.Message}" });

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""[{"type":"error","errorText":"handled: boom"}]"""), chunks), chunks.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "routes an invalid tool call through onError once", Coverage = UpstreamCoverage.Covered)]
    public async Task Routes_an_invalid_tool_call_through_onError_once()
    {
        var invalidToolCallError = new NoSuchToolError("test-tool", message: "invalid tool input");
        var errors = new List<Exception>();
        var chunks = await Convert(
            [
                new ToolCallPart(new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"invalid\"}") { Invalid = true, Dynamic = true, Error = invalidToolCallError }),
                new ToolErrorPart(new ExecutedTool("call-1", "test-tool", "{}", true) { Dynamic = true, ModelOutput = "Error: invalid tool input" }),
            ],
            new ToUIMessageChunkOptions
            {
                OnError = error =>
                {
                    errors.Add(error);
                    return "formatted tool error";
                },
            });

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""[{"type":"tool-input-error","toolCallId":"call-1","toolName":"test-tool","input":{"value":"invalid"},"dynamic":true,"errorText":"formatted tool error"}]"""),
                chunks),
            chunks.ToJsonString());
        Assert.Same(invalidToolCallError, Assert.Single(errors));
    }

    [Fact]
    [UpstreamTest(Prefix + "forwards tool errors for valid tool calls", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_tool_errors_for_valid_tool_calls()
    {
        var executionError = new JsError("execution failed");
        var errors = new List<Exception>();
        var chunks = await Convert(
            [
                new ToolCallPart(new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"valid\"}") { Dynamic = true }),
                new ToolErrorPart(new ExecutedTool("call-1", "test-tool", "{}", true) { Dynamic = true, Error = executionError, ModelOutput = "Error: execution failed" }),
            ],
            new ToUIMessageChunkOptions
            {
                OnError = error =>
                {
                    errors.Add(error);
                    return "formatted tool error";
                },
            });

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("""
                    [
                      {"type":"tool-input-available","toolCallId":"call-1","toolName":"test-tool","input":{"value":"valid"},"dynamic":true},
                      {"type":"tool-output-error","toolCallId":"call-1","dynamic":true,"errorText":"formatted tool error"}
                    ]
                    """),
                chunks),
            chunks.ToJsonString());
        Assert.Same(executionError, Assert.Single(errors));
    }

    [Theory]
    [InlineData("tool execution error")]
    [InlineData("invalid tool call")]
    [UpstreamTest(Prefix + "sends a $name that converts back to the tool result the model received", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Sends_a_name_that_converts_back_to_the_tool_result_the_model_received(string name)
    {
        ToolCallPart toolCall;
        ToolErrorPart toolError;
        object errorOutput;
        if (name == "tool execution error")
        {
            errorOutput = new JsError("execution failed");
            toolCall = new ToolCallPart(new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"input\"}") { Dynamic = true });
            toolError = new ToolErrorPart(new ExecutedTool("call-1", "test-tool", "{\"value\":\"input\"}", true) { Dynamic = true, Error = (Exception)errorOutput, ModelOutput = "Error: execution failed" });
        }
        else
        {
            // streamText follows an invalid tool call with a tool-error part that contains the error text for the model
            errorOutput = "Error: invalid tool input";
            toolCall = new ToolCallPart(new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"input\"}")
            {
                Dynamic = true,
                Invalid = true,
                Error = new JsError("invalid tool input"),
            });
            toolError = new ToolErrorPart(new ExecutedTool("call-1", "test-tool", "{\"value\":\"input\"}", true) { Dynamic = true, ModelOutput = (string)errorOutput });
        }

        TextStreamPart[] parts =
        [
            new StartPart(),
            new StartStepPart(),
            toolCall,
            toolError,
            TestStep,
            new FinishPart(FinishReason.Stop, new LanguageModelUsage(0, 0, 0)),
        ];

        UIMessage? uiMessage = null;
        await foreach (var message in UIMessageStreamConverter.ReadUIMessageStream(new ReadUIMessageStreamOptions(UIMessageStreamConverter.ToUIMessageStream(ToAsync(parts)))))
        {
            uiMessage = message;
        }

        var modelMessages = UIMessageConversion.ConvertToModelMessages([uiMessage!]);

        var toolMessage = Assert.IsType<ToolModelMessage>(modelMessages[modelMessages.Count - 1]);
        Assert.Equal("call-1", toolMessage.ToolCallId);
        Assert.Equal("test-tool", toolMessage.ToolName);

        // the output that streamText sends to the model
        var expected = Assert.IsType<ErrorTextToolModelOutput>(ToolOutputs.CreateToolModelOutput(
            "call-1",
            JsonDocument.Parse("{\"value\":\"input\"}").RootElement,
            errorOutput,
            true,
            null,
            "text"));
        Assert.Equal("error-text", toolMessage.OutputType);
        Assert.Equal(expected.Value, toolMessage.OutputJson);
    }

    [Fact]
    [UpstreamTest(Prefix + "reports UI chunk conversion failures as failed exactly once", Coverage = UpstreamCoverage.Covered, Commit = "1a94af38c72f7f4c0df39984a170f3b02ae24bdc")]
    public async Task Reports_UI_chunk_conversion_failures_as_failed_exactly_once()
    {
        var conversionError = new JsError("UI chunk conversion failed");
        var onEndEvents = new List<UIMessageStreamOnEndEvent>();

        var stream = UIMessageStreamConverter.ToUIMessageStream(
            ToAsync([new ErrorPart("generation failed") { Error = new JsError("generation failed") }]),
            new ToUIMessageChunkOptions
            {
                OnError = _ => throw conversionError,
                OnEnd = onEndEvent =>
                {
                    onEndEvents.Add(onEndEvent);
                    return Task.CompletedTask;
                },
            });

        var thrown = await Assert.ThrowsAsync<JsError>(async () =>
        {
            await foreach (var unused in stream)
            {
            }
        });
        Assert.Same(conversionError, thrown);

        var onEnd = Assert.Single(onEndEvents);
        Assert.Equal(UIMessageStreamOutcomeStatus.Failed, onEnd.Outcome.Status);
        Assert.Same(conversionError, onEnd.Outcome.Error);
    }

    private static readonly TextStreamPart TestStep = new StepFinishPart(
        new StepResult("", null, [], [], FinishReason.Stop, new LanguageModelUsage(0, 0, 0), []),
        new LanguageModelResponseMetadata("id-0", DateTimeOffset.FromUnixTimeMilliseconds(0), "mock-model-id"));

    internal static async Task<JsonArray> Convert(IReadOnlyList<TextStreamPart> parts, ToUIMessageChunkOptions? options = null)
    {
        var chunks = new JsonArray();
        await foreach (var chunk in UIMessageStreamConverter.ToUIMessageStream(ToAsync(parts), options))
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static async IAsyncEnumerable<TextStreamPart> ToAsync(IReadOnlyList<TextStreamPart> parts)
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }
}
