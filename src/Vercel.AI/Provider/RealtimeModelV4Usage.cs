// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Provider;

/// <summary>
/// Normalized realtime usage on completion events or provider custom snapshots. Maps to
/// <c>RealtimeModelV4Usage</c>. Snapshots without a response association must not be summed as increments.
/// Counts are the provider's raw counts: input buckets include cached tokens, and the cached portions are reported
/// separately. Every field is optional; the whole usage is omitted when nothing is observable.
/// </summary>
public sealed class RealtimeModelV4Usage
{
    /// <summary>Gross input tokens across all reported modalities; includes cached tokens.</summary>
    public int? InputTokens { get; set; }

    /// <summary>Gross output tokens across all reported modalities.</summary>
    public int? OutputTokens { get; set; }

    /// <summary>Cached portion of gross input tokens, when the provider reports a total.</summary>
    public int? CachedInputTokens { get; set; }

    /// <summary>Input text tokens (includes any cached text tokens).</summary>
    public int? InputTextTokens { get; set; }

    /// <summary>Input audio tokens (includes any cached audio tokens).</summary>
    public int? InputAudioTokens { get; set; }

    /// <summary>Output text tokens.</summary>
    public int? OutputTextTokens { get; set; }

    /// <summary>Output audio tokens.</summary>
    public int? OutputAudioTokens { get; set; }

    /// <summary>Cached portion of the input text tokens.</summary>
    public int? CachedInputTextTokens { get; set; }

    /// <summary>Cached portion of the input audio tokens.</summary>
    public int? CachedInputAudioTokens { get; set; }

    /// <summary>Audio duration in seconds. Populated for duration-billed transcription.</summary>
    public double? Seconds { get; set; }
}
