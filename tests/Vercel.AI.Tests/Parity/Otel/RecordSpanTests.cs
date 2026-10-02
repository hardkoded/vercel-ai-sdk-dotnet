// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class RecordSpanTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should execute the function and return its result",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Executes_the_function_and_returns_its_result()
    {
        var tracer = new RecordingTracer();
        var result = await AiSpanRecorder.RecordAsync(
            "test-span",
            tracer,
            new Dictionary<string, object?> { ["key"] = "value" },
            _ => Task.FromResult("test-result"));

        Assert.Equal("test-result", result);
        Assert.Single(tracer.Spans);
        Assert.Equal("test-span", tracer.Spans[0].Name);
        Assert.Equal("value", tracer.Spans[0].Attributes["key"] as string);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should end span when endWhenDone is true (default)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Ends_the_span_when_end_when_done_is_true()
    {
        var tracer = new RecordingTracer();
        await AiSpanRecorder.RecordAsync(
            "test-span",
            tracer,
            new Dictionary<string, object?>(),
            _ => Task.FromResult("result"));

        Assert.Single(tracer.Spans);
        Assert.True(tracer.Spans[0].Ended);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should not end span when endWhenDone is false",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_end_the_span_when_end_when_done_is_false()
    {
        var tracer = new RecordingTracer();
        await AiSpanRecorder.RecordAsync(
            "test-span",
            tracer,
            new Dictionary<string, object?>(),
            _ => Task.FromResult("result"),
            endWhenDone: false);

        Assert.Single(tracer.Spans);
        Assert.False(tracer.Spans[0].Ended);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should record error and end span on exception",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Records_an_error_and_ends_the_span_on_exception()
    {
        var tracer = new RecordingTracer();
        var error = new InvalidOperationException("Test error");
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AiSpanRecorder.RecordAsync<string>(
                "test-span",
                tracer,
                new Dictionary<string, object?>(),
                _ => Task.FromException<string>(error)));

        Assert.Same(error, thrown);
        Assert.Single(tracer.Spans);
        var status = tracer.Spans[0].Status!;
        Assert.Equal(AiSpanRecorder.ErrorStatus, status.Code);
        Assert.Equal("Test error", status.Message);
        Assert.Equal("exception", Assert.Single(tracer.Spans[0].Events).Name);
        Assert.True(tracer.Spans[0].Ended);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordSpan::should support async attributes",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_async_attributes()
    {
        var tracer = new RecordingTracer();
        await AiSpanRecorder.RecordAsync(
            "test-span",
            tracer,
            Task.FromResult<IReadOnlyDictionary<string, object?>>(new Dictionary<string, object?> { ["async"] = "attribute" }),
            _ => Task.FromResult("result"));

        Assert.Single(tracer.Spans);
        Assert.Equal("attribute", tracer.Spans[0].Attributes["async"] as string);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should record exception for Error instances",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Records_an_exception_event_for_error_instances()
    {
        var tracer = new RecordingTracer();
        var error = new InvalidOperationException("Test error");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AiSpanRecorder.RecordAsync<string>(
                "test-span",
                tracer,
                new Dictionary<string, object?>(),
                span =>
                {
                    AiSpanRecorder.RecordError(span, error);
                    return Task.FromException<string>(error);
                }));

        Assert.Equal(2, tracer.Spans[0].Events.Count);
        Assert.Equal("exception", tracer.Spans[0].Events[0].Name);
        Assert.Equal("exception", tracer.Spans[0].Events[1].Name);
        var status = tracer.Spans[0].Status!;
        Assert.Equal(AiSpanRecorder.ErrorStatus, status.Code);
        Assert.Equal("Test error", status.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should set error status for non-Error exceptions",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sets_error_status_without_an_exception_event_for_non_exceptions()
    {
        var tracer = new RecordingTracer();
        var failure = new AiNonExceptionFailure("string error");
        await Assert.ThrowsAsync<AiNonExceptionFailure>(() =>
            AiSpanRecorder.RecordAsync<string>(
                "test-span",
                tracer,
                new Dictionary<string, object?>(),
                span =>
                {
                    AiSpanRecorder.RecordError(span, failure);
                    return Task.FromException<string>(failure);
                }));

        Assert.Empty(tracer.Spans[0].Events);
        var status = tracer.Spans[0].Status!;
        Assert.Equal(AiSpanRecorder.ErrorStatus, status.Code);
        Assert.Null(status.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should record the HTTP status code from an API call error",
        Coverage = UpstreamCoverage.Covered)]
    public void Records_the_http_status_from_an_api_error()
    {
        var tracer = new RecordingTracer();
        var span = tracer.StartSpan("test-span");
        AiSpanRecorder.RecordError(span, new BadRequestException("Bad request", null));

        Assert.Equal(400, (int)span.Attributes["http.response.status_code"]!);
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/record-span.test.ts::recordErrorOnSpan::should record the HTTP status code from the last retry error",
        Coverage = UpstreamCoverage.Covered)]
    public void Records_the_http_status_from_the_last_retry_error()
    {
        var tracer = new RecordingTracer();
        var span = tracer.StartSpan("test-span");
        AiSpanRecorder.RecordError(
            span,
            new AiRetryException("Retries exhausted", new RateLimitException("Too many requests", null)));

        Assert.Equal(429, (int)span.Attributes["http.response.status_code"]!);
    }

    private sealed class RecordingTracer : IAiTracer
    {
        public List<RecordingSpan> Spans { get; } = new();

        public IAiSpan StartSpan(string name)
        {
            var span = new RecordingSpan(name, null);
            Spans.Add(span);
            return span;
        }

        public IAiSpan StartActiveSpan(string name, IReadOnlyDictionary<string, object?>? attributes)
        {
            var span = new RecordingSpan(name, attributes);
            Spans.Add(span);
            return span;
        }
    }

    private sealed class RecordingSpan : IAiSpan
    {
        private readonly List<AiSpanEvent> events = new();

        public RecordingSpan(string name, IReadOnlyDictionary<string, object?>? attributes)
        {
            Name = name;
            Attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
            if (attributes != null)
            {
                foreach (var pair in attributes)
                {
                    Attributes[pair.Key] = pair.Value;
                }
            }
        }

        public string Name { get; }

        public IDictionary<string, object?> Attributes { get; }

        public IReadOnlyList<AiSpanEvent> Events
        {
            get { return events; }
        }

        public AiSpanStatus? Status { get; private set; }

        public bool Ended { get; private set; }

        public void SetAttribute(string name, object? value)
        {
            Attributes[name] = value;
        }

        public void SetStatus(int code, string? message = null)
        {
            Status = new AiSpanStatus(code, message);
        }

        public void RecordException(Exception exception)
        {
            events.Add(new AiSpanEvent("exception"));
        }

        public void End()
        {
            Ended = true;
        }
    }
}
