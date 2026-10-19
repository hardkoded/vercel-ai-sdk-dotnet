// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.BlackForestLabsImageModel.BlackForestLabsImageModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>Port of <c>black-forest-labs-image-model.test.ts</c> &gt; <c>BlackForestLabsImageModel</c> &gt; <c>FLUX 3 Klein</c>.</summary>
public sealed class Flux3KleinTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > FLUX 3 Klein::";
    private const string ModelId = "flux-3-image-klein";
    private const string Body = "{\"prompt\":\"" + PromptText + "\"";

    [Fact]
    [UpstreamTest(Id + "leaves resolution and aspect ratio defaults to the endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task LeavesResolutionAndAspectRatioDefaultsToTheEndpoint()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("768sq")]
    [InlineData("1k")]
    [InlineData("1.5k")]
    [InlineData("2k")]
    [InlineData("4k")]
    [UpstreamTest(Id + "sends resolution %s without explicit dimensions", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsResolutionWithoutExplicitDimensions(string resolution)
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(
            Call(aspectRatio: "16:9", blackForestLabs: "{\"resolution\":\"" + resolution + "\"}"),
            CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"16:9\",\"resolution\":\"" + resolution + "\"}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "sends URL, binary and base64 references in the images array", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsUrlBinaryAndBase64ReferencesInTheImagesArray()
    {
        var server = new BlackForestLabsServer();
        var files = new[]
        {
            ImageModelFile.FromUrl("https://example.com/reference.png"),
            ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
            ImageModelFile.FromFile(Convert.FromBase64String("dGVzdA=="), "image/png"),
        };

        await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(files: files), CancellationToken.None);

        AssertJson(Body + ",\"images\":[\"https://example.com/reference.png\",\"AQID\",\"dGVzdA==\"]}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "preserves seed, output format, steps and safety tolerance", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesSeedOutputFormatStepsAndSafetyTolerance()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(
            Call(seed: 42, blackForestLabs: "{\"outputFormat\":\"png\",\"steps\":16,\"safetyTolerance\":7}"),
            CancellationToken.None);

        AssertJson(Body + ",\"seed\":42,\"output_format\":\"png\",\"steps\":16,\"safety_tolerance\":7}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("768x768")]
    [InlineData("1024x1024")]
    [InlineData("1536x1536")]
    [InlineData("2048x2048")]
    [InlineData("4096x4096")]
    [UpstreamTest(Id + "sends exact dimensions for size %s", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsExactDimensionsForSize(string size)
    {
        var server = new BlackForestLabsServer();
        var parts = size.Split('x');

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(size: size), CancellationToken.None);

        AssertJson(Body + ",\"width\":" + parts[0] + ",\"height\":" + parts[1] + "}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "prefers exact dimensions over aspect ratio and resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersExactDimensionsOverAspectRatioAndResolution()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(
            Call(size: "1536x640", aspectRatio: "3:1", blackForestLabs: "{\"resolution\":\"4k\"}"),
            CancellationToken.None);

        AssertJson(Body + ",\"width\":1536,\"height\":640}", server.Calls[0].Body);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "prefers provider dimensions over size and accepts a wide 4K image", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersProviderDimensionsOverSizeAndAcceptsAWide4KImage()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, ModelId).DoGenerateAsync(
            Call(size: "1024x1024", blackForestLabs: "{\"width\":6256,\"height\":2688}"),
            CancellationToken.None);

        AssertJson(Body + ",\"width\":6256,\"height\":2688}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "accepts the minimum dimensions and fills the other dimension from size", Coverage = UpstreamCoverage.Covered)]
    public async Task AcceptsTheMinimumDimensionsAndFillsTheOtherDimensionFromSize()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(size: "1024x64", blackForestLabs: "{\"width\":64}"), CancellationToken.None);

        AssertJson(Body + ",\"width\":64,\"height\":64}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "normalizes reduced ultrawide aspect ratios", Coverage = UpstreamCoverage.Covered)]
    public async Task NormalizesReducedUltrawideAspectRatios()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(aspectRatio: "7:3"), CancellationToken.None);

        AssertJson(Body + ",\"aspect_ratio\":\"21:9\"}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "omits unsupported aspect ratios with a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsUnsupportedAspectRatiosWithAWarning()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(Call(aspectRatio: "3:1"), CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Id + "omits base-model and legacy options with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsBaseModelAndLegacyOptionsWithWarnings()
    {
        var server = new BlackForestLabsServer();

        var result = await CreateBasicModel(server, ModelId).DoGenerateAsync(
            Call(blackForestLabs: "{\"grounding\":false,\"version\":\"latest\",\"guidance\":4,\"inputImage\":\"dGVzdA==\"}"),
            CancellationToken.None);

        AssertJson(Body + "}", server.Calls[0].Body);
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.grounding", null));
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.version", null));
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.guidance", null));
        Assert.Contains(result.Warnings, warning => Warning(warning) == ("unsupported", "blackForestLabs.inputImage", null));
        Assert.Equal(4, result.Warnings.Count);
    }

    [Theory]
    [InlineData("{\"width\":63,\"height\":64}")]
    [InlineData("{\"width\":64,\"height\":6257}")]
    [InlineData("{\"width\":6256,\"height\":6256}")]
    [InlineData("{\"width\":1024}")]
    [InlineData("{\"height\":1024}")]
    [InlineData("{\"steps\":0}")]
    [InlineData("{\"steps\":17}")]
    [InlineData("{\"safetyTolerance\":8}")]
    [InlineData("{\"resolution\":\"8k\"}")]
    [UpstreamTest(Id + "rejects invalid provider options %j before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsInvalidProviderOptionsBeforeSubmission(string blackForestLabs)
    {
        var server = new BlackForestLabsServer();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, ModelId).DoGenerateAsync(Call(blackForestLabs: blackForestLabs), CancellationToken.None));

        Assert.Empty(server.Calls);
    }

    [Theory]
    [InlineData("63x64")]
    [InlineData("6257x64")]
    [InlineData("4096x4108")]
    [UpstreamTest(Id + "rejects unsupported size %s before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsUnsupportedSizeBeforeSubmission(string size)
    {
        var server = new BlackForestLabsServer();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, ModelId).DoGenerateAsync(Call(size: size), CancellationToken.None));

        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Id + "rejects masks before submission", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsMasksBeforeSubmission()
    {
        var server = new BlackForestLabsServer();

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateBasicModel(server, ModelId).DoGenerateAsync(Call(mask: ImageModelFile.FromUrl("https://example.com/mask.png")), CancellationToken.None));

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
            CreateBasicModel(server, ModelId).DoGenerateAsync(Call(files: files), CancellationToken.None));

        Assert.Contains("Black Forest Labs supports up to 10 input images.", exception.Message);
        Assert.Empty(server.Calls);
    }
}
