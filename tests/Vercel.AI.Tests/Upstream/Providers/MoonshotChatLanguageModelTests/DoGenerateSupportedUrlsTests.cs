// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; supportedUrls</c>.</summary>
public sealed class DoGenerateSupportedUrlsTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > supportedUrls::should natively support ms:// file references",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_natively_support_ms_file_references()
    {
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new UpstreamCapture());

        var urls = provider.LanguageModel("kimi-k3").SupportedUrls;

        Assert.Equal(2, urls.Count);
        Assert.Equal("^ms://", urls["image/*"]);
        Assert.Equal("^ms://", urls["video/*"]);
    }
}
