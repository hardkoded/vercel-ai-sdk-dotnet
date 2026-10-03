// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>A warning produced while preparing a Gemini request.</summary>
public sealed class GoogleWarning
{
    /// <summary>Creates a warning.</summary>
    public GoogleWarning(string type, string message, string? feature = null, string? details = null)
    {
        Type = type ?? "other";
        Message = message ?? string.Empty;
        Feature = feature;
        Details = details;
    }

    /// <summary>Warning category: <c>unsupported</c>, <c>compatibility</c>, or <c>other</c>.</summary>
    public string Type { get; }

    /// <summary>Human-readable warning.</summary>
    public string Message { get; }

    /// <summary>Unsupported feature name, when the warning is about one.</summary>
    public string? Feature { get; }

    /// <summary>Extra detail.</summary>
    public string? Details { get; }

    /// <summary>Maps this warning onto the shared call-warning type.</summary>
    public CallWarning ToCallWarning()
    {
        var message = Message;
        if (string.IsNullOrEmpty(message))
        {
            message = Feature ?? Type;
            if (!string.IsNullOrEmpty(Details))
            {
                message = message + ": " + Details;
            }
        }

        return new CallWarning(Type, message);
    }

    /// <summary>Unsupported feature warning.</summary>
    public static GoogleWarning Unsupported(string feature, string? details = null)
    {
        var message = string.IsNullOrEmpty(details) ? feature : feature + ": " + details;
        return new GoogleWarning("unsupported", message, feature, details);
    }

    /// <summary>Compatibility mapping warning.</summary>
    public static GoogleWarning Compatibility(string feature, string details)
    {
        return new GoogleWarning("compatibility", feature + ": " + details, feature, details);
    }
}
