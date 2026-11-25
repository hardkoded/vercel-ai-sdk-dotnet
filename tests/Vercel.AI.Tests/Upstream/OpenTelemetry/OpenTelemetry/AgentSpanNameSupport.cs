// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using OtelAgentSpanName = Vercel.AI.OpenTelemetry.AgentSpanName;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>The <c>it.each</c> table and the body shared by the <c>agentSpanName</c> describes of <c>open-telemetry.test.ts</c>.</summary>
internal static class AgentSpanNameSupport
{
    public const string Commit = "e6a568d67b361bfe9171c1605f37ef579c718b58";

    public static TheoryData<string, string?, string> Cases =>
        new()
        {
            { "default", "my-agent", "invoke_agent gpt-4" },
            { "modelId", "my-agent", "invoke_agent gpt-4" },
            { "functionId", "my-agent", "invoke_agent my-agent" },
            { "functionId", null, "invoke_agent gpt-4" },
        };

    public static async Task AssertName(
        string agentSpanName,
        string? functionId,
        string expectedName,
        IOperationStartEvent start
    )
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                AgentSpanName = agentSpanName switch
                {
                    "modelId" => OtelAgentSpanName.ModelId,
                    "functionId" => OtelAgentSpanName.FunctionId,
                    _ => null,
                },
            }
        );

        await integration.OnStart!(start, default);

        var span = Assert.Single(tracer.Spans);
        Assert.Equal(expectedName, span.Name);
        Assert.Equal("invoke_agent", span.InitAttributes["gen_ai.operation.name"]);
        Assert.Equal("gpt-4", span.InitAttributes["gen_ai.request.model"]);
        span.InitAttributes.TryGetValue("gen_ai.agent.name", out var agentName);
        Assert.Equal(functionId, agentName);
    }
}
