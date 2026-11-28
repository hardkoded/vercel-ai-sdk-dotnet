// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportA;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>
/// Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>file data</c>.
/// </summary>
public sealed class DoGenerateFileDataTests
{
    private const string Prefix = MoonshotLanguageModelSupportA.Id + "file data::";

    [Theory]
    [InlineData("image", "[{\"type\":\"image_url\",\"image_url\":{\"url\":\"ms://file-image\"}}]")]
    [InlineData("video", "[{\"type\":\"video_url\",\"video_url\":{\"url\":\"ms://file-video\"}}]")]
    [UpstreamTest(
        Prefix + "should send ms:// references with top-level $mediaType media types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_ms_references_with_top_level_mediaType_media_types(
        string mediaType,
        string content
    )
    {
        var (model, handler) = Model("kimi-k3");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage([
                    new FileContentPart(mediaType, "ms://file-" + mediaType, null, null),
                ])
            ),
            CancellationToken.None
        );

        Strict(Body(handler)["messages"]![0]!["content"], content);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send text data and Moonshot provider references as native content parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_text_data_and_Moonshot_provider_references_as_native_content_parts()
    {
        var (model, handler) = Model("kimi-k2.6");

        await model.DoGenerateAsync(
            Messages(
                new UserModelMessage([
                    FileContentPart.FromText("text/plain", "inline document text"),
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string> { ["moonshotai"] = "ms://image-file-123" }
                    ),
                    FileContentPart.FromProviderReference(
                        "video/mp4",
                        new Dictionary<string, string> { ["moonshotai"] = "ms://video-file-123" }
                    ),
                ])
            ),
            CancellationToken.None
        );

        Strict(
            Body(handler)["messages"]![0]!["content"],
            "[{\"type\":\"text\",\"text\":\"inline document text\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"ms://image-file-123\"}},{\"type\":\"video_url\",\"video_url\":{\"url\":\"ms://video-file-123\"}}]"
        );
    }
}
