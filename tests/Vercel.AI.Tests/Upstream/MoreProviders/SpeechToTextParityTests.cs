// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AssemblyAI;
using Vercel.AI.Gladia;
using Vercel.AI.Hume;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.RevAI;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class SpeechToTextParityTests
{
    private const string ExhaustedMessage = "{\n  \"error\": {\n    \"code\": 429,\n    \"message\": \"Resource has been exhausted (e.g. check quota).\",\n    \"status\": \"RESOURCE_EXHAUSTED\"\n  }\n}\n";

    private const string RevTranscript = "{\"monologues\":[{\"speaker\":0,\"elements\":[{\"type\":\"text\",\"value\":\"Hello\",\"ts\":0.075,\"end_ts\":0.425},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"from\",\"ts\":0.425,\"end_ts\":0.665},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"the\",\"ts\":0.665,\"end_ts\":0.785},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"Sal\",\"ts\":0.945,\"end_ts\":1.105},{\"type\":\"punct\",\"value\":\",\"},{\"type\":\"punct\",\"value\":\" \"},{\"type\":\"text\",\"value\":\"A-I-S-D-K\",\"ts\":1.185,\"end_ts\":2.145},{\"type\":\"punct\",\"value\":\".\"}]}]}";

    private static string ExhaustedJson()
    {
        return new JsonObject
        {
            ["error"] = new JsonObject { ["message"] = ExhaustedMessage, ["code"] = 429 },
        }.ToJsonString();
    }

    private const string AssemblyAIModelTest = "packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::";

    private const string AssemblyAITranscriptId = "9ea68fd3-f953-42c1-9742-976c447fb463";

    [Fact]
    [UpstreamTest("packages/assemblyai/src/assemblyai-error.test.ts::assemblyaiErrorDataSchema::should parse AssemblyAI resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void AssemblyAI_parses_the_resource_exhausted_error()
    {
        var result = AssemblyAIError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should pass the legacy model via the speech_model parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_best_as_speech_model_with_a_deprecation()
    {
        var handler = AssemblyAIHandler();
        var result = await AssemblyAI(handler).Transcription("best").TranscribeAsync(Audio(), null, CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("https://storage.assemblyai.com/mock-upload-url", body.GetProperty("audio_url").GetString());
        Assert.Equal("best", body.GetProperty("speech_model").GetString());
        Assert.False(body.TryGetProperty("speech_models", out _));
        var deprecation = Assert.IsType<DeprecatedWarning>(Assert.Single(result.Warnings));
        Assert.Equal("model 'best'", deprecation.Setting);
        Assert.Contains("universal-3-5-pro", deprecation.Message, StringComparison.Ordinal);
        Assert.Contains("https://www.assemblyai.com/docs/pre-recorded-audio/select-the-speech-model", deprecation.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should pass newer models via the speech_models parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_newer_models_in_speech_models()
    {
        var handler = AssemblyAIHandler();
        var result = await AssemblyAI(handler).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), null, CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("https://storage.assemblyai.com/mock-upload-url", body.GetProperty("audio_url").GetString());
        Assert.Equal("[\"universal-3-5-pro\"]", body.GetProperty("speech_models").GetRawText());
        Assert.False(body.TryGetProperty("speech_model", out _));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should route universal-3-pro via speech_models and nudge to universal-3-5-pro", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_nudges_universal_3_pro_toward_its_replacement()
    {
        var handler = AssemblyAIHandler();
        var result = await AssemblyAI(handler).Transcription("universal-3-pro").TranscribeAsync(Audio(), null, CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("[\"universal-3-pro\"]", body.GetProperty("speech_models").GetRawText());
        Assert.False(body.TryGetProperty("speech_model", out _));
        var nudge = result.Warnings.OfType<OtherWarning>().First();
        Assert.Contains("universal-3-5-pro", nudge.Message, StringComparison.Ordinal);
        Assert.Contains("replace 'universal-3-pro'", nudge.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should nudge universal-2 users toward universal-3-5-pro", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_nudges_universal_2_without_claiming_a_replacement()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-2").TranscribeAsync(Audio(), null, CancellationToken.None);
        var nudge = result.Warnings.OfType<OtherWarning>().First();
        Assert.Contains("universal-3-5-pro", nudge.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("replace 'universal-3-pro'", nudge.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should not special-case the removed nano model", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_nano_in_speech_models_without_a_deprecation()
    {
        var handler = AssemblyAIHandler();
        var result = await AssemblyAI(handler).Transcription("nano").TranscribeAsync(Audio(), null, CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("[\"nano\"]", body.GetProperty("speech_models").GetRawText());
        Assert.False(body.TryGetProperty("speech_model", out _));
        Assert.Empty(result.Warnings.OfType<DeprecatedWarning>());
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should still send provider options alongside speech_models", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_provider_options_with_speech_models()
    {
        var handler = AssemblyAIHandler();
        await AssemblyAI(handler).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest("{\"languageDetection\":true,\"punctuate\":false}"), CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("[\"universal-3-5-pro\"]", body.GetProperty("speech_models").GetRawText());
        Assert.True(body.GetProperty("language_detection").GetBoolean());
        Assert.False(body.GetProperty("punctuate").GetBoolean());
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should surface diarization + audio-intelligence via providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_returns_diarization_and_audio_intelligence_metadata()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.NotNull(result.ProviderMetadata);
        var metadata = result.ProviderMetadata!.Value.GetProperty("assemblyai");
        var utterance = metadata.GetProperty("utterances")[0];
        Assert.Equal("A", utterance.GetProperty("speaker").GetString());
        Assert.Equal("Hello, world!", utterance.GetProperty("text").GetString());
        var entity = metadata.GetProperty("entities")[0];
        Assert.Equal("location", entity.GetProperty("entity_type").GetString());
        Assert.Equal("Canada", entity.GetProperty("text").GetString());
        var sentiment = metadata.GetProperty("sentimentAnalysisResults")[0];
        Assert.Equal("POSITIVE", sentiment.GetProperty("sentiment").GetString());
        Assert.Equal("Hello, world!", sentiment.GetProperty("text").GetString());
        Assert.True(metadata.TryGetProperty("contentSafetyLabels", out _));
        Assert.True(metadata.TryGetProperty("iabCategoriesResult", out _));
        Assert.True(metadata.TryGetProperty("autoHighlightsResult", out _));
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should preserve the full raw response on response.body", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_keeps_the_raw_transcript_as_the_response_body()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), null, CancellationToken.None);
        using var body = JsonDocument.Parse(result.Response.Body);
        Assert.Equal("speaker", body.RootElement.GetProperty("words")[0].GetProperty("speaker").GetString());
        Assert.True(body.RootElement.TryGetProperty("chapters", out _));
        Assert.Equal("- Hello, world!", body.RootElement.GetProperty("summary").GetString());
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should pass the Universal-3-Pro input params", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_the_universal_3_pro_input_params()
    {
        var handler = AssemblyAIHandler();
        var options = "{\"prompt\":\"This is a conversation about the AI SDK.\",\"keytermsPrompt\":[\"Vercel\",\"AI SDK\"],\"temperature\":0.2,\"removeAudioTags\":\"speaker\",\"domain\":\"medical-v1\"}";
        await AssemblyAI(handler).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest(options), CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("[\"universal-3-5-pro\"]", body.GetProperty("speech_models").GetRawText());
        Assert.Equal("This is a conversation about the AI SDK.", body.GetProperty("prompt").GetString());
        Assert.Equal("[\"Vercel\",\"AI SDK\"]", body.GetProperty("keyterms_prompt").GetRawText());
        Assert.Equal(0.2, body.GetProperty("temperature").GetDouble());
        Assert.Equal("speaker", body.GetProperty("remove_audio_tags").GetString());
        Assert.Equal("medical-v1", body.GetProperty("domain").GetString());
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should pass the GA nested input params", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_sends_the_nested_input_params_in_snake_case()
    {
        var handler = AssemblyAIHandler();
        var options = "{\"redactPii\":true,"
            + "\"speakerOptions\":{\"minSpeakersExpected\":1,\"maxSpeakersExpected\":3},"
            + "\"languageDetectionOptions\":{\"expectedLanguages\":[\"en\",\"es\"],\"fallbackLanguage\":\"en\",\"codeSwitching\":true,\"codeSwitchingConfidenceThreshold\":0.5},"
            + "\"redactPiiAudioOptions\":{\"returnRedactedNoSpeechAudio\":true,\"overrideAudioRedactionMethod\":\"silence\"},"
            + "\"redactPiiReturnUnredacted\":true,"
            + "\"redactStaticEntities\":{\"INTERNAL_TOOL\":[\"Bearclaw\"]}}";
        await AssemblyAI(handler).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest(options), CancellationToken.None);
        var body = AssemblyAISubmitBody(handler);
        Assert.Equal("{\"min_speakers_expected\":1,\"max_speakers_expected\":3}", body.GetProperty("speaker_options").GetRawText());
        Assert.Equal(
            "{\"expected_languages\":[\"en\",\"es\"],\"fallback_language\":\"en\",\"code_switching\":true,\"code_switching_confidence_threshold\":0.5}",
            body.GetProperty("language_detection_options").GetRawText());
        Assert.Equal(
            "{\"return_redacted_no_speech_audio\":true,\"override_audio_redaction_method\":\"silence\"}",
            body.GetProperty("redact_pii_audio_options").GetRawText());
        Assert.True(body.GetProperty("redact_pii_return_unredacted").GetBoolean());
        Assert.Equal("{\"INTERNAL_TOOL\":[\"Bearclaw\"]}", body.GetProperty("redact_static_entities").GetRawText());
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should warn when deprecated wordBoost/boostParam options are used", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_deprecates_word_boost_and_boost_param()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest("{\"wordBoost\":[\"Vercel\"],\"boostParam\":\"high\"}"), CancellationToken.None);
        var deprecation = Assert.Single(result.Warnings.OfType<DeprecatedWarning>());
        Assert.Equal("wordBoost, boostParam", deprecation.Setting);
        Assert.Contains("keytermsPrompt", deprecation.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should attribute the deprecation warning to boostParam when only boostParam is set", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_deprecates_boost_param_alone()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest("{\"boostParam\":\"high\"}"), CancellationToken.None);
        var deprecation = Assert.Single(result.Warnings.OfType<DeprecatedWarning>());
        Assert.Equal("boostParam", deprecation.Setting);
        Assert.Contains("keytermsPrompt", deprecation.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should warn when redactPii-dependent options are set without redactPii", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_warns_when_redaction_options_lack_redact_pii()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest("{\"redactStaticEntities\":{\"TOOL\":[\"Vercel\"]}}"), CancellationToken.None);
        Assert.Contains(result.Warnings.OfType<OtherWarning>(), warning => warning.Message.Contains("redactPii", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should warn when redactPiiAudioOptions is set without redactPiiAudio", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_warns_when_audio_redaction_options_lack_redact_pii_audio()
    {
        var options = "{\"redactPii\":true,\"redactPiiAudioOptions\":{\"overrideAudioRedactionMethod\":\"silence\"}}";
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest(options), CancellationToken.None);
        Assert.Contains(result.Warnings.OfType<OtherWarning>(), warning => warning.Message.Contains("redactPiiAudio", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should warn when languageCode and languageDetection are combined", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_warns_when_language_code_and_detection_are_combined()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("universal-3-5-pro").TranscribeAsync(Audio(), AssemblyAIRequest("{\"languageCode\":\"en\",\"languageDetection\":true}"), CancellationToken.None);
        Assert.Contains(result.Warnings.OfType<OtherWarning>(), warning => warning.Message.Contains("languageDetection", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should report segment timings in seconds (ms converted)", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_converts_word_timings_to_seconds()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("best").TranscribeAsync(Audio(), null, CancellationToken.None);
        var segment = result.Segments[0];
        Assert.Equal("Hello,", segment.Text);
        Assert.Equal(0.25, segment.StartSecond);
        Assert.Equal(0.65, segment.EndSecond);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_returns_the_transcript_text()
    {
        var result = await AssemblyAI(AssemblyAIHandler()).Transcription("best").TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal("Hello, world!", result.Text);
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_returns_the_timestamp_model_and_poll_headers()
    {
        var handler = AssemblyAIHandler(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var result = await AssemblyAI(handler, () => DateTimeOffset.UnixEpoch).Transcription("best").TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("best", result.Response.ModelId);
        Assert.Equal("application/json", Header(result.Response.Headers, "content-type"));
        Assert.Equal("test-request-id", Header(result.Response.Headers, "x-request-id"));
        Assert.Equal("123", Header(result.Response.Headers, "x-ratelimit-remaining"));
    }

    [Fact]
    [UpstreamTest(AssemblyAIModelTest + "should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_uses_the_injected_clock_and_model_id()
    {
        var result = await AssemblyAI(AssemblyAIHandler(), () => DateTimeOffset.UnixEpoch).Transcription("best").TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("best", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-error.test.ts::revaiErrorDataSchema::should parse Rev.ai resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Revai_parses_the_resource_exhausted_error()
    {
        var result = RevAIError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_sends_the_transcriber_in_the_config_part()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Contains("name=\"media\"", handler.Calls[0].Text, StringComparison.Ordinal);
        Assert.Contains("{\"transcriber\":\"machine\"}", handler.Calls[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_sends_authorization_custom_headers_and_user_agent()
    {
        var handler = RevaiHandler();
        var options = new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = RevAIProvider.Create(options, handler);
        var request = new RevAITranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), request, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.StartsWith("multipart/form-data", call.Header("Content-Type"), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/revai/" + AiSdkVersion.Version, call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_joins_monologue_values_into_the_transcript()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal("Hello from the Sal, A-I-S-D-K.", result.Text);
        Assert.Equal(2.145, result.DurationInSeconds, 3);
        Assert.Equal(5, result.Segments.Count);
        Assert.Equal("Hello", result.Segments[0].Text);
        Assert.Equal("A-I-S-D-K", result.Segments[4].Text);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > response headers::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_returns_the_transcript_timestamp_model_and_headers()
    {
        var handler = RevaiHandler(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("machine", result.Response.ModelId);
        Assert.Equal("test-request-id", Header(result.Response.Headers, "x-request-id"));
        Assert.Equal("123", Header(result.Response.Headers, "x-ratelimit-remaining"));
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A-I-S-D-K", result.Response.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/revai/src/revai-transcription-model.test.ts::doGenerate > response metadata::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Revai_uses_the_injected_clock_and_model_id()
    {
        var handler = RevaiHandler();
        var provider = RevAIProvider.Create(new RevAIOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await ((RevAIProvider.RevAITranscriptionModel)provider.TranscriptionModel("machine")).TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("machine", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-error.test.ts::gladiaErrorDataSchema::should parse Gladia resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Gladia_parses_the_resource_exhausted_error()
    {
        var result = GladiaError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass audio_url to pre-recorded endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_posts_the_uploaded_audio_url()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[1].Text);
        Assert.Equal("https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6", body.RootElement.GetProperty("audio_url").GetString());
        Assert.Equal("https://api.gladia.io/v2/pre-recorded", handler.Calls[1].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::does not send the API key when the result URL is on a foreign origin", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_strips_the_api_key_for_a_foreign_result_url()
    {
        var handler = GladiaHandler(foreign: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        var poll = handler.Calls.Single(call => call.Uri.Host == "cdn.evil.example");
        Assert.Null(poll.Header("x-gladia-key"));
        Assert.Null(poll.Header("Authorization"));
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_sends_the_api_key_custom_headers_and_user_agent()
    {
        var handler = GladiaHandler();
        var options = new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = GladiaProvider.Create(options, handler);
        var request = new GladiaTranscriptionRequest { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await provider.Transcription().TranscribeAsync(Audio(), request, CancellationToken.None);
        var call = handler.Calls[1];
        Assert.Equal("test-api-key", call.Header("x-gladia-key"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/gladia/" + AiSdkVersion.Version, handler.Calls[0].Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_returns_the_full_transcript()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaText, result.Text);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should preserve utterance metadata in provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_keeps_utterance_metadata_on_the_raw_result()
    {
        var handler = GladiaHandler(metadata: true);
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        var utterances = result.ProviderMetadata.GetProperty("gladia").GetProperty("result").GetProperty("transcription").GetProperty("utterances");
        Assert.Equal(0, utterances[0].GetProperty("speaker").GetInt32());
        Assert.Equal(0.91, utterances[0].GetProperty("confidence").GetDouble(), 2);
        Assert.Equal("en", utterances[0].GetProperty("language").GetString());
        Assert.Equal("Galileo", utterances[0].GetProperty("words")[0].GetProperty("word").GetString());
        Assert.Equal("speaker-1", utterances[1].GetProperty("speaker").GetString());
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > transcription::should generate full response", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_returns_the_full_transcription_response()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(GladiaText, result.Text);
        Assert.Equal("en", result.Language);
        Assert.Equal(36.74, result.DurationInSeconds!.Value, 2);
        Assert.Empty(result.Warnings);
        Assert.Equal(11, result.Segments.Count);
        Assert.Equal(0.14, result.Segments[0].StartSecond, 2);
        Assert.Equal(5.341, result.Segments[0].EndSecond, 3);
        Assert.StartsWith("Galileo was an American robotic space program", result.Segments[0].Text, StringComparison.Ordinal);
        Assert.Equal("and became the first spacecraft to orbit Jupiter.", result.Segments[10].Text);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(JsonValueKind.Object, result.ProviderMetadata.ValueKind);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response headers::should include response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_includes_the_result_timestamp_model_and_content_type()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
        Assert.Contains("application/json", Header(result.Response.Headers, "content-type"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [UpstreamTest("packages/gladia/src/gladia-transcription-model.test.ts::doGenerate > response metadata::should include timestamp and modelId", Coverage = UpstreamCoverage.Covered)]
    public async Task Gladia_includes_the_timestamp_and_model_id()
    {
        var handler = GladiaHandler();
        var provider = GladiaProvider.Create(new GladiaOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch, PollDelay = _ => Task.CompletedTask }, handler);
        var result = await provider.Transcription().TranscribeAsync(Audio(), null, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("default", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-error.test.ts::humeErrorDataSchema::should parse Hume resource exhausted error", Coverage = UpstreamCoverage.Covered)]
    public void Hume_parses_the_resource_exhausted_error()
    {
        var result = HumeError.Parse(ExhaustedJson());
        Assert.True(result.Success);
        Assert.Equal(ExhaustedMessage, result.Value!.Message);
        Assert.Equal(429, result.Value.Code);
        Assert.Equal(result.Value.Message, result.RawValue!.Message);
        Assert.Equal(result.Value.Code, result.RawValue.Code);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass the model and text", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_the_default_voice_and_mp3_format()
    {
        var handler = HumeHandler(new byte[100]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        var utterance = body.RootElement.GetProperty("utterances")[0];
        Assert.Equal("Hello from the AI SDK!", utterance.GetProperty("text").GetString());
        Assert.Equal(HumeSpeechRequest.DefaultVoiceId, utterance.GetProperty("voice").GetProperty("id").GetString());
        Assert.Equal("HUME_AI", utterance.GetProperty("voice").GetProperty("provider").GetString());
        Assert.Equal("mp3", body.RootElement.GetProperty("format").GetProperty("type").GetString());
        Assert.False(utterance.TryGetProperty("speed", out _));
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_the_api_key_custom_headers_and_user_agent()
    {
        var handler = HumeHandler(new byte[100]);
        var options = new HumeOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = HumeProvider.Create(options, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } }, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("test-api-key", call.Header("X-Hume-Api-Key"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/hume/" + AiSdkVersion.Version, call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should pass options", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_sends_voice_speed_and_format()
    {
        var handler = HumeHandler(new byte[100]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { Voice = "test-voice", OutputFormat = "mp3", Speed = 1.5 }, CancellationToken.None);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        var utterance = body.RootElement.GetProperty("utterances")[0];
        Assert.Equal("test-voice", utterance.GetProperty("voice").GetProperty("id").GetString());
        Assert.Equal(1.5, utterance.GetProperty("speed").GetDouble(), 3);
        Assert.Equal("mp3", body.RootElement.GetProperty("format").GetProperty("type").GetString());
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should return audio data with correct content type", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_the_audio_bytes()
    {
        var audio = new byte[100];
        var handler = HumeHandler(audio, ("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!") { OutputFormat = "mp3" }, CancellationToken.None);
        Assert.Equal(audio, result.Audio);
        Assert.Equal("audio/mp3", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_the_timestamp_and_response_headers()
    {
        var handler = HumeHandler(new byte[8], ("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123"));
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
        Assert.Equal("audio/mp3", Header(result.Response.Headers, "content-type"));
        Assert.Equal("test-request-id", Header(result.Response.Headers, "x-request-id"));
        Assert.Equal("123", Header(result.Response.Headers, "x-ratelimit-remaining"));
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_uses_the_injected_clock_and_empty_model_id()
    {
        var handler = HumeHandler(new byte[4]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal(string.Empty, result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should handle different audio formats", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_each_audio_payload()
    {
        foreach (var format in new[] { "mp3", "pcm", "wav" })
        {
            var audio = new byte[] { (byte)format[0], 2, 3 };
            var handler = HumeHandler(audio, contentType: "audio/" + format);
            var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
            var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
            Assert.Equal(audio, result.Audio);
        }
    }

    [Fact]
    [UpstreamTest("packages/hume/src/hume-speech-model.test.ts::doGenerate::should include warnings if any are generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Hume_returns_no_warnings_for_a_plain_request()
    {
        var handler = HumeHandler(new byte[4]);
        var provider = HumeProvider.Create(new HumeOptions { ApiKey = "test-api-key" }, handler);
        var result = await provider.Speech().GenerateAsync(new HumeSpeechRequest("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    private static AudioInput Audio()
    {
        return new AudioInput(new byte[] { 1, 2, 3, 4 }, "audio/wav", "audio.wav");
    }

    private static AssemblyAIProvider AssemblyAI(ParityHandler handler, Func<DateTimeOffset>? clock = null)
    {
        return AssemblyAIProvider.Create(new AssemblyAIOptions { ApiKey = "test-api-key", Clock = clock, PollDelay = _ => Task.CompletedTask }, handler);
    }

    private static AssemblyAITranscriptionRequest AssemblyAIRequest(string assemblyaiOptions)
    {
        using var document = JsonDocument.Parse(assemblyaiOptions);
        return new AssemblyAITranscriptionRequest
        {
            ProviderOptions = new Dictionary<string, JsonElement> { ["assemblyai"] = document.RootElement.Clone() },
        };
    }

    private static JsonElement AssemblyAISubmitBody(ParityHandler handler)
    {
        Assert.Equal("https://api.assemblyai.com/v2/transcript", handler.Calls[1].Uri.AbsoluteUri);
        using var document = JsonDocument.Parse(handler.Calls[1].Text);
        return document.RootElement.Clone();
    }

    private static ParityHandler AssemblyAIHandler(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath == "/v2/upload")
            {
                return ParityHandler.Json("{\"upload_url\":\"https://storage.assemblyai.com/mock-upload-url\"}");
            }

            if (call.Method == HttpMethod.Post)
            {
                return ParityHandler.Json("{\"id\":\"" + AssemblyAITranscriptId + "\",\"status\":\"queued\"}");
            }

            return ParityHandler.Json(AssemblyAITranscript(), headers);
        });
    }

    private static string AssemblyAITranscript()
    {
        return new JsonObject
        {
            ["id"] = AssemblyAITranscriptId,
            ["status"] = "completed",
            ["text"] = "Hello, world!",
            ["language_code"] = "en_us",
            ["audio_duration"] = 281,
            ["words"] = new JsonArray(
                new JsonObject { ["confidence"] = 0.97465, ["start"] = 250, ["end"] = 650, ["text"] = "Hello,", ["channel"] = "channel", ["speaker"] = "speaker" },
                new JsonObject { ["confidence"] = 0.99999, ["start"] = 730, ["end"] = 1022, ["text"] = "world", ["channel"] = "channel", ["speaker"] = "speaker" }),
            ["utterances"] = new JsonArray(
                new JsonObject { ["confidence"] = 0.9359, ["start"] = 250, ["end"] = 26950, ["text"] = "Hello, world!", ["speaker"] = "A", ["channel"] = "channel" }),
            ["auto_highlights_result"] = new JsonObject { ["status"] = "success", ["results"] = new JsonArray() },
            ["content_safety_labels"] = new JsonObject { ["status"] = "success", ["results"] = new JsonArray() },
            ["iab_categories_result"] = new JsonObject { ["status"] = "success", ["results"] = new JsonArray() },
            ["chapters"] = new JsonArray(new JsonObject { ["gist"] = "Hello, world!", ["start"] = 250, ["end"] = 28840 }),
            ["summary"] = "- Hello, world!",
            ["sentiment_analysis_results"] = new JsonArray(
                new JsonObject { ["text"] = "Hello, world!", ["start"] = 250, ["end"] = 26950, ["sentiment"] = "POSITIVE", ["confidence"] = 0.9, ["speaker"] = "A" }),
            ["entities"] = new JsonArray(
                new JsonObject { ["entity_type"] = "location", ["text"] = "Canada", ["start"] = 2548, ["end"] = 3130 },
                new JsonObject { ["entity_type"] = "location", ["text"] = "the US", ["start"] = 5498, ["end"] = 6382 }),
        }.ToJsonString();
    }

    private static ParityHandler RevaiHandler(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/transcript", StringComparison.Ordinal))
            {
                return ParityHandler.Json(RevTranscript, headers);
            }

            if (call.Method == HttpMethod.Get)
            {
                return ParityHandler.Json("{\"id\":\"test-id\",\"status\":\"transcribed\",\"language\":\"en\"}", headers);
            }

            return ParityHandler.Json("{\"id\":\"test-id\",\"status\":\"in_progress\",\"language\":\"en\"}", headers);
        });
    }

    private const string GladiaText = "Galileo was an American robotic space program that studied the planet Jupiter and its moons, as well as several other solar system bodies. Named after the Italian astronomer Galileo Galilei, the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe. It was delivered into Earth orbit on October 18, 1989, by Space Shuttle Atlantis on the STS-34 mission. and arrived at Jupiter on December 7, 1995, after gravity-assist flybys of Venus and Earth, and became the first spacecraft to orbit Jupiter.";

    private static string GladiaResult(bool metadata)
    {
        var speaker = metadata ? "\"speaker\":\"speaker-1\"" : "\"speaker\":1";
        return "{\"status\":\"done\",\"result\":{\"metadata\":{\"audio_duration\":36.74},\"transcription\":{\"full_transcript\":" + JsonSerializer.Serialize(GladiaText) + ",\"languages\":[\"en\"],\"utterances\":["
            + "{\"text\":\"Galileo was an American robotic space program that studied the planet Jupiter and its moons,\",\"start\":0.14,\"end\":5.341,\"speaker\":0,\"confidence\":0.91,\"language\":\"en\",\"words\":[{\"word\":\"Galileo\"}]},"
            + "{\"text\":\"as well as several other solar system bodies.\",\"start\":5.662,\"end\":8.099," + speaker + "},"
            + "{\"text\":\"Named after the Italian astronomer Galileo Galilei,\",\"start\":9.138,\"end\":12.122},"
            + "{\"text\":\"the Galileo spacecraft consisted of an orbiter and an atmospheric entry probe.\",\"start\":12.419,\"end\":17.248},"
            + "{\"text\":\"It was delivered into Earth orbit on October 18,\",\"start\":18.185,\"end\":20.919},"
            + "{\"text\":\"1989,\",\"start\":20.92,\"end\":22.498},"
            + "{\"text\":\"by Space Shuttle Atlantis on the STS-34 mission.\",\"start\":22.499,\"end\":25.966},"
            + "{\"text\":\"and arrived at Jupiter on December 7,\",\"start\":26.4,\"end\":28.525},"
            + "{\"text\":\"1995,\",\"start\":28.526,\"end\":29.847},"
            + "{\"text\":\"after gravity-assist flybys of Venus and Earth,\",\"start\":30.247,\"end\":32.851},"
            + "{\"text\":\"and became the first spacecraft to orbit Jupiter.\",\"start\":33.132,\"end\":35.816}"
            + "]}}}";
    }

    private static ParityHandler GladiaHandler(bool foreign = false, bool metadata = false)
    {
        var resultUrl = foreign
            ? "https://cdn.evil.example/v2/pre-recorded/result"
            : "https://api.gladia.io/v2/pre-recorded/7b0137fd-5fd6-4d08-b9bf-4cdf9876797d";
        return new ParityHandler(call =>
        {
            if (call.Uri.AbsolutePath.EndsWith("/upload", StringComparison.Ordinal))
            {
                return ParityHandler.Json("{\"audio_url\":\"https://api.gladia.io/file/7403f025-be30-4335-ae29-20131e0adbd6\"}");
            }

            if (call.Method == HttpMethod.Post)
            {
                return ParityHandler.Json("{\"id\":\"7b0137fd-5fd6-4d08-b9bf-4cdf9876797d\",\"result_url\":\"" + resultUrl + "\"}");
            }

            return ParityHandler.Json(GladiaResult(metadata));
        });
    }

    private static ParityHandler HumeHandler(byte[] audio, params (string Name, string Value)[] headers)
    {
        return HumeHandler(audio, "audio/mp3", headers);
    }

    private static ParityHandler HumeHandler(byte[] audio, string contentType, params (string Name, string Value)[] headers)
    {
        return new ParityHandler(_ => ParityHandler.Bytes(audio, contentType, headers));
    }

    private static string? Header(IReadOnlyDictionary<string, string> headers, string name)
    {
        foreach (var pair in headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }
}
