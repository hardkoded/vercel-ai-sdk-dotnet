// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; motion control</c>.</summary>
public sealed class TokenHubKlingMotionControlTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > motion control::";

    [Fact]
    [UpstreamTest(Prefix + "sends the character image and motion video", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_the_character_image_and_motion_video()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-motion-control-v3.0").DoStartAsync(
            Start(
                duration: 5,
                generateAudio: false,
                image: Url("https://example.com/character.png"),
                inputReferences: [Url("https://example.com/motion.mp4", "video/mp4")],
                providerOptions: """{"tencent":{"characterOrientation":"video","elements":[{"elementId":"element-1","id":"e1"}]}}"""),
            CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/motion-control", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-motion-control-v3.0","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"image","url":"https://example.com/character.png"},{"type":"video","url":"https://example.com/motion.mp4"},{"type":"element","element_id":"element-1","id":"e1"}],"settings":{"character_orientation":"video","audio":"off"}}""");
        Assert.Equal(["duration"], Features(result.Warnings));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses the first image reference as the character image", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Uses_the_first_image_reference_as_the_character_image()
    {
        var server = new Server();
        await CreateModel(server, "kling-video-motion-control-v2.6").DoStartAsync(
            Start(prompt: null, inputReferences: [Url("https://example.com/character.png"), Url("https://example.com/motion.mp4", "video/mp4")], providerOptions: """{"tencent":{"characterOrientation":"image"}}"""),
            CancellationToken.None);

        Body(server, """{"model":"kling-video-motion-control-v2.6","contents":[{"type":"image","url":"https://example.com/character.png"},{"type":"video","url":"https://example.com/motion.mp4"}],"settings":{"character_orientation":"image"}}""");
    }
}
