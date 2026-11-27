// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelOptions;

/// <summary>Port of <c>amazon-bedrock-chat-language-model-options.test.ts</c>, checked through the provider options a call accepts.</summary>
public sealed class AmazonBedrockLanguageModelChatOptionsTests
{
    private const string File =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > ";

    [Theory]
    [InlineData("outputFormat")]
    [InlineData("jsonTool")]
    [InlineData("auto")]
    [UpstreamTest(File + "structuredOutputMode::accepts %s", Coverage = UpstreamCoverage.Covered)]
    public async Task StructuredOutputMode_accepts(string mode)
    {
        var handler = new ScriptedHandler();
        var result = await CreateModel(handler)
            .DoGenerateAsync(
                Call("{\"structuredOutputMode\":\"" + mode + "\"}"),
                CancellationToken.None
            );

        Assert.Equal("ok", result.Text);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        File + "structuredOutputMode::rejects invalid values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task StructuredOutputMode_rejects_invalid_values()
    {
        var handler = new ScriptedHandler();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateModel(handler)
                .DoGenerateAsync(
                    Call("{\"structuredOutputMode\":\"native\"}"),
                    CancellationToken.None
                )
        );
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        File + "serviceTier::accepts valid service tier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ServiceTier_accepts_valid_service_tier_values()
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
        File + "serviceTier::rejects invalid service tier values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ServiceTier_rejects_invalid_service_tier_values()
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

    [Fact]
    [UpstreamTest(
        File + "type inference::infers AmazonBedrockLanguageModelChatOptions type correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "C# has no inferred schema type. The test checks that both options are read from one options object."
    )]
    public async Task Type_inference_reads_both_options()
    {
        var handler = new ScriptedHandler();
        await CreateModel(handler)
            .DoGenerateAsync(
                Call("{\"serviceTier\":\"priority\",\"structuredOutputMode\":\"jsonTool\"}"),
                CancellationToken.None
            );

        using var document = JsonDocument.Parse(handler.Body);
        Assert.Equal(
            "priority",
            document.RootElement.GetProperty("serviceTier").GetProperty("type").GetString()
        );
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
