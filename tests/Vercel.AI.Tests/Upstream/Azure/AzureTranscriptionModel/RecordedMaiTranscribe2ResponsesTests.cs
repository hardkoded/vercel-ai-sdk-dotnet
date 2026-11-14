// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>recorded MAI-Transcribe-2 responses</c>.</summary>
public sealed class RecordedMaiTranscribe2ResponsesTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::recorded MAI-Transcribe-2 responses::";

    [Fact]
    [UpstreamTest(Prefix + "maps diarized word-level results into segments and metadata", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Maps_diarized_word_level_results_into_segments_and_metadata()
    {
        var body = JsonNode.Parse(Fixture("azure-speech-mai-transcribe-2-word-diarization.1"))!;
        var harness = Setup(body: body);

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"word\",\"diarization\":{\"enabled\":true}}}"),
            CancellationToken.None);

        Assert.Equal(body["combinedPhrases"]![0]!["text"]!.GetValue<string>(), result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal(17.579, result.DurationInSeconds!.Value, 6);
        var expected = new[]
        {
            ("Um, so, uh, did you finish the quarterly report for Versal yet?", 0.12, 4.32),
            ("Yeah, I, uh, I sent it over this morning. It covers the AI gateway numbers.", 4.48, 9.439),
            ("Great. Uh, can you, like, add the transcription pricing too?", 9.68, 13.8),
            ("Sure. I'll, um, update it by Friday.", 14.04, 17.159),
        };
        Assert.Equal(expected.Length, result.Segments.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index].Item1, result.Segments[index].Text);
            Assert.Equal(expected[index].Item2, result.Segments[index].StartSecond, 6);
            Assert.Equal(expected[index].Item3, result.Segments[index].EndSecond, 6);
        }

        var phrases = JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())!["azure"]!["phrases"]!.AsArray();
        Assert.Equal(new[] { 0, 1, 0, 1 }, phrases.Select(phrase => phrase!["speaker"]!.GetValue<int>()).ToArray());
        AssertJson("{\"text\":\"Um,\",\"offsetMilliseconds\":120,\"durationMilliseconds\":220}", phrases[0]!["words"]![0]);
        var word = result.Words![0];
        Assert.Equal("Um,", word.Text);
        Assert.Equal(0.12, word.StartSecond, 6);
        Assert.Equal(0.34, word.EndSecond, 6);
    }

    [Fact]
    [UpstreamTest(Prefix + "maps a timestamps=none result to one segment spanning the audio", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Maps_a_timestamps_none_result_to_one_segment_spanning_the_audio()
    {
        var harness = Setup(body: JsonNode.Parse(Fixture("azure-speech-mai-transcribe-2-no-timestamps.1")));

        var result = await harness.Transcription("mai-transcribe-2").DoGenerateAsync(
            Call("{\"azure\":{\"timestamps\":\"none\"}}"),
            CancellationToken.None);

        var segment = Assert.Single(result.Segments);
        Assert.Equal(0, segment.StartSecond, 6);
        Assert.Equal(17.579, segment.EndSecond, 6);
    }
}
