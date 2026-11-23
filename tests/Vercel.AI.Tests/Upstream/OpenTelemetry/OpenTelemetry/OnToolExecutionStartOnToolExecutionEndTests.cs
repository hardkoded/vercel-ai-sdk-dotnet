// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onToolExecutionStart / onToolExecutionEnd</c>.</summary>
public sealed class OnToolExecutionStartOnToolExecutionEndTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onToolExecutionStart / onToolExecutionEnd::";

    [Fact]
    [UpstreamTest(
        Test + "uses extension type for provider-executed tools",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Uses_extension_type_for_provider_executed_tools()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            new GenerateTextStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
            }.WithTelemetry(new TelemetryOptions()),
            default
        );
        await integration.OnStepStart!(
            new GenerateTextStepStartEvent
            {
                CallId = "call-1",
                Provider = "openai",
                ModelId = "gpt-4",
                StepNumber = 0,
            },
            default
        );
        await integration.OnToolExecutionStart!(
            new ToolExecutionStartEvent(
                new GeneratedToolCall(
                    "tool-call-1",
                    "mcp.ask_question",
                    "{\"question\":\"What is this repository?\"}"
                )
                {
                    ProviderExecuted = true,
                    Dynamic = true,
                },
                []
            )
            {
                CallId = "call-1",
            },
            default
        );

        Assert.Equal("extension", tracer.Spans[2].InitAttributes["gen_ai.tool.type"]);
    }
}
