// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Azure;
using Vercel.AI.Operations;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of <c>azure-openai-provider.test.ts</c> &gt; <c>transcription</c> &gt; <c>doGenerate</c>.</summary>
public sealed class TranscriptionDoGenerateTests
{
    private const string TranscriptionResponse =
        "{\"text\":\"Hello, world!\",\"segments\":[],\"language\":\"en\",\"duration\":5.0}";

    private static TranscriptionModelCall Call() =>
        new(
            Array.Empty<byte>(),
            "audio/wav",
            JsonSerializer.Deserialize<JsonElement>("{}"),
            new Dictionary<string, string>(),
            CancellationToken.None
        );

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::transcription > doGenerate::should use correct URL format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_correct_URL_format()
    {
        var handler = new UpstreamRecordingHandler(TranscriptionResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            handler
        );

        await provider.Transcription("whisper-1").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/v1/audio/transcriptions?api-version=v1",
            handler.Uri
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::transcription > doGenerate::should use deployment-based URL format when useDeploymentBasedUrls is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_deployment_based_URL_format_when_useDeploymentBasedUrls_is_true()
    {
        var handler = new UpstreamRecordingHandler(TranscriptionResponse);
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                ApiKey = "test-api-key",
                ResourceName = "test-resource",
                UseDeploymentBasedUrls = true,
            },
            handler
        );

        await provider.Transcription("whisper-1").DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal(
            "https://test-resource.openai.azure.com/openai/deployments/whisper-1/audio/transcriptions?api-version=v1",
            handler.Uri
        );
    }
}
