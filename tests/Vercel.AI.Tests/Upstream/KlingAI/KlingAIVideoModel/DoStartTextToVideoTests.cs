// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>doStart - text-to-video</c>.</summary>
public sealed class DoStartTextToVideoTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStart - text-to-video::";

    [Fact]
    [UpstreamTest(Prefix + "should POST to /v1/videos/text2video endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task should_POST_to_v1_videos_text2video_endpoint()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(TestBaseUrl + "/v1/videos/text2video", server.Calls[0].Uri.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should send model_name derived from model ID", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_model_name_derived_from_model_ID()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"model_name\":\"kling-v2-6\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert dots to hyphens in model_name", Coverage = UpstreamCoverage.Covered)]
    public async Task should_convert_dots_to_hyphens_in_model_name()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.1-master-t2v");
        await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"model_name\":\"kling-v2-1-master\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle model IDs without dots", Coverage = UpstreamCoverage.Covered)]
    public async Task should_handle_model_IDs_without_dots()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v1-t2v");
        await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"model_name\":\"kling-v1\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send prompt in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_prompt_in_request_body()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(Start(prompt: "A sunset over the ocean", providerOptions: StdOptions), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"prompt\":\"A sunset over the ocean\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should map SDK aspectRatio to aspect_ratio", Coverage = UpstreamCoverage.Covered)]
    public async Task should_map_SDK_aspectRatio_to_aspect_ratio()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(aspectRatio: "16:9"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"aspect_ratio\":\"16:9\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn about aspectRatio for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_not_warn_about_aspectRatio_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        var result = await model.DoStartAsync(StartT2V(aspectRatio: "16:9"), CancellationToken.None).ConfigureAwait(false);

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Prefix + "should map SDK duration to string", Coverage = UpstreamCoverage.Covered)]
    public async Task should_map_SDK_duration_to_string()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(duration: 10), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"duration\":\"10\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn about duration for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_not_warn_about_duration_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        var result = await model.DoStartAsync(StartT2V(duration: 5), CancellationToken.None).ConfigureAwait(false);

        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "duration");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send negative_prompt when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_negative_prompt_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"negativePrompt\":\"blurry, low quality\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"negative_prompt\":\"blurry, low quality\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send sound when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_sound_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"sound\":\"on\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"sound\":\"on\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send cfg_scale when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_cfg_scale_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"cfgScale\":0.7}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"cfg_scale\":0.7}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send camera_control when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_camera_control_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"cameraControl\":{\"type\":\"simple\",\"config\":{\"zoom\":5}}}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"camera_control\":{\"type\":\"simple\",\"config\":{\"zoom\":5}}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should derive model_name kling-v3 for kling-v3.0-t2v", Coverage = UpstreamCoverage.Covered)]
    public async Task should_derive_model_name_kling_v3_for_kling_v3_0_t2v()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-t2v");
        await model.DoStartAsync(StartT2V(), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"model_name\":\"kling-v3\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send multi_shot and shot_type when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_multi_shot_and_shot_type_when_provided()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"multiShot\":true,\"shotType\":\"customize\",\"multiPrompt\":[{\"index\":1,\"prompt\":\"A sunrise over mountains\",\"duration\":\"4\"},{\"index\":2,\"prompt\":\"A bird flying across the sky\",\"duration\":\"3\"}]}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"multi_shot\":true,\"shot_type\":\"customize\",\"multi_prompt\":[{\"index\":1,\"prompt\":\"A sunrise over mountains\",\"duration\":\"4\"},{\"index\":2,\"prompt\":\"A bird flying across the sky\",\"duration\":\"3\"}]}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send multi_shot with intelligence shot_type", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_multi_shot_with_intelligence_shot_type()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"multiShot\":true,\"shotType\":\"intelligence\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"multi_shot\":true,\"shot_type\":\"intelligence\"}");
        Assert.False(server.RequestBody().ContainsKey("multi_prompt"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send voice_list when provided for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_voice_list_when_provided_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"voiceList\":[{\"voice_id\":\"voice-abc\"}],\"sound\":\"on\"}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"voice_list\":[{\"voice_id\":\"voice-abc\"}],\"sound\":\"on\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send watermark_info when watermarkEnabled is set for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_send_watermark_info_when_watermarkEnabled_is_set_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"watermarkEnabled\":true}}"), CancellationToken.None).ConfigureAwait(false);

        Matches(server.RequestBody(), "{\"watermark_info\":{\"enabled\":true}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not send element_list for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_not_send_element_list_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v3.0-t2v");
        await model.DoStartAsync(StartT2V(providerOptions: "{\"klingai\":{\"mode\":\"std\",\"elementList\":[{\"element_id\":101}]}}"), CancellationToken.None).ConfigureAwait(false);

        Assert.False(server.RequestBody().ContainsKey("element_list"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when image is provided for T2V", Coverage = UpstreamCoverage.Covered)]
    public async Task should_warn_when_image_is_provided_for_T2V()
    {
        var server = new Server();
        var model = CreateModel(server.Handler, modelId: "kling-v2.6-t2v");
        var result = await model.DoStartAsync(StartT2V(image: VideoModelFile.FromUrl("https://example.com/image.png")), CancellationToken.None).ConfigureAwait(false);

        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "image");
    }
}
