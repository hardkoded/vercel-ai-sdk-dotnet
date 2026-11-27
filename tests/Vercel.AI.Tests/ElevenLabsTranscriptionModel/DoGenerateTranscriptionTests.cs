// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.ElevenLabsTranscriptionModel;

/// <summary>Port of <c>elevenlabs-transcription-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>transcription</c>.</summary>
public sealed class DoGenerateTranscriptionTests
{
    private const string Prefix =
        "packages/elevenlabs/src/elevenlabs-transcription-model.test.ts::doGenerate > transcription::";

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve word metadata in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_word_metadata_in_provider_metadata()
    {
        const string words = """
            [{"text":"hi","type":"word","start":0,"end":0.4,"speaker_id":"speaker_1"},{"text":" ","type":"spacing","start":0.4,"end":0.5},{"text":"(laughter)","type":"audio_event","start":0.5,"end":1.2,"speaker_id":"speaker_1"}]
            """;
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                $$"""{"language_code":"eng","language_probability":1,"text":"hi (laughter)","words":{{words}}}"""
            )
        );
        var model = (ITranscriptionCaller)
            ElevenLabsProvider
                .Create(new ElevenLabsOptions { ApiKey = "test-api-key" }, handler)
                .TranscriptionModel("scribe_v1");

        var result = await model.DoGenerateAsync(
            new TranscriptionModelCall(
                new byte[] { 1, 2, 3 },
                "audio/wav",
                JsonDocument.Parse("""{"elevenlabs":{"diarize":true}}""").RootElement.Clone(),
                new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"elevenlabs\":{\"words\":" + words + "}}"),
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
            )
        );
    }
}
