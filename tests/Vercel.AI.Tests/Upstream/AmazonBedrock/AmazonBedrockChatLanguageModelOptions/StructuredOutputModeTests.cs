// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelOptions;

public sealed class StructuredOutputModeTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > structuredOutputMode::";

    [Theory]
    [InlineData("outputFormat")]
    [InlineData("jsonTool")]
    [InlineData("auto")]
    [UpstreamTest(
        Prefix + "accepts %s",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks that the mode is accepted. Without a schema the sent request does not show which mode was read."
    )]
    public async Task Accepts(string mode)
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

    [Theory]
    [InlineData("\"native\"")]
    [InlineData("42")]
    [InlineData("null")]
    [UpstreamTest(Prefix + "rejects invalid values", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_invalid_values(string json)
    {
        var handler = new ScriptedHandler();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateModel(handler)
                .DoGenerateAsync(
                    Call("{\"structuredOutputMode\":" + json + "}"),
                    CancellationToken.None
                )
        );
        Assert.Equal(0, handler.Calls);
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
