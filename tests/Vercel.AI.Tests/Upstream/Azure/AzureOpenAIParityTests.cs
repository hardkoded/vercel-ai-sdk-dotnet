// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class AzureOpenAIParityTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void An_empty_api_version_falls_back_to_v1_for_every_route(string? apiVersion)
    {
        var provider = AzureOpenAIProvider.Create(
            new AzureOpenAIOptions
            {
                ApiKey = "test-api-key",
                ResourceName = "test-resource",
                ApiVersion = apiVersion!,
            },
            new UpstreamRecordingHandler(ChatResponse)
        );
        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/chat/completions?api-version=v1",
            provider.ChatUri("d").AbsoluteUri
        );
        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/completions?api-version=v1",
            provider.CompletionsUri("d").AbsoluteUri
        );
        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/embeddings?api-version=v1",
            provider.EmbeddingsUri("d").AbsoluteUri
        );
    }

    [Fact]
    public async Task Embeddings_use_the_deployment_route_when_requested()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":2,\"total_tokens\":2}}"
        );
        var provider = AzureOpenAIProvider.Create(
            new AzureOpenAIOptions
            {
                ApiKey = "test-api-key",
                ResourceName = "test-resource",
                UseDeploymentBasedUrls = true,
            },
            handler
        );
        await provider
            .EmbeddingModel("my-embedding")
            .DoEmbedAsync(new[] { "sunny day at the beach" }, null, CancellationToken.None);
        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/deployments/my-embedding/embeddings?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    public async Task Deployment_routes_send_an_explicit_dated_api_version()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = AzureOpenAIProvider.Create(
            new AzureOpenAIOptions
            {
                ApiKey = "test-api-key",
                ResourceName = "test-resource",
                UseDeploymentBasedUrls = true,
                ApiVersion = "2024-10-21",
            },
            handler
        );
        await provider
            .ChatModel("test-deployment", null)
            .DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/deployments/test-deployment/chat/completions?api-version=2024-10-21",
            handler.Uri
        );
    }

    private const string ChatResponse =
        "{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}";

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }
}
