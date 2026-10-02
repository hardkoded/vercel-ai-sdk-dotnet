// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using FinishReasonValue = Vercel.AI.Provider.FinishReason;

namespace Vercel.AI.Error;

/// <summary>No object could be generated, parsed, or validated against the schema.</summary>
public sealed class NoObjectGeneratedException : AiSdkException
{
    /// <summary>Upstream error name.</summary>
    public const string ErrorName = "AI_NoObjectGeneratedError";

    /// <summary>Marker used to recognize this error across assembly copies.</summary>
    public const string TypeMarker = "vercel.ai.error.AI_NoObjectGeneratedError";

    /// <summary>Creates the exception.</summary>
    public NoObjectGeneratedException(
        LanguageModelResponseMetadata response,
        LanguageModelUsage usage,
        FinishReasonValue finishReason,
        string? message = null,
        Exception? cause = null,
        string? text = null)
        : base(ErrorName, message ?? "No object generated.", cause)
    {
        SetMarker(TypeMarker);
        Response = response;
        Usage = usage;
        FinishReason = finishReason;
        Text = text;
    }

    /// <summary>Text the model generated, either raw text or tool-call text.</summary>
    public string? Text { get; }

    /// <summary>Response metadata for the call.</summary>
    public LanguageModelResponseMetadata Response { get; }

    /// <summary>Token usage for the call.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Why the model stopped.</summary>
    public FinishReasonValue FinishReason { get; }

    /// <summary>Returns true when <paramref name="error"/> carries <see cref="TypeMarker"/>.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, TypeMarker);
    }
}
