// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureMaiTestSupport;
using Harness = Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureMaiTranscriptionModel;

/// <summary>Port of <c>azure-mai-transcription-model.test.ts</c> &gt; <c>MAI routing</c>.</summary>
public sealed class MaiRoutingTests
{
    private const string TestPrefix = "packages/azure/src/azure-mai-transcription-model.test.ts::MAI routing::";

    [Fact]
    [UpstreamTest(TestPrefix + "rejects file transcription for the streaming model", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Rejects_file_transcription_for_the_streaming_model()
    {
        var model = Setup().Transcription("MAI-Transcribe-2-Streaming");

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() => model.DoGenerateAsync(Harness.Call(audio: new byte[] { 1 }), CancellationToken.None));
    }

    [Fact]
    [UpstreamTest(TestPrefix + "warns about the MAI language option on file transcription", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Warns_about_the_MAI_language_option_on_file_transcription()
    {
        var harness = Harness.Setup(body: JsonNode.Parse("{\"combinedPhrases\":[{\"text\":\"Hi\"}],\"durationMilliseconds\":1000}"));

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Harness.Call("{\"azure\":{\"language\":\"en\"}}", audio: new byte[] { 1 }),
            CancellationToken.None);

        Assert.Equal(
            new[] { ("unsupported", (string?)"providerOptions.azure.language", (string?)"This option requires MAI streaming transcription.") },
            Harness.Warnings(result));
    }
}
