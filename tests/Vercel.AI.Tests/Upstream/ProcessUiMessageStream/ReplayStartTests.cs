// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>replay start</c>.</summary>
public sealed class ReplayStartTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Theory]
    [InlineData("msg-123")]
    [InlineData(null)]
    [UpstreamTest(
        Prefix + "replay start::resets existing state only on the first start with message ID %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Resets_existing_state_only_on_the_first_start_with_message_ID_s(
        string? messageId
    )
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState(
            "msg-123",
            """
            {"id":"msg-123","role":"assistant","metadata":{"stale":true},"parts":[{"type":"text","text":"old text","state":"streaming"}]}
            """
        );
        harness.State.ActiveTextParts["old"] = new JsonObject
        {
            ["type"] = "text",
            ["text"] = "old text",
        };
        harness.State.ActiveReasoningParts["old"] = new JsonObject
        {
            ["type"] = "reasoning",
            ["text"] = "old reasoning",
        };
        harness.State.PartialToolCalls["old"] = new PartialToolCall
        {
            Text = "{",
            Index = 0,
            ToolName = "old",
        };
        harness.State.FinishReason = "length";

        var start = new JsonObject { ["type"] = "start" };
        if (messageId != null)
        {
            start["messageId"] = messageId;
        }

        var restart = new JsonObject { ["type"] = "start" };
        if (messageId != null)
        {
            restart["messageId"] = messageId;
        }

        restart["messageMetadata"] = new JsonObject { ["fresh"] = true };

        await harness.ProcessAsync(
            new JsonArray(
                start,
                JsonNode.Parse("""{"type":"start-step"}"""),
                JsonNode.Parse("""{"type":"text-start","id":"text-1"}"""),
                JsonNode.Parse("""{"type":"text-delta","id":"text-1","delta":"Hello"}"""),
                // Merged streams can emit another start for the same message.
                restart,
                JsonNode.Parse("""{"type":"text-delta","id":"text-1","delta":" world!"}"""),
                JsonNode.Parse("""{"type":"text-end","id":"text-1"}""")
            ).ToJsonString(),
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error)
            {
                ResetStateOnFirstMessageStart = true,
                ResetStateOnMessageIdChange = true,
            }
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"id":"msg-123","role":"assistant","parts":[]}
            """,
            harness.WriteCalls[0]!["message"]
        );
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"id":"msg-123","role":"assistant","metadata":{"fresh":true},"parts":[{"type":"step-start"},{"type":"text","text":"Hello world!","state":"done"}]}
            """,
            harness.Message
        );
        Assert.Empty(harness.State.ActiveTextParts);
        Assert.Empty(harness.State.ActiveReasoningParts);
        Assert.Empty(harness.State.PartialToolCalls);
        Assert.Null(harness.State.FinishReason);
    }
}
