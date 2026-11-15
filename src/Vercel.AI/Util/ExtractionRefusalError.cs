// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>An extraction model declined to extract one or more fields.</summary>
public sealed class ExtractionRefusalError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_ExtractionRefusalError";

    private static readonly JsonSerializerOptions Quote = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Creates the error for the refused fields.</summary>
    public ExtractionRefusalError(
        IReadOnlyList<string> fieldIds,
        string provider,
        string modelId,
        string? message = null)
        : base(
            "AI_ExtractionRefusalError",
            message ?? ("Extraction model \"" + modelId + "\" from provider \"" + provider + "\" refused " + (fieldIds.Count == 1 ? "field " : "fields ") + string.Join(", ", fieldIds.Select(id => JsonSerializer.Serialize(id, Quote))) + "."))
    {
        FieldIds = fieldIds;
        Provider = provider;
        ModelId = modelId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Refused field ids, in field order.</summary>
    public IReadOnlyList<string> FieldIds { get; }

    /// <summary>Extraction provider id.</summary>
    public string Provider { get; }

    /// <summary>Extraction model id.</summary>
    public string ModelId { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
