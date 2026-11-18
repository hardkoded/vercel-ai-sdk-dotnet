// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c>, the sandbox tests.</summary>
public sealed class GenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass sandbox to prepareCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSandboxToPrepareCall()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? recordedSandbox = null;

        var agent = new Agent(
            new AgentOptions
            {
                Model = new TestLanguageModel(),
                PrepareCall = options =>
                {
                    recordedSandbox = options.ExperimentalSandbox;
                    return options;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!", experimentalSandbox: sandbox);

        Assert.Same(sandbox, recordedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSandboxToToolExecution()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? recordedSandbox = null;

        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.GenerateModel(out _),
                Tools =
                [
                    ToolLoopAgentToolCalls.TestTool(
                        (value, options) =>
                        {
                            recordedSandbox = options.ExperimentalSandbox;
                            return value;
                        }
                    ),
                ],
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("test", experimentalSandbox: sandbox);

        Assert.Same(sandbox, recordedSandbox);
    }
}
