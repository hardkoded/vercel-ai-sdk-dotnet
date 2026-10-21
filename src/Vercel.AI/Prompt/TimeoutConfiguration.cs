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
    private int? _firstChunkMs;
    private int? _chunkMs;
    private int? _toolMs;
    private IReadOnlyDictionary<string, int>? _tools;

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
    /// Timeout until the first content chunk of each step, in milliseconds. Streaming only.
    /// It starts when the provider returns the response stream of the step, so it does not limit the wait for the response itself. A chunk without output, such as response metadata or an empty delta, does not end it.
    /// <c>GenerateTextAsync</c> does not support it.
    /// </summary>
    public int? FirstChunkMs
    {
        get => _firstChunkMs;
        set => _firstChunkMs = Validate(value, nameof(FirstChunkMs));
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
    /// Default timeout of each tool execution, in milliseconds. A tool that runs longer is cancelled and its call ends with a tool error, so the model can respond or retry.
    /// <see cref="Tools"/> overrides it for one tool.
    /// </summary>
    public int? ToolMs
    {
        get => _toolMs;
        set => _toolMs = Validate(value, nameof(ToolMs));
    }

    /// <summary>
    /// Per-tool timeouts in milliseconds. A key is the tool name followed by <c>Ms</c>, such as <c>weatherMs</c> for the tool <c>weather</c>, as in upstream.
    /// An entry takes precedence over <see cref="ToolMs"/>. Upstream types the keys from the tool set. C# cannot, so a key that matches no tool has no effect.
    /// </summary>
    public IReadOnlyDictionary<string, int>? Tools
    {
        get => _tools;
        set
        {
            // A copy, so the caller cannot add a negative limit after the check.
            if (value != null)
            {
                var copy = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var entry in value)
                {
                    copy[entry.Key] = Validate(entry.Value, entry.Key)!.Value;
                }

                value = copy;
            }

            _tools = value;
        }
    }

    /// <summary>
    /// The timeout of the tool <paramref name="toolName"/> in milliseconds: its entry in <see cref="Tools"/>, else <see cref="ToolMs"/>, else <c>null</c>.
    /// A <see cref="TimeSpan"/> converts to the total timeout only, so it has no tool timeout. Maps to <c>getToolTimeoutMs</c>.
    /// </summary>
    public static int? GetToolTimeoutMs(TimeoutConfiguration? timeout, string toolName)
    {
        if (timeout == null)
        {
            return null;
        }

        if (timeout.Tools != null && timeout.Tools.TryGetValue(toolName + "Ms", out var toolTimeoutMs))
        {
            return toolTimeoutMs;
        }

        return timeout.ToolMs;
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
