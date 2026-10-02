// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Chat provider-option validation.</summary>
public sealed class BedrockChatOptionsTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > structuredOutputMode::accepts %s", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_structured_output_modes()
    {
        foreach (var mode in new[] { "outputFormat", "jsonTool", "auto" })
        {
            var settings = AmazonBedrockChatOptions.Parse(BedrockParity.Options("{\"amazon-bedrock\":{\"structuredOutputMode\":\"" + mode + "\"}}"));

            Assert.Equal(mode, settings.StructuredOutputMode);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > structuredOutputMode::rejects invalid values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_unknown_structured_output_mode()
    {
        Assert.Throws<ArgumentException>(() => AmazonBedrockChatOptions.Parse(BedrockParity.Options("{\"amazon-bedrock\":{\"structuredOutputMode\":\"native\"}}")));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > serviceTier::accepts valid service tier values", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_service_tiers()
    {
        foreach (var tier in new[] { "reserved", "priority", "default", "flex" })
        {
            var settings = AmazonBedrockChatOptions.Parse(BedrockParity.Options("{\"amazon-bedrock\":{\"serviceTier\":\"" + tier + "\"}}"));

            Assert.Equal(tier, settings.ServiceTier);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > serviceTier::rejects invalid service tier values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_service_tiers()
    {
        foreach (var tier in new[] { "on-demand", "auto", "standard", "", "PRIORITY" })
        {
            Assert.Throws<ArgumentException>(() => AmazonBedrockChatOptions.Parse(BedrockParity.Options("{\"amazon-bedrock\":{\"serviceTier\":\"" + tier + "\"}}")));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > type inference::infers AmazonBedrockLanguageModelChatOptions type correctly", Coverage = UpstreamCoverage.Covered)]
    public void Reads_service_tier_and_structured_output_mode_together()
    {
        var settings = AmazonBedrockChatOptions.Parse(BedrockParity.Options("{\"amazon-bedrock\":{\"serviceTier\":\"priority\",\"structuredOutputMode\":\"jsonTool\"}}"));

        Assert.Equal("priority", settings.ServiceTier);
        Assert.Equal("jsonTool", settings.StructuredOutputMode);
    }
}
