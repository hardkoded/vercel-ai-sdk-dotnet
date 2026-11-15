// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel.TencentVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentVideoModel;

/// <summary>Port of <c>tencent-video-model.test.ts</c> &gt; <c>TencentVideoModel &gt; TokenHub Kling &gt; image-to-video</c>.</summary>
public sealed class TokenHubKlingImageToVideoTests
{
    private const string Prefix = UpstreamFile + "::TencentVideoModel > TokenHub Kling > image-to-video::";

    [Fact]
    [UpstreamTest(Prefix + "sends first and last frames as contents", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Sends_first_and_last_frames_as_contents()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v3").DoStartAsync(
            Start(
                aspectRatio: "9:16",
                duration: 10,
                frameImages: Frames((Url("https://example.com/first.png"), "first_frame"), (VideoModelFile.FromFile([1, 2, 3], "image/png"), "last_frame")),
                providerOptions: """{"tencent":{"elements":[{"elementId":"element-1"}]}}"""),
            CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/image-to-video", server.Calls[0].Uri.AbsoluteUri);
        Body(server, """{"model":"kling-video-v3","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"first_frame","url":"https://example.com/first.png"},{"type":"last_frame","url":"data:image/png;base64,AQID"},{"type":"element","element_id":"element-1"}],"settings":{"duration":10}}""");
        Assert.Equal(["aspectRatio"], Features(result.Warnings));
    }

    [Fact]
    [UpstreamTest(Prefix + "uses the legacy image option as the first frame", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Uses_the_legacy_image_option_as_the_first_frame()
    {
        var server = new Server();
        await CreateModel(server, "kling-video-v3-turbo").DoStartAsync(Start(image: VideoModelFile.FromFile(Convert.FromBase64String("AAAA"), "image/jpeg")), CancellationToken.None);

        Body(server, """{"model":"kling-video-v3-turbo","contents":[{"type":"prompt","text":"A cat walking on a beach"},{"type":"first_frame","url":"data:image/jpeg;base64,AAAA"}]}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "ignores references on models without omni support", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Ignores_references_on_models_without_omni_support()
    {
        var server = new Server();
        var result = await CreateModel(server, "kling-video-v3").DoStartAsync(Start(inputReferences: [Url("https://example.com/ref.png")]), CancellationToken.None);

        Assert.Equal(TokenHubBaseUrl + "/wand/kling/text-to-video", server.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(["inputReferences"], Features(result.Warnings));
    }
}
