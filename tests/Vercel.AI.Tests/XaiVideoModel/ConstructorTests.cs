// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.XaiVideoModel.XaiVideoModelFixture;

namespace Vercel.AI.Tests.XaiVideoModel;

/// <summary>Port of <c>xai-video-model.test.ts</c> &gt; <c>XaiVideoModel</c> &gt; <c>constructor</c>.</summary>
public sealed class ConstructorTests
{
    private const string Id =
        "packages/xai/src/xai-video-model.test.ts::XaiVideoModel > constructor::";

    [Fact]
    [UpstreamTest(
        Id + "should expose correct provider and model information",
        Coverage = UpstreamCoverage.Covered
    )]
    public void ShouldExposeCorrectProviderAndModelInformation()
    {
        var model = CreateModel(new XaiVideoServer());

        Assert.Equal("xai.video", model.Provider);
        Assert.Equal("grok-imagine-video", model.ModelId);
        Assert.Equal("v4", model.SpecificationVersion);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send the grok-imagine-video-1.5 model id in the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendTheGrokImagineVideo15ModelIdInTheRequestBody()
    {
        var server = new XaiVideoServer();

        await CreateModel(server, modelId: "grok-imagine-video-1.5")
            .DoStartAsync(Call(), CancellationToken.None);

        AssertMatches("{\"model\":\"grok-imagine-video-1.5\"}", RequestBody(server));
    }
}
