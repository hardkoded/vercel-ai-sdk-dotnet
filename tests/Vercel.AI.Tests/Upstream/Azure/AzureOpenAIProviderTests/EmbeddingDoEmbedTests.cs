// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>embedding</c> &gt; <c>doEmbed</c>.</summary>
public sealed class EmbeddingDoEmbedTests
{
    private static readonly string[] TestValues =
    [
        "sunny day at the beach",
        "rainy day in the city",
    ];

    private const string EmbeddingResponse =
        "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"index\":0,\"embedding\":[0.1,0.2,0.3,0.4,0.5]},{\"object\":\"embedding\",\"index\":1,\"embedding\":[0.6,0.7,0.8,0.9,1.0]}],\"model\":\"my-embedding\",\"usage\":{\"prompt_tokens\":8,\"total_tokens\":8}}";

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::embedding > doEmbed::should set the correct api version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_the_correct_api_version()
    {
        var handler = new UpstreamRecordingHandler(EmbeddingResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        await provider
            .EmbeddingModel("my-embedding")
            .DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/embeddings?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::embedding > doEmbed::should pass headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_headers()
    {
        var handler = new UpstreamRecordingHandler(EmbeddingResponse);
        var options = new AzureOpenAIOptions
        {
            ApiKey = "test-api-key",
            ResourceName = "test-resource",
        };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";

        await AzureProvider
            .Create(options, handler)
            .EmbeddingModel("my-embedding")
            .DoEmbedAsync(
                TestValues,
                null,
                CancellationToken.None,
                headers: new Dictionary<string, string?>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            );

        AzureResponsesToolSupport.AssertRequestHeaders(
            handler.Headers,
            AzureResponsesToolSupport.ProviderAndRequestHeaders
        );
    }
}
