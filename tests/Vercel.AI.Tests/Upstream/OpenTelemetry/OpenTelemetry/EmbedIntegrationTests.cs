// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>embed integration</c>.</summary>
public sealed class EmbedIntegrationTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > embed integration::";

    private static TelemetryOptions Integration(MockTracer tracer) =>
        new() { Integrations = [new OtelIntegration(new OtelOptions { Tracer = tracer.Source })] };

    private static double? InputTokens(MockSpan span)
    {
        if (!span.Attributes.TryGetValue("gen_ai.usage.input_tokens", out var tokens))
        {
            return null;
        }

        // A number only: a string or a rounded value must fail the comparison.
        Assert.True(tokens is int or long or double or float, "input tokens must be numeric");
        return Convert.ToDouble(tokens);
    }

    [Fact]
    [UpstreamTest(
        Test + "reports usage only on the provider request span",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_usage_only_on_the_provider_request_span()
    {
        using var tracer = new MockTracer();

        await Embed.EmbedAsync(
            new EmbedRequest
            {
                Model = new EmbedModel(_ => new EmbeddingModelResponse(
                    [
                        [0.1, 0.2, 0.3],
                    ],
                    tokens: 14
                )),
                Value = "sunny day at the beach",
                Telemetry = Integration(tracer),
            }
        );

        var spans = tracer.FinishedSpans;
        Assert.Equal(2, spans.Count);
        Assert.All(spans, span => Assert.Equal("embeddings text-embedding-model", span.Name));
        Assert.All(spans, span =>
            Assert.Equal("embeddings", span.Attributes["gen_ai.operation.name"])
        );
        Assert.Equal(14.0, InputTokens(spans[0]));
        Assert.Null(InputTokens(spans[1]));
    }

    [Fact]
    [UpstreamTest(
        Test + "reports batched usage only on provider request spans",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Reports_batched_usage_only_on_provider_request_spans()
    {
        using var tracer = new MockTracer();

        await EmbedMany.EmbedManyAsync(
            new EmbedManyRequest
            {
                Model = new EmbedModel(call => new EmbeddingModelResponse(
                    call.Values.Select(_ => new[] { 0.1, 0.2, 0.3 }).ToArray(),
                    tokens: call.Values.Count * 14
                ))
                {
                    MaxEmbeddingsPerCall = 2,
                },
                Values = ["sunny", "day", "beach"],
                Telemetry = Integration(tracer),
            }
        );

        var spans = tracer.FinishedSpans;
        Assert.Equal(3, spans.Count);
        Assert.All(spans, span => Assert.Equal("embeddings text-embedding-model", span.Name));
        Assert.All(spans, span =>
            Assert.Equal("embeddings", span.Attributes["gen_ai.operation.name"])
        );

        // The two provider request spans can end in either order. The root span ends last and has no usage.
        Assert.Equal(
            [14.0, 28.0],
            spans.Take(2).Select(span => InputTokens(span)!.Value).OrderBy(n => n)
        );
        Assert.Null(InputTokens(spans[2]));
    }

    [Fact]
    [UpstreamTest(
        Test + "omits usage attributes when the provider does not return usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Omits_usage_attributes_when_the_provider_does_not_return_usage()
    {
        using var tracer = new MockTracer();

        var result = await Embed.EmbedAsync(
            new EmbedRequest
            {
                Model = new EmbedModel(_ => new EmbeddingModelResponse([
                    [0.1, 0.2, 0.3],
                ])),
                Value = "sunny day at the beach",
                Telemetry = Integration(tracer),
            }
        );

        Assert.True(double.IsNaN(result.Usage.Tokens!.Value));
        Assert.Equal(2, tracer.FinishedSpans.Count);
        foreach (var span in tracer.FinishedSpans)
        {
            Assert.False(span.Attributes.ContainsKey("gen_ai.usage.input_tokens"));
        }
    }

    private sealed class EmbedModel(Func<EmbeddingModelCall, EmbeddingModelResponse> respond)
        : IEmbeddingCaller
    {
        public string Provider => "openai";

        public string ModelId => "text-embedding-model";

        public string SpecificationVersion => "v4";

        public int? MaxEmbeddingsPerCall { get; init; }

        public double? MaxInputBytesPerCall => null;

        public bool SupportsParallelCalls => true;

        public Task<JsonElement?> TransformProviderOptionsAsync(
            JsonElement? providerOptions,
            IReadOnlyList<string> values,
            int startIndex,
            int endIndex,
            CancellationToken cancellationToken
        ) => Task.FromResult(providerOptions);

        public Task<EmbeddingModelResponse> DoEmbedAsync(
            EmbeddingModelCall call,
            CancellationToken cancellationToken
        ) => Task.FromResult(respond(call));
    }
}
