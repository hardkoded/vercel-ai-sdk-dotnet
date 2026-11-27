// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>image</c> &gt; <c>image editing capabilities</c>.</summary>
public sealed class ImageImageEditingCapabilitiesTests
{
    [Theory]
    [InlineData("image")]
    [InlineData("imageModel")]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::image > image editing capabilities::leaves capabilities unknown for arbitrary deployment names created with %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Leaves_capabilities_unknown_for_arbitrary_deployment_names_created_with_factoryMethod(
        string factoryMethod
    )
    {
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }
        );

        foreach (var deploymentName in new[] { "gpt-image-production", "dall-e-3" })
        {
            var model = (AzureImageModel)(
                factoryMethod == "image"
                    ? provider.Image(deploymentName)
                    : provider.ImageModel(deploymentName)
            );

            Assert.Null(model.SupportsFileInputs);
            Assert.Null(model.SupportsMaskInputs);
        }
    }
}
