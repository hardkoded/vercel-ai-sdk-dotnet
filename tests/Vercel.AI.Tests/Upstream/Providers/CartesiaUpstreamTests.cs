// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Cartesia;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class CartesiaUpstreamTests
{
    private const string Speech =
        "packages/cartesia/src/cartesia-speech-model.test.ts::CartesiaSpeechModel > doGenerate::";
    private const string Transcription =
        "packages/cartesia/src/cartesia-transcription-model.test.ts::doGenerate > ";

    [Fact]
    [UpstreamTest(
        "packages/cartesia/src/cartesia-error.test.ts::cartesiaErrorDataSchema::should parse a Cartesia error",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Error_schema_parses_a_cartesia_error()
    {
        const string error =
            "{\"error_code\":\"authentication_failed\",\"title\":\"Authentication failed\",\"message\":\"Invalid API key.\",\"request_id\":\"550e8400-e29b-41d4-a716-446655440000\"}";
        var result = JsonParsing.SafeParse(error, CartesiaProvider.ErrorSchema);
        Assert.True(result.Success);
        JsonAssert.Equal(result.Value!.Value, error);
        JsonAssert.Equal(result.RawValue!.Value, error);
    }

    [Fact]
    [UpstreamTest(
        Speech + "should generate speech with required parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_sends_the_default_mp3_format()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        JsonAssert.Equal(
            Body(handler),
            "{\"model_id\":\"sonic-3.5\",\"transcript\":\"Hello, world!\",\"voice\":{\"mode\":\"id\",\"id\":\"test-voice-id\"},\"output_format\":{\"container\":\"mp3\",\"sample_rate\":44100,\"bit_rate\":128000}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should throw when no voice is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_requires_a_voice()
    {
        var (model, handler) = SpeechModel();
        var error = await Assert.ThrowsAsync<AiSdkException>(() =>
            model.DoGenerateAsync(SpeechCall(voice: null), CancellationToken.None)
        );
        Assert.Equal("Cartesia speech models require a `voice` to be set.", error.Message);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Speech + "should map wav output format", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_maps_wav_to_signed_16_bit_pcm()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(outputFormat: "wav"), CancellationToken.None);
        JsonAssert.Equal(
            Body(handler).GetProperty("output_format"),
            "{\"container\":\"wav\",\"encoding\":\"pcm_s16le\",\"sample_rate\":44100}"
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should map pcm output format with sample rate suffix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_reads_the_sample_rate_suffix()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(outputFormat: "pcm_24000"), CancellationToken.None);
        JsonAssert.Equal(
            Body(handler).GetProperty("output_format"),
            "{\"container\":\"raw\",\"encoding\":\"pcm_f32le\",\"sample_rate\":24000}"
        );
    }

    [Fact]
    [UpstreamTest(Speech + "should handle language parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_language()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(text: "Hola, mundo!", language: "es"),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("Hola, mundo!", body.GetProperty("transcript").GetString());
        Assert.Equal("es", body.GetProperty("language").GetString());
    }

    [Fact]
    [UpstreamTest(Speech + "should handle speed parameter", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_sends_the_speed_in_the_generation_config()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(speed: 1.5), CancellationToken.None);
        JsonAssert.Equal(Body(handler).GetProperty("generation_config"), "{\"speed\":1.5}");
    }

    [Fact]
    [UpstreamTest(
        Speech + "should warn and ignore an out-of-range generic speed",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_ignores_an_out_of_range_speed()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(speed: 2), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("generation_config", out _));
        AssertWarning(
            result.Warnings,
            "speed",
            "Cartesia speed must be between 0.6 and 1.5. The speed option was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should warn about unsupported instructions parameter",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_warns_about_instructions()
    {
        var (model, _) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(instructions: "Speak slowly"),
            CancellationToken.None
        );
        AssertWarning(
            result.Warnings,
            "instructions",
            "Cartesia speech models do not support instructions. Instructions parameter was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should pass provider-specific options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_applies_provider_options()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                providerOptions: "{\"cartesia\":{\"container\":\"raw\",\"encoding\":\"pcm_s16le\",\"sampleRate\":16000,\"speed\":0.8}}"
            ),
            CancellationToken.None
        );
        var body = Body(handler);
        JsonAssert.Equal(
            body.GetProperty("output_format"),
            "{\"container\":\"raw\",\"encoding\":\"pcm_s16le\",\"sample_rate\":16000}"
        );
        JsonAssert.Equal(body.GetProperty("generation_config"), "{\"speed\":0.8}");
    }

    [Fact]
    [UpstreamTest(
        Speech + "should ignore encoding for mp3 output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_ignores_an_encoding_for_mp3()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"cartesia\":{\"encoding\":\"pcm_s16le\"}}"),
            CancellationToken.None
        );
        JsonAssert.Equal(
            Body(handler).GetProperty("output_format"),
            "{\"container\":\"mp3\",\"sample_rate\":44100,\"bit_rate\":128000}"
        );
        AssertWarning(
            result.Warnings,
            "providerOptions.cartesia.encoding",
            "Cartesia MP3 output does not accept an encoding. The encoding option was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should warn about an unsupported sample rate suffix",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_warns_about_an_unsupported_sample_rate()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(outputFormat: "wav_12345"),
            CancellationToken.None
        );
        var format = Body(handler).GetProperty("output_format");
        Assert.Equal("wav", format.GetProperty("container").GetString());
        Assert.Equal(44100, format.GetProperty("sample_rate").GetInt32());
        AssertWarning(
            result.Warnings,
            "outputFormat",
            "Unsupported Cartesia sample rate in output format \"wav_12345\". Using 44100 Hz instead."
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should send volume in generation_config",
        Coverage = UpstreamCoverage.Covered,
        Commit = "392d2fa3a4feb69c1660c1ad1c25c4b21fdf45df"
    )]
    public async Task Speech_sends_volume_in_the_generation_config()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"cartesia\":{\"volume\":1.5}}"),
            CancellationToken.None
        );
        JsonAssert.Equal(Body(handler).GetProperty("generation_config"), "{\"volume\":1.5}");
    }

    [Fact]
    [UpstreamTest(
        Speech + "should send emotion in generation_config",
        Coverage = UpstreamCoverage.Covered,
        Commit = "392d2fa3a4feb69c1660c1ad1c25c4b21fdf45df"
    )]
    public async Task Speech_sends_emotion_in_the_generation_config()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"cartesia\":{\"emotion\":\"calm\"}}"),
            CancellationToken.None
        );
        JsonAssert.Equal(Body(handler).GetProperty("generation_config"), "{\"emotion\":\"calm\"}");
    }

    [Fact]
    [UpstreamTest(
        Speech + "should combine speed, volume and emotion in one generation_config",
        Coverage = UpstreamCoverage.Covered,
        Commit = "392d2fa3a4feb69c1660c1ad1c25c4b21fdf45df"
    )]
    public async Task Speech_combines_speed_volume_and_emotion()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                speed: 1.2,
                providerOptions: "{\"cartesia\":{\"volume\":0.8,\"emotion\":\"sad\"}}"
            ),
            CancellationToken.None
        );
        JsonAssert.Equal(
            Body(handler).GetProperty("generation_config"),
            "{\"speed\":1.2,\"volume\":0.8,\"emotion\":\"sad\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "should keep volume when an out-of-range generic speed is ignored",
        Coverage = UpstreamCoverage.Covered,
        Commit = "392d2fa3a4feb69c1660c1ad1c25c4b21fdf45df"
    )]
    public async Task Speech_keeps_volume_when_an_out_of_range_speed_is_ignored()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(speed: 2, providerOptions: "{\"cartesia\":{\"volume\":1.5}}"),
            CancellationToken.None
        );
        JsonAssert.Equal(Body(handler).GetProperty("generation_config"), "{\"volume\":1.5}");
        AssertWarning(
            result.Warnings,
            "speed",
            "Cartesia speed must be between 0.6 and 1.5. The speed option was ignored."
        );
    }

    [Theory]
    [InlineData("volume below the range", "{\"volume\":0.4}")]
    [InlineData("volume above the range", "{\"volume\":2.1}")]
    [InlineData("an unknown emotion", "{\"emotion\":\"banana\"}")]
    [InlineData("a null volume", "{\"volume\":null}")]
    [InlineData("a null emotion", "{\"emotion\":null}")]
    [UpstreamTest(
        Speech + "should reject %s before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "392d2fa3a4feb69c1660c1ad1c25c4b21fdf45df"
    )]
    public async Task Speech_rejects_invalid_volume_or_emotion_before_sending(
        string description,
        string options
    )
    {
        Assert.NotEmpty(description);
        var (model, handler) = SpeechModel();
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                SpeechCall(providerOptions: "{\"cartesia\":" + options + "}"),
                CancellationToken.None
            )
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Speech + "should pass pronunciationDictionaryId as pronunciation_dict_id",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f10414f247eff151d0315d70837530778affa81b"
    )]
    public async Task Speech_sends_pronunciation_dictionary_id()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                providerOptions: "{\"cartesia\":{\"pronunciationDictionaryId\":\"pdict_test\"}}"
            ),
            CancellationToken.None
        );
        Assert.Equal("pdict_test", Body(handler).GetProperty("pronunciation_dict_id").GetString());
    }

    [Fact]
    [UpstreamTest(
        Speech + "should reject a null pronunciationDictionaryId before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f10414f247eff151d0315d70837530778affa81b"
    )]
    public async Task Speech_rejects_a_null_pronunciation_dictionary_id()
    {
        var (model, handler) = SpeechModel();
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                SpeechCall(providerOptions: "{\"cartesia\":{\"pronunciationDictionaryId\":null}}"),
                CancellationToken.None
            )
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Speech + "should not send pronunciation_dict_id by default",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f10414f247eff151d0315d70837530778affa81b"
    )]
    public async Task Speech_omits_pronunciation_dict_id_by_default()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.False(Body(handler).TryGetProperty("pronunciation_dict_id", out _));
    }

    [Theory]
    [InlineData("locale")]
    [InlineData("normalization")]
    [UpstreamTest(
        Speech + "rejects a null %s before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_rejects_a_null_locale_or_normalization(string option)
    {
        var (model, handler) = SpeechModel();
        await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                SpeechCall(providerOptions: "{\"cartesia\":{\"" + option + "\":null}}"),
                CancellationToken.None
            )
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Speech + "combines locale and normalization with the other speech options",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_combines_locale_and_normalization_with_the_other_options()
    {
        var (model, handler) = SpeechModel();
        await model.DoGenerateAsync(
            SpeechCall(
                providerOptions: "{\"cartesia\":{\"locale\":\"en-GB\",\"normalization\":\"off\",\"pronunciationDictionaryId\":\"test-dictionary\",\"volume\":1.2,\"emotion\":\"excited\"}}"
            ),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("en-GB", body.GetProperty("locale").GetString());
        Assert.Equal("off", body.GetProperty("normalization").GetString());
        Assert.Equal("test-dictionary", body.GetProperty("pronunciation_dict_id").GetString());
        JsonAssert.Equal(
            body.GetProperty("generation_config"),
            "{\"volume\":1.2,\"emotion\":\"excited\"}"
        );
    }

    [Fact]
    [UpstreamTest(
        Speech + "rejects conflicting provider locale and language before sending a request",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_rejects_conflicting_locale_and_language()
    {
        var (model, handler) = SpeechModel();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            model.DoGenerateAsync(
                SpeechCall(
                    providerOptions: "{\"cartesia\":{\"locale\":\"en-GB\",\"language\":\"en\"}}"
                ),
                CancellationToken.None
            )
        );
        Assert.Equal("providerOptions", error.Parameter);
        Assert.Contains(
            "providerOptions.cartesia.locale cannot be combined with providerOptions.cartesia.language",
            error.Message
        );
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Speech + "preserves provider language precedence over generic language",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_preserves_provider_language_precedence()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(language: "en", providerOptions: "{\"cartesia\":{\"language\":\"es\"}}"),
            CancellationToken.None
        );
        Assert.Equal("es", Body(handler).GetProperty("language").GetString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Speech + "sends a locale alone without language or warnings",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_sends_a_locale_alone()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(providerOptions: "{\"cartesia\":{\"locale\":\"en-GB\"}}"),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("en-GB", body.GetProperty("locale").GetString());
        Assert.False(body.TryGetProperty("language", out _));
        Assert.False(body.TryGetProperty("normalization", out _));
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("off")]
    [InlineData("es-MX")]
    [UpstreamTest(
        Speech + "sends normalization %s independently of locale",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_sends_normalization_independently_of_locale(string normalization)
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(
                language: "en",
                providerOptions: "{\"cartesia\":{\"normalization\":\"" + normalization + "\"}}"
            ),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("en", body.GetProperty("language").GetString());
        Assert.Equal(normalization, body.GetProperty("normalization").GetString());
        Assert.False(body.TryGetProperty("locale", out _));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Speech
            + "should prioritize providerOptions locale over generic language and emit a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = "47b666b6c7c6b671489ec10e7111e95f32bed5ff"
    )]
    public async Task Speech_prioritizes_provider_locale_over_generic_language()
    {
        var (model, handler) = SpeechModel();
        var result = await model.DoGenerateAsync(
            SpeechCall(language: "en", providerOptions: "{\"cartesia\":{\"locale\":\"en-GB\"}}"),
            CancellationToken.None
        );
        var body = Body(handler);
        Assert.Equal("en-GB", body.GetProperty("locale").GetString());
        Assert.False(body.TryGetProperty("language", out _));
        AssertWarning(
            result.Warnings,
            "language",
            "Cartesia does not support both locale and language. The language option was ignored in favor of providerOptions.cartesia.locale."
        );
    }

    [Fact]
    [UpstreamTest(Speech + "should return audio data", Coverage = UpstreamCoverage.Covered)]
    public async Task Speech_returns_the_audio_bytes()
    {
        var (model, _) = SpeechModel();
        var result = await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal(new byte[100], result.Audio);
    }

    [Fact]
    [UpstreamTest(
        Speech + "should include response data with timestamp, modelId and headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Speech_returns_the_timestamp_model_and_headers()
    {
        var handler = new ParityHandler(_ =>
            ParityHandler.Bytes(new byte[100], "audio/mp3", ("x-request-id", "test-request-id"))
        );
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new CartesiaSpeechModel(provider, "sonic-3.5", () => DateTime.UnixEpoch);
        var result = await model.DoGenerateAsync(SpeechCall(), CancellationToken.None);
        Assert.Equal(DateTime.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("sonic-3.5", result.Response.ModelId);
        Assert.Equal("audio/mp3", result.Response.Headers!["content-type"]);
        Assert.Equal("test-request-id", result.Response.Headers["x-request-id"]);
    }

    private static (CartesiaSpeechModel Model, ParityHandler Handler) SpeechModel()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mp3"));
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (new CartesiaSpeechModel(provider, "sonic-3.5"), handler);
    }

    private static SpeechModelCall SpeechCall(
        string text = "Hello, world!",
        string? voice = "test-voice-id",
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string providerOptions = "{}"
    )
    {
        return new SpeechModelCall(
            text,
            voice,
            outputFormat,
            instructions,
            speed,
            language,
            Json(providerOptions),
            new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static JsonElement Body(ParityHandler handler)
    {
        return Json(handler.Calls[0].Text);
    }

    private static void AssertWarning(
        IReadOnlyList<OperationWarning> warnings,
        string feature,
        string details
    )
    {
        var warning = Assert.Single(warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal(feature, warning.Feature);
        Assert.Equal(details, warning.Details);
    }
}
