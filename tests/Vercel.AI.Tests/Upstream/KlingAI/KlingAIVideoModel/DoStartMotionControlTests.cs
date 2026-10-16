// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>doStart - motion control</c>.</summary>
public sealed class DoStartMotionControlTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStart - motion control::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with required fields", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_correct_request_body_with_required_fields()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal(server.RequestBody(), "{\"model_name\":\"kling-v2-6\",\"prompt\":\"A character performs a graceful dance\",\"video_url\":\"https://example.com/reference-motion.mp4\",\"character_orientation\":\"image\",\"mode\":\"std\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send prompt when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_prompt_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(prompt: "Dance gracefully"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"prompt\":\"Dance gracefully\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send image_url from URL-based image", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_image_url_from_URL_based_image()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(image: VideoModelFile.FromUrl("https://example.com/reference-image.png")), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"image_url\":\"https://example.com/reference-image.png\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send image_url as base64 from file data", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_image_url_as_base64_from_file_data()
    {
        var imageData = new byte[] { 137, 80, 78, 71 }; // PNG magic bytes
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(image: VideoModelFile.FromFile(imageData, "image/png")), CancellationToken.None).ConfigureAwait(false);

        // KlingAI expects raw base64, no data: prefix
        Matches(server.RequestBody(), "{\"image_url\":\"iVBORw==\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send keep_original_sound when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_keep_original_sound_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(providerOptions: "{\"klingai\":{\"videoUrl\":\"https://example.com/reference-motion.mp4\",\"characterOrientation\":\"image\",\"mode\":\"std\",\"keepOriginalSound\":\"no\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"keep_original_sound\":\"no\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send watermark_info when watermarkEnabled is set", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_watermark_info_when_watermarkEnabled_is_set()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(providerOptions: "{\"klingai\":{\"videoUrl\":\"https://example.com/reference-motion.mp4\",\"characterOrientation\":\"image\",\"mode\":\"std\",\"watermarkEnabled\":true}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"watermark_info\":{\"enabled\":true}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send mode=pro when specified", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_mode_pro_when_specified()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        await model.DoStartAsync(Start(providerOptions: "{\"klingai\":{\"videoUrl\":\"https://example.com/motion.mp4\",\"characterOrientation\":\"video\",\"mode\":\"pro\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"character_orientation\":\"video\",\"mode\":\"pro\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported aspectRatio", Coverage = UpstreamCoverage.Covered)]
    public async Task should_warn_about_unsupported_aspectRatio()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        var result = await model.DoStartAsync(Start(aspectRatio: "16:9"), CancellationToken.None).ConfigureAwait(false);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about unsupported duration", Coverage = UpstreamCoverage.Covered)]
    public async Task should_warn_about_unsupported_duration()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        var result = await model.DoStartAsync(Start(duration: 10), CancellationToken.None).ConfigureAwait(false);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "duration");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when n > 1", Coverage = UpstreamCoverage.Covered)]
    public async Task should_warn_when_n_1()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        var result = await model.DoStartAsync(Start(n: 3), CancellationToken.None).ConfigureAwait(false);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "n");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn when n is 1", Coverage = UpstreamCoverage.Covered)]
    public async Task should_not_warn_when_n_is_1()
    {
        var server = new Server();
        var model = CreateModel(server.Handler);
        var result = await model.DoStartAsync(Start(n: 1), CancellationToken.None).ConfigureAwait(false);

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "n");
    }

    [Fact]
    [UpstreamTest(Prefix + "should derive model_name kling-v3 for kling-v3.0-motion-control", Coverage = UpstreamCoverage.Covered)]
    public async Task should_derive_model_name_kling_v3_for_kling_v3_0_motion_control()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-motion-control");
        await model.DoStartAsync(Start(), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"model_name\":\"kling-v3\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send element_list when provided for motion control", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_element_list_when_provided_for_motion_control()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-motion-control");
        await model.DoStartAsync(Start(providerOptions: "{\"klingai\":{\"videoUrl\":\"https://example.com/reference-motion.mp4\",\"characterOrientation\":\"image\",\"mode\":\"std\",\"elementList\":[{\"element_id\":829836802793406551}]}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"element_list\":[{\"element_id\":829836802793406551}]}");
    }
}
