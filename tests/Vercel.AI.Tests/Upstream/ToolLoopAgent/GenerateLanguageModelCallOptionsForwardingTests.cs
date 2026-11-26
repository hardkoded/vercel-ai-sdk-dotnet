// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c> &gt; <c>LanguageModelCallOptions forwarding</c>.</summary>
public sealed class GenerateLanguageModelCallOptionsForwardingTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward temperature to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardTemperatureToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, Temperature = 0.5 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(0.5, Assert.Single(model.Calls).Temperature);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward maxOutputTokens to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardMaxOutputTokensToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, MaxOutputTokens = 256 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(256, Assert.Single(model.Calls).MaxOutputTokens);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward topP to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardTopPToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, TopP = 0.9 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(0.9, Assert.Single(model.Calls).TopP);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward topK to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardTopKToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, TopK = 40 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(40, Assert.Single(model.Calls).TopK);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward presencePenalty to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardPresencePenaltyToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, PresencePenalty = 0.1 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(0.1, Assert.Single(model.Calls).PresencePenalty);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward frequencyPenalty to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardFrequencyPenaltyToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, FrequencyPenalty = 0.2 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(0.2, Assert.Single(model.Calls).FrequencyPenalty);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward stopSequences to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardStopSequencesToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, StopSequences = ["STOP", "END"] },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(new[] { "STOP", "END" }, Assert.Single(model.Calls).StopSequences);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate > LanguageModelCallOptions forwarding::should forward seed to generateText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardSeedToGenerateText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, Seed = 42 },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test");

        Assert.Equal(42, Assert.Single(model.Calls).Seed);
    }
}
