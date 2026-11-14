// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiImageGenerateImage;

/// <summary>Port of <c>azure-mai-image-generate-image.test.ts</c> &gt; <c>Azure MAI-Image generateImage integration</c>.</summary>
public sealed class AzureMaiImageGenerateImageIntegrationTests
{
    private const string File = "packages/azure/src/azure-mai-image-generate-image.test.ts::Azure MAI-Image generateImage integration::";

    private static AzureImageHandler Handler()
    {
        return new AzureImageHandler(() => AzureImageHandler.JsonResponse(new
        {
            size = "1024x1024",
            usage = new { num_output_tokens = 1024, num_input_text_tokens = 5, num_input_image_tokens = 0 },
            data = new[] { new { b64_json = "iVBORw0KGgo=" } },
        }));
    }

    [Fact]
    [UpstreamTest(File + "returns n images for MAI-Image deployments", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsNImagesForMaiImageDeployments()
    {
        var handler = Handler();

        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = CreateProvider(handler).Image("MAI-Image-2.6"),
            Text = "A red apple",
            N = 2,
        });

        Assert.Equal(2, result.Images.Count);
        Assert.Equal(2, handler.Requests.Count);
    }

    // OpenAI-looking deployment names advertise a larger per-call limit, so core sends n > 1 in one call;
    // the MAI model must still return n images.
    [Fact]
    [UpstreamTest(File + "returns n images when api: mai overrides an OpenAI-looking deployment", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsNImagesWhenApiMaiOverridesAnOpenAILookingDeployment()
    {
        var handler = Handler();

        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = CreateProvider(handler).Image("gpt-image-production"),
            Text = "A red apple",
            N = 2,
            ProviderOptions = Json("{\"azure\":{\"api\":\"mai\"}}"),
        });

        Assert.Equal(2, result.Images.Count);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Equal(GenerationsUrl, request.Url));
        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(2048, result.Usage.OutputTokens);
    }
}
