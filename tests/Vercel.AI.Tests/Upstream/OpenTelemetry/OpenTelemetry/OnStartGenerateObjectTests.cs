// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetryEvents;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStart (generateObject)</c>.</summary>
public sealed class OnStartGenerateObjectTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry::onStart (generateObject)::";

    [Fact]
    [UpstreamTest(
        Test + "creates a root span with output.type json",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public async Task Creates_a_root_span_with_output_type_json()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            MakeObjectOnStartEvent(
                schema: JsonDocument.Parse("""{"type":"object"}""").RootElement,
                schemaName: "TestSchema",
                schemaDescription: "A test schema"
            ),
            default
        );

        var attrs = tracer.Spans[0].InitAttributes;
        Assert.Equal("invoke_agent", attrs["gen_ai.operation.name"]);
        Assert.Equal("json", attrs["gen_ai.output.type"]);
    }
}
