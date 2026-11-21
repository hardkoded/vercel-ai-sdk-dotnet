// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>Amazon Bedrock chat model family.</summary>
public enum AmazonBedrockChatModelFamily
{
    /// <summary>Anthropic Claude.</summary>
    Anthropic,
}

/// <summary>Settings for <see cref="AmazonBedrockLanguageModel"/>.</summary>
public sealed class AmazonBedrockChatModelSettings
{
    /// <summary>
    /// The chat model family.
    /// Set it when the model id does not name the underlying model, such as an application inference profile ARN.
    /// </summary>
    public AmazonBedrockChatModelFamily? ModelFamily { get; set; }
}
