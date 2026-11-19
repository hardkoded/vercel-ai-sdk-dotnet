// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.ToolLoopAgent;

public sealed class CallOptionsSchemaTests
{
    public sealed class CallOptions
    {
        public string Topic { get; set; } = "";
    }

    private static JsonNode Schema() =>
        JsonNode.Parse(
            """{"type":"object","properties":{"topic":{"type":"string","enum":["legal","medical"]}},"required":["topic"],"additionalProperties":false}"""
        )!;

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > callOptionsSchema::should reject options that fail callOptionsSchema validation before invoking the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldRejectOptionsThatFailCallOptionsSchemaValidationBeforeInvokingTheModel()
    {
        var modelCalled = false;
        var model = new TestLanguageModel
        {
            OnGenerate = _ =>
            {
                modelCalled = true;
                return TestLanguageModel.Text("reply");
            },
        };
        var agent = new Agent<CallOptions>(
            new AgentOptions<CallOptions> { Model = model, CallOptionsSchema = Schema() },
            ToolLoopAgentInstructions.Client()
        );

        var error = await Assert.ThrowsAsync<TypeValidationException>(() =>
            agent.GenerateAsync("hi", new CallOptions { Topic = "evil" })
        );

        Assert.Matches("Type validation failed for options", error.Message);
        Assert.False(modelCalled);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/agent/tool-loop-agent.test.ts::ToolLoopAgent > callOptionsSchema::should pass through options that satisfy callOptionsSchema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassThroughOptionsThatSatisfyCallOptionsSchema()
    {
        var model = new TestLanguageModel { OnGenerate = _ => TestLanguageModel.Text("reply") };
        string? seen = null;
        var agent = new Agent<CallOptions>(
            new AgentOptions<CallOptions>
            {
                Model = model,
                CallOptionsSchema = Schema(),
                PrepareCall = (call, options) =>
                {
                    seen = options.Topic;
                    return call;
                },
            },
            ToolLoopAgentInstructions.Client()
        );

        var result = await agent.GenerateAsync("hi", new CallOptions { Topic = "legal" });

        Assert.Equal("reply", result.Text);
        Assert.Equal("legal", seen);
    }
}
