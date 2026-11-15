// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>
/// Port of <c>azure-openai-provider.test.ts</c> &gt; <c>chat</c> &gt; <c>doGenerate</c>. The other tests of this
/// describe are ported in <c>AzureOpenAIParityTests</c>.
/// </summary>
public sealed class ChatDoGenerateTests
{
    private const string ChatResponse = "{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}";

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should accept options under the azure key", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_accept_options_under_the_azure_key()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, handler);

        await provider.ChatModel("test-deployment", null).DoGenerateAsync(
            Call(new Dictionary<string, JsonElement> { ["azure"] = Parse("{\"user\":\"azure-user\"}") }),
            CancellationToken.None);

        Assert.Equal("azure-user", JsonNode.Parse(handler.Body)!["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should merge openai and azure options, preferring azure", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_merge_openai_and_azure_options_preferring_azure()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, handler);

        await provider.ChatModel("test-deployment", null).DoGenerateAsync(
            Call(new Dictionary<string, JsonElement>
            {
                ["openai"] = Parse("{\"user\":\"openai-user\",\"logitBias\":{\"50256\":-100}}"),
                ["azure"] = Parse("{\"user\":\"azure-user\"}"),
            }),
            CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("azure-user", body["user"]!.GetValue<string>());
        Assert.Equal(-100, body["logit_bias"]!["50256"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should use the baseURL correctly", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_use_the_baseURL_correctly()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", BaseUrl = "https://test-resource.openai.azure.com/openai" }, handler);

        await provider.ChatModel("test-deployment", null).DoGenerateAsync(Call(new Dictionary<string, JsonElement>()), CancellationToken.None);

        Assert.Equal("https://test-resource.openai.azure.com/openai/v1/chat/completions?api-version=v1", handler.Uri);
    }

    [Theory]
    [InlineData("complete Azure OpenAI v1", "https://test-resource.openai.azure.com/openai/v1", "https://test-resource.openai.azure.com/openai/v1/chat/completions")]
    [InlineData("unversioned Foundry", "https://test-resource.services.ai.azure.com/openai", "https://test-resource.services.ai.azure.com/openai/v1/chat/completions?api-version=v1")]
    [InlineData("complete Foundry v1", "https://test-resource.services.ai.azure.com/openai/v1/", "https://test-resource.services.ai.azure.com/openai/v1/chat/completions")]
    [InlineData("unversioned Cognitive Services", "https://test-resource.cognitiveservices.azure.com/openai", "https://test-resource.cognitiveservices.azure.com/openai/v1/chat/completions?api-version=v1")]
    [InlineData("complete Cognitive Services v1", "https://test-resource.cognitiveservices.azure.com/openai/v1", "https://test-resource.cognitiveservices.azure.com/openai/v1/chat/completions")]
    [InlineData("unversioned Foundry project", "https://test-resource.services.ai.azure.com/api/projects/test-project/openai", "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1/chat/completions")]
    [InlineData("complete Foundry project v1", "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1", "https://test-resource.services.ai.azure.com/api/projects/test-project/openai/v1/chat/completions")]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should use $name baseURL correctly", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_use_name_baseURL_correctly(string name, string baseUrl, string expectedUrl)
    {
        Assert.NotEmpty(name);
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", BaseUrl = baseUrl }, handler);

        await provider.ChatModel("test-deployment", null).DoGenerateAsync(Call(new Dictionary<string, JsonElement>()), CancellationToken.None);

        Assert.Equal(expectedUrl, handler.Uri);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should use custom gateway baseURL as-is", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Should_use_custom_gateway_baseURL_as_is()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", BaseUrl = "https://our-gateway.example.com/azure" }, handler);

        await provider.ChatModel("test-deployment", null).DoGenerateAsync(Call(new Dictionary<string, JsonElement>()), CancellationToken.None);

        Assert.Equal("https://our-gateway.example.com/azure/chat/completions", handler.Uri);
    }

    private static LanguageModelCallOptions Call(IReadOnlyDictionary<string, JsonElement> providerOptions)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            ProviderOptions = providerOptions,
        };
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
