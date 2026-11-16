// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool title support &gt; tool with title in error state</c>.</summary>
public sealed class ToolTitleSupportToolWithTitleInErrorStateTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool title support > tool with title in error state::should preserve title even in error state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_title_even_in_error_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-error", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-error"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-error","toolName":"errorTool","title":"Error Tool"},{"type":"tool-input-available","toolCallId":"tool-call-error","toolName":"errorTool","input":{"invalid":"data"},"title":"Error Tool"},{"type":"tool-output-error","toolCallId":"tool-call-error","errorText":"Tool execution failed"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var toolPart = ProcessUIMessageStreamHarness.Find(harness.Parts, part =>
            (string?)part["type"] == "tool-errorTool"
        );
        Assert.NotNull(toolPart);
        Assert.Equal("Error Tool", (string?)toolPart["title"]);
        Assert.Equal("output-error", (string?)toolPart["state"]);
        Assert.Equal("Tool execution failed", (string?)toolPart["errorText"]);
    }
}
