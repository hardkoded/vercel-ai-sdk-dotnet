// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelOptions;

public sealed class RequestMetadataTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-options.test.ts::amazonBedrockLanguageModelChatOptions > requestMetadata::";

    [Fact]
    [UpstreamTest(Prefix + "accepts a Record<string, string>", Coverage = UpstreamCoverage.Covered)]
    public async Task Accepts_a_Record_string_string()
    {
        var handler = new ScriptedHandler();
        var result = await CreateModel(handler)
            .DoGenerateAsync(
                Call("{\"requestMetadata\":{\"team\":\"search\",\"environment\":\"prod\"}}"),
                CancellationToken.None
            );

        Assert.Equal("ok", result.Text);
        using var document = JsonDocument.Parse(handler.Body);
        var metadata = document.RootElement.GetProperty("requestMetadata");
        Assert.Equal(2, metadata.EnumerateObject().Count());
        Assert.Equal("search", metadata.GetProperty("team").GetString());
        Assert.Equal("prod", metadata.GetProperty("environment").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "rejects non-string values in the record",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_non_string_values_in_the_record()
    {
        var handler = new ScriptedHandler();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateModel(handler)
                .DoGenerateAsync(
                    Call("{\"requestMetadata\":{\"team\":42}}"),
                    CancellationToken.None
                )
        );
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "is optional", Coverage = UpstreamCoverage.Covered)]
    public async Task Is_optional()
    {
        var handler = new ScriptedHandler();
        var result = await CreateModel(handler).DoGenerateAsync(Call("{}"), CancellationToken.None);

        Assert.Equal("ok", result.Text);
        using var document = JsonDocument.Parse(handler.Body);
        Assert.False(document.RootElement.TryGetProperty("requestMetadata", out _));
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
