// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Anthropic;

/// <summary>A warning produced while preparing an Anthropic request.</summary>
public sealed class AnthropicWarning
{
    /// <summary>Creates a warning.</summary>
    public AnthropicWarning(string type, string? feature = null, string? details = null, string? message = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Feature = feature;
        Details = details;
        Message = message;
    }

    /// <summary>Warning category, such as <c>unsupported</c> or <c>compatibility</c>.</summary>
    public string Type { get; }

    /// <summary>Feature the warning refers to.</summary>
    public string? Feature { get; }

    /// <summary>Extra detail.</summary>
    public string? Details { get; }

    /// <summary>Free-form message used by <c>other</c> warnings.</summary>
    public string? Message { get; }
}
