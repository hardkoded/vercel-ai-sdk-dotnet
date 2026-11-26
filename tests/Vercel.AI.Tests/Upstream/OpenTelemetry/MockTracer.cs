// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry;

/// <summary>Port of <c>mock-tracer.ts</c>: an <see cref="ActivitySource"/> that records every span it starts.</summary>
internal sealed class MockTracer : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly object _gate = new();
    private readonly List<MockSpan> _spans = [];
    private readonly List<MockSpan> _finished = [];

    public delegate ActivitySamplingResult Sampler(
        ref ActivityCreationOptions<ActivityContext> options
    );

    public MockTracer(Sampler? sampler = null)
    {
        Source = new ActivitySource("mock-tracer-" + Guid.NewGuid().ToString("N"));
        _listener = new ActivityListener
        {
            ShouldListenTo = source => ReferenceEquals(source, Source),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) =>
                sampler is null ? ActivitySamplingResult.AllDataAndRecorded : sampler(ref options),
            ActivityStarted = activity =>
            {
                lock (_gate)
                {
                    _spans.Add(new MockSpan(activity));
                }
            },
            ActivityStopped = activity =>
            {
                lock (_gate)
                {
                    _finished.Add(_spans.First(span => ReferenceEquals(span.Activity, activity)));
                }
            },
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public ActivitySource Source { get; }

    public IReadOnlyList<MockSpan> Spans
    {
        get
        {
            lock (_gate)
            {
                return [.. _spans];
            }
        }
    }

    /// <summary>The spans in the order they ended, like <c>exporter.getFinishedSpans()</c>.</summary>
    public IReadOnlyList<MockSpan> FinishedSpans
    {
        get
        {
            lock (_gate)
            {
                return [.. _finished];
            }
        }
    }

    public void Dispose()
    {
        _listener.Dispose();
        Source.Dispose();
    }
}

/// <summary>A started span with the attributes it had when it started (<c>initAttributes</c>).</summary>
internal sealed class MockSpan
{
    public MockSpan(Activity activity)
    {
        Activity = activity;
        InitAttributes = activity.TagObjects.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal
        );
    }

    public Activity Activity { get; }

    public string Name => Activity.OperationName;

    public bool Ended => Activity.IsStopped;

    public IReadOnlyDictionary<string, object?> InitAttributes { get; }

    /// <summary>Attributes set after the span started (<c>runtimeAttributes</c>).</summary>
    public IReadOnlyDictionary<string, object?> RuntimeAttributes =>
        Activity
            .TagObjects.Where(pair =>
                !InitAttributes.TryGetValue(pair.Key, out var initial)
                || !Equals(initial, pair.Value)
            )
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    public IReadOnlyDictionary<string, object?> Attributes =>
        Activity.TagObjects.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal
        );

    public int ExceptionCount => Activity.Events.Count(e => e.Name == "exception");
}
