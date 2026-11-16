// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleSpeechModel;

/// <summary>Port of <c>google-speech-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string AudioResponse = "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"AQID\"}}]}}]}";

    private static Vercel.AI.Google.GoogleSpeechModel SpeechModel(RecordingHandler handler)
    {
        return new Vercel.AI.Google.GoogleSpeechModel(GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler), "gemini-2.5-flash-preview-tts");
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

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::rejects an empty transcript before fetching for %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_empty_speech_transcript_before_the_request()
    {
        var handler = new RecordingHandler();
        var error = Assert.Throws<ArgumentException>(() => Vercel.AI.Google.GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google", new GoogleSpeechCall("")));
        Assert.Contains("non-empty transcript", error.Message);
        Assert.Equal(0, handler.Calls);
    }
}
