// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>video input</c>.
/// </summary>
public sealed class DoGenerateVideoInputTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "video input::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send video data parts as video_url data URIs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_video_data_parts_as_video_url_data_URIs()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage([
                    new TextContentPart("Describe this video"),
                    new FileContentPart("video/mp4", null, [0, 1, 2, 3], null),
                ])
            ),
            CancellationToken.None
        );

        Strict(
            Body(handler)["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"Describe this video\"},{\"type\":\"video_url\",\"video_url\":{\"url\":\"data:video/mp4;base64,AAECAw==\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass through video URL parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_video_URL_parts()
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage([
                    new FileContentPart("video/*", "https://example.com/video.mp4", null, null),
                ])
            ),
            CancellationToken.None
        );

        Strict(
            Body(handler)["messages"]![0]!["content"],
            "[{\"type\":\"video_url\",\"video_url\":{\"url\":\"https://example.com/video.mp4\"}}]"
        );
    }
}
