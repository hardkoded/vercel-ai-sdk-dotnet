// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Testing;
using Vercel.AI.Tests.Upstream.GenerateText;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

/// <summary>Port of <c>tool-loop-agent.test.ts</c> &gt; <c>ToolLoopAgent</c> &gt; <c>stream</c> &gt; <c>toolOrder</c>.</summary>
public sealed class StreamToolOrderTests
{
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > stream > should forward toolOrder to streamText",
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
}
