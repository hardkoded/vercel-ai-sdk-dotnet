// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool title support &gt; static tool with title</c>.</summary>
public sealed class ToolTitleSupportStaticToolWithTitleTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "tool title support > static tool with title::should include title in tool invocation parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_title_in_tool_invocation_parts()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-0","toolName":"weatherTool","title":"Weather Information"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"{\"location\":\"Paris\"}"},{"type":"tool-input-available","toolCallId":"tool-call-0","toolName":"weatherTool","input":{"location":"Paris"},"title":"Weather Information"},{"type":"tool-output-available","toolCallId":"tool-call-0","output":"Sunny, 22°C"},{"type":"finish-step"},{"type":"finish"}]
            """);

        var toolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part => (string?)part["type"] == "tool-weatherTool");
        Assert.NotNull(toolPart);
        Assert.Equal("Weather Information", (string?)toolPart["title"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "tool title support > static tool with title::should preserve title through all states", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_title_through_all_states()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync("""
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-0","toolName":"weatherTool","title":"Weather Information"},{"type":"tool-input-delta","toolCallId":"tool-call-0","inputTextDelta":"{\"location\":\"Paris\"}"},{"type":"tool-input-available","toolCallId":"tool-call-0","toolName":"weatherTool","input":{"location":"Paris"},"title":"Weather Information"},{"type":"tool-output-available","toolCallId":"tool-call-0","output":"Sunny, 22°C"},{"type":"finish-step"},{"type":"finish"}]
            """);

        var inputStreamingWrite = harness.FindWriteCall(part => (string?)part["type"] == "tool-weatherTool" && (string?)part["state"] == "input-streaming");
        Assert.Equal("Weather Information", (string?)ProcessUIMessageStreamHarness.Find((JsonArray)inputStreamingWrite!["message"]!["parts"]!, part => (string?)part["type"] == "tool-weatherTool")!["title"]);
        var inputAvailableWrite = harness.FindWriteCall(part => (string?)part["type"] == "tool-weatherTool" && (string?)part["state"] == "input-available");
        Assert.Equal("Weather Information", (string?)ProcessUIMessageStreamHarness.Find((JsonArray)inputAvailableWrite!["message"]!["parts"]!, part => (string?)part["type"] == "tool-weatherTool")!["title"]);
        var outputAvailableWrite = harness.FindWriteCall(part => (string?)part["type"] == "tool-weatherTool" && (string?)part["state"] == "output-available");
        Assert.Equal("Weather Information", (string?)ProcessUIMessageStreamHarness.Find((JsonArray)outputAvailableWrite!["message"]!["parts"]!, part => (string?)part["type"] == "tool-weatherTool")!["title"]);
    }
}
