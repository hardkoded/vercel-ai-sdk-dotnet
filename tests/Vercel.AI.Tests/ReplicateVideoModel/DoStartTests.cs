// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ReplicateVideoModel.ReplicateVideoModelFixture;

namespace Vercel.AI.Tests.ReplicateVideoModel;

/// <summary>Port of <c>replicate-video-model.test.ts</c> &gt; <c>ReplicateVideoModel</c> &gt; <c>doStart</c>.</summary>
public sealed class DoStartTests
{
    private const string Id =
        "packages/replicate/src/replicate-video-model.test.ts::ReplicateVideoModel > doStart::";

    [Fact]
    [UpstreamTest(Id + "should return operation with getUrl", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldReturnOperationWithGetUrl()
    {
        var (model, _) = CreateModel(predictionId: "start-pred-123");

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Equal(
            "https://api.replicate.com/v1/predictions/start-pred-123",
            Operation(result)!["getUrl"]!.GetValue<string>()
        );
        Assert.Single(Operation(result)!.AsObject());
    }

    [Fact]
    [UpstreamTest(Id + "should pass correct request body", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassCorrectRequestBody()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should NOT send prefer header", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNOTSendPreferHeader()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.False(server.CreateCall!.Headers.ContainsKey("prefer"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should use correct URL for versioned models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseCorrectURLForVersionedModels()
    {
        var (model, server) = CreateModel(modelId: "stability-ai/stable-video-diffusion:abc123");

        await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Equal("https://api.replicate.com/v1/predictions", server.CreateCall!.Url);
        AssertMatches("{\"version\":\"abc123\"}", CapturedBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should use /models/ URL for unversioned models",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldUseModelsURLForUnversionedModels()
    {
        var (model, server) = CreateModel(modelId: "minimax/video-01");

        await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Equal(
            "https://api.replicate.com/v1/models/minimax/video-01/predictions",
            server.CreateCall!.Url
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should return warnings and response metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnWarningsAndResponseMetadata()
    {
        var testDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (model, _) = CreateModel(currentDate: () => testDate);

        var result = await model.DoStartAsync(Call(), CancellationToken.None);

        Assert.Empty(result.Warnings);
        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("minimax/video-01", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass webhookUrl as webhook body parameter",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassWebhookUrlAsWebhookBodyParameter()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(webhookUrl: "https://example.com/webhook"),
            CancellationToken.None
        );

        AssertMatches(
            "{\"webhook\":\"https://example.com/webhook\",\"webhook_events_filter\":[\"completed\"]}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should NOT include webhook when webhookUrl is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNOTIncludeWebhookWhenWebhookUrlIsNotProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(), CancellationToken.None);

        var body = CapturedBody(server).AsObject();
        Assert.False(body.ContainsKey("webhook"));
        Assert.False(body.ContainsKey("webhook_events_filter"));
    }

    [Fact]
    [UpstreamTest(Id + "should pass seed when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassSeedWhenProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(seed: 42), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"seed\":42}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass seed when it is 0",
        Coverage = UpstreamCoverage.Covered,
        Commit = "ef2dfb5e9fb7b72287b226eba8b743f809414172"
    )]
    public async Task ShouldPassSeedWhenItIs0()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(seed: 0), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"seed\":0}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass aspect ratio when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassAspectRatioWhenProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"aspect_ratio\":\"16:9\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through 9:16 aspect ratio",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassThrough916AspectRatio()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(aspectRatio: "9:16"), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"aspect_ratio\":\"9:16\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass through 1:1 aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassThrough11AspectRatio()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"aspect_ratio\":\"1:1\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through other aspect ratios",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassThroughOtherAspectRatios()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(aspectRatio: "4:3"), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"aspect_ratio\":\"4:3\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass resolution as size when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassResolutionAsSizeWhenProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(resolution: "1920x1080"), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"size\":\"1920x1080\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass duration when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassDurationWhenProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(duration: 5), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"duration\":5}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass fps when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassFpsWhenProvided()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(Call(fps: 30), CancellationToken.None);

        AssertMatches(
            "{\"input\":{\"prompt\":\"A rocket launching into space\",\"fps\":30}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should send URL-based image directly", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendURLBasedImageDirectly()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(image: VideoModelFile.FromUrl("https://example.com/image.png")),
            CancellationToken.None
        );

        Assert.Equal(
            "https://example.com/image.png",
            CapturedBody(server)["input"]!["image"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should convert base64 image to data URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldConvertBase64ImageToDataURI()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(image: VideoModelFile.FromBase64("base64-image-data", "image/png")),
            CancellationToken.None
        );

        Assert.Equal(
            "data:image/png;base64,base64-image-data",
            CapturedBody(server)["input"]!["image"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass guidance_scale option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassGuidanceScaleOption()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(replicate: "{\"guidance_scale\":7.5}"),
            CancellationToken.None
        );

        AssertMatches("{\"input\":{\"guidance_scale\":7.5}}", CapturedBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass num_inference_steps option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassNumInferenceStepsOption()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(replicate: "{\"num_inference_steps\":50}"),
            CancellationToken.None
        );

        AssertMatches("{\"input\":{\"num_inference_steps\":50}}", CapturedBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass motion_bucket_id for Stable Video Diffusion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassMotionBucketIdForStableVideoDiffusion()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(replicate: "{\"motion_bucket_id\":127}"),
            CancellationToken.None
        );

        AssertMatches("{\"input\":{\"motion_bucket_id\":127}}", CapturedBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass prompt_optimizer for MiniMax",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassPromptOptimizerForMiniMax()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(replicate: "{\"prompt_optimizer\":true}"),
            CancellationToken.None
        );

        AssertMatches("{\"input\":{\"prompt_optimizer\":true}}", CapturedBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should pass through custom options", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassThroughCustomOptions()
    {
        var (model, server) = CreateModel();

        await model.DoStartAsync(
            Call(replicate: "{\"custom_param\":\"custom_value\"}"),
            CancellationToken.None
        );

        AssertMatches("{\"input\":{\"custom_param\":\"custom_value\"}}", CapturedBody(server));
    }
}
