// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.CollectToolApprovals;

/// <summary>
/// Port of <c>collect-tool-approvals.test.ts</c> &gt; <c>collectToolApprovals</c> &gt; <c>repeated approval responses</c>.
/// <see cref="ApprovalResponsePart"/> has no <c>providerExecuted</c> field, so the metadata test varies only the reason.
/// </summary>
public sealed class RepeatedApprovalResponsesTests
{
    private const string Prefix = "packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals > repeated approval responses::";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Prefix + "should collect an identical response once when approved is %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_collect_an_identical_response_once_when_approved_is(bool approved)
    {
        var response = new ApprovalResponsePart("approval-1", approved);

        Assert.Equal(new[] { response }, Collect(new ApprovalHistoryPart[] { response, new ApprovalResponsePart("approval-1", approved) }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should ignore a replayed decision with different metadata: %j", Coverage = UpstreamCoverage.Partial)]
    public void Should_ignore_a_replayed_decision_with_different_metadata()
    {
        var response1 = new ApprovalResponsePart("approval-1", true);

        Assert.Equal(new[] { response1 }, Collect(new ApprovalHistoryPart[] { response1, new ApprovalResponsePart("approval-1", true, "replayed") }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should keep the first of conflicting decisions", Coverage = UpstreamCoverage.Covered)]
    public void Should_keep_the_first_of_conflicting_decisions()
    {
        var response1 = new ApprovalResponsePart("approval-1", true);
        var denial = new ApprovalResponsePart("approval-1", false);

        Assert.Equal(new[] { response1 }, Collect(new ApprovalHistoryPart[] { response1, denial }));
        Assert.Equal(new[] { denial }, Collect(new ApprovalHistoryPart[] { denial, response1 }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should collect one response per tool call across approval IDs", Coverage = UpstreamCoverage.Covered)]
    public void Should_collect_one_response_per_tool_call_across_approval_IDs()
    {
        var response1 = new ApprovalResponsePart("approval-1", true);
        var response2 = new ApprovalResponsePart("approval-2", true);

        Assert.Equal(new[] { response1 }, Collect(new ApprovalHistoryPart[] { response1, response2 }));
    }

    [Fact]
    [UpstreamTest(Prefix + "should collect a denial with an existing execution-denied result once", Coverage = UpstreamCoverage.Covered)]
    public void Should_collect_a_denial_with_an_existing_execution_denied_result_once()
    {
        var denial = new ApprovalResponsePart("approval-1", false);

        Assert.Equal(
            new[] { denial },
            Collect(
                new ApprovalHistoryPart[] { denial, new ApprovalResponsePart("approval-1", false) },
                new ApprovalHistoryPart[] { new ApprovalToolResultPart("call-1", "tool1", "execution-denied") }));
    }

    private static ApprovalResponsePart[] Collect(ApprovalHistoryPart[] responses, ApprovalHistoryPart[]? results = null)
    {
        using var input = JsonDocument.Parse("{}");
        var messages = new[]
        {
            new ApprovalHistoryMessage("assistant", parts: new ApprovalHistoryPart[]
            {
                new ApprovalToolCallPart("call-1", "tool1", input.RootElement.Clone()),
                new ApprovalRequestPart("approval-1", "call-1"),
                new ApprovalRequestPart("approval-2", "call-1"),
            }),
            new ApprovalHistoryMessage("tool", parts: responses.Concat(results ?? Array.Empty<ApprovalHistoryPart>()).ToArray()),
        };
        var result = ToolApprovalCollection.CollectToolApprovals(messages);
        return result.ApprovedToolApprovals.Concat(result.DeniedToolApprovals).Select(approval => approval.ApprovalResponse).ToArray();
    }
}
