// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
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
