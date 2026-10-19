// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.BlackForestLabsImageModel.BlackForestLabsImageModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>Port of <c>black-forest-labs-image-model.test.ts</c> &gt; <c>BlackForestLabsImageModel</c> &gt; <c>FLUX 3</c>.</summary>
public sealed class Flux3Tests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > FLUX 3::";
    private const string Body = "{\"prompt\":\"" + PromptText + "\"";

    private static ImageModelFile[] ReferenceImages()
    {
        return new[]
        {
            ImageModelFile.FromUrl("https://example.com/reference.png"),
            ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
            ImageModelFile.FromFile(Convert.FromBase64String("dGVzdA=="), "image/png"),
        };
    }

    [Fact]
    [UpstreamTest(Id + "leaves aspect ratio and resolution defaults to the endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task LeavesAspectRatioAndResolutionDefaultsToTheEndpoint()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("768sq")]
    [InlineData("1k")]
    [InlineData("1.5k")]
    [InlineData("2k")]
    [InlineData("4k")]
    [UpstreamTest(Id + "sends resolution %s and FLUX 3 provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsResolutionAndFlux3ProviderOptions(string resolution)
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(
            Call(aspectRatio: "16:9", blackForestLabs: "{\"resolution\":\"" + resolution + "\",\"grounding\":false,\"safetyTolerance\":7,\"version\":\"latest\"}"),
            CancellationToken.None);

        AssertJson(
            Body + ",\"aspect_ratio\":\"16:9\",\"resolution\":\"" + resolution + "\",\"grounding\":false,\"safety_tolerance\":7,\"version\":\"latest\"}",
            server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "sends URL and base64 reference images in the images array", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsUrlAndBase64ReferenceImagesInTheImagesArray()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(files: ReferenceImages()), CancellationToken.None);

        AssertJson(Body + ",\"images\":[\"https://example.com/reference.png\",\"AQID\",\"dGVzdA==\"]}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "derives aspect ratio from size without sending dimensions or seed", Coverage = UpstreamCoverage.Covered)]
    public async Task DerivesAspectRatioFromSizeWithoutSendingDimensionsOrSeed()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(size: "1536x1024", seed: 42), CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"3:2\"}", server.Calls[0].Body);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal(("unsupported", "size"), (result.Warnings[0].Type, result.Warnings[0].Feature));
        Assert.Equal(("unsupported", "seed", null), Warning(result.Warnings[1]));
    }

    [Fact]
    [UpstreamTest(Id + "prefers an explicit aspect ratio over size", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersAnExplicitAspectRatioOverSize()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(size: "1024x1024", aspectRatio: "16:9"), CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"16:9\"}", server.Calls[0].Body);
    }

    [Theory]
    [InlineData("1792x1024", "7:4")]
    [InlineData("1536x640", "12:5")]
    [UpstreamTest(Id + "omits unsupported aspect ratio $ratio derived from $size", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsUnsupportedAspectRatioDerivedFromSize(string size, string ratio)
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(size: size), CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Contains(
            result.Warnings,
            warning => Warning(warning) == ("unsupported", "aspectRatio", "FLUX 3 does not support aspect ratio " + ratio + ". Using the endpoint's default auto aspect ratio."));
    }

    [Fact]
    [UpstreamTest(Id + "omits unsupported explicit aspect ratios even when size has a supported ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsUnsupportedExplicitAspectRatiosEvenWhenSizeHasASupportedRatio()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(size: "1536x1024", aspectRatio: "3:1"), CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Contains(
            result.Warnings,
            warning => Warning(warning) == ("unsupported", "aspectRatio", "FLUX 3 does not support aspect ratio 3:1. Using the endpoint's default auto aspect ratio."));
    }

    [Theory]
    [InlineData("2520x1080", "21:9")]
    [InlineData("1080x2520", "9:21")]
    [UpstreamTest(Id + "uses the accepted aspect ratio $ratio for $size", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesTheAcceptedAspectRatioForSize(string size, string ratio)
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(size: size), CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"" + ratio + "\"}", server.Calls[0].Body);
    }

    [Theory]
    [InlineData("21:9")]
    [InlineData("2:1")]
    [InlineData("16:9")]
    [InlineData("3:2")]
    [InlineData("7:5")]
    [InlineData("4:3")]
    [InlineData("5:4")]
    [InlineData("1:1")]
    [InlineData("4:5")]
    [InlineData("3:4")]
    [InlineData("5:7")]
    [InlineData("2:3")]
    [InlineData("9:16")]
    [InlineData("1:2")]
    [InlineData("9:21")]
    [UpstreamTest(Id + "passes the supported aspect ratio %s", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesTheSupportedAspectRatio(string aspectRatio)
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(aspectRatio: aspectRatio), CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"" + aspectRatio + "\"}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "omits legacy endpoint fields and warns about unsupported options", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsLegacyEndpointFieldsAndWarnsAboutUnsupportedOptions()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, "flux-3-image").DoGenerateAsync(
            Call(blackForestLabs: "{\"width\":1024,\"height\":1024,\"outputFormat\":\"png\",\"steps\":20,\"guidance\":4,\"promptUpsampling\":true,\"raw\":false,\"imagePrompt\":\"dGVzdA==\",\"imagePromptStrength\":0.5,\"inputImage\":\"dGVzdA==\",\"webhookUrl\":\"https://example.com/webhook\",\"webhookSecret\":\"secret\"}"),
            CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Equal(12, result.Warnings.Count);
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.width", null));
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.raw", null));
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.inputImage", null));
    }

    [Theory]
    [InlineData("{\"safetyTolerance\":8}")]
    [InlineData("{\"safetyTolerance\":-1}")]
    [InlineData("{\"safetyTolerance\":1.5}")]
    [InlineData("{\"resolution\":\"8k\"}")]
    [InlineData("{\"version\":\"unknown\"}")]
    [UpstreamTest(Id + "rejects invalid provider options %j before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsInvalidProviderOptionsBeforeSubmission(string blackForestLabs)
    {
        var server = new BlackForestLabsServer();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(blackForestLabs: blackForestLabs), CancellationToken.None));

        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "preserves the legacy safety tolerance range", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesTheLegacySafetyToleranceRange()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server).DoGenerateAsync(Call(blackForestLabs: "{\"safetyTolerance\":6}"), CancellationToken.None);

        AssertJson(Body + ",\"safety_tolerance\":6}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "rejects masks before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsMasksBeforeSubmission()
    {
        var server = new BlackForestLabsServer();

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(mask: ImageModelFile.FromUrl("https://example.com/mask.png")), CancellationToken.None));

        Assert.Contains("FLUX 3 image masks", exception.Message);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "rejects more than ten reference images before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsMoreThanTenReferenceImagesBeforeSubmission()
    {
        var server = new BlackForestLabsServer();
        var files = Enumerable.Range(0, 11).Select(_ => ImageModelFile.FromUrl("https://example.com/image.png")).ToArray();

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(files: files), CancellationToken.None));

        Assert.Contains("Black Forest Labs supports up to 10 input images.", exception.Message);
        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "polls through Pending, Reasoning, and Generating until Ready", Coverage = UpstreamCoverage.Covered)]
    public async Task PollsThroughPendingReasoningAndGeneratingUntilReady()
    {
        var server = new BlackForestLabsServer();
        var statuses = new[] { "Pending", "Reasoning", "Generating", "Ready" };
        var pollCount = 0;
        server.Urls["https://api.example.com/poll"] = _ =>
        {
            var status = statuses[pollCount++];
            var body = status == "Ready"
                ? "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\",\"duration\":2}}"
                : "{\"status\":\"" + status + "\"}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        };

        var result = await CreateBasicModel(server, "flux-3-image", pollIntervalMillis: 1).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(4, pollCount);
        Assert.Single(result.Images);
        Assert.Equal(2, Metadata(result)!["duration"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("Request Moderated")]
    [InlineData("Content Moderated")]
    [InlineData("Task not found")]
    [UpstreamTest(Id + "stops polling on %s", Coverage = UpstreamCoverage.Covered)]
    public async Task StopsPollingOn(string status)
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"" + status + "\"}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains("Black Forest Labs generation failed: " + status + ".", exception.Message);
        Assert.Equal(2, server.Calls.Count);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("state")]
    [UpstreamTest(Id + "treats HTTP 503 with a terminal %s as non-retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task TreatsHttp503WithATerminalAsNonRetryable(string field)
    {
        var server = new BlackForestLabsServer();
        server.Respond("https://api.example.com/poll", 503, "{\"" + field + "\":\"Error\"}");

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal(503, exception.StatusCode);
        Assert.False(exception.IsRetryable);
        Assert.Equal("Black Forest Labs generation failed: Error.", exception.Message);
        Assert.Equal(2, server.Calls.Count);
    }

    [Fact]
    [UpstreamTest(Id + "keeps HTTP 503 without a terminal task status retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task KeepsHttp503WithoutATerminalTaskStatusRetryable()
    {
        var server = new BlackForestLabsServer();
        server.Respond("https://api.example.com/poll", 503, "{\"detail\":\"Service unavailable\"}");

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            CreateBasicModel(server, "flux-3-image").DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal(503, exception.StatusCode);
        Assert.True(exception.IsRetryable);
        Assert.Equal("Service unavailable", exception.Message);
    }
}
