// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock embedding model family. It selects the request format and the batch size.</summary>
public enum AmazonBedrockEmbeddingModelFamily
{
    /// <summary>Amazon Titan. One value per call.</summary>
    Titan,

    /// <summary>Cohere. Up to 96 values per call.</summary>
    Cohere,

    /// <summary>Amazon Nova. One value per call.</summary>
    Nova,
}

/// <summary>Settings for <see cref="AmazonBedrockEmbeddingModel"/>.</summary>
public sealed class AmazonBedrockEmbeddingModelSettings
{
    /// <summary>
    /// The embedding model family.
    /// Set it when the model id does not name the model, such as an application inference profile ARN.
    /// When unset, the family comes from the model id.
    /// </summary>
    public AmazonBedrockEmbeddingModelFamily? ModelFamily { get; set; }
}
