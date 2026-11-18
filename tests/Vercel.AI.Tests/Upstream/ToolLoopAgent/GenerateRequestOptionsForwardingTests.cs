// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c> &gt; <c>RequestOptions forwarding</c>, the tool approval test.</summary>
public sealed class GenerateRequestOptionsForwardingTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > RequestOptions forwarding::should honor toolApproval in generate",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldHonorToolApprovalInGenerate()
    {
        var executed = 0;
        var model = ToolLoopAgentToolCalls.GenerateModel(out var modelCallCount);

        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Tools =
                [
                    ToolLoopAgentToolCalls.TestTool(
                        (_, _) =>
                        {
                            executed++;
                            return "tool-result";
                        }
                    ),
                ],
                ToolApproval = new ToolApprovalConfiguration { { "testTool", "user-approval" } },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = await agent.GenerateAsync("test");

        Assert.Equal(1, modelCallCount());
        Assert.Equal(0, executed);
        ToolLoopAgentToolCalls.AssertApprovalRequestOnly(result.ResponseMessages);
    }
}
