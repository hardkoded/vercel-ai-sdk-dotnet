// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests.Upstream.Azure.AzureModelId;

/// <summary>Port of <c>azure-model-id.test.ts</c> &gt; <c>separate Azure deployment and model IDs</c>.</summary>
public sealed class SeparateAzureDeploymentAndModelIdsTests
{
    private const string DeploymentId = "production-chat";
    private const string ModelId = "gpt-5";

    private static AzureOpenAIProvider CreateProvider(HttpMessageHandler handler)
    {
        return AzureOpenAIProvider.Create(new AzureOpenAIOptions { ResourceName = "test-resource", ApiKey = "test-api-key" }, handler);
    }

    private static ILanguageModel CreateModel(AzureOpenAIProvider provider, string factory, AzureOpenAILanguageModelSettings? settings)
    {
        return factory switch
        {
            "default" or "languageModel" => provider.LanguageModel(DeploymentId, settings),
            "responses" => provider.ResponsesModel(DeploymentId, settings),
            "chat" => provider.ChatModel(DeploymentId, settings),
            _ => provider.CompletionModel(DeploymentId, settings),
        };
    }

    [Theory]
    [InlineData("default")]
    [InlineData("languageModel")]
    [InlineData("responses")]
    [InlineData("chat")]
    [InlineData("completion")]
    [UpstreamTest("packages/azure/src/azure-model-id.test.ts::separate Azure deployment and model IDs::%s reports the supplied model ID and falls back to the deployment ID", Coverage = UpstreamCoverage.Covered)]
    public void ReportsTheSuppliedModelIdAndFallsBackToTheDeploymentId(string factory)
    {
        var provider = CreateProvider(new UpstreamRecordingHandler("{}"));

        Assert.Equal(ModelId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings { ModelId = ModelId }).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings()).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, new AzureOpenAILanguageModelSettings { ModelId = null }).ModelId);
        Assert.Equal(DeploymentId, CreateModel(provider, factory, null).ModelId);
    }
}
