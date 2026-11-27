// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelOptions;

public sealed class ServiceTierTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > serviceTier::";

    [Fact]
    [UpstreamTest(
        Prefix + "accepts valid service tier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Accepts_valid_service_tier_values()
    {
        foreach (var value in new[] { "reserved", "priority", "default", "flex" })
        {
            var handler = new ScriptedHandler();
            await CreateModel(handler)
                .DoGenerateAsync(
                    Call("{\"serviceTier\":\"" + value + "\"}"),
                    CancellationToken.None
                );

            using var document = JsonDocument.Parse(handler.Body);
            Assert.Equal(
                value,
                document.RootElement.GetProperty("serviceTier").GetProperty("type").GetString()
            );
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects invalid service tier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_invalid_service_tier_values()
    {
        foreach (var value in new[] { "on-demand", "auto", "standard", "", "PRIORITY" })
        {
            var handler = new ScriptedHandler();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateModel(handler)
                    .DoGenerateAsync(
                        Call("{\"serviceTier\":\"" + value + "\"}"),
                        CancellationToken.None
                    )
            );
            Assert.Equal(0, handler.Calls);
        }
    }

    private static AmazonBedrockLanguageModel CreateModel(HttpMessageHandler handler)
    {
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(
            new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" },
            handler
        );
        return (AmazonBedrockLanguageModel)provider.LanguageModel("anthropic.claude-3-haiku");
    }

    private static LanguageModelCallOptions Call(string bedrockOptionsJson)
    {
        using var document = JsonDocument.Parse(bedrockOptionsJson);
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("hi") },
            ProviderOptions = new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                ["amazon-bedrock"] = document.RootElement.Clone(),
            },
        };
    }
}
