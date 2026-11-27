// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.AlibabaVideoModel.AlibabaVideoModelFixture;

namespace Vercel.AI.Tests.AlibabaVideoModel;

/// <summary>Port of <c>alibaba-video-model.test.ts</c> &gt; <c>AlibabaVideoModel</c> &gt; <c>doStart</c> &gt; <c>text-to-video</c>.</summary>
public sealed class DoStartTextToVideoTests
{
    private const string Id =
        "packages/alibaba/src/alibaba-video-model.test.ts::AlibabaVideoModel > doStart > text-to-video::";

    [Fact]
    [UpstreamTest(
        Id + "should send correct request body for T2V",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendCorrectRequestBodyForT2V()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server), Call());

        AssertJson(
            "{\"model\":\"wan2.6-t2v\",\"input\":{\"prompt\":\""
                + PromptText
                + "\"},\"parameters\":{}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send size parameter for T2V resolution (x converted to *)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendSizeParameterForT2VResolutionXConvertedTo()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server), Call(resolution: "1920x1080"));

        AssertMatches("{\"parameters\":{\"size\":\"1920*1080\"}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send duration parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendDurationParameter()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server), Call(duration: 10));

        AssertMatches("{\"parameters\":{\"duration\":10}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(Id + "should send seed parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendSeedParameter()
    {
        var server = new AlibabaVideoServer();

        await Start(CreateModel(server), Call(seed: 42));

        AssertMatches("{\"parameters\":{\"seed\":42}}", RequestBody(server));
    }

    [Fact]
    [UpstreamTest(
        Id + "should send provider options (negativePrompt, promptExtend, shotType, watermark)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldSendProviderOptionsNegativePromptPromptExtendShotTypeWatermark()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server),
            Call(
                alibaba: "{\"negativePrompt\":\"blurry, low quality\",\"promptExtend\":true,\"shotType\":\"multi\",\"watermark\":false}"
            )
        );

        AssertMatches(
            "{\"input\":{\"prompt\":\""
                + PromptText
                + "\",\"negative_prompt\":\"blurry, low quality\"},\"parameters\":{\"prompt_extend\":true,\"shot_type\":\"multi\",\"watermark\":false}}",
            RequestBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should send audioUrl in input", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendAudioUrlInInput()
    {
        var server = new AlibabaVideoServer();

        await Start(
            CreateModel(server),
            Call(alibaba: "{\"audioUrl\":\"https://example.com/audio.mp3\"}")
        );

        AssertMatches(
            "{\"input\":{\"prompt\":\""
                + PromptText
                + "\",\"audio_url\":\"https://example.com/audio.mp3\"}}",
            RequestBody(server)
        );
    }
}
