// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel.DeepSeekChatLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.DeepSeekChatLanguageModel;

/// <summary>Port of <c>deepseek-chat-language-model.test.ts</c> &gt; <c>DeepSeekChatLanguageModel &gt; supportedUrls</c>.</summary>
public sealed class SupportedUrlsTests
{
    [Fact]
    [UpstreamTest(
        TestFile + " > supportedUrls::should natively support HTTP image URLs",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Should_natively_support_HTTP_image_URLs()
    {
        var model = Model(new UpstreamCapture(), "deepseek-chat");

        // Upstream's /^https?:\/\/.*$/ is "^https?://.*\\z" in .NET, which rejects a trailing newline.
        var entry = Assert.Single(model.SupportedUrls);
        Assert.Equal("image/*", entry.Key);
        Assert.Equal("^https?://.*\\z", entry.Value);
    }
}
