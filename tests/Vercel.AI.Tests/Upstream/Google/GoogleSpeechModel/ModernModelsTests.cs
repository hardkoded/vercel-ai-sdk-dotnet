// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.Upstream.Google.GoogleSpeechModel;

/// <summary>Port of <c>google-speech-model.test.ts</c> &gt; <c>describe.each(modernModels)</c>.</summary>
public sealed class ModernModelsTests
{
    private const string PcmBase64 = "AQIDBAUGBwg=";
    private const string WavBase64 =
        "UklGRiwAAABXQVZFZm10IBAAAAABAAEAwF0AAIC7AAACABAAZGF0YQgAAAABAgMEBQYHCA==";
    private const string Multi =
        "{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}";

    public static TheoryData<string> ModernModels =>
        ["gemini-3.8-flash-tts", "gemini-3.8-flash-lite-tts", "custom-tts-model"];

    public static TheoryData<string, string, string, string> RawFormats =>
        new()
        {
            { "gemini-3.8-flash-tts", "pcm", "AUDIO_L16", "audio/l16" },
            { "gemini-3.8-flash-tts", "audio/l16", "AUDIO_L16", "audio/l16" },
            { "gemini-3.8-flash-tts", "mulaw", "AUDIO_MULAW", "audio/mulaw" },
            { "gemini-3.8-flash-tts", "audio/mulaw", "AUDIO_MULAW", "audio/mulaw" },
            { "gemini-3.8-flash-tts", "alaw", "AUDIO_ALAW", "audio/alaw" },
            { "gemini-3.8-flash-tts", "audio/alaw", "AUDIO_ALAW", "audio/alaw" },
            { "gemini-3.8-flash-lite-tts", "pcm", "AUDIO_L16", "audio/l16" },
            { "gemini-3.8-flash-lite-tts", "alaw", "AUDIO_ALAW", "audio/alaw" },
            { "custom-tts-model", "mulaw", "AUDIO_MULAW", "audio/mulaw" },
            { "custom-tts-model", "audio/alaw", "AUDIO_ALAW", "audio/alaw" },
        };

    private static (Vercel.AI.Google.GoogleSpeechModel Model, RecordingHandler Handler) Create(
        string modelId,
        string mimeType = "audio/wav",
        string data = WavBase64
    )
    {
        var handler = new RecordingHandler
        {
            ResponseText =
                "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\""
                + mimeType
                + "\",\"data\":\""
                + data
                + "\"}}]}}]}",
        };
        var provider = GoogleProvider.Create(
            new GoogleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (new Vercel.AI.Google.GoogleSpeechModel(provider, modelId), handler);
    }

    private static IReadOnlyDictionary<string, System.Text.Json.JsonElement> Options(string json)
    {
        return GoogleUpstream.ProviderOptions(json)!;
    }

    private static JsonNode Sent(RecordingHandler handler)
    {
        return JsonNode.Parse(handler.Body)!;
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::preserves the transcript and maps instructions to speech metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Preserves_the_transcript_and_maps_instructions_to_speech_metadata(
        string modelId
    )
    {
        var (model, handler) = Create(modelId);
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("Hello. <laugh> How are you? <short pause>")
                {
                    Instructions = "whispering",
                },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        GoogleUpstream.JsonEqual(
            Sent(handler),
            "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello. <laugh> How are you? <short pause>\",\"speechMetadata\":{\"style\":\"whispering\"}}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}}}"
        );
        Assert.Equal(Convert.FromBase64String(WavBase64), result.Audio);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::allows an empty style to override instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Allows_an_empty_style_to_override_instructions(string modelId)
    {
        var (model, handler) = Create(modelId);
        await model
            .GenerateAsync(
                new GoogleSpeechCall("Hello")
                {
                    Instructions = "excited",
                    ProviderOptions = Options("{\"google\":{\"speechMetadata\":{\"style\":\"\"}}}"),
                },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        var part = Sent(handler)["contents"]![0]!["parts"]![0]!;
        Assert.Equal("Hello", (string?)part["text"]);
        Assert.Equal("", (string?)part["speechMetadata"]!["style"]);
    }

    [Theory]
    [InlineData("gemini-3.8-flash-tts", "voice_custom")]
    [InlineData("gemini-3.8-flash-tts", "voicekey_custom")]
    [InlineData("gemini-3.8-flash-lite-tts", "voice_custom")]
    [InlineData("gemini-3.8-flash-lite-tts", "voicekey_custom")]
    [InlineData("custom-tts-model", "voice_custom")]
    [InlineData("custom-tts-model", "voicekey_custom")]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::rejects custom voice %s before fetching",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_custom_voice_before_fetching(string modelId, string voice)
    {
        var (model, handler) = Create(modelId);
        var error = await Assert
            .ThrowsAsync<ArgumentException>(() =>
                model.GenerateAsync(
                    new GoogleSpeechCall("Hello") { Voice = voice },
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);

        Assert.Equal("voice", error.ParamName);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData("gemini-3.8-flash-tts", "{\"voice\":\"voice_custom\"}")]
    [InlineData("gemini-3.8-flash-tts", "{\"voice\":\"voicekey_custom\"}")]
    [InlineData(
        "gemini-3.8-flash-tts",
        "{\"voice\":\"voice_custom\",\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}"
    )]
    [InlineData("gemini-3.8-flash-lite-tts", "{\"voice\":\"voice_custom\"}")]
    [InlineData("gemini-3.8-flash-lite-tts", "{\"voice\":\"voicekey_custom\"}")]
    [InlineData(
        "gemini-3.8-flash-lite-tts",
        "{\"voice\":\"voice_custom\",\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}"
    )]
    [InlineData("custom-tts-model", "{\"voice\":\"voice_custom\"}")]
    [InlineData("custom-tts-model", "{\"voice\":\"voicekey_custom\"}")]
    [InlineData(
        "custom-tts-model",
        "{\"voice\":\"voice_custom\",\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}"
    )]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::rejects custom speaker voice configuration %j before fetching",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_custom_speaker_voice_configuration_before_fetching(
        string modelId,
        string voiceConfig
    )
    {
        var (model, handler) = Create(modelId);
        var error = await Assert
            .ThrowsAsync<ArgumentException>(() =>
                model.GenerateAsync(
                    new GoogleSpeechCall("")
                    {
                        ProviderOptions = Options(
                            "{\"google\":{\"turns\":[{\"text\":\"Hello\",\"speechMetadata\":{\"speaker\":\"Joe\"}}],\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":"
                                + voiceConfig
                                + "}]}}}"
                        ),
                    },
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);

        Assert.Equal("providerOptions", error.ParamName);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::sends separate turns with explicit speakers and per-turn styles",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Sends_separate_turns_with_explicit_speakers_and_per_turn_styles(
        string modelId
    )
    {
        var (model, handler) = Create(modelId);
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("")
                {
                    Language = "de-DE",
                    Instructions = "speaking slowly",
                    ProviderOptions = Options(
                        "{\"google\":{\"multiSpeakerVoiceConfig\":"
                            + Multi
                            + ",\"turns\":[{\"text\":\"Hi.\",\"speechMetadata\":{\"speaker\":\"Joe\"}},{\"text\":\"<sigh> Hello.\",\"speechMetadata\":{\"speaker\":\"Jane\",\"style\":\"\"}}]}}"
                    ),
                },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        var sent = Sent(handler);
        var parts = sent["contents"]![0]!["parts"]!.AsArray();
        Assert.Equal("Hi.", (string?)parts[0]!["text"]);
        Assert.Equal("Joe", (string?)parts[0]!["speechMetadata"]!["speaker"]);
        Assert.Equal("speaking slowly", (string?)parts[0]!["speechMetadata"]!["style"]);
        Assert.Equal("<sigh> Hello.", (string?)parts[1]!["text"]);
        Assert.Equal("Jane", (string?)parts[1]!["speechMetadata"]!["speaker"]);
        Assert.Equal("", (string?)parts[1]!["speechMetadata"]!["style"]);
        var speechConfig = sent["generationConfig"]!["speechConfig"]!;
        Assert.Equal("de-DE", (string?)speechConfig["languageCode"]);
        Assert.True(
            JsonNode.DeepEquals(speechConfig["multiSpeakerVoiceConfig"], JsonNode.Parse(Multi))
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::forwards language alongside a single structured speech voice",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_language_alongside_a_single_structured_speech_voice(string modelId)
    {
        var (model, handler) = Create(modelId);
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("Hallo.") { Language = "de-DE" },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        var sent = Sent(handler);
        Assert.Equal("Hallo.", (string?)sent["contents"]![0]!["parts"]![0]!["text"]);
        var speechConfig = sent["generationConfig"]!["speechConfig"]!;
        Assert.Equal("de-DE", (string?)speechConfig["languageCode"]);
        Assert.Equal(
            "Kore",
            (string?)speechConfig["voiceConfig"]!["prebuiltVoiceConfig"]!["voiceName"]
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("gemini-3.8-flash-tts", null)]
    [InlineData("gemini-3.8-flash-tts", "Unknown")]
    [InlineData("gemini-3.8-flash-lite-tts", null)]
    [InlineData("gemini-3.8-flash-lite-tts", "Unknown")]
    [InlineData("custom-tts-model", null)]
    [InlineData("custom-tts-model", "Unknown")]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::rejects missing or unconfigured speaker %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_missing_or_unconfigured_speaker(string modelId, string? speaker)
    {
        var (model, handler) = Create(modelId);
        var metadata = speaker == null ? "{}" : "{\"speaker\":\"" + speaker + "\"}";
        var error = await Assert
            .ThrowsAsync<ArgumentException>(() =>
                model.GenerateAsync(
                    new GoogleSpeechCall("")
                    {
                        ProviderOptions = Options(
                            "{\"google\":{\"multiSpeakerVoiceConfig\":"
                                + Multi
                                + ",\"turns\":[{\"text\":\"Hello\",\"speechMetadata\":"
                                + metadata
                                + "}]}}"
                        ),
                    },
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);

        Assert.Contains("Every multi-speaker turn must specify", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::rejects a labelled transcript without structured speakers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_a_labelled_transcript_without_structured_speakers(string modelId)
    {
        var (model, handler) = Create(modelId);
        var error = await Assert
            .ThrowsAsync<ArgumentException>(() =>
                model.GenerateAsync(
                    new GoogleSpeechCall("Joe: Hi. Jane: Hello.")
                    {
                        ProviderOptions = Options(
                            "{\"google\":{\"multiSpeakerVoiceConfig\":" + Multi + "}}"
                        ),
                    },
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);

        Assert.Contains("Every multi-speaker turn must specify", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::rejects conflicting global and per-turn metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_conflicting_global_and_per_turn_metadata(string modelId)
    {
        var (model, _) = Create(modelId);
        var error = await Assert
            .ThrowsAsync<ArgumentException>(() =>
                model.GenerateAsync(
                    new GoogleSpeechCall("")
                    {
                        ProviderOptions = Options(
                            "{\"google\":{\"speechMetadata\":{\"style\":\"excited\"},\"turns\":[{\"text\":\"Hello\"}]}}"
                        ),
                    },
                    CancellationToken.None
                )
            )
            .ConfigureAwait(false);

        Assert.Contains("Set speechMetadata on each turn", error.Message);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::warns when turns replace nonempty top-level text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Warns_when_turns_replace_nonempty_top_level_text(string modelId)
    {
        var (model, handler) = Create(modelId);
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("Ignored")
                {
                    ProviderOptions = Options("{\"google\":{\"turns\":[{\"text\":\"Spoken\"}]}}"),
                },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        Assert.Contains(result.Warnings, warning => warning.Feature == "text");
        Assert.Equal("Spoken", (string?)Sent(handler)["contents"]![0]!["parts"]![0]!["text"]);
    }

    [Theory]
    [MemberData(nameof(RawFormats))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::requests %s and preserves raw bytes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Requests_output_format_and_preserves_raw_bytes(
        string modelId,
        string outputFormat,
        string mimeType,
        string responseMimeType
    )
    {
        var (model, handler) = Create(
            modelId,
            responseMimeType + "; rate=24000; channels=1",
            PcmBase64
        );
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("Hello") { OutputFormat = outputFormat },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        Assert.Equal(
            mimeType,
            (string?)Sent(handler)["generationConfig"]!["responseFormat"]!["audio"]!["mimeType"]
        );
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, result.Audio);
        Assert.Empty(result.Warnings);
        Assert.Equal(24000, result.SampleRate);
    }

    [Theory]
    [InlineData("gemini-3.8-flash-tts", "wav")]
    [InlineData("gemini-3.8-flash-tts", "audio/wav")]
    [InlineData("gemini-3.8-flash-lite-tts", "wav")]
    [InlineData("gemini-3.8-flash-lite-tts", "audio/wav")]
    [InlineData("custom-tts-model", "wav")]
    [InlineData("custom-tts-model", "audio/wav")]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::requests %s without adding another header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Requests_wav_without_adding_another_header(
        string modelId,
        string outputFormat
    )
    {
        var (model, handler) = Create(modelId);
        var result = await model
            .GenerateAsync(
                new GoogleSpeechCall("Hello") { OutputFormat = outputFormat },
                CancellationToken.None
            )
            .ConfigureAwait(false);

        Assert.Equal(
            "AUDIO_WAV",
            (string?)Sent(handler)["generationConfig"]!["responseFormat"]!["audio"]!["mimeType"]
        );
        Assert.Equal(Convert.FromBase64String(WavBase64), result.Audio);
    }

    [Theory]
    [MemberData(nameof(ModernModels))]
    [UpstreamTest(
        "packages/google/src/google-speech-model.test.ts::%s::returns empty audio without a WAV header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Returns_empty_audio_without_a_WAV_header(string modelId)
    {
        var (model, _) = Create(modelId, "audio/wav", "");
        var result = await model
            .GenerateAsync(new GoogleSpeechCall("Hello"), CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Empty(result.Audio);
    }
}
