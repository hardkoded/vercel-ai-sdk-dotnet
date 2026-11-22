// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; tool execution approval &gt; when experimental_toolApprovalSecret is configured &gt; when the model calls a tool</c>.</summary>
public sealed class WhenTheModelCallsAToolTests
{
    private const string Id =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > tool execution approval > when experimental_toolApprovalSecret is configured > when the model calls a tool::";

    private const string TestSecret = "test-hmac-secret-do-not-use-in-production";

    private static Task<GenerateTextResult<string>> CallTool(
        Func<string> getStatus,
        Action onExecute,
        IReadOnlyList<ModelMessage>? messages = null
    )
    {
        var step = 0;
        var tool = ValueTool(
            (_, _) =>
            {
                onExecute();
                return Task.FromResult("result1");
            }
        );

        return Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            step++ == 0
                                ? ToolCalls("tool1", ("call-1", "{ \"value\": \"test\" }"))
                                : Text("done"),
                    },
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        { "tool1", (_, _) => Task.FromResult<ToolApprovalStatus?>(new ToolApprovalStatus(getStatus())) },
                    },
                    ExperimentalToolApprovalSecret = TestSecret,
                    StopWhen = StopWhen.IsStepCount(3),
                    Messages = messages ?? new ModelMessage[] { new UserModelMessage("test") },
                }
            );
    }

    private static AssistantToolApprovalRequestPart GetApprovalRequest(
        IEnumerable<ModelMessage> messages
    ) =>
        messages
            .OfType<AssistantModelMessage>()
            .SelectMany(message => message.Parts)
            .OfType<AssistantToolApprovalRequestPart>()
            .First();

    [Theory]
    [InlineData("user-approval")]
    [InlineData("approved")]
    [UpstreamTest(
        Id + "should sign the approval request when the tool approval status is %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "63593a7f6a405b6ac8f1283aa6e95cff4c44a5ab"
    )]
    public async Task Should_sign_the_approval_request_when_the_tool_approval_status_is(
        string status
    )
    {
        var result = await CallTool(() => status, () => { });

        var approvalRequest = GetApprovalRequest(result.ResponseMessages);

        using var input = JsonDocument.Parse("{\"value\":\"test\"}");
        Assert.Equal(
            ToolApprovalSignatures.SignToolApproval(
                TestSecret,
                approvalRequest.ApprovalId,
                "call-1",
                "tool1",
                input.RootElement
            ),
            approvalRequest.Signature
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should not sign the approval request when the tool call is denied automatically",
        Coverage = UpstreamCoverage.Covered,
        Commit = "63593a7f6a405b6ac8f1283aa6e95cff4c44a5ab"
    )]
    public async Task Should_not_sign_the_approval_request_when_the_tool_call_is_denied_automatically()
    {
        var result = await CallTool(() => "denied", () => { });

        var approvalRequest = GetApprovalRequest(result.ResponseMessages);

        Assert.Equal("call-1", approvalRequest.ToolCallId);
        Assert.True(approvalRequest.IsAutomatic);
        Assert.Null(approvalRequest.Signature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject an approved response to a tool call that was denied automatically",
        Coverage = UpstreamCoverage.Covered,
        Commit = "63593a7f6a405b6ac8f1283aa6e95cff4c44a5ab"
    )]
    public async Task Should_reject_an_approved_response_to_a_tool_call_that_was_denied_automatically()
    {
        var executeCalls = 0;
        var status = "denied";

        var result = await CallTool(() => status, () => executeCalls++);

        // The client omits the history that caused the denial, so the policy
        // no longer denies the tool call when the approval is checked again.
        status = "not-applicable";

        var assistantMessage = result.ResponseMessages[0];

        var error = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() =>
            CallTool(
                () => status,
                () => executeCalls++,
                new ModelMessage[]
                {
                    new UserModelMessage("test"),
                    assistantMessage,
                    new ToolApprovalResponseModelMessage(
                        GetApprovalRequest(result.ResponseMessages).ApprovalId,
                        true
                    ),
                }
            )
        );

        Assert.Matches("missing signature", error.Message);
        Assert.Equal(0, executeCalls);
    }

    [Fact]
    [UpstreamTest(
        Id + "should continue a conversation that ends with an automatic denial",
        Coverage = UpstreamCoverage.Covered,
        Commit = "63593a7f6a405b6ac8f1283aa6e95cff4c44a5ab"
    )]
    public async Task Should_continue_a_conversation_that_ends_with_an_automatic_denial()
    {
        var executeCalls = 0;

        var result = await CallTool(() => "denied", () => executeCalls++);

        var continued = await CallTool(
            () => "denied",
            () => executeCalls++,
            new ModelMessage[]
            {
                new UserModelMessage("test"),
                result.ResponseMessages[0],
                result.ResponseMessages[1],
            }
        );

        Assert.Equal("done", continued.Text);
        Assert.Equal(0, executeCalls);
    }
}
