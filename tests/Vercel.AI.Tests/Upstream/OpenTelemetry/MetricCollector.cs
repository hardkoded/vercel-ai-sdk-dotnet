// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics.Metrics;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry;

/// <summary>Stands in for upstream's <c>createMetricReader</c> and <c>collectInferenceDuration</c>: collects <c>gen_ai.client.inference.duration</c> measurements.</summary>
internal sealed class MetricCollector : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly object _gate = new();
    private readonly List<(double Value, Dictionary<string, object?> Attributes)> _points = [];

    public MetricCollector(string requestModel)
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Name == "gen_ai.client.inference.duration")
            {
                Instrument = instrument;
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<double>(
            (_, value, tags, _) =>
            {
                var attributes = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var tag in tags)
                {
                    attributes[tag.Key] = tag.Value;
                }

                // Other tests record into the same process-wide meter: keep only this test's model.
                if (Equals(attributes.GetValueOrDefault("gen_ai.request.model"), requestModel))
                {
                    lock (_gate)
                    {
                        _points.Add((value, attributes));
                    }
                }
            }
        );
        _listener.Start();
    }

    public Instrument? Instrument { get; private set; }

    public IReadOnlyList<(double Value, Dictionary<string, object?> Attributes)> DataPoints
    {
        get
        {
            lock (_gate)
            {
                return [.. _points];
            }
        }
    }

    public void Dispose() => _listener.Dispose();
}
