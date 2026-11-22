// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.AssemblyAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.AssemblyAISpeechModel;

public sealed class DoGenerateTests
{
    private const string Prefix =
        "packages/assemblyai/src/assemblyai-speech-model.test.ts::AssemblyAISpeechModel > doGenerate::";

    private const string Commit = "e6ad3867b8bfa5efcabe2cf0e737385c726d8fea";

    [Fact]
    [UpstreamTest(
        Prefix + "should send a buffered mp3 request by default",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_a_buffered_mp3_request_by_default()
    {
        var (model, handler) = Model();

        var result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!"),
            CancellationToken.None
        );

        Assert.Equal("https://tts.us.assemblyai.com/v1/speech", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(
            "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"mp3\",\"stream\":false}",
            handler.Calls[0].Text
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass the API key, custom headers, and user agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_the_API_key_custom_headers_and_user_agent()
    {
        var handler = AudioHandler();
        var options = new AssemblyAIOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = AssemblyAIProvider.Create(options, handler);

        await provider
            .Speech("assemblyai-tts-1")
            .GenerateAsync(
                new AssemblyAISpeechRequest("Hello, world!")
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    },
                },
                CancellationToken.None
            );

        var call = handler.Calls[0];
        Assert.Equal("test-api-key", call.Header("Authorization"));
        Assert.StartsWith(
            "application/json",
            call.Header("Content-Type"),
            StringComparison.OrdinalIgnoreCase
        );
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains(
            "ai-sdk/assemblyai/" + AiSdkVersion.Version,
            call.Header("User-Agent"),
            StringComparison.Ordinal
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the voice and translate the language code",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_the_voice_and_translate_the_language_code()
    {
        var (model, handler) = Model();

        await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hola, mundo!") { Voice = "aai-kyla", Language = "es" },
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.Equal("aai-kyla", body.GetProperty("voice_id").GetString());
        Assert.Equal("spanish", body.GetProperty("language").GetString());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept a full language name",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_accept_a_full_language_name()
    {
        var (model, handler) = Model();

        await model.GenerateAsync(
            new AssemblyAISpeechRequest("Bonjour") { Language = "French" },
            CancellationToken.None
        );

        Assert.Equal("french", Body(handler).GetProperty("language").GetString());
    }

    [Theory]
    [InlineData(
        "wav",
        "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"pcm_s16le\",\"stream\":false,\"container\":\"wav\"}"
    )]
    [InlineData(
        "pcm",
        "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"pcm_s16le\",\"stream\":false,\"container\":\"raw\"}"
    )]
    [InlineData(
        "mulaw",
        "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"pcm_mulaw\",\"stream\":false,\"container\":\"raw\",\"sample_rate\":8000}"
    )]
    [InlineData(
        "alaw",
        "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"pcm_alaw\",\"stream\":false,\"container\":\"raw\",\"sample_rate\":8000}"
    )]
    [UpstreamTest(
        Prefix + "should map the %s output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_the_output_format(string outputFormat, string expected)
    {
        var (model, handler) = Model();

        await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!") { OutputFormat = outputFormat },
            CancellationToken.None
        );

        AssertJsonEqual(expected, handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to mp3 for an unknown output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_fall_back_to_mp3_for_an_unknown_output_format()
    {
        var (model, handler) = Model();

        var result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!") { OutputFormat = "opus" },
            CancellationToken.None
        );

        Assert.Equal("mp3", Body(handler).GetProperty("encoding").GetString());
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(result.Warnings));
        Assert.Equal("outputFormat", warning.Feature);
        Assert.Equal(
            "Unknown output format \"opus\". Falling back to mp3. Use providerOptions.assemblyai to configure encoding, container, and sampleRate directly.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should apply provider options over the output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_apply_provider_options_over_the_output_format()
    {
        var (model, handler) = Model();

        await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!")
            {
                OutputFormat = "mulaw",
                ProviderOptions = Options("{\"encoding\":\"pcm_s16le\",\"sampleRate\":16000}"),
            },
            CancellationToken.None
        );

        // The mulaw container and 8 kHz rate do not carry over to pcm_s16le.
        AssertJsonEqual(
            "{\"text\":\"Hello, world!\",\"model_id\":\"assemblyai-tts-1\",\"encoding\":\"pcm_s16le\",\"sample_rate\":16000,\"stream\":false}",
            handler.Calls[0].Text
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the bit rate for mp3 output",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_the_bit_rate_for_mp3_output()
    {
        var (model, handler) = Model();

        await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!")
            {
                ProviderOptions = Options("{\"bitRate\":128000}"),
            },
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.Equal("mp3", body.GetProperty("encoding").GetString());
        Assert.Equal(128000, body.GetProperty("bit_rate").GetInt32());
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore options that do not fit the encoding",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_ignore_options_that_do_not_fit_the_encoding()
    {
        var (model, handler) = Model();

        var mp3Result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!")
            {
                ProviderOptions = Options("{\"container\":\"wav\"}"),
            },
            CancellationToken.None
        );
        var wavResult = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!")
            {
                OutputFormat = "wav",
                ProviderOptions = Options("{\"bitRate\":64000}"),
            },
            CancellationToken.None
        );

        Assert.False(Body(handler, 0).TryGetProperty("container", out _));
        Assert.False(Body(handler, 1).TryGetProperty("bit_rate", out _));
        var warnings = mp3Result.Warnings.Concat(wavResult.Warnings).Cast<UnsupportedWarning>().ToList();
        Assert.Equal(2, warnings.Count);
        Assert.Equal("providerOptions.assemblyai.container", warnings[0].Feature);
        Assert.Equal(
            "AssemblyAI MP3 output has no container. The container option was ignored.",
            warnings[0].Details
        );
        Assert.Equal("providerOptions.assemblyai.bitRate", warnings[1].Feature);
        Assert.Equal(
            "AssemblyAI only accepts a bit rate for MP3 output. The bitRate option was ignored.",
            warnings[1].Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should warn on unsupported language, speed, and instructions",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_on_unsupported_language_speed_and_instructions()
    {
        var (model, handler) = Model();

        var result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!")
            {
                Language = "ja",
                Speed = 1.2,
                Instructions = "Speak slowly",
            },
            CancellationToken.None
        );

        var body = Body(handler);
        Assert.False(body.TryGetProperty("language", out _));
        Assert.False(body.TryGetProperty("speed", out _));
        Assert.Equal(3, result.Warnings.Count);
        Assert.All(result.Warnings, warning => Assert.IsType<UnsupportedWarning>(warning));
        Assert.Contains(
            result.Warnings.Cast<UnsupportedWarning>(),
            warning =>
                warning.Feature == "language"
                && warning.Details
                    == "AssemblyAI text-to-speech does not support language \"ja\" (supported: de, en, es, fr, it, pt). The language option was ignored."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit automatic language detection with a warning",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_automatic_language_detection_with_a_warning()
    {
        var (model, handler) = Model();

        var result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!") { Language = "auto" },
            CancellationToken.None
        );

        Assert.False(Body(handler).TryGetProperty("language", out _));
        var warning = Assert.IsType<UnsupportedWarning>(Assert.Single(result.Warnings));
        Assert.Equal("language", warning.Feature);
        Assert.Equal(
            "AssemblyAI text-to-speech does not detect the language. The voice speaks its own language.",
            warning.Details
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not warn for the default speed",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_warn_for_the_default_speed()
    {
        var (model, _) = Model();

        var result = await model.GenerateAsync(
            new AssemblyAISpeechRequest("Hello, world!") { Speed = 1 },
            CancellationToken.None
        );

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return the audio, usage, and response metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_the_audio_usage_and_response_metadata()
    {
        var audio = new byte[100];
        var handler = new ParityHandler(_ =>
            ParityHandler.Bytes(
                audio,
                "audio/mpeg",
                ("x-request-id", "test-request-id"),
                ("x-tts-model", "assemblyai-tts-1"),
                ("x-tts-voice-id", "aai-kyla"),
                ("x-tts-language", "english"),
                ("x-tts-encoding", "mp3"),
                ("x-tts-sample-rate", "24000"),
                ("x-tts-audio-duration-ms", "3200"),
                ("x-tts-billed-characters", "13")
            )
        );
        var provider = AssemblyAIProvider.Create(
            new AssemblyAIOptions
            {
                ApiKey = "test-api-key",
                Clock = () => DateTimeOffset.UnixEpoch,
            },
            handler
        );

        var result = await provider
            .Speech("assemblyai-tts-1")
            .GenerateAsync(new AssemblyAISpeechRequest("Hello, world!"), CancellationToken.None);

        Assert.Equal(audio, result.Audio);
        Assert.Equal(13, result.Usage!.Characters);
        Assert.Equal(
            "{\"assemblyai\":{\"requestId\":\"test-request-id\",\"model\":\"assemblyai-tts-1\",\"voiceId\":\"aai-kyla\",\"language\":\"english\",\"encoding\":\"mp3\",\"sampleRate\":24000,\"audioDurationMs\":3200,\"billedCharacters\":13}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Equal(DateTimeOffset.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("assemblyai-tts-1", result.Response.ModelId);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should surface API errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_surface_API_errors()
    {
        var handler = new ParityHandler(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(
                "{\"code\":\"voice_not_found\",\"message\":\"voice_id: no voice with this ID or slug\",\"param\":\"voice_id\",\"request_id\":\"test-request-id\",\"errors\":null,\"retryable\":false,\"retry_after_seconds\":null,\"error\":\"voice_id: no voice with this ID or slug\"}",
                Encoding.UTF8,
                "application/json"
            ),
        });
        var provider = AssemblyAIProvider.Create(
            new AssemblyAIOptions { ApiKey = "test-api-key" },
            handler
        );

        var error = Assert.IsAssignableFrom<ApiException>(
            await Record.ExceptionAsync(() =>
                provider
                    .Speech("assemblyai-tts-1")
                    .GenerateAsync(
                        new AssemblyAISpeechRequest("Hello, world!") { Voice = "missing" },
                        CancellationToken.None
                    )
            )
        );

        Assert.Equal("voice_id: no voice with this ID or slug", error.Message);
        Assert.Equal(422, error.StatusCode);
    }

    private static ParityHandler AudioHandler()
    {
        return new ParityHandler(_ => ParityHandler.Bytes(new byte[100], "audio/mpeg"));
    }

    private static (AssemblyAIProvider.AssemblyAISpeechModel Model, ParityHandler Handler) Model()
    {
        var handler = AudioHandler();
        var provider = AssemblyAIProvider.Create(
            new AssemblyAIOptions { ApiKey = "test-api-key" },
            handler
        );
        return (provider.Speech("assemblyai-tts-1"), handler);
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string assemblyai)
    {
        using var document = JsonDocument.Parse(assemblyai);
        return new Dictionary<string, JsonElement>
        {
            ["assemblyai"] = document.RootElement.Clone(),
        };
    }

    private static JsonElement Body(ParityHandler handler, int call = 0)
    {
        using var document = JsonDocument.Parse(handler.Calls[call].Text);
        return document.RootElement.Clone();
    }

    private static void AssertJsonEqual(string expected, string actual)
    {
        using var left = JsonDocument.Parse(expected);
        using var right = JsonDocument.Parse(actual);
        var l = left.RootElement.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal);
        var r = right.RootElement.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal);
        Assert.Equal(l.Select(p => p.Name + "=" + p.Value.GetRawText()), r.Select(p => p.Name + "=" + p.Value.GetRawText()));
    }
}
