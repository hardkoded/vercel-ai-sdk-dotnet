// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using OtelAgentSpanName = Vercel.AI.OpenTelemetry.AgentSpanName;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>agentSpanName</c>.</summary>
public sealed class OpenTelemetryTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::agentSpanName::ai.generateText::";

    public static TheoryData<string, string?, string> AgentSpanNameCases =>
        new()
        {
            { "default", "my-agent", "invoke_agent gpt-4" },
            { "modelId", "my-agent", "invoke_agent gpt-4" },
            { "functionId", "my-agent", "invoke_agent my-agent" },
            { "functionId", null, "invoke_agent gpt-4" },
        };

    [Theory]
    [MemberData(nameof(AgentSpanNameCases))]
    [UpstreamTest(
        Test + "uses $expectedName with agentSpanName=$agentSpanName and functionId=$functionId",
        Coverage = UpstreamCoverage.Partial,
        Commit = "e6a568d67b361bfe9171c1605f37ef579c718b58",
        Note = "Only ai.generateText runs. ITelemetry has no OnEnd for generateObject, so the integration does not create object operation spans."
    )]
    public async Task Uses_the_expected_name(
        string agentSpanName,
        string? functionId,
        string expectedName
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

        await integration.OnStart!(
            new GenerateTextStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
            }.WithTelemetry(new TelemetryOptions { FunctionId = functionId }),
            default
        );

        var span = Assert.Single(tracer.Spans);
        Assert.Equal(expectedName, span.Name);
        Assert.Equal("invoke_agent", span.InitAttributes["gen_ai.operation.name"]);
        Assert.Equal("gpt-4", span.InitAttributes["gen_ai.request.model"]);
        span.InitAttributes.TryGetValue("gen_ai.agent.name", out var agentName);
        Assert.Equal(functionId, agentName);
    }
}
