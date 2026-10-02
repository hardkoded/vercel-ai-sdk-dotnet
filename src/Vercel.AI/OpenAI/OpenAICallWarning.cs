// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>A call warning with the upstream <c>feature</c>, <c>details</c>, and <c>message</c> fields.</summary>
public sealed class OpenAICallWarning : CallWarning
{
    /// <summary>Creates a warning. <paramref name="message"/> is the upstream <c>other</c> message.</summary>
    public OpenAICallWarning(string type, string? feature, string? details, string? message)
        : base(type, message ?? details ?? feature ?? string.Empty)
    {
        Feature = feature;
        Details = details;
        WarningMessage = message;
    }

    /// <summary>Unsupported or compatibility feature name.</summary>
    public string? Feature { get; }

    /// <summary>Why the feature was changed or dropped.</summary>
    public string? Details { get; }

    /// <summary>Message for <c>other</c> warnings.</summary>
    public string? WarningMessage { get; }
}
