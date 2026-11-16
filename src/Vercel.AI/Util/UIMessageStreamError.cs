// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>
/// Error thrown when a UI message stream reports an error or contains invalid or out-of-sequence chunks.
/// Maps to <c>UIMessageStreamError</c>.
/// </summary>
public sealed class UIMessageStreamError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_UIMessageStreamError";

    /// <summary>Creates the error.</summary>
    /// <param name="chunkType">The type of chunk that caused the error, for example <c>text-delta</c>.</param>
    /// <param name="chunkId">The id of the failing chunk (part id or tool call id), or an empty string.</param>
    /// <param name="message">The error message.</param>
    public UIMessageStreamError(string chunkType, string chunkId, string message)
        : base("AI_UIMessageStreamError", message)
    {
        ChunkType = chunkType;
        ChunkId = chunkId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>The type of chunk that caused the error.</summary>
    public string ChunkType { get; }

    /// <summary>The id of the failing chunk, or an empty string when the chunk has no id.</summary>
    public string ChunkId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
