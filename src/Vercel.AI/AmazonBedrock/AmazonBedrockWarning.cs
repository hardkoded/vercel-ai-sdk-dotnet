// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>A warning produced while preparing an Amazon Bedrock request.</summary>
public sealed class AmazonBedrockWarning
{
    /// <summary>Creates a warning.</summary>
    public AmazonBedrockWarning(string type, string feature, string? details)
    {
        Type = type ?? "unsupported";
        Feature = feature ?? string.Empty;
        Details = details;
    }

    /// <summary>Warning category, such as <c>unsupported</c> or <c>compatibility</c>.</summary>
    public string Type { get; }

    /// <summary>Feature that triggered the warning.</summary>
    public string Feature { get; }

    /// <summary>Human-readable detail, when the provider supplied one.</summary>
    public string? Details { get; }
}
