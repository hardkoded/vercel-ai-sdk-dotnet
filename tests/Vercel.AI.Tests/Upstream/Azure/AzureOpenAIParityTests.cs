// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AzureOpenAIParityTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should set the correct default api version",
        Coverage = UpstreamCoverage.Partial,
        Note = "Deployment chat URLs keep api-version 2024-10-21. Upstream chat calls send v1.")]
    public async Task Chat_uses_the_deployment_api_version()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}");
        var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, handler);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Contains("api-version=2024-10-21", handler.Uri);
        Assert.Contains("openai/deployments/test-deployment/chat/completions", handler.Uri);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should set the correct modified api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_sends_a_modified_api_version()
    {
        var uri = await Chat("2025-04-01-preview");
        Assert.Contains("api-version=2025-04-01-preview", uri);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should set the correct modified api version", Coverage = UpstreamCoverage.Covered)]
    public async Task Default_language_model_sends_a_modified_api_version()
    {
        var uri = await Chat("2025-04-01-preview");
        Assert.Contains("api-version=2025-04-01-preview", uri);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should set the correct default api version",
        Coverage = UpstreamCoverage.Partial,
        Note = "The deployment route sends api-version 2024-10-21. Upstream responses calls send v1.")]
    public async Task Default_language_model_uses_the_deployment_api_version()
    {
        var uri = await Chat("2024-10-21");
        Assert.Contains("api-version=2024-10-21", uri);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::chat > doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Partial,
        Note = "Provider headers and the api-key header are sent. Per-call headers and the ai-sdk user-agent suffix are not forwarded by the shared chat client.")]
    public async Task Chat_sends_provider_headers()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}");
        var options = new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = AzureOpenAIProvider.Create(options, handler);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("test-api-key", handler.Headers["api-key"]);
        Assert.Equal("provider-header-value", handler.Headers["Custom-Provider-Header"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should use tokenProvider for Microsoft Entra ID auth",
        Coverage = UpstreamCoverage.Partial,
        Note = "Authorization is Bearer test-azure-ad-token and api-key is absent. The ai-sdk user-agent suffix is not sent.")]
    public async Task Uses_the_entra_token()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}");
        var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions
        {
            ResourceName = "test-resource",
            TokenProvider = () => "test-azure-ad-token",
        }, handler);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("Bearer test-azure-ad-token", handler.Headers["Authorization"]);
        Assert.False(handler.Headers.ContainsKey("api-key"));
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should call tokenProvider for every request", Coverage = UpstreamCoverage.Covered)]
    public async Task Calls_the_token_provider_for_every_request()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}");
        var count = 0;
        var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions
        {
            ResourceName = "test-resource",
            TokenProvider = () => "token-" + (++count),
        }, handler);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("Bearer token-1", handler.Headers["Authorization"]);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal(2, count);
        Assert.Equal("Bearer token-2", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/azure/src/azure-openai-provider.test.ts::responses (default language model) > doGenerate::should reject explicit apiKey with tokenProvider", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_api_key_combined_with_a_token_provider()
    {
        var exception = Assert.Throws<ArgumentException>(() => AzureOpenAIProvider.Create(new AzureOpenAIOptions
        {
            ResourceName = "test-resource",
            ApiKey = "test-api-key",
            TokenProvider = () => "test-azure-ad-token",
        }));
        Assert.Equal("Both apiKey and tokenProvider were provided. Please use only one authentication method.", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::embedding > doEmbed::should set the correct api version",
        Coverage = UpstreamCoverage.Partial,
        Note = "Embedding calls use the deployments route and api-version 2024-10-21. Upstream sends v1.")]
    public async Task Embeddings_use_the_deployment_api_version()
    {
        var handler = new UpstreamRecordingHandler("{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":2,\"total_tokens\":2}}");
        var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, handler);
        await provider.EmbeddingModel("my-embedding").DoEmbedAsync(new[] { "sunny day at the beach" }, null, CancellationToken.None);
        Assert.Contains("api-version=2024-10-21", handler.Uri);
        Assert.Contains("openai/deployments/my-embedding/embeddings", handler.Uri);
    }

    private static async Task<string> Chat(string apiVersion)
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}");
        var provider = AzureOpenAIProvider.Create(new AzureOpenAIOptions
        {
            ApiKey = "test-api-key",
            ResourceName = "test-resource",
            ApiVersion = apiVersion,
        }, handler);
        await provider.LanguageModel("test-deployment").DoGenerateAsync(Prompt(), CancellationToken.None);
        return handler.Uri;
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }
}
