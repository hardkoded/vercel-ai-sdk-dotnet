// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel.KlingAIVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;

/// <summary>Port of <c>klingai-video-model.test.ts</c> &gt; <c>KlingAIVideoModel</c> &gt; <c>doStart - image-to-video</c>.</summary>
public sealed class DoStartImageToVideoTests
{
    private const string Prefix = "packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStart - image-to-video::";

    private static string I2V(string extra)
    {
        return "{\"klingai\":{\"mode\":\"std\"," + extra + "}}";
    }

    private static async Task<(Server Server, VideoStartResult Result)> Run(string modelId, VideoModelCall call)
    {
        var server = new Server();
        var result = await CreateModel(server.Handler, modelId).DoStartAsync(call, CancellationToken.None).ConfigureAwait(false);
        return (server, result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should POST to /v1/videos/image2video endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_POST_to_v1_videos_image2video_endpoint()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V()).ConfigureAwait(false);
        Assert.Equal(TestBaseUrl + "/v1/videos/image2video", server.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send model_name derived from model ID", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_model_name_derived_from_model_ID()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V()).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"model_name\":\"kling-v2-6\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert dots to hyphens in I2V model_name", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_dots_to_hyphens_in_I2V_model_name()
    {
        var (server, _) = await Run("kling-v2.5-turbo-i2v", StartI2V()).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"model_name\":\"kling-v2-5-turbo\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send image from URL-based input", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_image_from_URL_based_input()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(image: VideoModelFile.FromUrl("https://example.com/start-frame.png"))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"image\":\"https://example.com/start-frame.png\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send image as base64 from file data", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_image_as_base64_from_file_data()
    {
        var imageData = new byte[] { 137, 80, 78, 71 }; // PNG magic bytes
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(image: VideoModelFile.FromFile(imageData, "image/png"))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"image\":\"iVBORw==\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send image_tail when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_image_tail_when_provided()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(providerOptions: I2V("\"imageTail\":\"https://example.com/end-frame.png\""))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"image_tail\":\"https://example.com/end-frame.png\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send prompt with image", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_prompt_with_image()
    {
        var call = Start(prompt: "The cat walks away", image: StartFrame, providerOptions: StdOptions);
        var (server, _) = await Run("kling-v2.6-i2v", call).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"prompt\":\"The cat walks away\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should map SDK duration to string for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_map_SDK_duration_to_string_for_I2V()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(duration: 10)).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"duration\":\"10\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not warn about duration for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_warn_about_duration_for_I2V()
    {
        var (_, result) = await Run("kling-v2.6-i2v", StartI2V(duration: 5)).ConfigureAwait(false);
        Assert.DoesNotContain(result.Warnings, warning => warning.Feature == "duration");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn about aspectRatio for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_about_aspectRatio_for_I2V()
    {
        var (_, result) = await Run("kling-v2.6-i2v", StartI2V(aspectRatio: "16:9")).ConfigureAwait(false);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "aspectRatio");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send static_mask when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_static_mask_when_provided()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(providerOptions: I2V("\"staticMask\":\"https://example.com/mask.png\""))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"static_mask\":\"https://example.com/mask.png\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send dynamic_masks when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_dynamic_masks_when_provided()
    {
        const string dynamicMasks = "[{\"mask\":\"https://example.com/dynamic-mask.png\",\"trajectories\":[{\"x\":279,\"y\":219},{\"x\":417,\"y\":65}]}]";
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(providerOptions: I2V("\"dynamicMasks\":" + dynamicMasks))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"dynamic_masks\":" + dynamicMasks + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should derive model_name kling-v3 for kling-v3.0-i2v", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_derive_model_name_kling_v3_for_kling_v3_0_i2v()
    {
        var (server, _) = await Run("kling-v3.0-i2v", StartI2V()).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"model_name\":\"kling-v3\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send multi_shot and multi_prompt for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_multi_shot_and_multi_prompt_for_I2V()
    {
        const string multiPrompt = "[{\"index\":1,\"prompt\":\"The cat stretches lazily\",\"duration\":\"3\"},{\"index\":2,\"prompt\":\"The cat pounces on a toy\",\"duration\":\"2\"}]";
        var (server, _) = await Run("kling-v3.0-i2v", StartI2V(providerOptions: I2V("\"multiShot\":true,\"shotType\":\"customize\",\"multiPrompt\":" + multiPrompt))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"multi_shot\":true,\"shot_type\":\"customize\",\"multi_prompt\":" + multiPrompt + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send element_list when provided for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_element_list_when_provided_for_I2V()
    {
        const string elementList = "[{\"element_id\":101},{\"element_id\":202}]";
        var (server, _) = await Run("kling-v3.0-i2v", StartI2V(providerOptions: I2V("\"elementList\":" + elementList))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"element_list\":" + elementList + "}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send voice_list when provided for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_voice_list_when_provided_for_I2V()
    {
        const string voiceList = "[{\"voice_id\":\"voice-abc\"},{\"voice_id\":\"voice-def\"}]";
        var (server, _) = await Run("kling-v3.0-i2v", StartI2V(providerOptions: I2V("\"voiceList\":" + voiceList + ",\"sound\":\"on\""))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"voice_list\":" + voiceList + ",\"sound\":\"on\"}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send watermark_info when watermarkEnabled is set for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_watermark_info_when_watermarkEnabled_is_set_for_I2V()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(providerOptions: I2V("\"watermarkEnabled\":false"))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"watermark_info\":{\"enabled\":false}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should send negative_prompt for I2V", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_negative_prompt_for_I2V()
    {
        var (server, _) = await Run("kling-v2.6-i2v", StartI2V(providerOptions: I2V("\"negativePrompt\":\"blurry\""))).ConfigureAwait(false);
        Matches(server.RequestBody(), "{\"negative_prompt\":\"blurry\"}");
    }
}
