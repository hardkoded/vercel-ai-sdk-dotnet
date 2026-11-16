// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c>.</summary>
public sealed class ProcessUiMessageStreamTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream::";

    [Fact]
    [UpstreamTest(
        Prefix + "should apply output chunk tool metadata to static and dynamic tool parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_apply_output_chunk_tool_metadata_to_static_and_dynamic_tool_parts()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"dynamic-success","toolName":"tool-name","input":{"query":"test"},"dynamic":true},{"type":"tool-output-available","toolCallId":"dynamic-success","output":{"result":"provider-result"},"dynamic":true,"toolMetadata":{"phase":"dynamic-output-available"}},{"type":"tool-input-available","toolCallId":"dynamic-error","toolName":"tool-name","input":{"query":"test"},"dynamic":true,"toolMetadata":{"phase":"dynamic-input"}},{"type":"tool-output-error","toolCallId":"dynamic-error","errorText":"error-text","dynamic":true,"toolMetadata":{"phase":"dynamic-output-error"}},{"type":"tool-input-available","toolCallId":"static-success","toolName":"tool-name","input":{"query":"test"}},{"type":"tool-output-available","toolCallId":"static-success","output":{"result":"provider-result"},"toolMetadata":{"phase":"static-output-available"}},{"type":"tool-input-available","toolCallId":"static-error","toolName":"tool-name","input":{"query":"test"},"toolMetadata":{"phase":"static-input"}},{"type":"tool-output-error","toolCallId":"static-error","errorText":"error-text","toolMetadata":{"phase":"static-output-error"}},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [{"toolCallId":"dynamic-success","state":"output-available","toolMetadata":{"phase":"dynamic-output-available"}},{"toolCallId":"dynamic-error","state":"output-error","toolMetadata":{"phase":"dynamic-output-error"}},{"toolCallId":"static-success","state":"output-available","toolMetadata":{"phase":"static-output-available"}},{"toolCallId":"static-error","state":"output-error","toolMetadata":{"phase":"static-output-error"}}]
            """,
            new JsonArray(
                harness
                    .Parts.OfType<JsonObject>()
                    .Where(part => UIMessageJson.IsToolPart(part))
                    .Select(part =>
                        (JsonNode?)
                            ProcessUIMessageStreamHarness.PickKeys(
                                part,
                                "toolCallId",
                                "state",
                                "toolMetadata"
                            )
                    )
                    .ToArray()
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call onToolCall for client-executed tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onToolCall_for_client_executed_tools()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var onToolCallInvoked = false;
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"query":"test"}},{"type":"finish-step"},{"type":"finish"}]
            """,
            new ProcessUIMessageStreamOptions(harness.RunUpdateMessageJob, error => throw error)
            {
                OnToolCall = _ =>
                {
                    onToolCallInvoked = true;
                    return Task.CompletedTask;
                },
            }
        );

        Assert.True(onToolCallInvoked);
        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "type": "step-start"
              },
              {
                "input": {
                  "query": "test"
                },
                "state": "input-available",
                "toolCallId": "tool-call-id",
                "type": "tool-tool-name"
              }
            ]
            """,
            harness.Parts
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves approval descriptors through request and response states",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_approval_descriptors_through_request_and_response_states()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"input":{"userId":"user-123"},"toolCallId":"call-1","toolName":"deleteAccount","type":"tool-input-available"},{"approvalDescriptor":{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"},"approvalId":"approval-1","toolCallId":"call-1","type":"tool-approval-request"},{"approvalId":"approval-1","approved":true,"type":"tool-approval-response"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [{"id":"approval-1","descriptor":{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"}},{"id":"approval-1","approved":true,"descriptor":{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"}}]
            """,
            harness.WriteCallApprovals()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves an approval descriptor restored from a persisted message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_an_approval_descriptor_restored_from_a_persisted_message()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState(
            "msg-123",
            """
            {"role":"assistant","id":"msg-123","parts":[{"type":"tool-deleteAccount","toolCallId":"call-1","state":"approval-requested","input":{"userId":"user-123"},"approval":{"id":"approval-1","descriptor":{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"}}}]}
            """
        );
        await harness.ProcessAsync(
            """
            [{"approvalId":"approval-1","approved":true,"type":"tool-approval-response"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [{"id":"approval-1","approved":true,"descriptor":{"action":"deleteAccount","permissions":["account:delete"],"risk":"high"}}]
            """,
            harness.WriteCallApprovals()
        );
    }
}
