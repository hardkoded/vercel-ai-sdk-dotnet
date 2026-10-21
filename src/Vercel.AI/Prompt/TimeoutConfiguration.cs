// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Prompt;

/// <summary>
/// Timeout configuration for <c>GenerateTextAsync</c> and <c>StreamTextAsync</c>. Maps to <c>TimeoutConfiguration</c>.
/// Upstream takes a number (the total timeout in milliseconds) or an object. A <see cref="TimeSpan"/> converts to the total timeout, as the number form does upstream.
/// An expired timeout aborts the call with a <see cref="Vercel.AI.Util.TimeoutError"/> reason.
/// </summary>
public sealed class TimeoutConfiguration
{
    private int? _totalMs;
    private int? _stepMs;
    private int? _chunkMs;

    /// <summary>Total timeout for the entire call, including all steps, in milliseconds.</summary>
    public int? TotalMs
    {
        get => _totalMs;
        set => _totalMs = Validate(value, nameof(TotalMs));
    }

    /// <summary>
    /// Timeout of each complete step, in milliseconds. A step covers model generation, retries, and local tool execution.
    /// The budget starts again for each step.
    /// </summary>
    public int? StepMs
    {
        get => _stepMs;
        set => _stepMs = Validate(value, nameof(StepMs));
    }

    /// <summary>
    /// Timeout between meaningful model output chunks, in milliseconds. Streaming only.
    /// It starts with the first output chunk, so it does not limit the wait for that chunk. It ends when the model response ends.
    /// <c>GenerateTextAsync</c> does not support it.
    /// </summary>
    public int? ChunkMs
    {
        get => _chunkMs;
        set => _chunkMs = Validate(value, nameof(ChunkMs));
    }

    /// <summary>
    /// A configuration whose total timeout is <paramref name="timeout"/>, rounded up to a whole millisecond.
    /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> and a value longer than <see cref="int.MaxValue"/> milliseconds mean no timeout.
    /// </summary>
    public static implicit operator TimeoutConfiguration(TimeSpan timeout)
    {
        if (timeout == System.Threading.Timeout.InfiniteTimeSpan)
        {
            return new TimeoutConfiguration();
        }

        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "A timeout cannot be negative.");
        }

        var milliseconds = Math.Ceiling(timeout.TotalMilliseconds);
        if (milliseconds > int.MaxValue)
        {
            return new TimeoutConfiguration();
        }

        return new TimeoutConfiguration { TotalMs = (int)milliseconds };
    }

    private static int? Validate(int? value, string name)
    {
        if (value is < 0)
        {
            throw new ArgumentOutOfRangeException(name, value, "A timeout cannot be negative.");
        }

        return value;
    }
}
