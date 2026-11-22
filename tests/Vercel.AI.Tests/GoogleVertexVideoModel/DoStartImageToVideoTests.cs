// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVertexVideoModel.GoogleVertexVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

/// <summary>Port of <c>google-vertex-video-model.test.ts</c> &gt; <c>GoogleVertexVideoModel</c> &gt; <c>doStart</c> &gt; <c>Image-to-Video</c>.</summary>
public sealed class DoStartImageToVideoTests
{
    private const string Id =
        "packages/google-vertex/src/google-vertex-video-model.test.ts::GoogleVertexVideoModel > doStart > Image-to-Video::";

    [Fact]
    [UpstreamTest(
        Id + "should send image as bytesBase64Encoded",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendImageAsBytesBase64Encoded()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(image: VideoModelFile.FromBase64("base64-image-data", "image/png")),
                CancellationToken.None
            );

        AssertMatches(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\",\"image\":{\"bytesBase64Encoded\":\"base64-image-data\",\"mimeType\":\"image/png\"}}],\"parameters\":{\"sampleCount\":1}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn when URL-based image is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldWarnWhenURLBasedImageIsProvided()
    {
        var result = await CreateModel(new VertexVideoServer())
            .DoStartAsync(
                Call(image: VideoModelFile.FromUrl("https://example.com/image.png")),
                CancellationToken.None
            );

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("URL-based image input", warning.Feature);
    }
}
