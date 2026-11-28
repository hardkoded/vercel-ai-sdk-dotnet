// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel.MoonshotLanguageModelSupportB;

namespace Vercel.AI.Tests.Upstream.MoonshotAI.MoonshotAIChatLanguageModel;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; supportedUrls</c>.</summary>
public sealed class DoGenerateSupportedUrlsTests
{
    [Fact]
    [UpstreamTest(
        TestFile + "::doGenerate > supportedUrls::should natively support ms:// file references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_natively_support_ms_file_references()
    {
        var model = CreateProvider(new SseStreamHandler([])).CreateChatModel("kimi-k3");

        Assert.Equal(2, model.SupportedUrls.Count);
        Assert.Equal("^ms://", model.SupportedUrls["image/*"]);
        Assert.Equal("^ms://", model.SupportedUrls["video/*"]);
    }
}
