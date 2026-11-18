// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when a single tool needs approval (user-defined)</c>.</summary>
public sealed class WhenASingleToolNeedsApprovalUserDefinedTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when a single tool needs approval (user-defined)::";

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the full stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_full_stream()
    {
        var parts = await ReadAsync(Run());

        Assert.Equal(
            new[]
            {
                "start",
                "start-step",
                "tool-call",
                "tool-approval-request",
                "finish-step",
                "finish",
            },
            Types(parts)
        );
        AssertStartStep(parts[1]);
        var call = Assert.IsType<ToolCallPart>(parts[2]).ToolCall;
        AssertToolCall(call, "call-1", "tool1", "{\"value\":\"value\"}");
        var request = Assert.IsType<ToolApprovalRequestPart>(parts[3]);
        Assert.Equal("id-1", request.ApprovalId);
        AssertToolCall(request.ToolCall, "call-1", "tool1", "{\"value\":\"value\"}");
        AssertStepAndFinish(parts, 4, FinishReason.ToolCalls, null);
    }

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the UI message stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_UI_message_stream()
    {
        AssertJson(
            """
            [
              { "type": "start" },
              { "type": "start-step" },
              { "input": { "value": "value" }, "toolCallId": "call-1", "toolName": "tool1", "type": "tool-input-available" },
              { "approvalId": "id-1", "toolCallId": "call-1", "type": "tool-approval-request" },
              { "type": "finish-step" },
              { "finishReason": "tool-calls", "type": "finish" }
            ]
            """,
            await UiChunksAsync(Run())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the content",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_content()
    {
        var content = await ContentAsync(Run());

        Assert.Equal(2, content.Count);
        AssertToolCall(content[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequest(content[1], "id-1", "call-1", "tool1", "{\"value\":\"value\"}");
    }

    [Fact]
    [UpstreamTest(
        Id + "should add tool approval requests to the response messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_tool_approval_requests_to_the_response_messages()
    {
        var messages = await Run().ResponseMessages;

        var assistant = Assert.IsType<AssistantModelMessage>(Assert.Single(messages));
        Assert.Equal(2, assistant.Parts.Count);
        AssertToolCallPart(assistant.Parts[0], "call-1", "tool1", "{\"value\":\"value\"}");
        AssertApprovalRequestPart(assistant.Parts[1], "id-1", "call-1");
    }

    private static StreamTextResult Run() =>
        Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new ScriptedModel(_ =>
                        ToolCallsStep(("call-1", "{ \"value\": \"value\" }"))
                    ),
                    Prompt = "test-input",
                    GenerateId = StreamMockId("id"),
                    NowMs = () => 0,
                    Tools = new[] { ValueTool(null) },
                    ToolApproval = new ToolApprovalConfiguration { { "tool1", "user-approval" } },
                }
            );
}
