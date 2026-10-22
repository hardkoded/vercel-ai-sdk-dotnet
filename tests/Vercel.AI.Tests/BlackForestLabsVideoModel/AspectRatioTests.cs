// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsVideoModel;

/// <summary>Port of <c>black-forest-labs-video-model.test.ts</c> &gt; <c>BlackForestLabsVideoModel</c> &gt; <c>aspect ratio</c>.</summary>
public sealed class AspectRatioTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-video-model.test.ts::BlackForestLabsVideoModel > aspect ratio::";

    [Fact]
    [UpstreamTest(Id + "should send a supported aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendASupportedAspectRatio()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(aspectRatio: "21:9"), CancellationToken.None);

        AssertMatches("{\"aspect_ratio\":\"21:9\"}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should warn and omit an unsupported aspect ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldWarnAndOmitAnUnsupportedAspectRatio()
    {
        var server = CreateServer();

        var result = await CreateModel(server).DoGenerateAsync(Call(aspectRatio: "5:2"), CancellationToken.None);

        AssertNoProperty("aspect_ratio", RequestBody(server));
        Assert.Contains(
            ("unsupported", "aspectRatio", "FLUX 3 video does not support the aspect ratio \"5:2\". Using the provider default (auto)."),
            result.Warnings.Select(Warning));
    }

    [Fact]
    [UpstreamTest(Id + "should prefer providerOptions.aspectRatio, which can express auto", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPreferProviderOptionsAspectRatioWhichCanExpressAuto()
    {
        var server = CreateServer();

        await CreateModel(server).DoGenerateAsync(Call(aspectRatio: "16:9", blackForestLabs: "{\"aspectRatio\":\"auto\"}"), CancellationToken.None);

        AssertMatches("{\"aspect_ratio\":\"auto\"}", RequestBody(server));
    }
}
