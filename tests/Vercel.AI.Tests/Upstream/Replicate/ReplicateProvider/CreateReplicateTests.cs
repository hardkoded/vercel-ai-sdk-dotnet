// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.Upstream.Replicate.ReplicateImageModel.ReplicateImageModelSupport;
using ImageModel = Vercel.AI.Replicate.ReplicateImageModel;
using ReplicateApiProvider = Vercel.AI.Replicate.ReplicateProvider;

namespace Vercel.AI.Tests.Upstream.Replicate.ReplicateProvider;

/// <summary>Port of <c>replicate-provider.test.ts</c> &gt; <c>createReplicate</c>.</summary>
public sealed class CreateReplicateTests
{
    private const string Prefix = "packages/replicate/src/replicate-provider.test.ts::createReplicate::";

    [Fact]
    [UpstreamTest(Prefix + "creates a provider with required settings", Coverage = UpstreamCoverage.Covered)]
    public void Provider_with_a_token_creates_image_models()
    {
        Assert.NotNull(ReplicateApiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token" }).ImageModel("black-forest-labs/flux-schnell"));
    }

    [Fact]
    [UpstreamTest(Prefix + "creates a provider with custom settings", Coverage = UpstreamCoverage.Covered)]
    public void Provider_with_a_custom_base_url_creates_image_models()
    {
        var provider = ReplicateApiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token", BaseUrl = "https://custom.replicate.com" });
        Assert.NotNull(provider.ImageModel("black-forest-labs/flux-schnell"));
        Assert.Equal("https://custom.replicate.com", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects an empty baseURL during provider creation", Coverage = UpstreamCoverage.Covered)]
    public void Provider_rejects_an_empty_base_url()
    {
        var error = Assert.Throws<ArgumentException>(() => ReplicateApiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token", BaseUrl = string.Empty }));
        Assert.Equal("baseURL", error.ParamName);
        Assert.StartsWith("baseURL must be a non-empty string.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "creates an image model instance", Coverage = UpstreamCoverage.Covered)]
    public void Provider_image_models_are_replicate_image_models()
    {
        Assert.IsType<ImageModel>(ReplicateApiProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-token" }).ImageModel("black-forest-labs/flux-schnell"));
    }
}
