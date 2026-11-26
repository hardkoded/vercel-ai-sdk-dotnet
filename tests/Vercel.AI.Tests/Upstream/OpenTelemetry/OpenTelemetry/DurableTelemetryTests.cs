// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry.OpenTelemetrySupport;
using OtelIntegration = Vercel.AI.OpenTelemetry.OpenTelemetry;
using OtelOptions = Vercel.AI.OpenTelemetry.OpenTelemetryOptions;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.OpenTelemetry;

/// <summary>Port of <c>open-telemetry.test.ts</c> &gt; <c>OpenTelemetry</c> &gt; <c>durable telemetry</c>.</summary>
public sealed class DurableTelemetryTests
{
    private const string Test =
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > durable telemetry::";

    [Fact]
    [UpstreamTest(
        Test + "uses event timestamps for span start and end times",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7760e33531f08f5affe7b85849989064ce3b6a24"
    )]
    public async Task Uses_event_timestamps_for_span_start_and_end_times()
    {
        using var tracer = new MockTracer();
        var integration = new OtelIntegration(new OtelOptions { Tracer = tracer.Source });

        // Epoch times before the process time origin must not be treated as relative values.
        await integration.OnStart!(MakeOnStartEvent(timestamp: 1_000), default);
        await integration.OnStepStart!(MakeStepStartEvent(timestamp: 1_100), default);
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(timestamp: 1_200),
            default
        );
        await integration.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(timestamp: 1_300),
            default
        );
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(timestamp: 1_400), default);
        await integration.OnToolExecutionEnd!(
            MakeToolCallFinishEvent(true, timestamp: 1_500),
            default
        );
        await integration.OnStepEnd!(MakeStepFinishEvent(timestamp: 1_600), default);
        await integration.OnEnd!(MakeFinishEvent(timestamp: 1_700), default);

        Assert.Equal(
            new Dictionary<string, (long, long)>
            {
                ["invoke_agent gpt-4"] = (1_000, 1_700),
                ["step 1"] = (1_100, 1_600),
                ["chat gpt-4"] = (1_200, 1_300),
                ["execute_tool myTool"] = (1_400, 1_500),
            },
            tracer.FinishedSpans.ToDictionary(span => span.Name, span => Times(span.Activity))
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves the parent trace and sampler trace state",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7760e33531f08f5affe7b85849989064ce3b6a24"
    )]
    public async Task Preserves_the_parent_trace_and_sampler_trace_state()
    {
        using var parent = new Activity("parent");
        parent.SetParentId(
            ActivityTraceId.CreateFromString(new string('a', 32).AsSpan()),
            ActivitySpanId.CreateFromString(new string('b', 16).AsSpan()),
            ActivityTraceFlags.Recorded
        );
        parent.TraceStateString = "vendor=parent";
        parent.Start();
        var durable = new OpenTelemetryIdGenerator(
            new OpenTelemetryIdGeneratorOptions
            {
                Sampler = (_, _, _, _, _) =>
                    new DurableSamplingResult
                    {
                        Decision = 2,
                        TraceState = "vendor=sampled",
                        Attributes = new Dictionary<string, object?>
                        {
                            ["sampling.attribute"] = "preserved",
                        },
                    },
            }
        );
        using var tracer = CreateDurableTracer(durable);
        var integration = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, Durable = durable }
        );
        var run = (await integration.ExperimentalDurable!.CaptureContextAsync(MakeOnStartEvent()))!;
        var parentTraceId = parent.TraceId.ToHexString();
        var parentSpanId = parent.SpanId.ToHexString();
        parent.Stop();
        Activity.Current = null;

        Assert.Equal(parentTraceId, run.TraceId);
        Assert.Equal(1, run.TraceFlags);
        Assert.Equal("vendor=sampled", run.TraceState);
        Assert.Equal(parentSpanId, run.ParentSpanContext!.SpanId);

        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(durableContext: Live(run, 0)),
            default
        );
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);
        await integration.OnStart!(MakeOnStartEvent(durableContext: Replay(run)), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        var spans = tracer.FinishedSpans;
        Assert.Equal(3, spans.Count);
        foreach (var span in spans)
        {
            Assert.Equal(parentTraceId, span.Activity.TraceId.ToHexString());
            Assert.Equal("vendor=sampled", span.Activity.TraceStateString);
        }

        var root = spans.First(span =>
            span.Name.StartsWith("invoke_agent", StringComparison.Ordinal)
        );
        Assert.Equal(parentSpanId, root.Activity.ParentSpanId.ToHexString());
        Assert.Equal("preserved", root.Attributes["sampling.attribute"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "preserves filtered context and headers on live model spans",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7760e33531f08f5affe7b85849989064ce3b6a24"
    )]
    public async Task Preserves_filtered_context_and_headers_on_live_model_spans()
    {
        var durable = new OpenTelemetryIdGenerator();
        using var tracer = CreateDurableTracer(durable);
        var enriched = new List<EnrichSpanContext>();
        var integration = new OtelIntegration(
            new OtelOptions
            {
                Tracer = tracer.Source,
                Durable = durable,
                RuntimeContext = true,
                Headers = true,
                EnrichSpan = context =>
                {
                    enriched.Add(context);
                    return null;
                },
            }
        );
        var run = (await integration.ExperimentalDurable!.CaptureContextAsync(MakeOnStartEvent()))!;
        var runtimeContext = new Dictionary<string, object?>
        {
            ["traceIdentity"] = new Dictionary<string, object?> { ["sessionId"] = "session-1" },
        };
        await integration.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(
                durableContext: new DurableTelemetryEventContext
                {
                    Run = run,
                    Phase = DurableTelemetryPhase.Live,
                    StepNumber = 0,
                    RuntimeContext = runtimeContext,
                    Headers = new Dictionary<string, string?> { ["x-test"] = "value" },
                }
            ),
            default
        );
        await integration.OnLanguageModelCallEnd!(MakeLanguageModelCallEndEvent(), default);

        Assert.Contains(enriched, context =>
            context.SpanType == OpenTelemetrySpanType.LanguageModel
            && ReferenceEquals(context.RuntimeContext, runtimeContext)
        );
        var chat = tracer.FinishedSpans[0];
        Assert.Equal("value", chat.Attributes["ai.request.headers.x-test"]);
    }

    [Fact]
    [UpstreamTest(
        Test + "reuses the captured sampling decision for model and tool spans",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7760e33531f08f5affe7b85849989064ce3b6a24"
    )]
    public async Task Reuses_the_captured_sampling_decision_for_model_and_tool_spans()
    {
        var sampleCalls = 0;
        var durable = new OpenTelemetryIdGenerator(
            new OpenTelemetryIdGeneratorOptions
            {
                Sampler = (_, _, _, _, _) =>
                {
                    sampleCalls++;
                    return new DurableSamplingResult { Decision = 2 };
                },
            }
        );
        using var tracer = CreateDurableTracer(durable);
        var live = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, Durable = durable }
        );
        var run = (await live.ExperimentalDurable!.CaptureContextAsync(MakeOnStartEvent()))!;
        await live.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(durableContext: Live(run, 0)),
            default
        );
        await live.OnLanguageModelCallEnd!(
            MakeLanguageModelCallEndEvent(
                content:
                [
                    new GeneratedToolCall("provider-tool", "search", "{}")
                    {
                        ProviderExecuted = true,
                    },
                ]
            ),
            default
        );
        var integration = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, Durable = durable }
        );
        await integration.OnStart!(MakeOnStartEvent(durableContext: Replay(run)), default);
        await integration.OnStepStart!(MakeStepStartEvent(), default);
        await integration.OnToolExecutionStart!(MakeToolCallStartEvent(), default);
        await integration.OnToolExecutionEnd!(MakeToolCallFinishEvent(true), default);
        await integration.OnStepEnd!(MakeStepFinishEvent(), default);
        await integration.OnEnd!(MakeFinishEvent(), default);

        Assert.Equal(1, sampleCalls);
        Assert.Equal(5, tracer.FinishedSpans.Count);
    }

    [Fact]
    [UpstreamTest(
        Test + "closes failed durable attempts and a failed retry",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7760e33531f08f5affe7b85849989064ce3b6a24"
    )]
    public async Task Closes_failed_durable_attempts_and_a_failed_retry()
    {
        var durable = new OpenTelemetryIdGenerator();
        using var tracer = CreateDurableTracer(durable);
        var live = new OtelIntegration(
            new OtelOptions { Tracer = tracer.Source, Durable = durable }
        );
        var run = (await live.ExperimentalDurable!.CaptureContextAsync(MakeOnStartEvent()))!;
        await live.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(durableContext: Live(run, 0), timestamp: 1_000),
            default
        );
        await live.OnLanguageModelCallStart!(
            MakeLanguageModelCallStartEvent(durableContext: Live(run, 0), timestamp: 1_100),
            default
        );
        await live.OnError!(
            new TelemetryErrorEvent(CallId, new Exception("rejected")) { Timestamp = 1_200 },
            default
        );

        var spans = tracer.FinishedSpans;
        Assert.Equal(2, spans.Count);
        var failed = spans[0].Activity;
        var retried = spans[1].Activity;
        Assert.Equal(ActivityStatusCode.Error, failed.Status);
        Assert.Equal(ActivityStatusCode.Error, retried.Status);
        Assert.Equal(1_200, Times(retried).End);
        Assert.Equal(Times(failed).End, Times(retried).Start);
        Assert.Equal(failed.ParentSpanId, retried.ParentSpanId);
        Assert.NotEqual(failed.SpanId, retried.SpanId);
    }

    private static (long Start, long End) Times(Activity activity)
    {
        var start = new DateTimeOffset(activity.StartTimeUtc).ToUnixTimeMilliseconds();
        return (start, start + (long)Math.Round(activity.Duration.TotalMilliseconds));
    }

    private static DurableTelemetryEventContext Live(DurableTelemetryContext run, int stepNumber) =>
        new()
        {
            Run = run,
            Phase = DurableTelemetryPhase.Live,
            StepNumber = stepNumber,
        };

    private static DurableTelemetryEventContext Replay(DurableTelemetryContext run) =>
        new() { Run = run, Phase = DurableTelemetryPhase.Replay };

    // Port of createDurableTracer: the bridge is also the sampler of the tracer.
    private static MockTracer CreateDurableTracer(OpenTelemetryIdGenerator durable) =>
        new(
            (ref ActivityCreationOptions<ActivityContext> options) =>
            {
                var result = durable.ShouldSample(
                    options.Parent,
                    options.TraceId.ToHexString(),
                    options.Name,
                    options.Kind,
                    new Dictionary<string, object?>()
                );
                return result.Decision switch
                {
                    2 => ActivitySamplingResult.AllDataAndRecorded,
                    1 => ActivitySamplingResult.AllData,
                    _ => ActivitySamplingResult.None,
                };
            }
        );
}
