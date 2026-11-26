// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>onStart (embed)</c>.</summary>
public sealed class OnStartEmbedTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStart (embed)::creates an embeddings operation span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Creates_an_embeddings_operation_span()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        await integration.OnStart!(
            new EmbedStartEvent(
                "call-1",
                "ai.embed",
                new Dictionary<string, object?>(),
                "openai",
                "gpt-4",
                EmbedEventValue<string, IReadOnlyList<string>>.FromOne("test text"),
                2,
                new Dictionary<string, string>(),
                null
            ),
            default
        );

        var span = Assert.Single(tracer.Spans);
        Assert.False(span.Ended);
        Assert.Equal("embeddings gpt-4", span.Name);
        Assert.Equal(
            new Dictionary<string, object?>
            {
                ["gen_ai.operation.name"] = "embeddings",
                ["gen_ai.provider.name"] = "openai",
                ["gen_ai.request.model"] = "gpt-4",
            },
            span.InitAttributes
        );
        Assert.Empty(span.RuntimeAttributes);
    }
}
