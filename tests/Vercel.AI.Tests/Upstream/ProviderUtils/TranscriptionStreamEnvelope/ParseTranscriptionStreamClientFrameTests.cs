// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Envelope = Vercel.AI.ProviderUtils.TranscriptionStreamEnvelope;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.TranscriptionStreamEnvelope;

/// <summary>Port of <c>transcription-stream-envelope.test.ts</c> &gt; <c>parseTranscriptionStreamClientFrame</c>.</summary>
public sealed class ParseTranscriptionStreamClientFrameTests
{
    private const string Prefix =
        "packages/provider-utils/src/transcription-stream-envelope.test.ts::parseTranscriptionStreamClientFrame::";

    [Fact]
    [UpstreamTest(
        Prefix + "should parse a minimal start frame",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_parse_a_minimal_start_frame()
    {
        var result = Envelope.ParseTranscriptionStreamClientFrame(
            """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm"}}"""
        );

        Assert.Equal("start", result.Type);
        Assert.Equal("transcription-stream.start", result.Frame!.Type);
        Assert.Equal("audio/pcm", result.Frame.InputAudioFormat.Type);
        Assert.Null(result.Frame.InputAudioFormat.Rate);
        Assert.Null(result.Frame.ProviderOptions);
        Assert.Null(result.Frame.IncludeRawChunks);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse a full start frame", Coverage = UpstreamCoverage.Covered)]
    public void Should_parse_a_full_start_frame()
    {
        var result = Envelope.ParseTranscriptionStreamClientFrame(
            """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm","rate":16000},"providerOptions":{"openai":{"language":"en"}},"includeRawChunks":true}"""
        );

        Assert.Equal("start", result.Type);
        Assert.Equal("audio/pcm", result.Frame!.InputAudioFormat.Type);
        Assert.Equal(16000, result.Frame.InputAudioFormat.Rate);
        Assert.Equal(
            """{"openai":{"language":"en"}}""",
            result.Frame.ProviderOptions!.Value.GetRawText()
        );
        Assert.True(result.Frame.IncludeRawChunks);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not restrict the audio format type",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_restrict_the_audio_format_type()
    {
        var result = Envelope.ParseTranscriptionStreamClientFrame(
            """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/some-future-format"}}"""
        );

        Assert.Equal("start", result.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should parse an audio-done frame", Coverage = UpstreamCoverage.Covered)]
    public void Should_parse_an_audio_done_frame()
    {
        Assert.Equal(
            "audio-done",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.audio-done"}"""
                )
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify unrecognized frame types as unknown",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_unrecognized_frame_types_as_unknown()
    {
        Assert.Equal(
            "unknown",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.some-future-frame"}"""
                )
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify malformed JSON as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_malformed_JSON_as_invalid()
    {
        var result = Envelope.ParseTranscriptionStreamClientFrame("{not json");

        Assert.Equal("invalid", result.Type);
        Assert.Equal("malformed JSON", result.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a prototype-pollution start frame as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_prototype_pollution_start_frame_as_invalid()
    {
        var frame =
            "{\"type\":\"transcription-stream.start\",\"inputAudioFormat\":{\"type\":\"audio/pcm\"},\"__proto__\":{\"polluted\":true}}";

        var result = Envelope.ParseTranscriptionStreamClientFrame(frame);

        Assert.Equal("invalid", result.Type);
        Assert.Equal("malformed JSON", result.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a constructor.prototype-pollution start frame as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_constructor_prototype_pollution_start_frame_as_invalid()
    {
        var frame =
            "{\"type\":\"transcription-stream.start\",\"inputAudioFormat\":{\"type\":\"audio/pcm\"},\"constructor\":{\"prototype\":{\"polluted\":true}}}";

        Assert.Equal("invalid", Envelope.ParseTranscriptionStreamClientFrame(frame).Type);
    }

    [Theory]
    [InlineData("\"a string\"")]
    [InlineData("[1, 2]")]
    [InlineData("null")]
    [InlineData("42")]
    [UpstreamTest(
        Prefix + "should classify non-object JSON as invalid: %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_non_object_JSON_as_invalid(string text)
    {
        Assert.Equal("invalid", Envelope.ParseTranscriptionStreamClientFrame(text).Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a missing frame type as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_missing_frame_type_as_invalid()
    {
        var result = Envelope.ParseTranscriptionStreamClientFrame("""{"foo":"bar"}""");

        Assert.Equal("invalid", result.Type);
        Assert.Equal("frame type must be a string", result.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a start frame without inputAudioFormat as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_start_frame_without_inputAudioFormat_as_invalid()
    {
        Assert.Equal(
            "invalid",
            Envelope
                .ParseTranscriptionStreamClientFrame("""{"type":"transcription-stream.start"}""")
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a start frame with a non-string format type as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_start_frame_with_a_non_string_format_type_as_invalid()
    {
        Assert.Equal(
            "invalid",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.start","inputAudioFormat":{"type":42}}"""
                )
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a start frame with a non-number rate as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_start_frame_with_a_non_number_rate_as_invalid()
    {
        Assert.Equal(
            "invalid",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm","rate":"16000"}}"""
                )
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a start frame with non-object providerOptions as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_start_frame_with_non_object_providerOptions_as_invalid()
    {
        Assert.Equal(
            "invalid",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm"},"providerOptions":"nope"}"""
                )
                .Type
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should classify a start frame with non-boolean includeRawChunks as invalid",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_classify_a_start_frame_with_non_boolean_includeRawChunks_as_invalid()
    {
        Assert.Equal(
            "invalid",
            Envelope
                .ParseTranscriptionStreamClientFrame(
                    """{"type":"transcription-stream.start","inputAudioFormat":{"type":"audio/pcm"},"includeRawChunks":"yes"}"""
                )
                .Type
        );
    }
}
