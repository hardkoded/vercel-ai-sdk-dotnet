// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel.BedrockImageTestSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockImageModel;

/// <summary>Port of <c>amazon-bedrock-image-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/amazon-bedrock-image-model.test.ts::doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the model and the settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_model_and_the_settings()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    size: "1024x1024",
                    seed: 1234,
                    providerOptions: "{\"bedrock\":{\"negativeText\":\"bad\",\"quality\":\"premium\",\"cfgScale\":1.2}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"cfgScale\":1.2,\"height\":1024,\"numberOfImages\":1,\"quality\":\"premium\",\"seed\":1234,\"width\":1024},\"taskType\":\"TEXT_IMAGE\",\"textToImageParams\":{\"negativeText\":\"bad\",\"text\":\"A cute baby sea otter\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass seed when it is 0",
        Coverage = UpstreamCoverage.Covered,
        Commit = "28d17e6fec07d892c27b930991bc4e50cead33b4"
    )]
    public async Task Should_pass_seed_when_it_is_0()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler)).DoGenerateAsync(Call(seed: 0), CancellationToken.None);

        var config = Body(handler)!["imageGenerationConfig"]!.AsObject();
        Assert.True(config.ContainsKey("seed"));
        Assert.Equal(0, config["seed"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept the new `amazonBedrock` providerOptions key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_the_new_amazonBedrock_providerOptions_key()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    size: "1024x1024",
                    seed: 1234,
                    providerOptions: "{\"amazonBedrock\":{\"negativeText\":\"bad\",\"quality\":\"premium\",\"cfgScale\":1.2,\"style\":\"PHOTOREALISM\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"cfgScale\":1.2,\"height\":1024,\"numberOfImages\":1,\"quality\":\"premium\",\"seed\":1234,\"width\":1024},\"taskType\":\"TEXT_IMAGE\",\"textToImageParams\":{\"negativeText\":\"bad\",\"style\":\"PHOTOREALISM\",\"text\":\"A cute baby sea otter\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer `amazonBedrock` providerOptions over the legacy `bedrock` key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_amazonBedrock_providerOptions_over_the_legacy_bedrock_key()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    providerOptions: "{\"bedrock\":{\"negativeText\":\"legacy\",\"quality\":\"standard\",\"cfgScale\":1,\"style\":\"DESIGN_SKETCH\"},\"amazonBedrock\":{\"negativeText\":\"preferred\",\"quality\":\"premium\",\"cfgScale\":2,\"style\":\"PHOTOREALISM\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"cfgScale\":2,\"numberOfImages\":1,\"quality\":\"premium\"},\"taskType\":\"TEXT_IMAGE\",\"textToImageParams\":{\"negativeText\":\"preferred\",\"style\":\"PHOTOREALISM\",\"text\":\"A cute baby sea otter\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject invalid provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_invalid_provider_options()
    {
        var model = Model(CreateProvider(new UpstreamRecordingHandler(ImagesBody)));

        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                Call(providerOptions: "{\"amazonBedrock\":{\"quality\":\"ultra\"}}"),
                CancellationToken.None
            )
        );

        Assert.Contains("invalid amazonBedrock provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should properly combine headers from all sources",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_properly_combine_headers_from_all_sources()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);
        var model = Model(CreateProvider(handler, options =>
            {
                options.Headers["model-header"] = "model-value";
                options.Headers["shared-header"] = "model-shared";
            }));

        await model.DoGenerateAsync(
            Call(
                headers: new Dictionary<string, string>
                {
                    ["options-header"] = "options-value",
                    ["shared-header"] = "options-shared",
                }
            ),
            CancellationToken.None
        );

        Assert.Equal("options-value", handler.Headers["options-header"]);
        Assert.Equal("model-value", handler.Headers["model-header"]);
        Assert.StartsWith("AWS4-HMAC-SHA256", handler.Headers["Authorization"]);
        Assert.Equal("options-shared", handler.Headers["shared-header"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should respect maxImagesPerCall setting",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_respect_maxImagesPerCall_setting()
    {
        var provider = CreateProvider(new UpstreamRecordingHandler(ImagesBody));

        Assert.Equal(5, provider.Image("amazon.nova-canvas-v1:0").MaxImagesPerCall);
        Assert.Equal(1, provider.Image("unknown-model").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return warnings for unsupported settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_warnings_for_unsupported_settings()
    {
        var result = await Model(CreateProvider(new UpstreamRecordingHandler(ImagesBody)))
            .DoGenerateAsync(Call(size: "1024x1024", aspectRatio: "1:1"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("aspectRatio", warning.Feature);
        Assert.Equal(
            "This model does not support aspect ratio. Use `size` instead.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract the generated images",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_the_generated_images()
    {
        var result = await Model(CreateProvider(new UpstreamRecordingHandler(ImagesBody)))
            .DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(new object?[] { "base64-image-1", "base64-image-2" }, result.Images);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_response_data_with_timestamp__modelId_and_headers()
    {
        var testDate = new DateTime(2024, 3, 15, 12, 0, 0, DateTimeKind.Utc);
        var model = Model(
            CreateProvider(new UpstreamRecordingHandler(ImagesBody)),
            currentDate: () => testDate
        );

        var result = await model.DoGenerateAsync(Call(size: "1024x1024"), CancellationToken.None);

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("amazon.nova-canvas-v1:0", result.Response.ModelId);
        Assert.StartsWith("application/json", result.Response.Headers!["content-type"]);
        Assert.Equal(
            System.Text.Encoding.UTF8.GetByteCount(ImagesBody).ToString(),
            result.Response.Headers["content-length"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_real_date_when_no_custom_date_provider_is_specified()
    {
        var before = DateTime.UtcNow;

        var result = await Model(CreateProvider(new UpstreamRecordingHandler(ImagesBody)))
            .DoGenerateAsync(Call(seed: 1234), CancellationToken.None);

        var after = DateTime.UtcNow;
        Assert.InRange(result.Response.Timestamp!.Value, before, after);
        Assert.Equal("amazon.nova-canvas-v1:0", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the style parameter when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_the_style_parameter_when_provided()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    size: "1024x1024",
                    seed: 1234,
                    providerOptions: "{\"bedrock\":{\"negativeText\":\"bad\",\"quality\":\"premium\",\"cfgScale\":1.2,\"style\":\"PHOTOREALISM\"}}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            Body(handler),
            "{\"imageGenerationConfig\":{\"cfgScale\":1.2,\"height\":1024,\"numberOfImages\":1,\"quality\":\"premium\",\"seed\":1234,\"width\":1024},\"taskType\":\"TEXT_IMAGE\",\"textToImageParams\":{\"negativeText\":\"bad\",\"style\":\"PHOTOREALISM\",\"text\":\"A cute baby sea otter\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include style parameter when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_style_parameter_when_not_provided()
    {
        var handler = new UpstreamRecordingHandler(ImagesBody);

        await Model(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    size: "1024x1024",
                    seed: 1234,
                    providerOptions: "{\"bedrock\":{\"quality\":\"standard\"}}"
                ),
                CancellationToken.None
            );

        Assert.False(Body(handler)!["textToImageParams"]!.AsObject().ContainsKey("style"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw error when request is moderated",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_error_when_request_is_moderated()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"id\":\"fe7256d1-50d9-4663-8592-85eaf002e80c\",\"status\":\"Request Moderated\",\"result\":null,\"progress\":null,\"details\":{\"Moderation Reasons\":[\"Derivative Works Filter\"]},\"preview\":null}"
        );

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            Model(CreateProvider(handler))
                .DoGenerateAsync(
                    Call(prompt: "Generate something that triggers moderation"),
                    CancellationToken.None
                )
        );

        Assert.Contains(
            "Amazon Bedrock request was moderated: Derivative Works Filter",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw error when no images are returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_error_when_no_images_are_returned()
    {
        var handler = new UpstreamRecordingHandler("{\"images\":[]}");

        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            Model(CreateProvider(handler))
                .DoGenerateAsync(Call(prompt: "Generate an image"), CancellationToken.None)
        );

        Assert.Contains("Amazon Bedrock returned no images", error.Message);
    }
}
