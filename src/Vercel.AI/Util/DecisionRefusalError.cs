// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>A decision model refused one or more questions.</summary>
public sealed class DecisionRefusalError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_DecisionRefusalError";

    private static readonly JsonSerializerOptions Quote = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Creates the error for the refused questions.</summary>
    public DecisionRefusalError(
        IReadOnlyList<string> questionIds,
        string provider,
        string modelId,
        string? message = null)
        : base(
            "AI_DecisionRefusalError",
            message ?? ("Decision model \"" + modelId + "\" from provider \"" + provider + "\" refused " + (questionIds.Count == 1 ? "question " : "questions ") + string.Join(", ", questionIds.Select(id => JsonSerializer.Serialize(id, Quote))) + "."))
    {
        QuestionIds = questionIds;
        Provider = provider;
        ModelId = modelId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Refused question ids, in question order.</summary>
    public IReadOnlyList<string> QuestionIds { get; }

    /// <summary>Decision provider id.</summary>
    public string Provider { get; }

    /// <summary>Decision model id.</summary>
    public string ModelId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
