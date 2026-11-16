// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool title support &gt; dynamic tool with title</c>.</summary>
public sealed class ToolTitleSupportDynamicToolWithTitleTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool title support > dynamic tool with title::should include title in dynamic tool invocation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_title_in_dynamic_tool_invocation()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-456", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-456"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"calculate","dynamic":true,"title":"Calculator"},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{\"a\":5,\"b\":3}"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"calculate","input":{"a":5,"b":3},"dynamic":true,"title":"Calculator"},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":8},"dynamic":true},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var toolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part =>
            (string?)part["type"] == "dynamic-tool"
        );
        Assert.NotNull(toolPart);
        Assert.Equal("Calculator", (string?)toolPart["title"]);
        Assert.Equal("calculate", (string?)toolPart["toolName"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool title support > dynamic tool with title::should maintain title through dynamic tool states",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_maintain_title_through_dynamic_tool_states()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-456", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-456"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-1","toolName":"calculate","dynamic":true,"title":"Calculator"},{"type":"tool-input-delta","toolCallId":"tool-call-1","inputTextDelta":"{\"a\":5,\"b\":3}"},{"type":"tool-input-available","toolCallId":"tool-call-1","toolName":"calculate","input":{"a":5,"b":3},"dynamic":true,"title":"Calculator"},{"type":"tool-output-available","toolCallId":"tool-call-1","output":{"result":8},"dynamic":true},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var finalToolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part =>
            (string?)part["type"] == "dynamic-tool"
        )!;
        Assert.Equal("output-available", (string?)finalToolPart["state"]);
        Assert.Equal("Calculator", (string?)finalToolPart["title"]);
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"a":5,"b":3}
            """,
            finalToolPart["input"]
        );
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"result":8}
            """,
            finalToolPart["output"]
        );
    }
}
