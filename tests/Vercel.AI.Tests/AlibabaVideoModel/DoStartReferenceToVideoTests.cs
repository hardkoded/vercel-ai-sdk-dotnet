// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c> &gt; <c>reference-to-video</c>.</summary>
public sealed class DoStartReferenceToVideoTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart > reference-to-video::";

    [Fact]
    [UpstreamTest(
        Id + "should send reference_urls for R2V model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendReferenceUrlsForR2VModel()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, "wan2.6-r2v-flash"),
            Call(
                alibaba: "{\"referenceUrls\":[\"https://example.com/ref-image.jpg\",\"https://example.com/ref-video.mp4\"]}"
            )
        );

        AssertMatches(
            "{\"model\":\"wan2.6-r2v-flash\",\"input\":{\"prompt\":\""
                + PromptText
                + "\",\"reference_urls\":[\"https://example.com/ref-image.jpg\",\"https://example.com/ref-video.mp4\"]}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send size parameter for R2V resolution (x converted to *)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendSizeParameterForR2VResolutionXConvertedTo()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server, "wan2.6-r2v"), Call(resolution: "1280x720"));

        AssertMatches("{\"parameters\":{\"size\":\"1280*720\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should not send reference_urls for non-R2V model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNotSendReferenceUrlsForNonR2VModel()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server, "wan2.6-t2v"),
            Call(alibaba: "{\"referenceUrls\":[\"https://example.com/ref.jpg\"]}")
        );

        AssertNoProperty("reference_urls", RequestBody(server)["input"]);
    }
}
