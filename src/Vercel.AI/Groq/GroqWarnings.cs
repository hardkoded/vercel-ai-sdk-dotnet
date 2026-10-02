// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Groq;

/// <summary>Call warnings whose message keeps the upstream feature and details.</summary>
public static class GroqWarnings
{
    /// <summary>Builds an unsupported warning.</summary>
    public static CallWarning Unsupported(string feature, string? details = null)
    {
        return new CallWarning("unsupported", Format(feature, details));
    }

    /// <summary>Builds a compatibility warning.</summary>
    public static CallWarning Compatibility(string feature, string details)
    {
        return new CallWarning("compatibility", Format(feature, details));
    }

    /// <summary>True when <paramref name="warning"/> carries the upstream type, feature, and details.</summary>
    public static bool Matches(CallWarning warning, string type, string feature, string? details = null)
    {
        return string.Equals(warning.Type, type, StringComparison.Ordinal)
            && string.Equals(warning.Message, Format(feature, details), StringComparison.Ordinal);
    }

    /// <summary>Joins a feature name and optional details.</summary>
    public static string Format(string feature, string? details)
    {
        return details == null ? feature : feature + " | " + details;
    }
}
