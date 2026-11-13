// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; omni</c>.</summary>
public sealed class TokenHubKlingOmniTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > omni::";

    [Fact]
    [UpstreamTest(Prefix + "sends reference images and videos to the omni endpoint", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_reference_images_and_videos_to_the_omni_endpoint()
    {
        var server = new Server();
        await CreateModel(server, "kling-video-v3-omni").DoStartAsync(
            Start(aspectRatio: "1:1", inputReferences: [Url("https://example.com/ref.png"), Url("https://example.com/ref.mp4", "video/mp4")], providerOptions: """{"tencent":{"referenceVideoType":"base","audio":"original"}}"""),
            CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/omni-video", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-v3-omni","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"refer_image","url":"https://example.com/ref.png"},{"type":"base_video","url":"https://example.com/ref.mp4"}],"settings":{"aspect_ratio":"1:1","audio":"original"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "routes kling-video-v4 with references and voices to the omni endpoint", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Routes_kling_video_v4_with_references_and_voices_to_the_omni_endpoint()
    {
        var server = new Server();
        await CreateModel(server, "kling-video-v4").DoStartAsync(
            Start(aspectRatio: "adaptive", inputReferences: [Url("https://example.com/motion.mp4", "video/mp4")], providerOptions: """{"tencent":{"voices":[{"voiceId":"voice-1","id":"1"}]}}"""),
            CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/omni-video", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-v4","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"feature_video","url":"https://example.com/motion.mp4"},{"type":"voice","voice_id":"voice-1","id":"1"}],"settings":{"aspect_ratio":"adaptive"}}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "routes kling-video-v4 with elements and no first frame to the omni endpoint", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Routes_kling_video_v4_with_elements_and_no_first_frame_to_the_omni_endpoint()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v4").DoStartAsync(Start(providerOptions: """{"tencent":{"elements":[{"elementId":"element-1"}]}}"""), CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/omni-video", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-v4","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"element","element_id":"element-1"}]}""");
        Assert.Empty(result.Warnings);
    }
}
