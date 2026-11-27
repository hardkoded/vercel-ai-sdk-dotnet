// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>image</c> &gt; <c>imageModel method</c>.</summary>
public sealed class ImageImageModelMethodTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > imageModel method::should create the same model as image method",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_create_the_same_model_as_image_method()
    {
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }
        );

        var imageModel = provider.ImageModel("dalle-deployment");
        var imageModelAlias = provider.ImageModel("dalle-deployment");

        Assert.Equal(imageModel.Provider, imageModelAlias.Provider);
        Assert.Equal(imageModel.ModelId, imageModelAlias.ModelId);
    }
}
