// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleSpeechModel;

/// <summary>Port of <c>google-speech-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    // 8 bytes of raw PCM ([1..8]) base64-encoded.
    private const string PcmBase64 = "AQIDBAUGBwg=";
    private const string WavBase64 = "UklGRiwAAABXQVZFZm10IBAAAAABAAEAwF0AAIC7AAACABAAZGF0YQgAAAABAgMEBQYHCA==";
    private static readonly byte[] PcmBytes = [1, 2, 3, 4, 5, 6, 7, 8];

    private const string AudioResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"AQID\"}}]}}]}";

    private static readonly string[] ModernModels = ["gemini-3.8-flash-tts", "gemini-3.8-flash-lite-tts", "custom-tts-model"];

    private static Vercel.AI.Google.GoogleSpeechModel SpeechModel(RecordingHandler handler, string modelId = "gemini-2.5-flash-preview-tts", Func<DateTimeOffset>? clock = null)
    {
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        if (clock != null)
        {
            provider.Clock = clock;
        }

        return new Vercel.AI.Google.GoogleSpeechModel(provider, modelId);
    }

    private static string AudioBody(string mimeType = "audio/L16;rate=24000", string data = PcmBase64)
    {
        return "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"" + mimeType + "\",\"data\":\"" + data + "\"}}]}}]}";
    }

    private static uint UInt32At(byte[] audio, int offset)
    {
        return BitConverter.ToUInt32(audio, offset);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should send the text and the default voice", Coverage = UpstreamCoverage.Covered)]
    public void Sends_speech_text_and_the_default_Kore_voice()
    {
        var prepared = Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", new GoogleSpeechCall("Hello from the AI SDK!"));
        GoogleUpstream.JsonEqual(prepared.Body, "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello from the AI SDK!\"}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}}}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should use the provided voice", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_requested_speech_voice()
    {
        var prepared = Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", new GoogleSpeechCall("Hello from the AI SDK!") { Voice = "Puck" });
        Assert.Equal("Puck", (string?)prepared.Body["generationConfig"]!["speechConfig"]!["voiceConfig"]!["prebuiltVoiceConfig"]!["voiceName"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should warn for the unsupported speed option", Coverage = UpstreamCoverage.Covered, Commit = "3c6fb4322cc8b12a1b6cfb2d6564c90272a54770")]
    public async Task Warns_for_the_unsupported_speech_speed_option()
    {
        var model = SpeechModel(new RecordingHandler { ResponseText = AudioResponse });
        await model.DoGenerateAsync(new SpeechCallOptions("Hello from the AI SDK!") { Speed = 1.5, Language = "en" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(model.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "speed");
        Assert.DoesNotContain(model.Warnings, warning => warning.Feature == "language");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should map language onto single-speaker speechConfig", Coverage = UpstreamCoverage.Covered, Commit = "3c6fb4322cc8b12a1b6cfb2d6564c90272a54770")]
    public async Task Maps_speech_language_onto_the_single_speaker_speech_config()
    {
        var handler = new RecordingHandler { ResponseText = AudioResponse };
        await SpeechModel(handler).DoGenerateAsync(new SpeechCallOptions("Hello from the AI SDK!") { Language = "en-US" }, CancellationToken.None).ConfigureAwait(false);
        GoogleUpstream.JsonEqual(System.Text.Json.Nodes.JsonNode.Parse(handler.Body)!["generationConfig"]!["speechConfig"]!, "{\"languageCode\":\"en-US\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData("auto", false)]
    [InlineData("auto", true)]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::omits an automatic language without warnings (%j)", Coverage = UpstreamCoverage.Covered, Commit = "3c6fb4322cc8b12a1b6cfb2d6564c90272a54770")]
    public void Omits_an_automatic_speech_language_without_warnings(string? language, bool multi)
    {
        var call = new GoogleSpeechCall(multi ? "Joe: Hello." : "Hello.") { Language = language };
        if (multi)
        {
            call.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}]}}}");
        }

        var prepared = Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", call);
        Assert.Null(prepared.Body["generationConfig"]!["speechConfig"]!["languageCode"]);
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should map multi-speaker provider options into speechConfig", Coverage = UpstreamCoverage.Covered, Commit = "3c6fb4322cc8b12a1b6cfb2d6564c90272a54770")]
    public void Maps_multi_speaker_speech_provider_options_into_the_speech_config()
    {
        var prepared = Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", new GoogleSpeechCall("Joe: Hi. Jane: Hello.")
        {
            Language = "en-US",
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}}}"),
        });
        GoogleUpstream.JsonEqual(prepared.Body, "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Joe: Hi. Jane: Hello.\"}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"languageCode\":\"en-US\",\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should read provider options under `googleVertex` for a Vertex provider", Coverage = UpstreamCoverage.Covered, Commit = "3c6fb4322cc8b12a1b6cfb2d6564c90272a54770")]
    public void Reads_speech_provider_options_under_googleVertex_for_a_Vertex_provider()
    {
        var prepared = Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.vertex.speech", new GoogleSpeechCall("Joe: Hi. Jane: Hello.")
        {
            Language = "en-US",
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"googleVertex\":{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}}}"),
        });
        GoogleUpstream.JsonEqual(prepared.Body, "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Joe: Hi. Jane: Hello.\"}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"languageCode\":\"en-US\",\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}}}}");
        Assert.Empty(prepared.Warnings);
    }

    [Theory]
    [InlineData("gemini-2.5-flash-preview-tts")]
    [InlineData("gemini-3.1-flash-tts-preview")]
    [InlineData("gemini-3.8-flash-tts")]
    [InlineData("gemini-3.8-flash-lite-tts")]
    [InlineData("custom-tts-model")]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::preserves raw usage including cached input tokens for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_raw_usage_including_cached_input_tokens(string modelId)
    {
        const string body = "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"data\":\"" + WavBase64 + "\",\"mimeType\":\"audio/wav\"}}]}}],\"usageMetadata\":{\"promptTokenCount\":6,\"cachedContentTokenCount\":5,\"candidatesTokenCount\":67,\"totalTokenCount\":73,\"candidatesTokensDetails\":[{\"modality\":\"AUDIO\",\"tokenCount\":67}]}}";
        var model = SpeechModel(new RecordingHandler { ResponseText = body }, modelId);

        var result = await model.GenerateAsync(new GoogleSpeechCall("Hello."), CancellationToken.None).ConfigureAwait(false);

        GoogleUpstream.JsonEqual(result.ResponseBody, body);
        GoogleUpstream.JsonEqual(result.Usage, "{\"promptTokenCount\":6,\"cachedContentTokenCount\":5,\"candidatesTokenCount\":67,\"totalTokenCount\":73,\"candidatesTokensDetails\":[{\"modality\":\"AUDIO\",\"tokenCount\":67}]}");
    }

    [Theory]
    [InlineData("gemini-2.5-flash-preview-tts")]
    [InlineData("gemini-2.5-pro-preview-tts")]
    [InlineData("gemini-2.5-flash-tts")]
    [InlineData("gemini-3.1-flash-tts-preview")]
    [InlineData("gemini-3.1-flash-tts")]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::preserves legacy requests and PCM conversion for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_legacy_requests_and_PCM_conversion(string modelId)
    {
        var handler = new RecordingHandler { ResponseText = AudioBody() };
        var result = await SpeechModel(handler, modelId).GenerateAsync(new GoogleSpeechCall("Hello.") { Instructions = "Whisper", OutputFormat = "wav" }, CancellationToken.None).ConfigureAwait(false);

        GoogleUpstream.JsonEqual(System.Text.Json.Nodes.JsonNode.Parse(handler.Body), "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Whisper: Hello.\"}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}}}");
        Assert.Equal(Convert.FromBase64String(WavBase64), result.Audio);
    }

    [Theory]
    [InlineData("gemini-2.5-flash-preview-tts")]
    [InlineData("gemini-3.1-flash-tts-preview")]
    [InlineData("gemini-3.8-flash-tts")]
    [InlineData("gemini-3.8-flash-lite-tts")]
    [InlineData("custom-tts-model")]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::rejects an empty transcript before fetching for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_an_empty_transcript_before_fetching(string modelId)
    {
        var handler = new RecordingHandler();
        var model = SpeechModel(handler, modelId);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => model.DoGenerateAsync(new SpeechCallOptions("") { Instructions = "Whisper" }, CancellationToken.None)).ConfigureAwait(false);

        Assert.Equal("text", error.ParamName);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData("gemini-3.8-flash-tts")]
    [InlineData("gemini-3.8-flash-lite-tts")]
    [InlineData("custom-tts-model")]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::rejects empty turns even with top-level text for %s", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_empty_turns_even_with_top_level_text(string modelId)
    {
        var handler = new RecordingHandler();
        var model = SpeechModel(handler, modelId);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => model.GenerateAsync(new GoogleSpeechCall("Ignored") { ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"turns\":[{\"text\":\"\"}]}}") }, CancellationToken.None)).ConfigureAwait(false);

        Assert.Equal("text", error.ParamName);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_and_request_headers()
    {
        var handler = new RecordingHandler { ResponseText = AudioBody() };
        var options = new GoogleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = GoogleProvider.Create(options, handler);

        await new Vercel.AI.Google.GoogleSpeechModel(provider, "gemini-2.5-flash-preview-tts").GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!") { Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" } }, CancellationToken.None).ConfigureAwait(false);

        Assert.Contains("application/json", handler.RequestHeaders["Content-Type"], StringComparison.Ordinal);
        Assert.Equal("test-api-key", handler.RequestHeaders["x-goog-api-key"]);
        Assert.Equal("provider-header-value", handler.RequestHeaders["custom-provider-header"]);
        Assert.Equal("request-header-value", handler.RequestHeaders["custom-request-header"]);
        Assert.Contains("ai-sdk/google", handler.RequestHeaders["user-agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should wrap PCM audio in a WAV container by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Wraps_PCM_audio_in_a_WAV_container_by_default()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody() }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        var audio = result.Audio;

        // 44-byte WAV header + 8 bytes PCM payload.
        Assert.Equal(52, audio.Length);
        Assert.Equal(new byte[] { 0x52, 0x49, 0x46, 0x46 }, audio[..4]);
        Assert.Equal(new byte[] { 0x57, 0x41, 0x56, 0x45 }, audio[8..12]);
        Assert.Equal(24000u, UInt32At(audio, 24));
        Assert.Equal(1, BitConverter.ToUInt16(audio, 22));
        Assert.Equal(16, BitConverter.ToUInt16(audio, 34));
        Assert.Equal(PcmBytes, audio[44..]);
        Assert.Equal("audio/wav", result.MediaType);
        Assert.Equal("audio/L16;rate=24000", result.MimeType);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should preserve WAV responses even from older models", Coverage = UpstreamCoverage.Covered)]
    public async Task Preserves_WAV_responses_even_from_older_models()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody("audio/wav", WavBase64) }).GenerateAsync(new GoogleSpeechCall("Hello"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(Convert.FromBase64String(WavBase64), result.Audio);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should derive the WAV sample rate from the response mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task Derives_the_WAV_sample_rate_from_the_response_mime_type()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody("audio/L16;rate=16000") }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(16000u, UInt32At(result.Audio, 24));
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should return raw PCM and warn for outputFormat \"pcm\"", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_raw_PCM_and_warns_for_outputFormat_pcm()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody() }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!") { OutputFormat = "pcm" }, CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(PcmBytes, result.Audio);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "outputFormat");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should prepend instructions to the prompt text", Coverage = UpstreamCoverage.Covered)]
    public async Task Prepends_instructions_to_the_prompt_text()
    {
        var handler = new RecordingHandler { ResponseText = AudioBody() };
        await SpeechModel(handler).GenerateAsync(new GoogleSpeechCall("Hello there") { Instructions = "Say cheerfully" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Say cheerfully: Hello there", (string?)System.Text.Json.Nodes.JsonNode.Parse(handler.Body)!["contents"]![0]!["parts"]![0]!["text"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should ignore instructions (with a warning) when multi-speaker is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Ignores_instructions_with_a_warning_when_multi_speaker_is_set()
    {
        var handler = new RecordingHandler { ResponseText = AudioBody() };
        var result = await SpeechModel(handler).GenerateAsync(new GoogleSpeechCall("Joe: Hi. Jane: Hello.")
        {
            Instructions = "Say cheerfully",
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Joe\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}},{\"speaker\":\"Jane\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Puck\"}}}]}}}"),
        }, CancellationToken.None).ConfigureAwait(false);

        // instructions are NOT prepended to the multi-speaker transcript.
        Assert.Equal("Joe: Hi. Jane: Hello.", (string?)System.Text.Json.Nodes.JsonNode.Parse(handler.Body)!["contents"]![0]!["parts"]![0]!["text"]);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "instructions");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should expose sample rate and mime type in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Exposes_sample_rate_and_mime_type_in_provider_metadata()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody() }).DoGenerateAsync(new SpeechModelCall("Hello from the AI SDK!", null, null, null, null, null, default, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None).ConfigureAwait(false);
        GoogleUpstream.JsonEqual(result.ProviderMetadata!.Value, "{\"google\":{\"sampleRate\":24000,\"mimeType\":\"audio/L16;rate=24000\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should return empty audio when no inline data is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_empty_audio_when_no_inline_data_is_present()
    {
        var body = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"no audio here\"}]}}]}";
        var result = await SpeechModel(new RecordingHandler { ResponseText = body }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Empty(result.Audio);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_data_with_timestamp_model_id_and_headers()
    {
        var handler = new RecordingHandler { ResponseText = AudioBody() };
        handler.ResponseHeaders["x-request-id"] = "test-request-id";
        var testDate = DateTimeOffset.UnixEpoch;

        var result = await SpeechModel(handler, clock: () => testDate).DoGenerateAsync(new SpeechModelCall("Hello from the AI SDK!", null, null, null, null, null, default, new Dictionary<string, string>(), CancellationToken.None), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("gemini-2.5-flash-preview-tts", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should use the real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_real_date_when_no_custom_date_provider_is_specified()
    {
        var before = DateTimeOffset.UtcNow;
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody() }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(result.Timestamp, before, after);
        Assert.Equal("gemini-2.5-flash-preview-tts", result.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should have no warnings on the happy path", Coverage = UpstreamCoverage.Covered)]
    public async Task Has_no_warnings_on_the_happy_path()
    {
        var result = await SpeechModel(new RecordingHandler { ResponseText = AudioBody() }).GenerateAsync(new GoogleSpeechCall("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Empty(result.Warnings);
    }
}
