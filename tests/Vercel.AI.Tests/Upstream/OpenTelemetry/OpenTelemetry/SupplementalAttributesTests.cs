// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>supplemental attributes</c>.</summary>
public sealed class SupplementalAttributesTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::supplemental attributes::";

    [Fact]
    [UpstreamTest(
        Test + "emits supplemental AI SDK attributes on existing spans when enabled",
        Coverage = UpstreamCoverage.Partial,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e",
        Note = "Only the toolChoice option is checked: ai.prompt.toolChoice on the step span, and no attribute without the option. The embedding, reranking, and schema options and the other attributes of the snapshot are not ported."
    )]
    public async Task Emits_the_toolChoice_attribute_on_the_step_span_when_enabled()
    {
        foreach (var enabled in new[] { true, false })
        {
            using var tracer = new MockTracer();
            var integration = new OtelIntegration(
                new OtelOptions { Tracer = tracer.Source, ToolChoice = enabled }
            );

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
                    ToolChoice = ToolChoice.Auto,
                },
                default
            );

            var step = tracer.Spans[1];
            Assert.Equal("step 1", step.Name);
            step.InitAttributes.TryGetValue("ai.prompt.toolChoice", out var toolChoice);
            Assert.Equal(enabled ? "{\"type\":\"auto\"}" : null, toolChoice);
        }
    }
}
