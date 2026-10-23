// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>stream</c>, the timeout tests.</summary>
public sealed class StreamTimeoutTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass timeout to streamText", Coverage = UpstreamCoverage.Covered, Note = "A timeout is merged into the abort signal of the model call, so the test checks that the call has one.")]
    public async Task ShouldPassTimeoutToStreamText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(new AgentOptions { Model = model }, ToolLoopAgentInstructions.Client());

        var result = agent.Stream("Hello, world!", timeout: TimeSpan.FromMilliseconds(5000));
        await result.Text;

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass settings timeout to streamText", Coverage = UpstreamCoverage.Covered, Note = "A timeout is merged into the abort signal of the model call, so the test checks that the call has one.")]
    public async Task ShouldPassSettingsTimeoutToStreamText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(new AgentOptions { Model = model, Timeout = TimeSpan.FromMilliseconds(5000) }, ToolLoopAgentInstructions.Client());

        var result = agent.Stream("Hello, world!");
        await result.Text;

        Assert.NotNull(Assert.Single(model.Calls).AbortSignal);
    }
}
