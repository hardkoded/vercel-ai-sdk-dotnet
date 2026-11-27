// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using Vercel.AI.Provider;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>speech</c> &gt; <c>doGenerate</c>.</summary>
public sealed class SpeechDoGenerateTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::speech > doGenerate::should use correct URL format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_correct_URL_format()
    {
        var handler = new UpstreamRecordingHandler("\u0001\u0002\u0003");
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        await provider
            .Speech("tts-1")
            .DoGenerateAsync(new SpeechCallOptions("Hello, world!"), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/audio/speech?api-version=v1",
            handler.Uri
        );
    }
}
