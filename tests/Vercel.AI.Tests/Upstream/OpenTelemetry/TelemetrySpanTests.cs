// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class TelemetrySpanTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should execute the function and return its result",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_executes_the_function_and_returns_its_result()
    {
        using var probe = new Probe();
        var result = await TelemetrySpan.RecordAsync(
            "test-span",
            _ => Task.FromResult("test-result"),
            Task.FromResult<IReadOnlyDictionary<string, object?>>(new Dictionary<string, object?> { ["key"] = "value" }),
            activitySource: probe.Source);

        Assert.Equal("test-result", result);
        Assert.Equal("test-span", probe.Stopped!.OperationName);
        Assert.Equal("value", probe.Stopped.GetTagItem("key"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should end span when endWhenDone is true (default)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_ends_the_span_when_end_when_done_is_true()
    {
        using var probe = new Probe();
        await TelemetrySpan.RecordAsync("test-span", _ => Task.FromResult("result"), activitySource: probe.Source);

        Assert.NotNull(probe.Stopped);
        Assert.True(probe.Stopped!.IsStopped);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should not end span when endWhenDone is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_leaves_the_span_open_when_end_when_done_is_false()
    {
        using var probe = new Probe();
        Activity? activity = null;
        await TelemetrySpan.RecordAsync(
            "test-span",
            current =>
            {
                activity = current;
                return Task.FromResult("result");
            },
            endWhenDone: false,
            activitySource: probe.Source);

        Assert.NotNull(activity);
        Assert.False(activity!.IsStopped);
        Assert.Null(probe.Stopped);
        activity.Dispose();
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should record error and end span on exception",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_sets_error_status_and_ends_the_span_on_exception()
    {
        using var probe = new Probe();
        var error = new InvalidOperationException("Test error");

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => TelemetrySpan.RecordAsync<string>(
            "test-span",
            _ => throw error,
            activitySource: probe.Source));

        Assert.Same(error, thrown);
        Assert.Equal(ActivityStatusCode.Error, probe.Stopped!.Status);
        Assert.Equal("Test error", probe.Stopped.StatusDescription);
        Assert.Equal("exception", Assert.Single(probe.Stopped.Events).Name);
        Assert.True(probe.Stopped.IsStopped);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should support async attributes",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_applies_attributes_that_arrive_asynchronously()
    {
        using var probe = new Probe();
        var attributes = Task.FromResult<IReadOnlyDictionary<string, object?>>(new Dictionary<string, object?>
        {
            ["async"] = "attribute",
        });

        await TelemetrySpan.RecordAsync("test-span", _ => Task.FromResult("result"), attributes, activitySource: probe.Source);

        Assert.Equal("attribute", probe.Stopped!.GetTagItem("async"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should record exception for Error instances",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Record_error_records_an_exception_event_for_an_error()
    {
        using var probe = new Probe();
        var error = new InvalidOperationException("Test error");

        await Assert.ThrowsAsync<InvalidOperationException>(() => TelemetrySpan.RecordAsync<string>(
            "test-span",
            activity =>
            {
                TelemetrySpan.RecordError(activity!, error);
                throw error;
            },
            activitySource: probe.Source));

        Assert.Equal(ActivityStatusCode.Error, probe.Stopped!.Status);
        Assert.Equal("Test error", probe.Stopped.StatusDescription);
        Assert.Equal(2, probe.Stopped.Events.Count());
        Assert.All(probe.Stopped.Events, item => Assert.Equal("exception", item.Name));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should set error status for non-Error exceptions",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_error_sets_status_without_a_message_for_a_non_exception()
    {
        using var probe = new Probe();
        var activity = probe.Source.StartActivity("test-span");
        Assert.NotNull(activity);

        TelemetrySpan.RecordError(activity!, "string error");

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.True(string.IsNullOrEmpty(activity.StatusDescription));
        Assert.Empty(activity.Events);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should record the HTTP status code from an API call error",
        Coverage = UpstreamCoverage.Covered)]
    public void Record_error_sets_the_http_status_from_an_api_exception()
    {
        using var probe = new Probe();
        var activity = probe.Source.StartActivity("test-span");
        Assert.NotNull(activity);

        TelemetrySpan.RecordError(activity!, new BadRequestException("Bad request", null));

        Assert.Equal(400, activity.GetTagItem("http.response.status_code"));
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Bad request", activity.StatusDescription);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/open-telemetry.test.ts::OpenTelemetry > onStepFinish::sets cache token attributes when available",
        Coverage = UpstreamCoverage.Covered)]
    public void Usage_records_cache_token_attributes_when_they_are_present()
    {
        using var probe = new Probe();
        var activity = probe.Source.StartActivity("usage")!;
        new OpenTelemetryAiTelemetry().OnUsage(
            activity,
            new LanguageModelUsage(
                inputTokens: 100,
                outputTokens: 50,
                totalTokens: 150,
                cacheReadTokens: 20,
                cacheWriteTokens: 10,
                reasoningTokens: 10));

        Assert.Equal(100, activity.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Equal(50, activity.GetTagItem("gen_ai.usage.output_tokens"));
        Assert.Equal(20, activity.GetTagItem("gen_ai.usage.cache_read.input_tokens"));
        Assert.Equal(10, activity.GetTagItem("gen_ai.usage.cache_creation.input_tokens"));
    }

    [Fact]
    public void Finish_reason_sets_the_response_attribute_and_error_status()
    {
        using var probe = new Probe();
        var telemetry = new OpenTelemetryAiTelemetry();
        var stopped = probe.Source.StartActivity("finish-stop")!;
        telemetry.OnFinish(stopped, FinishReason.Stop);
        Assert.Equal(new[] { "stop" }, Reasons(stopped));
        Assert.Equal(ActivityStatusCode.Unset, stopped.Status);

        var toolCalls = probe.Source.StartActivity("finish-tools")!;
        telemetry.OnFinish(toolCalls, FinishReason.ToolCalls);
        Assert.Equal(new[] { "tool-calls" }, Reasons(toolCalls));

        var filtered = probe.Source.StartActivity("finish-filter")!;
        telemetry.OnFinish(filtered, FinishReason.ContentFilter);
        Assert.Equal(new[] { "content-filter" }, Reasons(filtered));

        var failed = probe.Source.StartActivity("finish-error")!;
        telemetry.OnFinish(failed, FinishReason.Error);
        Assert.Equal(new[] { "error" }, Reasons(failed));
        Assert.Equal(ActivityStatusCode.Error, failed.Status);

        var speech = probe.Source.StartActivity("speech")!;
        telemetry.OnUsage(speech, new AudioUsage(characters: 12));
        Assert.Equal(12, speech.GetTagItem("gen_ai.usage.characters"));
        Assert.Null(speech.GetTagItem("gen_ai.usage.input_tokens"));
        Assert.Null(speech.GetTagItem("gen_ai.usage.cache_read.input_tokens"));
        telemetry.OnUsage(speech, new AudioUsage(seconds: 1.5));
        Assert.Equal(1.5, speech.GetTagItem("gen_ai.usage.seconds"));
    }

    private static string[] Reasons(Activity activity)
    {
        var value = activity.GetTagItem("gen_ai.response.finish_reasons");
        return Assert.IsAssignableFrom<IEnumerable<string>>(value).ToArray();
    }

    private sealed class Probe : IDisposable
    {
        private readonly ActivityListener _listener;
        private readonly string _name = "Vercel.AI.Tests.Probe." + Guid.NewGuid().ToString("N");

        public Probe()
        {
            Source = new ActivitySource(_name);
            _listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == _name,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity => Stopped = activity,
            };
            ActivitySource.AddActivityListener(_listener);
        }

        public ActivitySource Source { get; }

        public Activity? Stopped { get; private set; }

        public void Dispose()
        {
            _listener.Dispose();
            Source.Dispose();
        }
    }
}
