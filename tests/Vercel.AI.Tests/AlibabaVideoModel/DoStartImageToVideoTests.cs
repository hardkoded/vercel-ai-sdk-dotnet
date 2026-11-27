// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c> &gt; <c>image-to-video</c>.</summary>
public sealed class DoStartImageToVideoTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart > image-to-video::";

    [Fact]
    [UpstreamTest(
        Id + "should send img_url from URL-based image for I2V model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendImgUrlFromUrlBasedImageForI2VModel()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, "wan2.6-i2v"),
            Call(image: Url("https://example.com/image.jpg"))
        );

        AssertMatches(
            "{\"model\":\"wan2.6-i2v\",\"input\":{\"prompt\":\""
                + PromptText
                + "\",\"img_url\":\"https://example.com/image.jpg\"}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send img_url as base64 from file data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendImgUrlAsBase64FromFileData()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, "wan2.6-i2v-flash"),
            Call(image: VideoModelFile.FromFile(ImageData, "image/png"))
        );

        // base64 of the bytes
        AssertMatches(
            "{\"model\":\"wan2.6-i2v-flash\",\"input\":{\"img_url\":\"iVBORw==\"}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should map resolution to I2V format (WxH -> \"720P\"/\"1080P\")",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldMapResolutionToI2VFormat()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, "wan2.6-i2v"), Call(resolution: "1920x1080"));

        AssertMatches("{\"parameters\":{\"resolution\":\"1080P\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should map 720p resolution for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldMap720pResolutionForI2V()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, "wan2.6-i2v"), Call(resolution: "1280x720"));

        AssertMatches("{\"parameters\":{\"resolution\":\"720P\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should not send img_url for T2V model even if image provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNotSendImgUrlForT2VModelEvenIfImageProvided()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, "wan2.6-t2v"),
            Call(image: Url("https://example.com/image.jpg"))
        );

        AssertNoProperty("img_url", RequestBody(server)["input"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should send audio provider option for I2V",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendAudioProviderOptionForI2V()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, "wan2.6-i2v-flash"), Call(alibaba: "{\"audio\":false}"));

        AssertMatches("{\"parameters\":{\"audio\":false}}", RequestBody(server));
    }
}
