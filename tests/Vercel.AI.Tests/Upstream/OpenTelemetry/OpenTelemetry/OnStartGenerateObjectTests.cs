// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStart (generateObject)</c>.</summary>
public sealed class OnStartGenerateObjectTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStart (generateObject)::creates a root span with output.type json",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_a_root_span_with_output_type_json()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            new GenerateObjectStartEvent(CallId, "openai.chat", "gpt-4", null, null)
            {
                OperationId = "ai.generateObject",
                Schema = JsonDocument.Parse("{\"type\":\"object\"}").RootElement,
                SchemaName = "TestSchema",
                SchemaDescription = "A test schema",
                Output = "object",
            },
            default
        );

        var attributes = Assert.Single(tracer.Spans).InitAttributes;
        Assert.Equal("invoke_agent", attributes["gen_ai.operation.name"]);
        Assert.Equal("json", attributes["gen_ai.output.type"]);
    }
}
