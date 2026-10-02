// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>A UI message stream reported an error or contained an invalid or out-of-sequence chunk.</summary>
public sealed class UIMessageStreamException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_UIMessageStreamError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_UIMessageStreamError";

    /// <summary>Creates the exception.</summary>
    public UIMessageStreamException(string chunkType, string chunkId, string message)
        : base(ErrorName, message, null)
    {
        SetMarker(TypeMarker);
        ChunkType = chunkType;
        ChunkId = chunkId;
    }

    /// <summary>Chunk type that caused the error, such as <c>text-delta</c> or <c>reasoning-end</c>.</summary>
    public string ChunkType { get; }

    /// <summary>Part id or tool call id for the failing chunk. Empty when the chunk has no id.</summary>
    public string ChunkId { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
