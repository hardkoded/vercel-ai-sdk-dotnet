// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>stream</c>, the <c>allowSystemInMessages</c> tests.</summary>
public sealed class StreamSystemMessagesTests
{
    private static IReadOnlyList<ModelMessage> Messages()
    {
        return [new SystemModelMessage("SYSTEM INSTRUCTIONS")];
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should allow system messages when allowSystemInMessages is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldAllowSystemMessagesWhenAllowSystemInMessagesIsTrue()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, AllowSystemInMessages = true },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream(Messages());
        await result.Text;

        var prompt = Assert.Single(model.Calls).Prompt;
        ToolLoopAgentInstructions.AssertSystem(Assert.Single(prompt), "SYSTEM INSTRUCTIONS", null);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should allow prepareCall to return allowSystemInMessages",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldAllowPrepareCallToReturnAllowSystemInMessages()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                PrepareCall = call =>
                {
                    call.AllowSystemInMessages = true;
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream(Messages());
        await result.Text;

        var prompt = Assert.Single(model.Calls).Prompt;
        ToolLoopAgentInstructions.AssertSystem(Assert.Single(prompt), "SYSTEM INSTRUCTIONS", null);
    }
}
