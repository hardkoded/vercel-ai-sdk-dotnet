// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.OpenTelemetry;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>stream errors</c>.</summary>
public sealed class StreamErrorsTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > stream errors::";

    [Fact]
    [UpstreamTest(
        Test + "records and exports streamText spans when the provider stream errors",
        Coverage = UpstreamCoverage.Partial,
        Note = "The metric error.type is the type name of the thrown exception (InvalidOperationException), where upstream records the JavaScript Error name (Error)."
    )]
    public async Task Records_and_exports_streamText_spans_when_the_provider_stream_errors()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        using var collector = new MetricCollector("gpt-4-stream-errors");

        async IAsyncEnumerable<LanguageModelStreamPart> Parts(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token
        )
        {
            yield return new StreamStartStreamPart([]);
            yield return new TextStartStreamPart("1");
            yield return new TextDeltaStreamPart("1", "Hello");
            await Task.Yield();
            throw new InvalidOperationException("socket closed");
        }

        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = new OpenAiChatMockModel(doStream: Parts, modelId: "gpt-4-stream-errors"),
                Prompt = "test-input",
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );

        try
        {
            await result.ConsumeStreamAsync();
        }
        catch (InvalidOperationException) { }

        var rootSpan = tracer.FinishedSpans.Single(span =>
            span.Name == "invoke_agent gpt-4-stream-errors"
        );
        var stepSpan = tracer.FinishedSpans.Single(span => span.Name == "step 1");
        var chatSpan = tracer.FinishedSpans.Single(span => span.Name == "chat gpt-4-stream-errors");

        foreach (var span in new[] { rootSpan, stepSpan, chatSpan })
        {
            Assert.Equal(ActivityStatusCode.Error, span.Activity.Status);
            Assert.True(span.ExceptionCount >= 1);
        }

        Assert.Equal(rootSpan.Activity.SpanId, stepSpan.Activity.ParentSpanId);
        Assert.Equal(stepSpan.Activity.SpanId, chatSpan.Activity.ParentSpanId);
        var point = Assert.Single(collector.DataPoints);
        Assert.Equal("InvalidOperationException", point.Attributes["error.type"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [UpstreamTest(
        Test + "records failed stream attempts when streamRetries is %i",
        Coverage = UpstreamCoverage.Partial,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f",
        Note = "The port's generation clock cannot be replaced, so the test checks the number of measurements and their error.type, not the mocked 0.5 and 0.75 second sums."
    )]
    public async Task Records_failed_stream_attempts_when_streamRetries_is(int streamRetries)
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });
        var modelId = "gpt-4-stream-retries-" + streamRetries;
        using var collector = new MetricCollector(modelId);
        var streamCalls = 0;

        async IAsyncEnumerable<LanguageModelStreamPart> Parts()
        {
            await Task.Yield();
            if (Interlocked.Increment(ref streamCalls) == 1)
            {
                yield return new ErrorStreamPart("provider failed");
            }
            else
            {
                yield return new TextStartStreamPart("0");
                yield return new TextDeltaStreamPart("0", "recovered");
                yield return new TextEndStreamPart("0");
                yield return new FinishStreamPart(
                    FinishReason.Stop,
                    new LanguageModelUsage(2, 1, 3, 0, 0, 0)
                );
            }
        }

        var result = new AiClient(
            GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
        ).StreamTextAsync(
            new StreamTextOptions
            {
                Model = new OpenAiChatMockModel(doStream: _ => Parts(), modelId: modelId),
                Prompt = "hello",
                MaxRetries = 0,
                StreamRetries = streamRetries,
                OnError = (_, _) => Task.CompletedTask,
                Telemetry = new TelemetryOptions { Integrations = [integration] },
            }
        );
        await result.ConsumeStreamAsync();

        Assert.Equal(streamRetries + 1, streamCalls);
        Assert.Equal(streamRetries + 1, collector.DataPoints.Count);
        Assert.Single(collector.DataPoints, point =>
            Equals(point.Attributes.GetValueOrDefault("error.type"), "_OTHER")
        );
        if (streamRetries == 1)
        {
            Assert.Equal("recovered", await result.Text);
            Assert.Single(collector.DataPoints, point =>
                !point.Attributes.ContainsKey("error.type")
            );
        }
    }
}
