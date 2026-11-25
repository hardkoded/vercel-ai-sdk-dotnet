// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel.BedrockImageTestSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel;

/// <summary>Port of <c>amazon-bedrock-image-model.test.ts</c> &gt; <c>Image Editing</c>.</summary>
public sealed class ImageEditingTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-image-model.test.ts::Image Editing::";

    private const string EditedBody = "{\"images\":[\"edited-image-base64\"]}";

    [Fact]
    [UpstreamTest(
        Prefix + "should send inpainting request with files and maskPrompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_inpainting_request_with_files_and_maskPrompt()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "a cute corgi dog",
                    files: new[] { File(PngBytes) },
                    seed: 42,
                    providerOptions: "{\"bedrock\":{\"maskPrompt\":\"cat\",\"quality\":\"standard\",\"cfgScale\":7.0}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"cfgScale\":7,\"numberOfImages\":1,\"quality\":\"standard\",\"seed\":42},\"inPaintingParams\":{\"image\":\"iVBORw==\",\"maskPrompt\":\"cat\",\"text\":\"a cute corgi dog\"},\"taskType\":\"INPAINTING\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send inpainting request with files and mask image",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_inpainting_request_with_files_and_mask_image()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "A sunlit indoor lounge area with a pool containing a flamingo",
                    files: new[] { File(PngBytes) },
                    mask: File(MaskBytes),
                    providerOptions: "{\"bedrock\":{\"quality\":\"standard\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"numberOfImages\":1,\"quality\":\"standard\"},\"inPaintingParams\":{\"image\":\"iVBORw==\",\"maskImage\":\"////AA==\",\"text\":\"A sunlit indoor lounge area with a pool containing a flamingo\"},\"taskType\":\"INPAINTING\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send inpainting request with base64 string data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_inpainting_request_with_base64_string_data()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);
        const string base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk";

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Edit this image",
                    files: new[] { ImageModelFile.FromBase64(base64Image, "image/png") },
                    providerOptions: "{\"bedrock\":{\"maskPrompt\":\"background\"}}"
                ),
                CancellationToken.None
            );

        var body = Body(handler)!;
        Assert.Equal("INPAINTING", (string?)body["taskType"]);
        Assert.Equal(base64Image, (string?)body["inPaintingParams"]!["image"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include negativeText in inpainting params",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_negativeText_in_inpainting_params()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "a beautiful garden",
                    files: new[] { File(PngBytes) },
                    providerOptions: "{\"bedrock\":{\"maskPrompt\":\"sky\",\"negativeText\":\"clouds, rain\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"numberOfImages\":1},\"inPaintingParams\":{\"image\":\"iVBORw==\",\"maskPrompt\":\"sky\",\"negativeText\":\"clouds, rain\",\"text\":\"a beautiful garden\"},\"taskType\":\"INPAINTING\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract edited images from response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_edited_images_from_response()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        var result = await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Edit this image",
                    files: new[] { File(PngBytes) },
                    providerOptions: "{\"bedrock\":{\"maskPrompt\":\"object\"}}"
                ),
                CancellationToken.None
            );

        Assert.Equal(new object?[] { "edited-image-base64" }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw error for URL-based images",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_error_for_URL_based_images()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            Model(CreateProvider(handler))
                .DoGenerateAsync(
                    Call(
                        prompt: "Edit this image",
                        files: new[] { ImageModelFile.FromUrl("https://example.com/image.png") }
                    ),
                    CancellationToken.None
                )
        );

        Assert.Contains(
            "URL-based images are not supported for Amazon Bedrock image editing.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send outpainting request with taskType OUTPAINTING",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_outpainting_request_with_taskType_OUTPAINTING()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Extend the background with a beautiful sunset",
                    files: new[] { File(PngBytes) },
                    mask: File(MaskBytes),
                    providerOptions: "{\"bedrock\":{\"taskType\":\"OUTPAINTING\",\"outPaintingMode\":\"DEFAULT\",\"negativeText\":\"bad quality\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"numberOfImages\":1},\"outPaintingParams\":{\"image\":\"iVBORw==\",\"maskImage\":\"////AA==\",\"negativeText\":\"bad quality\",\"outPaintingMode\":\"DEFAULT\",\"text\":\"Extend the background with a beautiful sunset\"},\"taskType\":\"OUTPAINTING\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send outpainting request with maskPrompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_outpainting_request_with_maskPrompt()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Replace the background with mountains",
                    files: new[] { File(PngBytes) },
                    providerOptions: "{\"bedrock\":{\"taskType\":\"OUTPAINTING\",\"maskPrompt\":\"background\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"numberOfImages\":1},\"outPaintingParams\":{\"image\":\"iVBORw==\",\"maskPrompt\":\"background\",\"text\":\"Replace the background with mountains\"},\"taskType\":\"OUTPAINTING\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send background removal request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_background_removal_request()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: null,
                    files: new[] { File(PngBytes) },
                    providerOptions: "{\"bedrock\":{\"taskType\":\"BACKGROUND_REMOVAL\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"backgroundRemovalParams\":{\"image\":\"iVBORw==\"},\"taskType\":\"BACKGROUND_REMOVAL\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send image variation request with single image",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_image_variation_request_with_single_image()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Create a variation in anime style",
                    files: new[] { File(PngBytes) },
                    n: 3,
                    size: "512x512",
                    providerOptions: "{\"bedrock\":{\"taskType\":\"IMAGE_VARIATION\",\"similarityStrength\":0.7,\"negativeText\":\"bad quality, low resolution\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"height\":512,\"numberOfImages\":3,\"width\":512},\"imageVariationParams\":{\"images\":[\"iVBORw==\"],\"negativeText\":\"bad quality, low resolution\",\"similarityStrength\":0.7,\"text\":\"Create a variation in anime style\"},\"taskType\":\"IMAGE_VARIATION\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send image variation request with multiple images",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_image_variation_request_with_multiple_images()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);
        byte[] image2 = { 255, 216, 255, 224 };

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Combine these images into one cohesive scene",
                    files: new[] { File(PngBytes), File(image2, "image/jpeg") },
                    providerOptions: "{\"bedrock\":{\"taskType\":\"IMAGE_VARIATION\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"numberOfImages\":1},\"imageVariationParams\":{\"images\":[\"iVBORw==\",\"/9j/4A==\"],\"text\":\"Combine these images into one cohesive scene\"},\"taskType\":\"IMAGE_VARIATION\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default to IMAGE_VARIATION when files provided without mask or maskPrompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_to_IMAGE_VARIATION_when_files_provided_without_mask_or_maskPrompt()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(prompt: "Create variations", files: new[] { File(PngBytes) }),
                CancellationToken.None
            );

        Assert.Equal("IMAGE_VARIATION", (string?)Body(handler)!["taskType"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default to INPAINTING when files provided with mask",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_to_INPAINTING_when_files_provided_with_mask()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Edit masked area",
                    files: new[] { File(PngBytes) },
                    mask: File(MaskBytes)
                ),
                CancellationToken.None
            );

        Assert.Equal("INPAINTING", (string?)Body(handler)!["taskType"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default to INPAINTING when files provided with maskPrompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_to_INPAINTING_when_files_provided_with_maskPrompt()
    {
        var handler = new UpstreamRecordingHandler(EditedBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    prompt: "Edit the cat",
                    files: new[] { File(PngBytes) },
                    providerOptions: "{\"bedrock\":{\"maskPrompt\":\"cat\"}}"
                ),
                CancellationToken.None
            );

        Assert.Equal("INPAINTING", (string?)Body(handler)!["taskType"]);
    }
}
