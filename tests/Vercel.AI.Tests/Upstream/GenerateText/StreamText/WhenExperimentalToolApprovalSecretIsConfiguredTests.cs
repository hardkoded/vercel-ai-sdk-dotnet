// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.GenerateText.ToolExecutionApprovalSupport;
using static Vercel.AI.Tests.Upstream.GenerateText.StreamText.ToolExecutionApprovalStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText &gt; tool execution approval &gt; when experimental_toolApprovalSecret is configured</c>.</summary>
public sealed class WhenExperimentalToolApprovalSecretIsConfiguredTests
{
    private const string Id =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > tool execution approval > when experimental_toolApprovalSecret is configured::";

    private const string TestSecret = "test-hmac-secret-do-not-use-in-production";

    private static StreamTextResult CallTool(
        Func<string> getStatus,
        Action onExecute,
        IReadOnlyList<ModelMessage>? messages = null
    )
    {
        var model = new ScriptedModel(step =>
            step == 0 ? ToolCallsStep(("call-1", "{ \"value\": \"test\" }")) : TextStep("done")
        );
        var tool = ValueTool(
            (_, _) =>
            {
                onExecute();
                return Task.FromResult("result1");
            }
        );

        return Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Tools = new[] { tool },
                    ToolApproval = new ToolApprovalConfiguration
                    {
                        {
                            "tool1",
                            (_, _) =>
                                Task.FromResult<ToolApprovalStatus?>(
                                    new ToolApprovalStatus(getStatus())
                                )
                        },
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
        var result = CallTool(() => status, () => { });

        var approvalRequest = GetApprovalRequest(await result.ResponseMessages);

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
        var result = CallTool(() => "denied", () => { });

        var approvalRequest = GetApprovalRequest(await result.ResponseMessages);

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

        var responseMessages = await CallTool(() => status, () => executeCalls++).ResponseMessages;

        // The client omits the history that caused the denial, so the policy
        // no longer denies the tool call when the approval is checked again.
        status = "not-applicable";

        // The port raises a stream error from the stream instead of passing it to onError.
        var thrown = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() =>
            CallTool(
                    () => status,
                    () => executeCalls++,
                    new ModelMessage[]
                    {
                        new UserModelMessage("test"),
                        responseMessages[0],
                        new ToolApprovalResponseModelMessage(
                            GetApprovalRequest(responseMessages).ApprovalId,
                            true
                        ),
                    }
                )
                .ConsumeStreamAsync()
        );

        Assert.Matches("missing signature", thrown.Message);
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

        var responseMessages = await CallTool(
            () => "denied",
            () => executeCalls++
        ).ResponseMessages;

        var continued = CallTool(
            () => "denied",
            () => executeCalls++,
            new ModelMessage[]
            {
                new UserModelMessage("test"),
                responseMessages[0],
                responseMessages[1],
            }
        );

        Assert.Equal("done", await continued.Text);
        Assert.Equal(0, executeCalls);
    }
}
