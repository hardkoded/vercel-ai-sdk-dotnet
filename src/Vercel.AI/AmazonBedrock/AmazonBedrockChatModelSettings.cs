// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.AmazonBedrock;

/// <summary>The chat model family of a Bedrock model. Maps to <c>AmazonBedrockChatModelSettings['modelFamily']</c>.</summary>
public enum AmazonBedrockChatModelFamily
{
    /// <summary>An Anthropic model.</summary>
    Anthropic,
}

/// <summary>Settings for <see cref="AmazonBedrockLanguageModel"/>. Maps to <c>AmazonBedrockChatModelSettings</c>.</summary>
public sealed class AmazonBedrockChatModelSettings
{
    /// <summary>
    /// The chat model family. Set it to <see cref="AmazonBedrockChatModelFamily.Anthropic"/> when the model id does not
    /// name an Anthropic model, such as an application inference profile ARN. When unset, the family comes from the model id.
    /// </summary>
    public AmazonBedrockChatModelFamily? ModelFamily { get; set; }
}
