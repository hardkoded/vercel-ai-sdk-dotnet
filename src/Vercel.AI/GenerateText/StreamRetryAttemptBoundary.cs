// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>
/// Maps to <c>StreamRetryAttemptBoundaryPart</c>: the marker that the model call of a step sends when it starts a new attempt after a
/// stream retry. It is internal and never reaches a consumer. <c>ExecuteToolsFromStream</c> forwards it and drops the tool calls
/// that it queued for the failed attempt.
/// </summary>
internal sealed class StreamRetryAttemptBoundaryPart : TextStreamPart
{
    public StreamRetryAttemptBoundaryPart(IReadOnlyList<CallWarning> warnings)
        : base("stream-retry-attempt-boundary")
    {
        Warnings = warnings;
    }

    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Maps to <c>createStreamRetryAttemptBoundaryPart</c>.</summary>
    public static StreamRetryAttemptBoundaryPart CreateStreamRetryAttemptBoundaryPart(
        IReadOnlyList<CallWarning> warnings
    ) => new(warnings);

    /// <summary>Maps to <c>isStreamRetryAttemptBoundaryPart</c>.</summary>
    public static bool IsStreamRetryAttemptBoundaryPart(TextStreamPart? part) =>
        part is StreamRetryAttemptBoundaryPart;
}
