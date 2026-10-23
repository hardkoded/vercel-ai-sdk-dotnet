// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c>, the timeout tests.</summary>
public sealed class GenerateTimeoutTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass timeout to generateText", Coverage = UpstreamCoverage.Covered, Note = "A timeout is merged into the abort signal of the model call, so the test checks that the call has one.")]
    public async Task ShouldPassTimeoutToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(new AgentOptions { Model = model }, ToolLoopAgentInstructions.Client());

        await agent.GenerateAsync("Hello, world!", timeout: TimeSpan.FromMilliseconds(5000));

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass settings timeout to generateText", Coverage = UpstreamCoverage.Covered, Note = "A timeout is merged into the abort signal of the model call, so the test checks that the call has one.")]
    public async Task ShouldPassSettingsTimeoutToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(new AgentOptions { Model = model, Timeout = TimeSpan.FromMilliseconds(5000) }, ToolLoopAgentInstructions.Client());

        await agent.GenerateAsync("Hello, world!");

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }
}
