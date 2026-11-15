// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; text-to-video</c>.</summary>
public sealed class TokenHubKlingTextToVideoTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > text-to-video::";

    [Fact]
    [UpstreamTest(Prefix + "sends prompt and settings", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_prompt_and_settings()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v3").DoStartAsync(Start(aspectRatio: "16:9", duration: 5, resolution: "1920x1080", generateAudio: true), CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/text-to-video", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-v3","prompt":"A cat walking on a beach","settings":{"resolution":"1080p","aspect_ratio":"16:9","duration":5,"audio":"native"}}""");
        Assert.Equal("Bearer test-key", server.Calls[0].Header("authorization"));
        JsonAssert.Equal((System.Text.Json.JsonElement)result.Operation!, """{"taskId":"task-123"}""");
        Assert.Empty(result.Warnings);
        Assert.Equal(CurrentDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("kling-video-v3", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "sends multi-shot, watermark and external task ID", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_multi_shot_watermark_and_external_task_ID()
    {
        var server = new Server();
        await CreateModel(server, "kling-video-v3").DoStartAsync(Start(providerOptions: """{"tencent":{"multiShot":false,"watermark":true,"externalTaskId":"my-task","resolution":"4k"}}"""), CancellationToken.None);

        Body(server, """{"model":"kling-video-v3","prompt":"A cat walking on a beach","settings":{"resolution":"4k","multi_shot":false},"options":{"external_task_id":"my-task","watermark_info":{"enabled":true}}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "omits audio and multi-shot for models that do not support them", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Omits_audio_and_multi_shot_for_models_that_do_not_support_them()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v4").DoStartAsync(Start(generateAudio: true, providerOptions: """{"tencent":{"multiShot":true}}"""), CancellationToken.None);

        Body(server, """{"model":"kling-video-v4","prompt":"A cat walking on a beach"}""");
        Assert.Equal(["generateAudio", "providerOptions.tencent.multiShot"], Features(result.Warnings));
    }

    [Fact]
    [UpstreamTest(Prefix + "warns on unsupported options", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Warns_on_unsupported_options()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v3").DoStartAsync(
            Start(seed: 42, fps: 24, n: 2, resolution: "640x480", webhookUrl: "https://example.com/webhook", providerOptions: """{"tencent":{"negativePrompt":"dogs"}}"""),
            CancellationToken.None);

        Body(server, """{"model":"kling-video-v3","prompt":"A cat walking on a beach"}""");
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
        Assert.Equal(["fps", "n", "seed", "webhookUrl", "providerOptions.tencent.negativePrompt", "resolution"], Features(result.Warnings));
    }
}
