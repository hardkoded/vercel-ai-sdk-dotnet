// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

public sealed class StreamTests
{
    private static async Task<IReadOnlyList<ModelMessage>> PromptAsync(Vercel.AI.Prompt.Instructions instructions)
    {
        var model = new TestLanguageModel();
        var agent = new Agent(new AgentOptions { Model = model, Instructions = instructions }, ToolLoopAgentInstructions.Client());

        var result = agent.Stream("Hello, world!");
        await result.Text;

        return Assert.Single(model.Calls).Prompt;
    }

    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass string instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassStringInstructions()
    {
        var prompt = await PromptAsync("INSTRUCTIONS");

        Assert.Equal(2, prompt.Count);
        ToolLoopAgentInstructions.AssertSystem(prompt[0], "INSTRUCTIONS", null);
        ToolLoopAgentInstructions.AssertUser(prompt[1]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass system message instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassSystemMessageInstructions()
    {
        var prompt = await PromptAsync(new SystemModelMessage("INSTRUCTIONS", ToolLoopAgentInstructions.Options("test")));

        Assert.Equal(2, prompt.Count);
        ToolLoopAgentInstructions.AssertSystem(prompt[0], "INSTRUCTIONS", "test");
        ToolLoopAgentInstructions.AssertUser(prompt[1]);
    }
}
