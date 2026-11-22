// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream.ExecuteToolsFromStreamSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolsFromStream;

/// <summary>Port of <c>execute-tools-from-stream.test.ts</c> &gt; <c>executeToolsFromStream</c> &gt; <c>when a tool approval secret is configured</c>.</summary>
public sealed class WhenAToolApprovalSecretIsConfiguredTests
{
    private const string Id =
        Prefix + "executeToolsFromStream > when a tool approval secret is configured::";

    private const string ToolApprovalSecret = "test-hmac-secret-do-not-use-in-production";

    private static async Task<ToolApprovalRequestPart> GetApprovalRequest(string status)
    {
        var tools = new[]
        {
            ValueTool("tool1", (value, _) =>
                Task.FromResult(JsonSerializer.Serialize(value + "-result"))
            ),
        };

        var parts = await Run(Options(tools, options =>
            {
                options.ToolApproval = new ToolApprovalConfiguration
                {
                    { "tool1", new ToolApprovalStatus(status) },
                };
                options.ExperimentalToolApprovalSecret = ToolApprovalSecret;
            }), Call("call-1", "tool1"), FinishChunk());

        return parts.OfType<ToolApprovalRequestPart>().First();
    }

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
        var approvalRequest = await GetApprovalRequest(status);

        using var input = JsonDocument.Parse("{\"value\":\"test\"}");
        Assert.Equal(
            ToolApprovalSignatures.SignToolApproval(
                ToolApprovalSecret,
                "id-0",
                "call-1",
                "tool1",
                input.RootElement
            ),
            approvalRequest.Signature
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should not sign the approval request of auto-denied tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "63593a7f6a405b6ac8f1283aa6e95cff4c44a5ab"
    )]
    public async Task Should_not_sign_the_approval_request_of_auto_denied_tools()
    {
        var approvalRequest = await GetApprovalRequest("denied");

        Assert.Equal("id-0", approvalRequest.ApprovalId);
        Assert.True(approvalRequest.IsAutomatic);
        Assert.Null(approvalRequest.Signature);
    }
}
