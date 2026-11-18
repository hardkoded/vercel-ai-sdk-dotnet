// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel.MistralTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralTranscriptionModel;

/// <summary>Port of <c>mistral-transcription-model.test.ts</c> &gt; <c>doGenerate</c> &gt; <c>%s</c> (one run per Voxtral 3 model id).</summary>
public sealed class DoGenerateVoxtral3Tests
{
    private const string Id =
        "packages/mistral/src/mistral-transcription-model.test.ts::doGenerate::%s::";

    [Theory]
    [InlineData("voxtral-mini-transcribe-3-0")]
    [InlineData("voxtral-mini-transcribe-3")]
    [InlineData("voxtral-mini-transcribe-latest")]
    [InlineData("voxtral-small-transcribe-3-0")]
    [InlineData("voxtral-small-transcribe-3")]
    [InlineData("voxtral-small-transcribe-latest")]
    [UpstreamTest(
        Id + "should allow language with segment timestamps and diarization",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_allow_language_with_segment_timestamps_and_diarization(
        string voxtral3ModelId
    )
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler), voxtral3ModelId)
            .DoGenerateAsync(
                Call(
                    "{\"mistral\":{\"language\":\"en\",\"returnLanguage\":false,\"timestampGranularities\":[\"segment\"],\"diarize\":true}}"
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal(voxtral3ModelId, Field(call, "model"));
        Assert.Equal("en", Field(call, "language"));
        Assert.Equal("false", Field(call, "return_language"));
        Assert.Equal("segment", Field(call, "timestamp_granularities"));
        Assert.Equal("true", Field(call, "diarize"));
    }

    [Theory]
    [InlineData("voxtral-mini-transcribe-3-0")]
    [InlineData("voxtral-mini-transcribe-3")]
    [InlineData("voxtral-mini-transcribe-latest")]
    [InlineData("voxtral-small-transcribe-3-0")]
    [InlineData("voxtral-small-transcribe-3")]
    [InlineData("voxtral-small-transcribe-latest")]
    [UpstreamTest(
        Id + "should forward word timestamps for the API to validate",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_forward_word_timestamps_for_the_API_to_validate(string voxtral3ModelId)
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler), voxtral3ModelId)
            .DoGenerateAsync(
                Call("{\"mistral\":{\"timestampGranularities\":[\"word\"]}}"),
                CancellationToken.None
            );

        Assert.Equal("word", Field(Assert.Single(handler.Calls), "timestamp_granularities"));
    }

    [Theory]
    [InlineData("voxtral-mini-transcribe-3-0")]
    [InlineData("voxtral-mini-transcribe-3")]
    [InlineData("voxtral-mini-transcribe-latest")]
    [InlineData("voxtral-small-transcribe-3-0")]
    [InlineData("voxtral-small-transcribe-3")]
    [InlineData("voxtral-small-transcribe-latest")]
    [UpstreamTest(
        Id + "should forward diarization without timestamps for the API to validate",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_forward_diarization_without_timestamps_for_the_API_to_validate(
        string voxtral3ModelId
    )
    {
        var handler = JsonHandler();

        await Transcription(CreateProvider(handler), voxtral3ModelId)
            .DoGenerateAsync(Call("{\"mistral\":{\"diarize\":true}}"), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        Assert.Equal("true", Field(call, "diarize"));
        Assert.Empty(Values(call, "timestamp_granularities"));
    }
}
