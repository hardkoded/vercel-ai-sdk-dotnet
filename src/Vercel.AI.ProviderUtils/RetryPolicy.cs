// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Retry policy aligned with the AI SDK defaults: 2 retries, 500–5000ms backoff, 25% jitter.
/// Retryable statuses are 408, 429, and 500–599. <c>Retry-After</c> is honored up to 60 seconds.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>Maximum retries after the first attempt.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Initial delay.</summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Maximum delay.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMilliseconds(5000);

    /// <summary>Jitter fraction applied to the delay. <c>0.25</c> is 25%.</summary>
    public double Jitter { get; set; } = 0.25;

    /// <summary>SDK default policy.</summary>
    public static RetryPolicy Default { get; } = new();

    /// <summary>Computes the delay before the next attempt. <paramref name="attempt"/> is zero-based.</summary>
    public TimeSpan GetDelay(int attempt, TimeSpan? retryAfter)
    {
        if (retryAfter is { } after && after > TimeSpan.Zero)
        {
            var cap = TimeSpan.FromSeconds(60);
            return after > cap ? cap : after;
        }

        var exponential = InitialDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var capped = Math.Min(MaxDelay.TotalMilliseconds, exponential);
        var spread = capped * Jitter;
        var offset = (NextRandom() * 2d) - 1d;
        var milliseconds = Math.Max(0, capped + (spread * offset));
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    private static double NextRandom()
    {
        lock (Gate)
        {
            return Random.NextDouble();
        }
    }

    private static readonly object Gate = new();
    private static readonly Random Random = new();
}
