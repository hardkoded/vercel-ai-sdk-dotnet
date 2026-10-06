// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>A provider has no model with the requested id. Maps to <c>NoSuchModelError</c>.</summary>
public class NoSuchModelError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_NoSuchModelError";

    /// <summary>Creates the error. <paramref name="modelType"/> is a name such as <c>languageModel</c>.</summary>
    public NoSuchModelError(string modelId, string modelType, string? message = null)
        : this("AI_NoSuchModelError", modelId, modelType, message ?? "No such " + modelType + ": " + modelId)
    {
    }

    /// <summary>Creates the error with a subclass error name.</summary>
    protected NoSuchModelError(string errorName, string modelId, string modelType, string message)
        : base(errorName, message)
    {
        ModelId = modelId;
        ModelType = modelType;
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Requested model id.</summary>
    public string ModelId { get; }

    /// <summary>Requested model type, such as <c>languageModel</c>.</summary>
    public string ModelType { get; }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
