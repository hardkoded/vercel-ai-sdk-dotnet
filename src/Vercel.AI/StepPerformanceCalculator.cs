// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI;

internal static class StepPerformanceCalculator
{
    public static StepPerformance Create(
        double stepTimeMs,
        double responseTimeMs,
        IReadOnlyList<double> outputChunkTimesMs,
        IReadOnlyDictionary<string, double> toolExecutionMs,
        LanguageModelUsage usage)
    {
        double inputTokens = usage.InputTokens ?? 0;
        double outputTokens = usage.OutputTokens ?? 0;
        var requestSeconds = responseTimeMs / 1000;
        double? timeToFirstOutputMs = outputChunkTimesMs.Count > 0 ? outputChunkTimesMs[0] : null;
        double? outputTokensPerSecond = null;
        double? inputTokensPerSecond = null;
        if (timeToFirstOutputMs is double first)
        {
            var outputStreamSeconds = (responseTimeMs - first) / 1000;
            outputTokensPerSecond = outputStreamSeconds > 0 ? outputTokens / outputStreamSeconds : 0;
            inputTokensPerSecond = first > 0 ? inputTokens / (first / 1000) : 0;
        }

        return new StepPerformance
        {
            StepTimeMs = stepTimeMs,
            ResponseTimeMs = responseTimeMs,
            ToolExecutionMs = toolExecutionMs,
            EffectiveOutputTokensPerSecond = requestSeconds > 0 ? outputTokens / requestSeconds : 0,
            EffectiveTotalTokensPerSecond = requestSeconds > 0 ? (inputTokens + outputTokens) / requestSeconds : 0,
            OutputTokensPerSecond = outputTokensPerSecond,
            InputTokensPerSecond = inputTokensPerSecond,
            TimeToFirstOutputMs = timeToFirstOutputMs,
            TimeBetweenOutputChunksMs = Gaps(outputChunkTimesMs),
        };
    }

    private static OutputChunkTimingStats? Gaps(IReadOnlyList<double> times)
    {
        if (times.Count < 2)
        {
            return null;
        }

        var gaps = new double[times.Count - 1];
        for (var i = 1; i < times.Count; i++)
        {
            gaps[i - 1] = times[i] - times[i - 1];
        }

        Array.Sort(gaps);
        return new OutputChunkTimingStats(gaps[0], gaps[gaps.Length - 1], gaps.Average(), Percentile(gaps, 0.5), Percentile(gaps, 0.1), Percentile(gaps, 0.9));
    }

    private static double Percentile(double[] sorted, double fraction)
    {
        var position = (sorted.Length - 1) * fraction;
        var lower = (int)Math.Floor(position);
        var upper = (int)Math.Ceiling(position);
        return sorted[lower] + ((sorted[upper] - sorted[lower]) * (position - lower));
    }
}
