// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>resolution</c>.</summary>
public sealed class ResolutionTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > resolution::";

    [Fact]
    [UpstreamTest(Id + "should pass a named tier through", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassANamedTierThrough()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(resolution: "fhd"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"fhd\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should accept a named tier case-insensitively", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAcceptANamedTierCaseInsensitively()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(resolution: "FHD"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"fhd\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map 1280x720 onto hd without warning", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMap1280x720OntoHdWithoutWarning()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "1280x720"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"hd\"}", RequestBody(server));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should map 1920x1080 onto fhd without warning", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMap1920x1080OntoFhdWithoutWarning()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "1920x1080"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"fhd\"}", RequestBody(server));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should snap an in-between resolution to a tier and report it", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSnapAnInBetweenResolutionToATierAndReportIt()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "854x480"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"hd\"}", RequestBody(server));
        Assert.Contains(
            ("compatibility", "resolution", "FLUX 3 video renders at \"hd\" or \"fhd\"; the requested resolution \"854x480\" was mapped to \"hd\"."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should warn and omit an unparseable resolution", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAndOmitAnUnparseableResolution()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "huge"), CancellationToken.None);

        AssertNoProperty("resolution", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "resolution",
                "Unrecognized resolution \"huge\". FLUX 3 video supports \"hd\" and \"fhd\", or a {width}x{height} value to map onto one."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should prefer providerOptions.resolution over the top-level value", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferProviderOptionsResolutionOverTheTopLevelValue()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "1920x1080", blackForestLabs: "{\"resolution\":\"hd\"}"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"hd\"}", RequestBody(server));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Id + "should keep providerOptions.resolution and warn when the top-level value is unrecognized", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldKeepProviderOptionsResolutionAndWarnWhenTheTopLevelValueIsUnrecognized()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(resolution: "huge", blackForestLabs: "{\"resolution\":\"fhd\"}"), CancellationToken.None);

        AssertMatches("{\"resolution\":\"fhd\"}", RequestBody(server));
        Assert.Contains(
            (
                "unsupported",
                "resolution",
                "Unrecognized resolution \"huge\". FLUX 3 video supports \"hd\" and \"fhd\", so providerOptions.blackForestLabs.resolution (\"fhd\") was used instead."),
            result.Warnings.Select(Warning));
    }
}
