// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.Tests.MoreProviders;
using AlibabaEmbeddingModelType = Vercel.AI.Alibaba.AlibabaEmbeddingModel;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaProvider;

/// <summary>Port of <c>alibaba-provider.test.ts</c> &gt; <c>AlibabaProvider</c> &gt; <c>embedding</c>.</summary>
public sealed class EmbeddingTests
{
    private const string Prefix =
        "packages/alibaba/src/alibaba-provider.test.ts::AlibabaProvider > embedding::";

    [Fact]
    [UpstreamTest(
        Prefix + "should construct an embedding model with correct provider",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_construct_an_embedding_model_with_correct_provider()
    {
        var provider = AlibabaProviderType.Create();
        var model = provider.Embedding("text-embedding-v4");

        Assert.IsType<AlibabaEmbeddingModelType>(model);
        Assert.Equal("text-embedding-v4", model.ModelId);
        Assert.Equal("alibaba.embedding", model.Config.Provider);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use default embeddingBaseURL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_default_embeddingBaseURL()
    {
        var provider = AlibabaProviderType.Create();
        var model = provider.Embedding("text-embedding-v4");

        Assert.Equal("https://dashscope-intl.aliyuncs.com/api/v1", model.Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom embeddingBaseURL",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_use_custom_embeddingBaseURL()
    {
        var provider = AlibabaProviderType.Create(
            new AlibabaProviderSettings
            {
                EmbeddingBaseUrl = "https://custom-embedding.example.com/api/v1",
            }
        );
        var model = provider.Embedding("text-embedding-v4");

        Assert.Equal("https://custom-embedding.example.com/api/v1", model.Config.BaseUrl);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass custom fetch to embedding model",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's fetch is the HttpClient the provider is created with."
    )]
    public void Should_pass_custom_fetch_to_embedding_model()
    {
        using var customFetch = new HttpClient(new ParityHandler());
        var provider = new AlibabaProviderType(customFetch);
        var model = provider.Embedding("text-embedding-v4");

        Assert.Same(customFetch, model.Config.HttpClient);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass headers function to embedding model",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks loadApiKey to return mock-api-key. The port has no such mock, so the bearer token is the test-key the provider gets."
    )]
    public async Task Should_pass_headers_function_to_embedding_model()
    {
        var options = new AlibabaProviderSettings { ApiKey = "test-key" };
        options.Headers["X-Custom"] = "value";
        var provider = AlibabaProviderType.Create(options);
        var model = provider.Embedding("text-embedding-v4");

        var headers = await model.Config.Headers(CancellationToken.None);

        Assert.Equal("Bearer test-key", headers["authorization"]);
        Assert.Equal("value", headers["x-custom"]);
    }
}
