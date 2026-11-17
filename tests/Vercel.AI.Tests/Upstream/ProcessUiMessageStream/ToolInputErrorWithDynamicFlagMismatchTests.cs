// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>tool input error with dynamic flag mismatch</c>.</summary>
public sealed class ToolInputErrorWithDynamicFlagMismatchTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool input error with dynamic flag mismatch::should produce exactly one tool part (no duplicate)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_produce_exactly_one_tool_part_no_duplicate()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-start"},{"inputTextDelta":"{ \"foo\": \"bar\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","input":"{ \"foo\": \"bar\" }","toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-error","dynamic":true},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        Assert.Single(harness.Parts.OfType<JsonObject>(), part =>
            (string?)part["toolCallId"] == "call-1"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool input error with dynamic flag mismatch::should keep the static tool type from tool-input-start",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_the_static_tool_type_from_tool_input_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-start"},{"inputTextDelta":"{ \"foo\": \"bar\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","input":"{ \"foo\": \"bar\" }","toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-error","dynamic":true},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        Assert.Equal("tool-nonExistentTool", (string?)harness.FindToolPart("call-1")!["type"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "tool input error with dynamic flag mismatch::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start"},{"type":"start-step"},{"toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-start"},{"inputTextDelta":"{ \"foo\": \"bar\" }","toolCallId":"call-1","type":"tool-input-delta"},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","input":"{ \"foo\": \"bar\" }","toolCallId":"call-1","toolName":"nonExistentTool","type":"tool-input-error","dynamic":true},{"errorText":"Model tried to call unavailable tool 'nonExistentTool'.","toolCallId":"call-1","type":"tool-output-error"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "type": "step-start"
              },
              {
                "errorText": "Model tried to call unavailable tool 'nonExistentTool'.",
                "input": "{ \"foo\": \"bar\" }",
                "state": "output-error",
                "toolCallId": "call-1",
                "type": "tool-nonExistentTool"
              }
            ]
            """,
            harness.Parts
        );
    }
}
