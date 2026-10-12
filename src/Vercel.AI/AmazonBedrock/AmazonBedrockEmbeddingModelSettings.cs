// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Settings for <see cref="AmazonBedrockEmbeddingModel"/>.</summary>
public sealed class AmazonBedrockEmbeddingModelSettings
{
    /// <summary>
    /// The embedding model family: <c>titan</c>, <c>cohere</c>, or <c>nova</c>.
    /// Set it when the model id does not name the model, such as an application inference profile ARN.
    /// When unset, the family comes from the model id.
    /// </summary>
    public string? ModelFamily { get; set; }
}
