// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

public sealed class StreamTests
{
    private static async Task<IReadOnlyList<ModelMessage>> PromptAsync(
        Vercel.AI.Prompt.Instructions instructions
    )
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model, Instructions = instructions },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!");
        await result.Text;

        return Assert.Single(model.Calls).Prompt;
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass string instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassStringInstructions()
    {
        var prompt = await PromptAsync("INSTRUCTIONS");

        Assert.Equal(2, prompt.Count);
        ToolLoopAgentInstructions.AssertSystem(prompt[0], "INSTRUCTIONS", null);
        ToolLoopAgentInstructions.AssertUser(prompt[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass system message instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSystemMessageInstructions()
    {
        var prompt = await PromptAsync(
            new SystemModelMessage("INSTRUCTIONS", ToolLoopAgentInstructions.Options("test"))
        );

        Assert.Equal(2, prompt.Count);
        ToolLoopAgentInstructions.AssertSystem(prompt[0], "INSTRUCTIONS", "test");
        ToolLoopAgentInstructions.AssertUser(prompt[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass sandbox to prepareCall",
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

        var result = agent.Stream("Hello, world!", experimentalSandbox: sandbox);
        await result.Text;

        Assert.Same(sandbox, recordedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass sandbox to tool execution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassSandboxToToolExecution()
    {
        var sandbox = new TestSandboxSession();
        ISandboxSession? recordedSandbox = null;

        var agent = new Agent(
            new AgentOptions
            {
                Model = ToolLoopAgentToolCalls.StreamModel(out _),
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

        var result = agent.Stream("test", experimentalSandbox: sandbox);
        await result.Text;

        Assert.Same(sandbox, recordedSandbox);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should honor toolApproval in stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldHonorToolApprovalInStream()
    {
        var executed = 0;
        var model = ToolLoopAgentToolCalls.StreamModel(out var modelCallCount);

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

        var result = agent.Stream("test");
        await result.Text;

        Assert.Equal(1, modelCallCount());
        Assert.Equal(0, executed);
        ToolLoopAgentToolCalls.AssertApprovalRequestOnly(await result.ResponseMessages);
    }

    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should forward toolOrder to streamText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardToolOrderToStreamText()
    {
        var model = new TestLanguageModel();

        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Tools =
                [
                    Tool.Function("zebra", null, Schema, (_, _) => Task.FromResult("\"zebra\"")),
                    Tool.Function("alpha", null, Schema, (_, _) => Task.FromResult("\"alpha\"")),
                    Tool.Function("middle", null, Schema, (_, _) => Task.FromResult("\"middle\"")),
                ],
                ToolOrder = ["middle"],
            },
            ToolLoopAgentInstructions.Client()
        );

        await ToolExecutionCallbackSupport.ConsumeAsync(agent.Stream("Hello, world!"));

        Assert.Equal(
            new[] { "middle", "alpha", "zebra" },
            model.Calls[0].Tools!.Select(tool => tool.Name)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should use prepareCall",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUsePrepareCall()
    {
        var model = new TestLanguageModel();
        var agent = new Agent<PrepareCallOptions>(
            new AgentOptions<PrepareCallOptions>
            {
                Model = model,
                PrepareCall = (call, options) =>
                {
                    call.ProviderOptions = ToolLoopAgentInstructions.Options(options.Value);
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!", new PrepareCallOptions { Value = "test" });
        await ToolExecutionCallbackSupport.ConsumeAsync(result);

        var options = Assert.IsAssignableFrom<IReadOnlyDictionary<string, JsonElement>>(
            Assert.Single(model.Calls).ProviderOptions
        );
        Assert.Equal("test", options["test"].GetProperty("value").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should pass abortSignal to streamText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassAbortSignalToStreamText()
    {
        var abortController = new AbortController();
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions { Model = model },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!", abortSignal: abortController.Signal);
        await ToolExecutionCallbackSupport.ConsumeAsync(result);

        Assert.Same(abortController.Signal, Assert.Single(model.Calls).AbortSignal);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream::should forward include to streamText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldForwardIncludeToStreamText()
    {
        var model = new TestLanguageModel();
        var agent = new Agent(
            new AgentOptions
            {
                Model = model,
                Include = new AgentInclude { RawChunks = true },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = agent.Stream("Hello, world!");
        await ToolExecutionCallbackSupport.ConsumeAsync(result);

        Assert.True(Assert.Single(model.Calls).IncludeRawChunks);
    }

    private sealed class PrepareCallOptions
    {
        public string Value { get; set; } = "";
    }
}
