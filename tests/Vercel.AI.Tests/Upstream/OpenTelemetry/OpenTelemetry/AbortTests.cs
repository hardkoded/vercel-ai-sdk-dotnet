// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.OpenTelemetry;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>abort</c>.</summary>
public sealed class AbortTests
{
    private const string Test = "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > abort::";

    [Fact]
    [UpstreamTest(
        Test + "closes streamText spans when AbortController aborts the stream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Closes_streamText_spans_when_AbortController_aborts_the_stream()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        using var cancellation = new CancellationTokenSource();
        using var collector = new MetricCollector("gpt-4-abort");

        async IAsyncEnumerable<LanguageModelStreamPart> Parts(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token
        )
        {
            yield return new StreamStartStreamPart([]);
            yield return new TextStartStreamPart("1");
            yield return new TextDeltaStreamPart("1", "Hello");
            await cancellation.CancelAsync();
            throw new OperationCanceledException("The user aborted a request.", token);
        }

        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = new OpenAiChatMockModel(doStream: Parts, modelId: "gpt-4-abort"),
                Prompt = "test-input",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            },
            cancellation.Token
        );

        try
        {
            await result.ConsumeStreamAsync();
        }
        catch (OperationCanceledException) { }

        Assert.Equal(
            [("invoke_agent gpt-4-abort", true), ("step 1", true), ("chat gpt-4-abort", true)],
            tracer.Spans.Select(span => (span.Name, span.Ended))
        );
        var point = Assert.Single(collector.DataPoints);
        Assert.Equal("AbortError", point.Attributes["error.type"]);
    }
}
