// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Mapper = Vercel.AI.OpenAI.OpenAIRealtimeEventMapper;
using SessionOptions = Vercel.AI.OpenAI.OpenAIRealtimeSessionOptions;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIRealtimeEventMapper;

/// <summary>Port of <c>openai-realtime-event-mapper.test.ts</c> &gt; <c>buildOpenAISessionConfig</c>.</summary>
public sealed class BuildOpenAISessionConfigTests
{
    private const string Prefix = RealtimeMapperSupport.File + "buildOpenAISessionConfig::";

    [Theory]
    [InlineData("voice")]
    [InlineData("instructions")]
    [InlineData("tools")]
    [InlineData("outputModalities")]
    [UpstreamTest(
        Prefix + "rejects unsupported transcription configuration: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = RealtimeMapperSupport.Commit
    )]
    public void Rejects_unsupported_transcription_configuration(string setting)
    {
        var config = new RealtimeModelV4SessionConfig();
        switch (setting)
        {
            case "voice":
                config.Voice = "alloy";
                break;
            case "instructions":
                config.Instructions = "hello";
                break;
            case "tools":
                config.Tools = [];
                break;
            case "outputModalities":
                config.OutputModalities = ["audio"];
                break;
        }

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Mapper.BuildOpenAISessionConfig(
                config,
                "gpt-realtime-whisper",
                new SessionOptions { Intent = RealtimeModelV4SessionIntent.Transcription }
            )
        );
        Assert.Matches("transcription sessions do not support", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "builds a dedicated transcription session with the selected model id",
        Coverage = UpstreamCoverage.Covered,
        Commit = RealtimeMapperSupport.Commit
    )]
    public void Builds_a_dedicated_transcription_session_with_the_selected_model_id()
    {
        var result = Mapper.BuildOpenAISessionConfig(
            new RealtimeModelV4SessionConfig
            {
                InputAudioFormatType = "audio/pcm",
                InputAudioFormatRate = 24000,
                InputAudioTranscription = new RealtimeModelV4AudioTranscription
                {
                    Model = "gpt-realtime-whisper",
                    Language = "en",
                    Prompt = "Medical vocabulary.",
                },
            },
            "gpt-realtime",
            new SessionOptions { Intent = RealtimeModelV4SessionIntent.Transcription }
        );

        OpenAIUpstream.Equal(
            result,
            "{\"type\":\"transcription\",\"audio\":{\"input\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},\"transcription\":{\"model\":\"gpt-realtime-whisper\",\"language\":\"en\",\"prompt\":\"Medical vocabulary.\"}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "enables input audio transcription with a default model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Enables_input_audio_transcription_with_a_default_model()
    {
        var result = Mapper.BuildOpenAISessionConfig(
            new RealtimeModelV4SessionConfig
            {
                InputAudioTranscription = new RealtimeModelV4AudioTranscription(),
            },
            "gpt-realtime"
        );

        OpenAIUpstream.Equal(
            result["audio"],
            "{\"input\":{\"transcription\":{\"model\":\"gpt-realtime-whisper\"}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps input audio transcription options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Maps_input_audio_transcription_options()
    {
        var result = Mapper.BuildOpenAISessionConfig(
            new RealtimeModelV4SessionConfig
            {
                InputAudioTranscription = new RealtimeModelV4AudioTranscription
                {
                    Model = "gpt-4o-mini-transcribe",
                    Language = "en",
                    Prompt = "Transcribe short voice chat messages.",
                },
            },
            "gpt-realtime"
        );

        OpenAIUpstream.Equal(
            result["audio"],
            "{\"input\":{\"transcription\":{\"model\":\"gpt-4o-mini-transcribe\",\"language\":\"en\",\"prompt\":\"Transcribe short voice chat messages.\"}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not build translation sessions until the normalized codec supports them",
        Coverage = UpstreamCoverage.Covered,
        Commit = RealtimeMapperSupport.Commit
    )]
    public void Does_not_build_translation_sessions_until_the_normalized_codec_supports_them()
    {
        // Upstream passes the unsupported intent 'translation' (`as never`).
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Mapper.BuildOpenAISessionConfig(
                new RealtimeModelV4SessionConfig(),
                "gpt-realtime-translate",
                new SessionOptions { Intent = (RealtimeModelV4SessionIntent)2 }
            )
        );
        Assert.Matches("is not supported", exception.Message);
    }
}
