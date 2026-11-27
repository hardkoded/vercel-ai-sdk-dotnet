// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelOptions;

public sealed class TypeInferenceTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > type inference::";

    [Fact]
    [UpstreamTest(
        Prefix + "infers AmazonBedrockLanguageModelChatOptions type correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "C# has no inferred schema type. The test checks that serviceTier is read from an options object that also sets structuredOutputMode."
    )]
    public async Task Infers_AmazonBedrockLanguageModelChatOptions_type_correctly()
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
