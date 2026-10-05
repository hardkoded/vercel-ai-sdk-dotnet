// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Vercel.AI.Deepgram;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class DeepgramParityTests
{
    private const string Speech = "packages/deepgram/src/deepgram-speech-model.test.ts::doGenerate::";
    private const string Transcription = "packages/deepgram/src/deepgram-transcription-model.test.ts::doGenerate > ";
    private const string Hello = "Hello, welcome to Deepgram!";

    [Fact]
    [UpstreamTest("packages/deepgram/src/deepgram-error.test.ts::deepgramErrorDataSchema::should parse the err_code/err_msg error shape", Coverage = UpstreamCoverage.Covered)]
    public void Error_parser_reads_code_message_and_request_id()
    {
        var result = DeepgramError.Parse("{\"err_code\":\"INVALID_QUERY_PARAMETER\",\"err_msg\":\"Invalid 'model' value of 'aura-2-not-a-real-voice-en'.\",\"request_id\":\"01a00450-5a52-70f0-9253-2fc492123595\"}");
        Assert.True(result.Success);
        Assert.Equal("INVALID_QUERY_PARAMETER", result.Value!.ErrCode);
        Assert.Equal("Invalid 'model' value of 'aura-2-not-a-real-voice-en'.", result.Value.ErrMsg);
        Assert.Equal("01a00450-5a52-70f0-9253-2fc492123595", result.Value.RequestId);
        Assert.Same(result.Value, result.RawValue);
    }

    [Fact]
    [UpstreamTest("packages/deepgram/src/deepgram-error.test.ts::deepgramErrorDataSchema::should parse an auth error without relying on optional fields", Coverage = UpstreamCoverage.Covered)]
    public void Error_parser_accepts_a_missing_request_id()
    {
        var result = DeepgramError.Parse("{\"err_code\":\"INVALID_AUTH\",\"err_msg\":\"Invalid credentials.\"}");
        Assert.True(result.Success);
        Assert.Equal("INVALID_AUTH", result.Value!.ErrCode);
        Assert.Equal("Invalid credentials.", result.Value.ErrMsg);
        Assert.Null(result.Value.RequestId);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass the model and text", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_text_and_the_composed_model()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Assert.Equal(Hello, JsonDocument.Parse(handler.Calls[0].Text).RootElement.GetProperty("text").GetString());
        Assert.Equal("aura-2-helena-en", Query(handler)["model"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_token_json_custom_headers_and_user_agent()
    {
        var handler = SpeechHandler();
        var options = new DeepgramOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = (DeepgramProvider.DeepgramSpeechModel)DeepgramProvider.Create(options, handler).SpeechModel("aura-2");
        var request = new DeepgramSpeechRequest(Hello) { Voice = "helena", Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } };
        await model.GenerateAsync(request, CancellationToken.None);
        var call = handler.Calls[0];
        Assert.Equal("Token test-api-key", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/deepgram/0.0.0-test", call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should compose the upstream model ID from voice, defaulting language to en", Coverage = UpstreamCoverage.Covered)]
    public async Task Family_ids_default_the_language_to_en()
    {
        await AssertComposedModel("aura-2", "thalia", null, "aura-2-thalia-en");
    }

    [Fact]
    [UpstreamTest(Speech + "should compose the upstream model ID from voice and language", Coverage = UpstreamCoverage.Covered)]
    public async Task Family_ids_use_the_language()
    {
        await AssertComposedModel("aura-2", "celeste", "es", "aura-2-celeste-es");
    }

    [Fact]
    [UpstreamTest(Speech + "should compose with the aura family", Coverage = UpstreamCoverage.Covered)]
    public async Task The_aura_family_composes_too()
    {
        await AssertComposedModel("aura", "asteria", null, "aura-asteria-en");
    }

    [Fact]
    [UpstreamTest(Speech + "should trim whitespace from the voice when composing", Coverage = UpstreamCoverage.Covered)]
    public async Task The_voice_is_trimmed()
    {
        await AssertComposedModel("aura-2", " thalia ", null, "aura-2-thalia-en");
    }

    [Fact]
    [UpstreamTest(Speech + "should throw when no voice is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Family_ids_require_a_voice()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => SpeechModel(SpeechHandler()).GenerateAsync(new DeepgramSpeechRequest(Hello), CancellationToken.None));
        Assert.StartsWith("Deepgram speech model \"aura-2\" requires a `voice` to be set", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass through full voice model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Full_voice_ids_pass_through()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler, "aura-2-helena-en").GenerateAsync(new DeepgramSpeechRequest(Hello), CancellationToken.None);
        Assert.Equal("aura-2-helena-en", Query(handler)["model"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should warn about voice parameter with full voice model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Full_voice_ids_warn_about_the_voice()
    {
        var result = await SpeechModel(SpeechHandler(), "aura-2-helena-en").GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "different-voice" }, CancellationToken.None);
        AssertWarning(Assert.Single(result.Warnings), "unsupported", "voice", "Deepgram TTS models embed the voice in the model ID. The voice parameter \"different-voice\" was ignored. Use the model ID to select a voice (e.g., \"aura-2-helena-en\").");
    }

    [Fact]
    [UpstreamTest(Speech + "should warn about language parameter with full voice model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Full_voice_ids_warn_about_the_language()
    {
        var result = await SpeechModel(SpeechHandler(), "aura-2-helena-en").GenerateAsync(new DeepgramSpeechRequest(Hello) { Language = "en" }, CancellationToken.None);
        AssertWarning(Assert.Single(result.Warnings), "unsupported", "language", "Deepgram TTS models are language-specific via the model ID. Language parameter \"en\" was ignored. Select a model with the appropriate language suffix (e.g., \"-en\" for English).");
    }

    [Fact]
    [UpstreamTest(Speech + "should warn and use en when language is auto", Coverage = UpstreamCoverage.Covered)]
    public async Task Auto_language_warns_and_uses_en()
    {
        var handler = SpeechHandler();
        var result = await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "thalia", Language = "auto" }, CancellationToken.None);
        Assert.Equal("aura-2-thalia-en", Query(handler)["model"]);
        AssertWarning(Assert.Single(result.Warnings), "compatibility", "language", "Deepgram TTS models do not support automatic language detection. Language \"en\" was used instead.");
    }

    [Fact]
    [UpstreamTest(Speech + "should not warn when voice and language are consumed by composition", Coverage = UpstreamCoverage.Covered)]
    public async Task Composed_voice_and_language_do_not_warn()
    {
        var result = await SpeechModel(SpeechHandler()).GenerateAsync(new DeepgramSpeechRequest("Hola desde Deepgram!") { Voice = "celeste", Language = "es" }, CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Speech + "should map outputFormat to encoding/container", Coverage = UpstreamCoverage.Covered)]
    public async Task Wav_maps_to_linear16_in_a_wav_container()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", OutputFormat = "wav" }, CancellationToken.None);
        var query = Query(handler);
        Assert.Equal("wav", query["container"]);
        Assert.Equal("linear16", query["encoding"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_become_query_parameters()
    {
        var handler = SpeechHandler();
        var request = new DeepgramSpeechRequest(Hello)
        {
            Voice = "helena",
            ProviderOptions = Options("{\"encoding\":\"mp3\",\"bitRate\":48000,\"container\":\"wav\",\"callback\":\"https://example.com/callback\",\"callbackMethod\":\"POST\",\"mipOptOut\":true,\"tag\":\"test-tag\"}"),
        };
        await SpeechModel(handler).GenerateAsync(request, CancellationToken.None);
        var query = Query(handler);
        Assert.Equal("mp3", query["encoding"]);
        Assert.Equal("48000", query["bit_rate"]);
        Assert.Null(query["container"]);
        Assert.Equal("https://example.com/callback", query["callback"]);
        Assert.Equal("POST", query["callback_method"]);
        Assert.Equal("true", query["mip_opt_out"]);
        Assert.Equal("test-tag", query["tag"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should handle array tag", Coverage = UpstreamCoverage.Covered)]
    public async Task Array_tags_are_joined()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", ProviderOptions = Options("{\"tag\":[\"tag1\",\"tag2\"]}") }, CancellationToken.None);
        Assert.Equal("tag1,tag2", Query(handler)["tag"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should return audio data", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_returns_the_audio_bytes()
    {
        var result = await SpeechModel(SpeechHandler(("x-request-id", "test-request-id"))).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Assert.Equal(new byte[100], result.Audio);
    }

    [Fact]
    [UpstreamTest(Speech + "should extract provider metadata from Deepgram response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepgram_headers_become_provider_metadata()
    {
        var handler = SpeechHandler(
            ("dg-model-name", "aura-2-helena-en"),
            ("dg-model-uuid", "4fa750e6-8ade-4394-849f-c104ced3741e"),
            ("dg-additional-model-uuids", "0ec06c9b-0aa0-44d0-a001-3ec57d32229e,2e5096c7-7bf1-435e-bbdd-f673f88d0ebd"),
            ("dg-char-count", "69"),
            ("dg-breaks-applied", "0"),
            ("dg-pronunciations-applied", "2"),
            ("dg-pronunciation-warnings", "1 unknown word"),
            ("dg-request-id", "01a00436-34a3-7cb0-b491-53339eed8eb1"));
        var result = await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Upstream.JsonAssert.Equal(
            result.ProviderMetadata,
            "{\"deepgram\":{\"modelName\":\"aura-2-helena-en\",\"modelUuid\":\"4fa750e6-8ade-4394-849f-c104ced3741e\",\"additionalModelUuids\":[\"0ec06c9b-0aa0-44d0-a001-3ec57d32229e\",\"2e5096c7-7bf1-435e-bbdd-f673f88d0ebd\"],\"charCount\":69,\"breaksApplied\":0,\"pronunciationsApplied\":2,\"pronunciationWarnings\":\"1 unknown word\",\"requestId\":\"01a00436-34a3-7cb0-b491-53339eed8eb1\"}}");
    }

    [Fact]
    [UpstreamTest(Speech + "should return empty provider metadata when Deepgram headers are absent", Coverage = UpstreamCoverage.Covered)]
    public async Task Missing_deepgram_headers_give_empty_metadata()
    {
        var result = await SpeechModel(SpeechHandler()).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Upstream.JsonAssert.Equal(result.ProviderMetadata, "{\"deepgram\":{}}");
    }

    [Fact]
    [UpstreamTest(Speech + "should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_response_has_timestamp_model_and_headers()
    {
        var result = await SpeechModel(SpeechHandler(("x-request-id", "test-request-id"))).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("aura-2", result.Response.ModelId);
        Assert.Equal("audio/mp3", result.Response.Headers["Content-Type"]);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should pass speed as a query parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Speed_is_a_query_parameter()
    {
        var handler = SpeechHandler();
        var result = await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", Speed = 1.5 }, CancellationToken.None);
        Assert.Equal("1.5", Query(handler)["speed"]);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Speech + "should warn about unsupported instructions parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Instructions_warn()
    {
        var result = await SpeechModel(SpeechHandler()).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", Instructions = "Speak slowly" }, CancellationToken.None);
        AssertWarning(Assert.Single(result.Warnings), "unsupported", "instructions", "Deepgram TTS REST API does not support instructions. Instructions parameter was ignored.");
    }

    [Fact]
    [UpstreamTest(Speech + "should surface the Deepgram err_msg as the APICallError message", Coverage = UpstreamCoverage.Covered)]
    public async Task Errors_use_the_deepgram_message()
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"err_code\":\"INVALID_QUERY_PARAMETER\",\"err_msg\":\"Invalid 'model' value of 'aura-2-not-a-real-voice-en'.\",\"request_id\":\"01a00450-5a52-70f0-9253-2fc492123595\"}", Encoding.UTF8, "application/json"),
        });
        var error = await Assert.ThrowsAsync<BadRequestException>(() => SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest("Hello, world!") { Voice = "not-a-real-voice" }, CancellationToken.None));
        Assert.Equal("Invalid 'model' value of 'aura-2-not-a-real-voice-en'.", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Speech + "should include request body in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_returns_the_request_body()
    {
        var result = await SpeechModel(SpeechHandler()).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena" }, CancellationToken.None);
        Assert.Equal("{\"text\":\"Hello, welcome to Deepgram!\"}", result.RequestBody);
    }

    [Fact]
    [UpstreamTest(Speech + "should clean up incompatible parameters when encoding changes via providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task Changing_the_encoding_drops_incompatible_parameters()
    {
        var handler = SpeechHandler();
        var model = SpeechModel(handler);
        await model.GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", OutputFormat = "linear16_16000", ProviderOptions = Options("{\"encoding\":\"mp3\"}") }, CancellationToken.None);
        await model.GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", OutputFormat = "linear16_16000", ProviderOptions = Options("{\"encoding\":\"opus\"}") }, CancellationToken.None);
        await model.GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", OutputFormat = "mp3", ProviderOptions = Options("{\"encoding\":\"linear16\",\"bitRate\":48000}") }, CancellationToken.None);
        var first = Query(handler, 0);
        Assert.Equal("mp3", first["encoding"]);
        Assert.Null(first["sample_rate"]);
        var second = Query(handler, 1);
        Assert.Equal("opus", second["encoding"]);
        Assert.Equal("ogg", second["container"]);
        Assert.Null(second["sample_rate"]);
        var third = Query(handler, 2);
        Assert.Equal("linear16", third["encoding"]);
        Assert.Null(third["bit_rate"]);
    }

    [Fact]
    [UpstreamTest(Speech + "should clean up incompatible parameters when container changes encoding implicitly", Coverage = UpstreamCoverage.Covered)]
    public async Task Changing_the_container_drops_incompatible_parameters()
    {
        var handler = SpeechHandler();
        await SpeechModel(handler).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = "helena", OutputFormat = "linear16_16000", ProviderOptions = Options("{\"container\":\"ogg\"}") }, CancellationToken.None);
        var query = Query(handler);
        Assert.Equal("opus", query["encoding"]);
        Assert.Equal("ogg", query["container"]);
        Assert.Null(query["sample_rate"]);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_returns_the_first_channel_transcript()
    {
        var result = await Transcribe(TranscriptHandler(), null);
        Assert.Equal("galileo was an american robotic space program that studied the planet jupiter and its moons as well as several other solar system bodies named after the italian astronomer galileo galilei the galileo spacecraft consisted of an orbiter and an atmospheric entry probe it was delivered into earth orbit on october eighteen nineteen eighty nine by space shuttle atlantis on the sts-thirty four mission and arrived at jupiter on december seven nineteen ninety five after gravity assist flybys of venus and earth and became the first spacecraft to orbit jupiter", result.Text);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should pass detectLanguage as detect_language query parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Detect_language_is_a_query_parameter()
    {
        var handler = TranscriptHandler();
        await Transcribe(handler, "{\"detectLanguage\":true}");
        Assert.Contains("detect_language=true", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should return detected language from response", Coverage = UpstreamCoverage.Covered)]
    public async Task Detected_language_is_returned()
    {
        var result = await Transcribe(TranscriptHandler(), "{\"detectLanguage\":true}");
        Assert.Equal("en", result.Language);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should not send diarize by default", Coverage = UpstreamCoverage.Covered)]
    public async Task Diarize_is_off_by_default()
    {
        var handler = TranscriptHandler();
        await Transcribe(handler, null);
        Assert.DoesNotContain("diarize", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should pass diarize when explicitly enabled", Coverage = UpstreamCoverage.Covered)]
    public async Task Diarize_can_be_enabled()
    {
        var handler = TranscriptHandler();
        await Transcribe(handler, "{\"diarize\":true}");
        Assert.Contains("diarize=true", handler.Calls[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Transcription + "transcription::should pass keyterm, paragraphs, intents, sentiment, and replace as query parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Analysis_options_are_query_parameters()
    {
        var handler = TranscriptHandler();
        await Transcribe(handler, "{\"keyterm\":\"galileo\",\"paragraphs\":true,\"intents\":true,\"sentiment\":true,\"redact\":\"numbers\",\"replace\":\"[redacted]\"}");
        var query = Query(handler);
        Assert.Equal("galileo", query["keyterm"]);
        Assert.Equal("true", query["paragraphs"]);
        Assert.Equal("true", query["intents"]);
        Assert.Equal("true", query["sentiment"]);
        Assert.Equal("numbers", query["redact"]);
        Assert.Equal("[redacted]", query["replace"]);
    }

    [Fact]
    [UpstreamTest(Transcription + "response headers::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_response_has_timestamp_model_headers_and_body()
    {
        var result = await Transcribe(TranscriptHandler(("x-request-id", "test-request-id"), ("x-ratelimit-remaining", "123")), null);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("nova-3", result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
        Assert.Equal("123", result.Response.Headers["x-ratelimit-remaining"]);
        Assert.Equal("application/json", result.Response.Headers["Content-Type"]);
        using var expected = JsonDocument.Parse(File.ReadAllText(Fixture()));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, result.Response.Body!.Value));
    }

    [Fact]
    [UpstreamTest(Transcription + "response metadata::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task Transcription_uses_the_configured_clock()
    {
        var result = await Transcribe(TranscriptHandler(), null);
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("nova-3", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(Transcription + "language detection::should return detected language from inline response", Coverage = UpstreamCoverage.Covered)]
    public async Task Inline_detected_language_is_returned()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"metadata\":{\"duration\":1.0},\"results\":{\"channels\":[{\"detected_language\":\"sv\",\"alternatives\":[{\"transcript\":\"hej\",\"words\":[]}]}]}}"));
        var result = await Transcribe(handler, "{\"detectLanguage\":true}");
        Assert.Equal("sv", result.Language);
    }

    [Fact]
    [UpstreamTest(Transcription + "language detection::should return undefined language when not detected", Coverage = UpstreamCoverage.Covered)]
    public async Task Language_is_null_when_not_detected()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"metadata\":{\"duration\":1.0},\"results\":{\"channels\":[{\"alternatives\":[{\"transcript\":\"hello\",\"words\":[]}]}]}}"));
        var result = await Transcribe(handler, null);
        Assert.Null(result.Language);
        Assert.Equal(1.0, result.DurationInSeconds);
    }

    private static async Task AssertComposedModel(string modelId, string voice, string? language, string expected)
    {
        var handler = SpeechHandler();
        await SpeechModel(handler, modelId).GenerateAsync(new DeepgramSpeechRequest(Hello) { Voice = voice, Language = language }, CancellationToken.None);
        Assert.Equal(expected, Query(handler)["model"]);
    }

    private static void AssertWarning(ModelWarning warning, string type, string feature, string details)
    {
        Assert.Equal(type, warning.Type);
        switch (warning)
        {
            case UnsupportedWarning unsupported:
                Assert.Equal(feature, unsupported.Feature);
                Assert.Equal(details, unsupported.Details);
                break;
            case CompatibilityWarning compatibility:
                Assert.Equal(feature, compatibility.Feature);
                Assert.Equal(details, compatibility.Details);
                break;
            default:
                Assert.Fail("Unexpected warning " + warning.GetType().Name);
                break;
        }
    }

    private static DeepgramProvider.DeepgramSpeechModel SpeechModel(ParityHandler handler, string modelId = "aura-2")
    {
        var options = new DeepgramOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch };
        return (DeepgramProvider.DeepgramSpeechModel)DeepgramProvider.Create(options, handler).SpeechModel(modelId);
    }

    private static Task<DeepgramTranscriptionResult> Transcribe(ParityHandler handler, string? deepgram)
    {
        var options = new DeepgramOptions { ApiKey = "test-api-key", Clock = () => DateTimeOffset.UnixEpoch };
        var request = new DeepgramTranscriptionRequest { ProviderOptions = deepgram == null ? null : Options(deepgram) };
        return DeepgramProvider.Create(options, handler).TranscribeAsync("nova-3", new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", null), request, CancellationToken.None);
    }

    private static ParityHandler SpeechHandler(params (string Name, string Value)[] headers)
    {
        return new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3", headers));
    }

    private static ParityHandler TranscriptHandler(params (string Name, string Value)[] headers)
    {
        var json = File.ReadAllText(Fixture());
        return new ParityHandler(_ => ParityHandler.Json(json, headers));
    }

    private static string Fixture()
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "deepgram-transcription.json");
    }

    private static JsonElement Options(string deepgram)
    {
        return JsonDocument.Parse("{\"deepgram\":" + deepgram + "}").RootElement.Clone();
    }

    private static System.Collections.Specialized.NameValueCollection Query(ParityHandler handler, int index = 0)
    {
        return HttpUtility.ParseQueryString(handler.Calls[index].Uri.Query);
    }
}
