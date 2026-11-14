// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToUiMessageChunk;

/// <summary>Port of <c>to-ui-message-chunk.test.ts</c> &gt; <c>toUIMessageChunk</c> &gt; <c>error text of failed tool calls</c>.</summary>
public sealed class ErrorTextOfFailedToolCallsTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk > error text of failed tool calls::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";
    private const string PartialNote = "The onError argument is the formatted error message, not the error object. The invalid tool call has a NoSuchToolError, because the converter has no tool list and marks only calls of unknown tools as dynamic. The expected text of the invalid call is the error string of that error.";

    [Fact]
    [UpstreamTest(Prefix + "keeps the text that the model received by default", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public void Keeps_the_text_that_the_model_received_by_default()
        => AssertErrorText(new ToUIMessageChunkOptions(), "Error: tool failed", "AI_NoSuchToolError: tool failed");

    [Fact]
    [UpstreamTest(Prefix + "uses the default onError when tool errors are redacted", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public void Uses_the_default_onError_when_tool_errors_are_redacted()
        => AssertErrorText(new ToUIMessageChunkOptions { RedactToolErrors = true }, "An error occurred.", "An error occurred.");

    [Fact]
    [UpstreamTest(Prefix + "uses a provided onError", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public void Uses_a_provided_onError()
        => AssertErrorText(new ToUIMessageChunkOptions { OnError = FormatError }, "handled: tool failed", "handled: AI_NoSuchToolError: tool failed");

    [Fact]
    [UpstreamTest(Prefix + "keeps the text that the model received when a provided onError does not redact tool errors", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public void Keeps_the_text_that_the_model_received_when_a_provided_onError_does_not_redact_tool_errors()
        => AssertErrorText(new ToUIMessageChunkOptions { OnError = FormatError, RedactToolErrors = false }, "Error: tool failed", "AI_NoSuchToolError: tool failed");

    [Fact]
    [UpstreamTest(Prefix + "uses onError for invalid tool calls of provider-executed tools", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public void Uses_onError_for_invalid_tool_calls_of_provider_executed_tools()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ToolCallPart(InvalidToolCall(providerExecuted: true)));

        AssertJson(
            """{"type":"tool-input-error","toolCallId":"call-1","toolName":"runtimeTool","input":"{broken","providerExecuted":true,"dynamic":true,"errorText":"An error occurred."}""",
            chunk);
    }

    private static void AssertJson(string expected, JsonObject? actual)
        => Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    private static string FormatError(string message) => "handled: " + message;

    private static GeneratedToolCall InvalidToolCall(bool providerExecuted = false)
        => new("call-1", "runtimeTool", "{broken") { Dynamic = true, Invalid = true, Error = new NoSuchToolError("runtimeTool", message: "tool failed"), ProviderExecuted = providerExecuted };

    private static void AssertErrorText(ToUIMessageChunkOptions options, string expectedErrorText, string expectedInvalidErrorText)
    {
        var toolError = new ToolErrorPart(new ExecutedTool("call-1", "runtimeTool", "{\"error\":\"tool failed\"}", true)
        {
            Dynamic = true,
            Error = new JsError("tool failed"),
            ModelOutput = "Error: tool failed",
        });

        AssertJson(
            $$"""{"type":"tool-output-error","toolCallId":"call-1","errorText":"{{expectedErrorText}}","dynamic":true}""",
            UIMessageStreamConverter.ToUIMessageChunk(toolError, options));
        AssertJson(
            $$"""{"type":"tool-input-error","toolCallId":"call-1","toolName":"runtimeTool","input":"{broken","dynamic":true,"errorText":"{{expectedInvalidErrorText}}"}""",
            UIMessageStreamConverter.ToUIMessageChunk(new ToolCallPart(InvalidToolCall()), options));
    }
}
