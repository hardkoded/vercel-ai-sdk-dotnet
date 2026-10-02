// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>A warning produced while preparing an OpenAI request.</summary>
public sealed class OpenAICallWarning
{
    /// <summary>Creates a warning. <paramref name="details"/> or <paramref name="message"/> is the text callers read.</summary>
    public OpenAICallWarning(string type, string? feature, string? details, string? message = null)
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

    /// <summary>Why the request was changed.</summary>
    public string? Details { get; }

    /// <summary>Free-form message used by <c>other</c> warnings.</summary>
    public string? Message { get; }

    /// <summary>The warning text stored on language-model results.</summary>
    public CallWarning ToCallWarning()
    {
        return new CallWarning(Type, Details ?? Message ?? Feature ?? Type);
    }
}
