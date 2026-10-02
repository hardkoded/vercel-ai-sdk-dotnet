// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AssemblyAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Tests.Parity.Media;

namespace Vercel.AI.Tests;

public sealed class AssemblyAITranscriptionTests
{
    private const string ErrorBody = "\n{\"error\":{\"message\":\"{\\n  \\\"error\\\": {\\n    \\\"code\\\": 429,\\n    \\\"message\\\": \\\"Resource has been exhausted (e.g. check quota).\\\",\\n    \\\"status\\\": \\\"RESOURCE_EXHAUSTED\\\"\\n  }\\n}\\n\",\"code\":429}}\n";

    private const string UploadUrl = "https://api.assemblyai.com/v2/upload";

    private const string SubmitUrl = "https://api.assemblyai.com/v2/transcript";

    private const string PollUrl = "https://api.assemblyai.com/v2/transcript/9ea68fd3-f953-42c1-9742-976c447fb463";

    private const string Completed = "{\"id\":\"9ea68fd3-f953-42c1-9742-976c447fb463\",\"status\":\"completed\",\"text\":\"Hello, world!\",\"language_code\":\"en_us\",\"audio_duration\":281,\"words\":[{\"confidence\":0.97465,\"start\":250,\"end\":650,\"text\":\"Hello,\",\"speaker\":\"speaker\"},{\"confidence\":0.99999,\"start\":730,\"end\":1022,\"text\":\"world\",\"speaker\":\"speaker\"}],\"utterances\":[{\"speaker\":\"A\",\"text\":\"Hello, world!\",\"start\":250,\"end\":26950}],\"entities\":[{\"entity_type\":\"location\",\"text\":\"Canada\",\"start\":2548,\"end\":3130}],\"sentiment_analysis_results\":[{\"text\":\"Hello, world!\",\"start\":250,\"end\":26950,\"sentiment\":\"POSITIVE\",\"confidence\":0.9,\"speaker\":\"A\"}],\"content_safety_labels\":{\"status\":\"success\"},\"iab_categories_result\":{\"status\":\"success\"},\"auto_highlights_result\":{\"status\":\"success\"},\"chapters\":[{\"gist\":\"Hello, world!\"}],\"summary\":\"- Hello, world!\"}";

    private static readonly byte[] Audio = new byte[] { 1, 2, 3, 4 };

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-error.test.ts::assemblyaiErrorDataSchema::should parse AssemblyAI resource exhausted error",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_a_resource_exhausted_error()
    {
        Assert.True(AssemblyAIError.TryParse(ErrorBody, out var error));
        Assert.NotNull(error);
        Assert.Equal(429, error!.Code);
        Assert.Contains("Resource has been exhausted (e.g. check quota).", error.Message, StringComparison.Ordinal);
        Assert.Contains("RESOURCE_EXHAUSTED", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass the legacy model via the speech_model parameter",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_best_as_speech_model_and_warns()
    {
        var handler = Server();
        var result = await Transcribe(handler, "best");
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal("https://storage.assemblyai.com/mock-upload-url", body["audio_url"]!.GetValue<string>());
        Assert.Equal("best", body["speech_model"]!.GetValue<string>());
        Assert.Null(body["speech_models"]);
        var warning = Assert.Single(result.Warnings, item => item.Type == "deprecated");
        Assert.Equal("model 'best'", warning.Setting);
        Assert.Contains("universal-3-5-pro", warning.Message, StringComparison.Ordinal);
        Assert.Contains("https://www.assemblyai.com/docs/pre-recorded-audio/select-the-speech-model", warning.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass newer models via the speech_models parameter",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_universal_3_5_pro_without_warnings()
    {
        var handler = Server();
        var result = await Transcribe(handler, "universal-3-5-pro");
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal("https://storage.assemblyai.com/mock-upload-url", body["audio_url"]!.GetValue<string>());
        Assert.Equal("universal-3-5-pro", body["speech_models"]![0]!.GetValue<string>());
        Assert.Null(body["speech_model"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should route universal-3-pro via speech_models and nudge to universal-3-5-pro",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Nudges_universal_3_pro_toward_the_flagship()
    {
        var handler = Server();
        var result = await Transcribe(handler, "universal-3-pro");
        var body = MediaJson.Parse(handler.Calls[1].Body);
        var nudge = Assert.Single(result.Warnings, item => item.Type == "other");

        Assert.Equal("universal-3-pro", body["speech_models"]![0]!.GetValue<string>());
        Assert.Null(body["speech_model"]);
        Assert.Contains("universal-3-5-pro", nudge.Message, StringComparison.Ordinal);
        Assert.Contains("replace 'universal-3-pro'", nudge.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should nudge universal-2 users toward universal-3-5-pro",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Nudges_universal_2_without_claiming_a_replacement()
    {
        var result = await Transcribe(Server(), "universal-2");
        var nudge = Assert.Single(result.Warnings, item => item.Type == "other");

        Assert.Contains("universal-3-5-pro", nudge.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("replace 'universal-3-pro'", nudge.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should not special-case the removed nano model",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_nano_as_a_speech_model_without_a_deprecation()
    {
        var handler = Server();
        var result = await Transcribe(handler, "nano");
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal("nano", body["speech_models"]![0]!.GetValue<string>());
        Assert.Null(body["speech_model"]);
        Assert.DoesNotContain(result.Warnings, item => item.Type == "deprecated");
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should still send provider options alongside speech_models",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_language_detection_and_punctuation_with_the_model()
    {
        var handler = Server();
        await Transcribe(handler, "universal-3-5-pro", Options("{\"languageDetection\":true,\"punctuate\":false}"));
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal("universal-3-5-pro", body["speech_models"]![0]!.GetValue<string>());
        Assert.True(body["language_detection"]!.GetValue<bool>());
        Assert.False(body["punctuate"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should surface diarization + audio-intelligence via providerMetadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Copies_diarization_and_intelligence_into_provider_metadata()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro");
        var metadata = result.ProviderMetadata!["assemblyai"]!;

        Assert.Equal("A", metadata["utterances"]![0]!["speaker"]!.GetValue<string>());
        Assert.Equal("Hello, world!", metadata["utterances"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("location", metadata["entities"]![0]!["entity_type"]!.GetValue<string>());
        Assert.Equal("Canada", metadata["entities"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("POSITIVE", metadata["sentimentAnalysisResults"]![0]!["sentiment"]!.GetValue<string>());
        Assert.Equal("Hello, world!", metadata["sentimentAnalysisResults"]![0]!["text"]!.GetValue<string>());
        Assert.NotNull(metadata["contentSafetyLabels"]);
        Assert.NotNull(metadata["iabCategoriesResult"]);
        Assert.NotNull(metadata["autoHighlightsResult"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should preserve the full raw response on response.body",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_the_raw_transcript_body()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro");

        Assert.Equal("speaker", result.ResponseBody!["words"]![0]!["speaker"]!.GetValue<string>());
        Assert.NotNull(result.ResponseBody["chapters"]);
        Assert.Equal("- Hello, world!", result.ResponseBody["summary"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass the Universal-3-Pro input params",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_prompt_keyterms_temperature_and_domain()
    {
        var handler = Server();
        await Transcribe(
            handler,
            "universal-3-5-pro",
            Options("{\"prompt\":\"This is a conversation about the AI SDK.\",\"keytermsPrompt\":[\"Vercel\",\"AI SDK\"],\"temperature\":0.2,\"removeAudioTags\":\"speaker\",\"domain\":\"medical-v1\"}"));
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal("universal-3-5-pro", body["speech_models"]![0]!.GetValue<string>());
        Assert.Equal("This is a conversation about the AI SDK.", body["prompt"]!.GetValue<string>());
        Assert.Equal("Vercel", body["keyterms_prompt"]![0]!.GetValue<string>());
        Assert.Equal("AI SDK", body["keyterms_prompt"]![1]!.GetValue<string>());
        Assert.Equal(0.2, body["temperature"]!.GetValue<double>());
        Assert.Equal("speaker", body["remove_audio_tags"]!.GetValue<string>());
        Assert.Equal("medical-v1", body["domain"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass the GA nested input params",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_nested_speaker_language_and_redaction_options()
    {
        var handler = Server();
        await Transcribe(
            handler,
            "universal-3-5-pro",
            Options("{\"redactPii\":true,\"speakerOptions\":{\"minSpeakersExpected\":1,\"maxSpeakersExpected\":3},\"languageDetectionOptions\":{\"expectedLanguages\":[\"en\",\"es\"],\"fallbackLanguage\":\"en\",\"codeSwitching\":true,\"codeSwitchingConfidenceThreshold\":0.5},\"redactPiiAudioOptions\":{\"returnRedactedNoSpeechAudio\":true,\"overrideAudioRedactionMethod\":\"silence\"},\"redactPiiReturnUnredacted\":true,\"redactStaticEntities\":{\"INTERNAL_TOOL\":[\"Bearclaw\"]}}"));
        var body = MediaJson.Parse(handler.Calls[1].Body);

        Assert.Equal(1, body["speaker_options"]!["min_speakers_expected"]!.GetValue<int>());
        Assert.Equal(3, body["speaker_options"]!["max_speakers_expected"]!.GetValue<int>());
        Assert.Equal("en", body["language_detection_options"]!["expected_languages"]![0]!.GetValue<string>());
        Assert.Equal("es", body["language_detection_options"]!["expected_languages"]![1]!.GetValue<string>());
        Assert.Equal("en", body["language_detection_options"]!["fallback_language"]!.GetValue<string>());
        Assert.True(body["language_detection_options"]!["code_switching"]!.GetValue<bool>());
        Assert.Equal(0.5, body["language_detection_options"]!["code_switching_confidence_threshold"]!.GetValue<double>());
        Assert.True(body["redact_pii_audio_options"]!["return_redacted_no_speech_audio"]!.GetValue<bool>());
        Assert.Equal("silence", body["redact_pii_audio_options"]!["override_audio_redaction_method"]!.GetValue<string>());
        Assert.True(body["redact_pii_return_unredacted"]!.GetValue<bool>());
        Assert.Equal("Bearclaw", body["redact_static_entities"]!["INTERNAL_TOOL"]![0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should warn when deprecated wordBoost/boostParam options are used",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_word_boost_and_boost_param_are_set()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro", Options("{\"wordBoost\":[\"Vercel\"],\"boostParam\":\"high\"}"));
        var warning = Assert.Single(result.Warnings, item => item.Type == "deprecated");

        Assert.Equal("wordBoost, boostParam", warning.Setting);
        Assert.Contains("keytermsPrompt", warning.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should attribute the deprecation warning to boostParam when only boostParam is set",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Attributes_the_boost_deprecation_to_boost_param()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro", Options("{\"boostParam\":\"high\"}"));
        var warning = Assert.Single(result.Warnings, item => item.Type == "deprecated");

        Assert.Equal("boostParam", warning.Setting);
        Assert.Contains("keytermsPrompt", warning.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should warn when redactPii-dependent options are set without redactPii",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_redaction_options_lack_redact_pii()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro", Options("{\"redactStaticEntities\":{\"TOOL\":[\"Vercel\"]}}"));

        Assert.Contains(result.Warnings, item => item.Type == "other" && item.Message.Contains("redactPii", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should warn when redactPiiAudioOptions is set without redactPiiAudio",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_audio_redaction_options_lack_the_flag()
    {
        var result = await Transcribe(
            Server(),
            "universal-3-5-pro",
            Options("{\"redactPii\":true,\"redactPiiAudioOptions\":{\"overrideAudioRedactionMethod\":\"silence\"}}"));

        Assert.Contains(result.Warnings, item => item.Type == "other" && item.Message.Contains("redactPiiAudio", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should warn when languageCode and languageDetection are combined",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Warns_when_language_code_and_detection_are_combined()
    {
        var result = await Transcribe(Server(), "universal-3-5-pro", Options("{\"languageCode\":\"en\",\"languageDetection\":true}"));

        Assert.Contains(result.Warnings, item => item.Type == "other" && item.Message.Contains("languageDetection", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should report segment timings in seconds (ms converted)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Converts_word_timings_from_milliseconds()
    {
        var result = await Transcribe(Server(), "best");

        Assert.Equal("Hello,", result.Segments[0].Text);
        Assert.Equal(0.25, result.Segments[0].StartSecond);
        Assert.Equal(0.65, result.Segments[0].EndSecond);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_api_key_user_agent_and_custom_headers()
    {
        var handler = Server();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = Model(handler, "best", options);
        await model.TranscribeAsync(
            new AssemblyAITranscriptionRequest(Audio)
            {
                MediaType = "audio/wav",
                Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
            },
            CancellationToken.None);

        Assert.Equal("test-api-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("application/octet-stream", handler.Calls[0].Header("content-type"));
        Assert.Equal("provider-header-value", handler.Calls[0].Header("custom-provider-header"));
        Assert.Equal("request-header-value", handler.Calls[0].Header("custom-request-header"));
        Assert.Contains("ai-sdk/assemblyai/0.0.0-test", handler.Calls[0].Header("user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should extract the transcription text",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_transcript_text()
    {
        var result = await Transcribe(Server(), "best");

        Assert.Equal("Hello, world!", result.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_timestamp_model_id_and_poll_headers()
    {
        var handler = Server(new Dictionary<string, string>
        {
            ["x-request-id"] = "test-request-id",
            ["x-ratelimit-remaining"] = "123",
        });
        var model = new AssemblyAITranscriptionModel(
            new HttpClient(handler, disposeHandler: false),
            "best",
            "test-provider",
            "https://api.assemblyai.com",
            () => new Dictionary<string, string?>())
        {
            PollInterval = TimeSpan.Zero,
            Clock = () => DateTimeOffset.UnixEpoch,
        };
        var result = await model.TranscribeAsync(new AssemblyAITranscriptionRequest(Audio), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("best", result.ModelId);
        Assert.Equal("application/json", result.ResponseHeaders["content-type"]);
        Assert.Equal("test-request-id", result.ResponseHeaders["x-request-id"]);
        Assert.Equal("123", result.ResponseHeaders["x-ratelimit-remaining"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should use real date when no custom date provider is specified",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_injected_clock_for_the_response_timestamp()
    {
        var model = Model(Server(), "best");
        model.Clock = () => DateTimeOffset.UnixEpoch;
        var result = await model.TranscribeAsync(new AssemblyAITranscriptionRequest(Audio), CancellationToken.None);

        Assert.Equal(DateTimeOffset.UnixEpoch, result.Timestamp);
        Assert.Equal("best", result.ModelId);
    }

    private static Task<AssemblyAITranscriptionResult> Transcribe(MediaHandler handler, string modelId, JsonElement? options = null)
    {
        return Model(handler, modelId).TranscribeAsync(
            new AssemblyAITranscriptionRequest(Audio) { MediaType = "audio/wav", ProviderOptions = options },
            CancellationToken.None);
    }

    private static AssemblyAITranscriptionModel Model(MediaHandler handler, string modelId, OpenAICompatibleOptions? options = null)
    {
        options ??= new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        var model = (AssemblyAITranscriptionModel)AssemblyAIProvider.Create(options, handler).TranscriptionModel(modelId);
        model.PollInterval = TimeSpan.Zero;
        return model;
    }

    private static MediaHandler Server(IDictionary<string, string>? pollHeaders = null)
    {
        var handler = new MediaHandler();
        handler.Json(UploadUrl, "{\"upload_url\":\"https://storage.assemblyai.com/mock-upload-url\"}");
        handler.Json(SubmitUrl, "{\"id\":\"9ea68fd3-f953-42c1-9742-976c447fb463\",\"status\":\"queued\"}");
        handler.Json(PollUrl, Completed, pollHeaders);
        return handler;
    }

    private static JsonElement Options(string json)
    {
        using (var document = JsonDocument.Parse(json))
        {
            return document.RootElement.Clone();
        }
    }
}
