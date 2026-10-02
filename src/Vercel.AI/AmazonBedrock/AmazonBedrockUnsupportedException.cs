// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Bedrock cannot represent the requested prompt or tool content.</summary>
public sealed class AmazonBedrockUnsupportedException : InvalidOperationException
{
    /// <summary>Creates an exception for <paramref name="functionality"/>.</summary>
    public AmazonBedrockUnsupportedException(string functionality)
        : this(functionality, functionality)
    {
    }

    /// <summary>Creates an exception with an explicit message.</summary>
    public AmazonBedrockUnsupportedException(string functionality, string message)
        : base(message ?? functionality ?? "Unsupported functionality.")
    {
        Functionality = functionality ?? string.Empty;
    }

    /// <summary>The functionality Bedrock rejected.</summary>
    public string Functionality { get; }
}
