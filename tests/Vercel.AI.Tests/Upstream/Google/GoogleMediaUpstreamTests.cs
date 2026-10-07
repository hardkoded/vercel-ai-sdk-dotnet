// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Embeddings, images, speech, and transcription.</summary>
public sealed class GoogleMediaUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_a_single_embedding_vector()
    {
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1,2,3]}}" };
        var result = await GoogleProvider.Create(Key(), handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "sunny day" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new[] { 1f, 2f, 3f }, result.Embeddings[0]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_embedding_model_and_text()
    {
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1]}}" };
        await GoogleProvider.Create(Key(), handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "sunny day" }, null, CancellationToken.None).ConfigureAwait(false);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body), "{\"model\":\"models/embedding-001\",\"content\":{\"parts\":[{\"text\":\"sunny day\"}]}}");
        Assert.Contains(":embedContent", handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should pass the outputDimensionality setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_outputDimensionality()
    {
        var options = Key();
        options.EmbeddingProviderOptions = GoogleUpstream.Element("{\"outputDimensionality\":256}");
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1]}}" };
        await GoogleProvider.Create(options, handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "sunny" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(256, JsonNode.Parse(handler.Body)!["outputDimensionality"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should pass the taskType setting", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_embedding_task_type()
    {
        var options = Key();
        options.EmbeddingProviderOptions = GoogleUpstream.Element("{\"taskType\":\"RETRIEVAL_DOCUMENT\"}");
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1]}}" };
        await GoogleProvider.Create(options, handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "sunny" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("RETRIEVAL_DOCUMENT", (string?)JsonNode.Parse(handler.Body)!["taskType"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should throw an error if too many values are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_more_than_one_hundred_embedding_values()
    {
        var handler = new RecordingHandler();
        var values = Enumerable.Range(0, 101).Select(index => "v" + index).ToArray();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => GoogleProvider.Create(Key(), handler).EmbeddingModel("embedding-001").DoEmbedAsync(values, null, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("100", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should expose the Google batch embedding API limit", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_the_batch_embedding_limit()
    {
        Assert.Equal(100, GoogleEmbeddingModel.MaxEmbeddingsPerCall);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should use the batch embeddings endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_batchEmbedContents_for_several_values()
    {
        var handler = new RecordingHandler { ResponseText = "{\"embeddings\":[{\"values\":[1]},{\"values\":[2]}]}" };
        var result = await GoogleProvider.Create(Key(), handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "a", "b" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(":batchEmbedContents", handler.Uris[0]);
        Assert.Equal(2, result.Embeddings.Count);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-embedding-model.test.ts::GoogleEmbeddingModel::should use the single embeddings endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_embedContent_for_one_value()
    {
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1]}}" };
        await GoogleProvider.Create(Key(), handler).EmbeddingModel("embedding-001").DoEmbedAsync(new[] { "a" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(":embedContent", handler.Uris[0]);
        Assert.DoesNotContain("batchEmbedContents", handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > maxImagesPerCall::should default to a supported per-call limit", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_max_images_per_call_to_one()
    {
        var model = new GoogleImageModel(GoogleProvider.Create(Key(), new RecordingHandler()), "gemini-2.5-flash-image");
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > maxImagesPerCall::should respect a custom setting", Coverage = UpstreamCoverage.Covered)]
    public void Honors_a_custom_max_images_per_call()
    {
        var model = new GoogleImageModel(GoogleProvider.Create(Key(), new RecordingHandler()), "gemini-2.5-flash-image", 5);
        Assert.Equal(5, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should reject non-Gemini model IDs before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_Gemini_image_models_before_the_request()
    {
        var handler = new RecordingHandler();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => new GoogleImageModel(GoogleProvider.Create(Key(), handler), "imagen-3.0-generate-002").DoGenerateAsync(new ImageCallOptions("cat"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("gemini-", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-image-model.test.ts::GoogleVertexImageModel > doGenerate::should reject non-Gemini model IDs before sending a request", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_non_Gemini_image_models_on_Vertex_before_the_request()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "test-api-key" }, handler);
        var error = await Assert.ThrowsAsync<AiSdkException>(() => provider.ImageModel("imagen-3.0-generate-002").DoGenerateAsync(new ImageCallOptions("cat"), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("gemini-", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should use the language model endpoint and extract generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task Posts_generateContent_and_extracts_image_bytes()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[{\"content\":{\"parts\":[{\"inlineData\":{\"mimeType\":\"image/png\",\"data\":\"AQID\"}}]}}]}" };
        var result = await new GoogleImageModel(GoogleProvider.Create(Key(), handler), "gemini-2.5-flash-image").DoGenerateAsync(new ImageCallOptions("cat"), CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(":generateContent", handler.Uris[0]);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Images[0].Data);
        Assert.Equal("image/png", result.Images[0].MediaType);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-image-model.test.ts::GoogleImageModel > doGenerate::should return a warning for the unsupported size option", Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_that_image_size_is_unsupported()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[]}" };
        var result = await new GoogleImageModel(GoogleProvider.Create(Key(), handler), "gemini-2.5-flash-image").DoGenerateAsync(new ImageCallOptions("cat") { Size = "1024x1024" }, CancellationToken.None).ConfigureAwait(false);
        var image = Assert.IsType<GoogleImageResult>(result);
        Assert.Contains(image.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "size");
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should send the text and the default voice", Coverage = UpstreamCoverage.Covered)]
    public void Sends_speech_text_and_the_default_Kore_voice()
    {
        var prepared = GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", new GoogleSpeechCall("Hello from the AI SDK!"));
        GoogleUpstream.JsonEqual(prepared.Body, "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"Hello from the AI SDK!\"}]}],\"generationConfig\":{\"responseModalities\":[\"AUDIO\"],\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}}}");
        Assert.Empty(prepared.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::should use the provided voice", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_requested_speech_voice()
    {
        var prepared = GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google.generative-ai", new GoogleSpeechCall("Hello from the AI SDK!") { Voice = "Puck" });
        Assert.Equal("Puck", (string?)prepared.Body["generationConfig"]!["speechConfig"]!["voiceConfig"]!["prebuiltVoiceConfig"]!["voiceName"]);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::doGenerate::rejects an empty transcript before fetching for %s", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_empty_speech_transcript_before_the_request()
    {
        var handler = new RecordingHandler();
        var error = Assert.Throws<ArgumentException>(() => GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google", new GoogleSpeechCall("")));
        Assert.Contains("non-empty transcript", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-model.test.ts::%s::rejects custom voice %s before fetching", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_custom_voice_before_the_request()
    {
        var error = Assert.Throws<ArgumentException>(() => GoogleSpeechModel.Prepare("gemini-2.5-flash-preview-tts", "google", new GoogleSpeechCall("Hello") { Voice = "voice_test" }));
        Assert.Contains("Custom voices", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::extracts only transcript text, preserving Unicode and inline tags", Coverage = UpstreamCoverage.Covered)]
    public void Joins_structured_speech_turns_without_a_separator()
    {
        var inspection = GoogleSpeechInput.Inspect("Ignored top-level text", null, GoogleUpstream.Element("{\"turns\":[{\"text\":\"Hello <sigh>\"},{\"text\":\"世界 👋\"}]}"));
        Assert.Equal("Hello <sigh>世界 👋", inspection.Text);
        Assert.False(inspection.UsesCustomVoice);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::preserves an empty structured transcript instead of using top-level text", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_an_empty_structured_transcript()
    {
        var inspection = GoogleSpeechInput.Inspect("Ignored", null, GoogleUpstream.Element("{\"turns\":[{\"text\":\"\"}]}"));
        Assert.Equal(string.Empty, inspection.Text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::leaves malformed options to provider validation: %j", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_the_top_level_transcript_for_malformed_speech_options()
    {
        foreach (var google in new string?[] { null, "null", "\"invalid\"", "{\"turns\":[]}", "{\"turns\":\"Hello\"}", "{\"turns\":[null]}", "{\"turns\":[{\"text\":123}]}", "{\"turns\":[{\"text\":\"Partial\"},{}]}" })
        {
            JsonElement? element = google == null ? null : GoogleUpstream.Element(google);
            var inspection = GoogleSpeechInput.Inspect("Hello", null, element);
            Assert.Equal("Hello", inspection.Text);
            Assert.False(inspection.UsesCustomVoice);
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::identifies a top-level custom voice: %s", Coverage = UpstreamCoverage.Covered)]
    public void Identifies_voice_and_voicekey_prefixes_as_custom_voices()
    {
        Assert.True(GoogleSpeechInput.Inspect("Hello", "voice_test", null).UsesCustomVoice);
        Assert.True(GoogleSpeechInput.Inspect("Hello", "voicekey_test", null).UsesCustomVoice);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::identifies explicit custom voice fields regardless of their value: %j", Coverage = UpstreamCoverage.Covered)]
    public void Treats_any_multi_speaker_voice_field_as_a_custom_voice()
    {
        foreach (var voice in new[] { "\"voice_test\"", "\"voicekey_test\"", "\"unprefixed-id\"", "\"\"", "null" })
        {
            var google = GoogleUpstream.Element("{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Alice\",\"voiceConfig\":{\"voice\":" + voice + "}}]}}");
            Assert.True(GoogleSpeechInput.Inspect("Hello", "Kore", google).UsesCustomVoice);
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::recognizes custom voices even when multi-speaker configuration overrides voice", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_custom_top_level_voice_when_speakers_are_prebuilt()
    {
        var google = GoogleUpstream.Element("{\"multiSpeakerVoiceConfig\":{\"speakerVoiceConfigs\":[{\"speaker\":\"Alice\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}]}}");
        Assert.True(GoogleSpeechInput.Inspect("Hello", "voice_test", google).UsesCustomVoice);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-speech-input.test.ts::getGoogleSpeechInput::does not infer custom voices from unrelated fields: %j", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_treat_prebuilt_speaker_voices_as_custom()
    {
        foreach (var config in new[] { "null", "\"invalid\"", "{\"speakerVoiceConfigs\":null}", "{\"speakerVoiceConfigs\":[null,{},{\"voiceConfig\":null}]}", "{\"speakerVoiceConfigs\":[{\"speaker\":\"Alice\",\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"Kore\"}}}]}" })
        {
            var google = GoogleUpstream.Element("{\"multiSpeakerVoiceConfig\":" + config + "}");
            Assert.False(GoogleSpeechInput.Inspect("Hello", "Kore", google).UsesCustomVoice);
        }
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::omits generation_config when no transcription options are set", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_generation_config_and_reads_the_transcript()
    {
        var handler = new RecordingHandler { ResponseText = "{\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"Hello world.\"}]}]}" };
        var result = await GoogleProvider.Create(Key(), handler).TranscriptionModel("gemini-3.5-transcribe").DoTranscribeAsync(new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", null), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Hello world.", result.Text);
        Assert.EndsWith("/interactions", handler.Uris[0]);
        Assert.False(JsonNode.Parse(handler.Body)!.AsObject().ContainsKey("generation_config"));
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body)!["input"]!, "[{\"type\":\"audio\",\"data\":\"AQIDBA==\",\"mime_type\":\"audio/wav\"}]");
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::transcribes audio via the Interactions API with transcription_config", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_sends_options_as_transcription_config()
    {
        var handler = new RecordingHandler { ResponseText = "{\"id\":\"interactions/test\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"Hello world.\"}]}],\"usage\":{\"total_tokens\":10,\"total_input_tokens\":10,\"total_output_tokens\":0}}" };
        var result = await Transcribe(handler, "{\"google\":{\"customVocabulary\":[\"Gemini\",\"Kubernetes\"],\"languageCodes\":[\"es-ES\"],\"mode\":\"SMART\"}}");
        Assert.Equal("Hello world.", result.Text);
        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body),
            "{\"model\":\"gemini-3.5-transcribe\",\"input\":[{\"type\":\"audio\",\"data\":\"AQIDBA==\",\"mime_type\":\"audio/wav\"}],\"generation_config\":{\"transcription_config\":{\"language_codes\":[\"es-ES\"],\"custom_vocabulary\":[\"Gemini\",\"Kubernetes\"],\"mode\":{\"type\":\"smart\"}}}}");
        Assert.Equal("test-api-key", handler.RequestHeaders["x-goog-api-key"]);
        GoogleUpstream.JsonEqual(result.ProviderMetadata!.Value, "{\"google\":{\"usage\":{\"total_tokens\":10,\"total_input_tokens\":10,\"total_output_tokens\":0}}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::maps diarization and word timestamps into the mode object", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_maps_diarization_and_word_timestamps_into_the_mode()
    {
        var handler = new RecordingHandler { ResponseText = "{\"status\":\"completed\",\"steps\":[]}" };
        await Transcribe(handler, "{\"google\":{\"diarization\":true,\"wordTimestamp\":true}}");
        GoogleUpstream.JsonEqual(
            JsonNode.Parse(handler.Body)!["generation_config"]!["transcription_config"],
            "{\"mode\":{\"type\":\"verbatim\",\"diarization_mode\":\"speaker\",\"timestamp_granularities\":[\"word\"]}}");
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::extracts word segments from word_info annotations", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_reads_word_segments_from_annotations()
    {
        var handler = new RecordingHandler
        {
            ResponseText = "{\"id\":\"interactions/test\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"The quick brown fox.\",\"annotations\":["
                + "{\"type\":\"word_info\",\"text\":\"The\",\"speaker\":\"spk:0\",\"start_offset\":\"0.100s\",\"end_offset\":\"0.100s\"},"
                + "{\"type\":\"word_info\",\"text\":\"quick\",\"speaker\":\"spk:0\",\"start_offset\":\"0.100s\",\"end_offset\":\"0.400s\"},"
                + "{\"type\":\"word_info\",\"text\":\"brown\",\"speaker\":\"spk:0\",\"start_offset\":\"0.400s\",\"end_offset\":\"0.700s\"},"
                + "{\"type\":\"word_info\",\"text\":\"fox.\",\"speaker\":\"spk:0\",\"start_offset\":\"0.700s\",\"end_offset\":\"1s\"}]}]}],\"usage\":{\"total_input_tokens\":64}}",
        };
        var result = await Transcribe(handler, "{}");
        Assert.Equal("The quick brown fox.", result.Text);
        Assert.Equal(
            new[] { ("The", 0.1, 0.1), ("quick", 0.1, 0.4), ("brown", 0.4, 0.7), ("fox.", 0.7, 1.0) },
            result.Segments.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doGenerate::rejects unary transcription on live model ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_unary_transcription_for_a_live_model()
    {
        var handler = new RecordingHandler();
        var error = await Assert.ThrowsAsync<ArgumentException>(() => GoogleProvider.Create(Key(), handler).TranscriptionModel("gemini-3.5-transcribe-live").DoTranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", null), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("only supports streaming transcription", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google/src/transcription/google-transcription-model.test.ts::doStream::rejects streaming on unary model ids", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_live_setup_for_a_unary_transcription_model()
    {
        var error = Assert.Throws<ArgumentException>(() => GoogleTranscriptionModel.BuildLiveSetup("gemini-3.5-transcribe"));
        Assert.Contains("does not support streaming transcription", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doGenerate::rejects unary transcription on live model ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_Vertex_unary_transcription_for_a_live_model()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "test-api-key" }, handler);
        var error = await Assert.ThrowsAsync<ArgumentException>(() => provider.TranscriptionModel("gemini-3.5-transcribe-live").DoTranscribeAsync(new AudioInput(new byte[] { 1 }, "audio/wav", null), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("only supports streaming transcription", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doGenerate::transcribes audio via Vertex generateContent with audioTranscriptionConfig", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcribes_Vertex_audio_with_an_audio_transcription_config()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello \"},{\"text\":\"world.\"}]}}],\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":4}}" };
        var result = await VertexGeminiTranscription(handler).DoGenerateAsync(TranscriptionCall("{\"googleVertex\":{\"customVocabulary\":[\"Gemini\",\"Kubernetes\"],\"languageCodes\":[\"es-ES\"],\"mode\":\"SMART\"}}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("Hello world.", result.Text);
        Assert.Equal(VertexTranscriptionBaseUrl + "/models/gemini-3.5-transcribe:generateContent", handler.Uris[0]);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body), "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"inlineData\":{\"mimeType\":\"audio/wav\",\"data\":\"AQIDBA==\"}}]}],\"generationConfig\":{\"audioTranscriptionConfig\":{\"languageCodes\":[\"es-ES\"],\"customVocabulary\":[\"Gemini\",\"Kubernetes\"],\"mode\":\"SMART\"}}}");
        Assert.Equal("Bearer test-oauth-token", handler.RequestHeaders["Authorization"]);
        GoogleUpstream.JsonEqual(result.ProviderMetadata!.Value, "{\"google\":{\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":4}}}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doGenerate::accepts options under the google namespace as a fallback", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_Vertex_transcription_options_from_the_google_namespace()
    {
        var handler = new RecordingHandler { ResponseText = "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello world.\"}]}}]}" };
        await VertexGeminiTranscription(handler).DoGenerateAsync(TranscriptionCall("{\"google\":{\"mode\":\"SMART\"}}"), CancellationToken.None).ConfigureAwait(false);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body)!["generationConfig"], "{\"audioTranscriptionConfig\":{\"mode\":\"SMART\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/gemini-transcription/google-vertex-gemini-transcription-model.test.ts::doGenerate::extracts text, language, and word segments from the audioTranscription part", Coverage = UpstreamCoverage.Covered)]
    public async Task Reads_Vertex_text_language_and_word_segments_from_the_audio_transcription()
    {
        var handler = new RecordingHandler
        {
            ResponseText = "{\"candidates\":[{\"content\":{\"parts\":[{\"audioTranscription\":{\"text\":\"The quick brown fox.\",\"languageCode\":\"en-US\",\"speakerLabel\":\"spk:0\",\"words\":["
                + "{\"word\":\"The\",\"startOffset\":\"0.100s\",\"endOffset\":\"0.100s\"},"
                + "{\"word\":\"quick\",\"startOffset\":\"0.100s\",\"endOffset\":\"0.400s\"},"
                + "{\"word\":\"brown\",\"startOffset\":\"0.400s\",\"endOffset\":\"0.700s\"},"
                + "{\"word\":\"fox.\",\"startOffset\":\"0.700s\",\"endOffset\":\"1s\"}]}}]}}],\"usageMetadata\":{\"promptTokenCount\":64}}",
        };
        var result = await VertexGeminiTranscription(handler).DoGenerateAsync(TranscriptionCall("{}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("The quick brown fox.", result.Text);
        Assert.Equal("en-US", result.Language);
        Assert.Equal(
            new[] { ("The", 0.1, 0.1), ("quick", 0.1, 0.4), ("brown", 0.4, 0.7), ("fox.", 0.7, 1d) },
            result.Segments.Select(segment => (segment.Text, segment.StartSecond, segment.EndSecond)));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding > maxEmbeddingsPerCall::should limit predict endpoint models to 250 values per call", Coverage = UpstreamCoverage.Covered)]
    public async Task Limits_Vertex_predict_embeddings_to_250_values()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var values = Enumerable.Range(0, 251).Select(index => "v").ToArray();
        var error = await Assert.ThrowsAsync<AiSdkException>(() => provider.EmbeddingModel("text-embedding-005").DoEmbedAsync(values, null, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("250", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding > maxEmbeddingsPerCall::should limit %s to one value per call", Coverage = UpstreamCoverage.Covered)]
    public async Task Limits_gemini_embedding_2_to_one_value()
    {
        var handler = new RecordingHandler();
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var error = await Assert.ThrowsAsync<AiSdkException>(() => provider.EmbeddingModel("gemini-embedding-2").DoEmbedAsync(new[] { "a", "b" }, null, CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("1", error.Message);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding::should use embedContent for gemini-embedding-2", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_embedContent_for_Vertex_gemini_embedding_2()
    {
        var handler = new RecordingHandler { ResponseText = "{\"embedding\":{\"values\":[1,2]}}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var result = await provider.EmbeddingModel("gemini-embedding-2").DoEmbedAsync(new[] { "hello" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(":embedContent", handler.Uris[0]);
        Assert.Equal(new[] { 1f, 2f }, result.Embeddings[0]);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-embedding-model.test.ts::GoogleVertexEmbeddingModel > embedding::should extract embeddings", Coverage = UpstreamCoverage.Covered)]
    public async Task Extracts_Vertex_predict_embeddings()
    {
        var handler = new RecordingHandler { ResponseText = "{\"predictions\":[{\"embeddings\":{\"values\":[1,2]}},{\"embeddings\":{\"values\":[3]}}]}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var result = await provider.EmbeddingModel("text-embedding-005").DoEmbedAsync(new[] { "a", "b" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains(":predict", handler.Uris[0]);
        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(3f, result.Embeddings[1][0]);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should target the Cloud Text-to-Speech synthesize endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Posts_Chirp_speech_to_the_synthesize_endpoint()
    {
        var handler = new RecordingHandler { ResponseText = "{\"audioContent\":\"AQIDBAUGBwg=\"}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "k" }, handler);
        await provider.SpeechModel("chirp-3-hd").DoGenerateAsync(new SpeechCallOptions("Hello from the AI SDK!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(GoogleVertexCloudSpeechModel.SynthesizeUrl, handler.Uris[0]);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should send text, a default voice and LINEAR16 audio config", Coverage = UpstreamCoverage.Covered)]
    public void Sends_the_default_Chirp_voice_and_LINEAR16()
    {
        var prepared = GoogleVertexCloudSpeechModel.Prepare("Hello from the AI SDK!", null, null, null, null);
        GoogleUpstream.JsonEqual(prepared.Body, "{\"input\":{\"text\":\"Hello from the AI SDK!\"},\"voice\":{\"languageCode\":\"en-US\",\"name\":\"en-US-Chirp3-HD-Kore\"},\"audioConfig\":{\"audioEncoding\":\"LINEAR16\"}}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should pass a fully-qualified Chirp 3: HD voice name through verbatim", Coverage = UpstreamCoverage.Covered)]
    public void Passes_a_fully_qualified_Chirp_voice_name()
    {
        var prepared = GoogleVertexCloudSpeechModel.Prepare("Bonjour !", "fr-FR-Chirp3-HD-Charon", null, null, null);
        GoogleUpstream.JsonEqual(prepared.Body["voice"]!, "{\"languageCode\":\"fr-FR\",\"name\":\"fr-FR-Chirp3-HD-Charon\"}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should default the language for a Chirp 3: HD voice name without a locale prefix", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_the_language_for_an_unprefixed_Chirp_voice()
    {
        var prepared = GoogleVertexCloudSpeechModel.Prepare("Hello!", "Chirp3-HD-Kore", null, null, null);
        GoogleUpstream.JsonEqual(prepared.Body["voice"]!, "{\"languageCode\":\"en-US\",\"name\":\"Chirp3-HD-Kore\"}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should decode the base64 audio content", Coverage = UpstreamCoverage.Covered)]
    public async Task Decodes_Chirp_audio_content()
    {
        var handler = new RecordingHandler { ResponseText = "{\"audioContent\":\"AQIDBAUGBwg=\"}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var result = await provider.SpeechModel("chirp-3-hd").DoGenerateAsync(new SpeechCallOptions("Hello!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, result.Audio);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-cloud-tts-speech-model.test.ts::doGenerate::should return empty audio when the response has no audio content", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_empty_audio_when_Chirp_omits_audio_content()
    {
        var handler = new RecordingHandler { ResponseText = "{}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "p", Region = "us-central1", ApiKey = "k" }, handler);
        var result = await provider.SpeechModel("chirp-3-hd").DoGenerateAsync(new SpeechCallOptions("Hello!"), CancellationToken.None).ConfigureAwait(false);
        Assert.Empty(result.Audio);
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-transcription-model.test.ts::doGenerate::should send the model, languageCodes, features and base64 content", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_recognize_body()
    {
        var handler = new RecordingHandler { ResponseText = "{\"results\":[{\"alternatives\":[{\"transcript\":\"hello world\"}]}]}" };
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", ApiKey = "k" }, handler);
        var result = await provider.TranscriptionModel("chirp_2").DoTranscribeAsync(new AudioInput(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "audio/wav", null), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("hello world", result.Text);
        GoogleUpstream.JsonEqual(JsonNode.Parse(handler.Body), "{\"config\":{\"model\":\"chirp_2\",\"languageCodes\":[\"auto\"],\"autoDecodingConfig\":{},\"features\":{\"enableWordTimeOffsets\":true,\"enableAutomaticPunctuation\":true}},\"content\":\"AQIDBAUGBwg=\"}");
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-transcription-model.test.ts::doGenerate::should target the regional Speech-to-Text endpoint", Coverage = UpstreamCoverage.Covered)]
    public void Targets_the_regional_recognize_endpoint()
    {
        Assert.Equal("https://us-central1-speech.googleapis.com/v2/projects/test-project/locations/us-central1/recognizers/_:recognize", GoogleVertexEndpoints.RecognizeUrl("test-project", "us-central1"));
    }

    [Fact]
    [UpstreamTest("packages/google-vertex/src/google-vertex-transcription-model.test.ts::doGenerate::should use the unprefixed host for the global location", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_unprefixed_speech_host_for_global()
    {
        Assert.Equal("https://speech.googleapis.com/v2/projects/test-project/locations/global/recognizers/_:recognize", GoogleVertexEndpoints.RecognizeUrl("test-project", "global"));
    }


    private const string VertexTranscriptionBaseUrl = "https://us-central1-aiplatform.googleapis.com/v1beta1/projects/test-project/locations/us-central1/publishers/google";

    private static Task<TranscriptionModelResult> Transcribe(RecordingHandler handler, string providerOptions)
    {
        var model = (GoogleTranscriptionModel)GoogleProvider.Create(Key(), handler).TranscriptionModel("gemini-3.5-transcribe");
        var call = new TranscriptionModelCall(new byte[] { 1, 2, 3, 4 }, "audio/wav", GoogleUpstream.Element(providerOptions), new Dictionary<string, string>(), CancellationToken.None);
        return model.DoGenerateAsync(call, CancellationToken.None);
    }

    private static GoogleOptions Key()
    {
        return new GoogleOptions { ApiKey = "test-api-key" };
    }

    private static GoogleVertexGeminiTranscriptionModel VertexGeminiTranscription(RecordingHandler handler)
    {
        var provider = GoogleVertexProvider.Create(new VertexOptions { Project = "test-project", Region = "us-central1", BaseUrl = VertexTranscriptionBaseUrl, ApiKey = "test-oauth-token" }, handler);
        return (GoogleVertexGeminiTranscriptionModel)provider.TranscriptionModel("gemini-3.5-transcribe");
    }

    private static TranscriptionModelCall TranscriptionCall(string providerOptions)
    {
        using var options = JsonDocument.Parse(providerOptions);
        return new TranscriptionModelCall(new byte[] { 1, 2, 3, 4 }, "audio/wav", options.RootElement.Clone(), new Dictionary<string, string>(), CancellationToken.None);
    }

}
