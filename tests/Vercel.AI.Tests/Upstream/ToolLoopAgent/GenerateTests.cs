// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>generate</c>, the sandbox and user-agent tests.</summary>
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

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should tag the user-agent with the agent identifier",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldTagTheUserAgentWithTheAgentIdentifier()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!");

        Assert.Contains(
            "ai-sdk-agent/tool-loop",
            Assert.Single(model.Calls).Headers!["user-agent"]
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > generate::should preserve a caller-provided user-agent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPreserveACallerProvidedUserAgent()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Headers = new Dictionary<string, string?> { ["user-agent"] = "my-app/1.0" },
            },
            ToolLoopAgentInstructions.Client()
        );

        await agent.GenerateAsync("Hello, world!");

        var ua = Assert.Single(model.Calls).Headers!["user-agent"];
        Assert.Contains("my-app/1.0", ua);
        Assert.Contains("ai-sdk-agent/tool-loop", ua);
    }
}
