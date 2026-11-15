// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>An extraction model does not support the type of a requested field.</summary>
public sealed class ExtractionUnsupportedFieldTypeError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_ExtractionUnsupportedFieldTypeError";

    /// <summary>Creates the error for the unsupported field.</summary>
    public ExtractionUnsupportedFieldTypeError(
        string fieldId,
        string fieldType,
        string provider,
        string modelId,
        string? message = null)
        : base(
            "AI_ExtractionUnsupportedFieldTypeError",
            message ?? ("Field \"" + fieldId + "\" has type \"" + fieldType + "\", which is not supported by provider \"" + provider + "\" and model \"" + modelId + "\"."))
    {
        FieldId = fieldId;
        FieldType = fieldType;
        Provider = provider;
        ModelId = modelId;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Field identifier.</summary>
    public string FieldId { get; }

    /// <summary>Field type the model rejected.</summary>
    public string FieldType { get; }

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
