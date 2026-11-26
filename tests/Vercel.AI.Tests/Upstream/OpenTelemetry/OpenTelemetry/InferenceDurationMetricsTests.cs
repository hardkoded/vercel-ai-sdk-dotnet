// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics.Metrics;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>inference duration metrics</c>.</summary>
public sealed class InferenceDurationMetricsTests
{
    private const string UpstreamCommit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e";
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > inference duration metrics::";

    private static readonly LanguageModelUsage Usage = new(2, 1, 3);

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    [Fact]
    [UpstreamTest(
        Test + "exports seconds and model attributes with a meter registered after initialization",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The model call is driven through generateText instead of integration callbacks, because the port builds the step result events itself. A listener registered after the integration exists stands in for the meter provider registered after initialization."
    )]
    public async Task Exports_seconds_and_model_attributes_with_a_meter_registered_after_initialization()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var client = Client();
        var modelId = "metrics-model-" + Guid.NewGuid().ToString("N");

        StepErrorModel Model() =>
            new(
                doGenerate: (_, _) =>
                    Task.FromResult(
                        new LanguageModelGenerateResult(
                            [new GeneratedText("hello")],
                            FinishReason.Stop,
                            Usage,
                            responseModelId: "response-model"
                        )
                    )
            );

        using var collector = new MetricCollector(modelId);
        await client.GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new ModelIdOverride(Model(), modelId),
                Prompt = "hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var point = Assert.Single(collector.DataPoints);
        Assert.Equal("s", collector.Instrument!.Unit);
        Assert.Equal("chat", point.Attributes["gen_ai.operation.name"]);
        Assert.Equal(modelId, point.Attributes["gen_ai.request.model"]);
        Assert.Equal("response-model", point.Attributes["gen_ai.response.model"]);
        Assert.False(point.Attributes.ContainsKey("error.type"));
        Assert.True(point.Value >= 0);
#if NET9_0_OR_GREATER
        Assert.Equal(
            [
                0.01,
                0.02,
                0.04,
                0.08,
                0.16,
                0.32,
                0.64,
                1.28,
                2.56,
                5.12,
                10.24,
                20.48,
                40.96,
                81.92,
            ],
            ((Instrument<double>)collector.Instrument).Advice!.HistogramBucketBoundaries
        );
#endif
    }

    [Fact]
    [UpstreamTest(
        Test + "records the full streamed response duration once",
        Coverage = UpstreamCoverage.Partial,
        Commit = UpstreamCommit,
        Note = "The port's generation clock cannot be replaced, so the test checks that one measurement is recorded after the stream finishes, not the mocked 1.6 seconds."
    )]
    public async Task Records_the_full_streamed_response_duration_once()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var client = Client();
        var modelId = "metrics-stream-model-" + Guid.NewGuid().ToString("N");
        var finish = new TaskCompletionSource();
        using var collector = new MetricCollector(modelId);

        async IAsyncEnumerable<LanguageModelStreamPart> Parts()
        {
            yield return new TextStartStreamPart("0");
            yield return new TextDeltaStreamPart("0", "hello");
            await finish.Task;
            yield return new TextEndStreamPart("0");
            yield return new FinishStreamPart(FinishReason.Stop, Usage);
        }

        var result = client.StreamTextAsync(
            new StreamTextOptions
            {
                Model = new ModelIdOverride(
                    new StepErrorModel(doStream: (_, _) => StepErrorModel.StreamOf(Parts())),
                    modelId
                ),
                Prompt = "hello",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        var text = new List<string>();
        await foreach (var delta in result.TextStream())
        {
            text.Add(delta);
            if (text.Count == 1)
            {
                Assert.Empty(collector.DataPoints);
                finish.SetResult();
            }
        }

        var point = Assert.Single(collector.DataPoints);
        Assert.True(point.Value >= 0);
    }

    private sealed class ModelIdOverride(ILanguageModel inner, string modelId) : ILanguageModel
    {
        public IReadOnlyDictionary<string, string> SupportedUrls => inner.SupportedUrls;

        public string SpecificationVersion => inner.SpecificationVersion;

        public string Provider => inner.Provider;

        public string ModelId => modelId;

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => inner.DoGenerateAsync(options, cancellationToken);

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => inner.DoStreamAsync(options, cancellationToken);
    }
}
