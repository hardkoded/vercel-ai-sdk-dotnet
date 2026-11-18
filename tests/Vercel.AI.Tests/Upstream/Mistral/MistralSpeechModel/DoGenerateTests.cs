// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Mistral.MistralSpeechModel.MistralSpeechTestSupport;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralSpeechModel;

/// <summary>Port of <c>mistral-speech-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id = "packages/mistral/src/mistral-speech-model.test.ts::doGenerate::";

    private static ISpeechCaller Speech(MistralProvider provider, string modelId = ModelId) =>
        (ISpeechCaller)provider.SpeechModel(modelId);

    private static SpeechModelCall Call(
        string text,
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string? providerOptions = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    ) =>
        new(
            text,
            voice,
            outputFormat,
            instructions,
            speed,
            language,
            providerOptions is null
                ? JsonDocument.Parse("{}").RootElement
                : JsonDocument.Parse(providerOptions).RootElement,
            headers ?? new Dictionary<string, string>(),
            cancellationToken
        );

    private static JsonNode Body(ParityCall call) => JsonNode.Parse(call.Text)!;

    [Fact]
    [UpstreamTest(
        Id + "should send a non-streaming request with default mp3 output",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_a_non_streaming_request_with_default_mp3_output()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"model\":\"voxtral-mini-tts-2603\",\"input\":\"Hello from the AI SDK!\",\"response_format\":\"mp3\",\"stream\":false}"
                ),
                Body(call)
            )
        );
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal(Url, call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(
        Id + "should map the voice to voice_id",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_the_voice_to_voice_id()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call("Hello from the AI SDK!", "voice-id"), CancellationToken.None);

        Assert.Equal("voice-id", (string?)Body(Assert.Single(handler.Calls))["voice_id"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass Voxtral 3 delivery controls and preserve inline tags",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_Voxtral_3_delivery_controls_and_preserve_inline_tags()
    {
        var handler = AudioHandler();

        var result = await Speech(CreateProvider(handler), "voxtral-mini-tts-3-0")
            .DoGenerateAsync(
                Call(
                    "[emotion=happy] I like tomato soup.",
                    "gb_jane_neutral",
                    providerOptions: "{\"mistral\":{\"expressiveness\":1.5,\"phonemeHints\":{\"tomato\":[\"t\",\"ə\",\"m\",\"ɑ\",\"t\",\"oʊ\"]}}}"
                ),
                CancellationToken.None
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"model\":\"voxtral-mini-tts-3-0\",\"input\":\"[emotion=happy] I like tomato soup.\",\"voice_id\":\"gb_jane_neutral\",\"expressiveness\":1.5,\"phoneme_hints\":{\"tomato\":[\"t\",\"ə\",\"m\",\"ɑ\",\"t\",\"oʊ\"]},\"response_format\":\"mp3\",\"stream\":false}"
                ),
                Body(Assert.Single(handler.Calls))
            )
        );
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("{\"expressiveness\":0.9}")]
    [InlineData("{\"expressiveness\":5.1}")]
    [InlineData("{\"phonemeHints\":{\"tomato\":[]}}")]
    [InlineData("{\"phonemeHints\":{\"tomato\":[\"\"]}}")]
    [UpstreamTest(
        Id + "should reject invalid delivery controls: %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_reject_invalid_delivery_controls(string options)
    {
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() =>
            Speech(CreateProvider(AudioHandler()), "voxtral-mini-tts-3-0")
                .DoGenerateAsync(
                    Call("I like tomato soup.", providerOptions: "{\"mistral\":" + options + "}"),
                    CancellationToken.None
                )
        );

        Assert.Contains("invalid mistral provider options", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should map refAudio to ref_audio and prefer it over voice",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_map_refAudio_to_ref_audio_and_prefer_it_over_voice()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call(
                    "Hello from the AI SDK!",
                    "voice-id",
                    providerOptions: "{\"mistral\":{\"refAudio\":\"cmVmZXJlbmNlLWF1ZGlv\"}}"
                ),
                CancellationToken.None
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"model\":\"voxtral-mini-tts-2603\",\"input\":\"Hello from the AI SDK!\",\"ref_audio\":\"cmVmZXJlbmNlLWF1ZGlv\",\"response_format\":\"mp3\",\"stream\":false}"
                ),
                Body(Assert.Single(handler.Calls))
            )
        );
    }

    [Theory]
    [InlineData("pcm")]
    [InlineData("wav")]
    [InlineData("mp3")]
    [InlineData("flac")]
    [InlineData("opus")]
    [UpstreamTest(
        Id + "should accept the %s output format",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_accept_the_output_format(string outputFormat)
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call("Hello from the AI SDK!", outputFormat: outputFormat),
                CancellationToken.None
            );

        Assert.Equal(outputFormat, (string?)Body(Assert.Single(handler.Calls))["response_format"]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn and use mp3 for unsupported output formats",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_and_use_mp3_for_unsupported_output_formats()
    {
        var handler = AudioHandler();

        var result = await Speech(CreateProvider(handler))
            .DoGenerateAsync(
                Call("Hello from the AI SDK!", outputFormat: "aac"),
                CancellationToken.None
            );

        Assert.Equal("mp3", (string?)Body(Assert.Single(handler.Calls))["response_format"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("outputFormat", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Id + "should warn for unsupported standard speech options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_warn_for_unsupported_standard_speech_options()
    {
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(
                Call(
                    "Hello from the AI SDK!",
                    instructions: "Speak cheerfully",
                    speed: 1.2,
                    language: "en"
                ),
                CancellationToken.None
            );

        Assert.Equal(
            new[] { "instructions", "speed", "language" },
            result.Warnings.Select(warning => warning.Feature).ToArray()
        );
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers and the Mistral user agent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_headers_and_the_Mistral_user_agent()
    {
        var handler = AudioHandler();
        var provider = CreateProvider(handler, options =>
            options.Headers["Custom-Provider-Header"] = "provider-header-value"
        );

        await Speech(provider)
            .DoGenerateAsync(
                Call(
                    "Hello from the AI SDK!",
                    headers: new Dictionary<string, string>
                    {
                        ["Custom-Request-Header"] = "request-header-value",
                    }
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal("Bearer test-api-key", call.Header("authorization"));
        Assert.StartsWith("application/json", call.Header("content-type"));
        Assert.Equal("provider-header-value", call.Header("custom-provider-header"));
        Assert.Equal("request-header-value", call.Header("custom-request-header"));
        Assert.Contains("ai-sdk/mistral/" + AiSdkVersion.Version, call.Header("user-agent"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should use a custom base URL",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_use_a_custom_base_URL()
    {
        var handler = AudioHandler();
        var provider = CreateProvider(handler, options =>
            options.BaseUrl = "https://custom.mistral.example/v2/"
        );

        await Speech(provider)
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal(
            "https://custom.mistral.example/v2/audio/speech",
            Assert.Single(handler.Calls).Uri.AbsoluteUri
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should use a custom fetch implementation",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The custom fetch maps to the HttpClient handler passed to MistralProvider.Create."
    )]
    public async Task Should_use_a_custom_fetch_implementation()
    {
        var handler = AudioHandler();

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal(Url, Assert.Single(handler.Calls).Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(
        Id + "should return base64 audio data",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The port returns the audio as bytes; the test compares the decoded base64."
    )]
    public async Task Should_return_base64_audio_data()
    {
        var result = await Speech(CreateProvider(AudioHandler("YXVkaW8=")))
            .DoGenerateAsync(Call("Hello from the AI SDK!"), CancellationToken.None);

        Assert.Equal("audio", Encoding.UTF8.GetString(result.Audio));
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include response data with timestamp, model id, and headers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_response_data_with_timestamp_model_id_and_headers()
    {
        var provider = CreateProvider(AudioHandler(headers: ("x-request-id", "test-request-id")));
        var testDate = new DateTime(0, DateTimeKind.Utc);
        var model = new Vercel.AI.Mistral.MistralSpeechModel(provider, ModelId, () => testDate);

        var result = await model.DoGenerateAsync(
            Call("Hello from the AI SDK!"),
            CancellationToken.None
        );

        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal(ModelId, result.Response.ModelId);
        Assert.Equal("test-request-id", result.Response.Headers!["x-request-id"]);
        Assert.Equal(
            "SUQzBAAAAAAA",
            result.Response.Body!.Value.GetProperty("audio_data").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should redact reference audio from request metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_redact_reference_audio_from_request_metadata()
    {
        var result = await Speech(CreateProvider(AudioHandler()))
            .DoGenerateAsync(
                Call(
                    "Hello from the AI SDK!",
                    providerOptions: "{\"mistral\":{\"refAudio\":\"sensitive-reference-audio\"}}"
                ),
                CancellationToken.None
            );

        Assert.Equal(
            "{\"model\":\"voxtral-mini-tts-2603\",\"input\":\"Hello from the AI SDK!\",\"ref_audio\":\"[redacted]\",\"response_format\":\"mp3\",\"stream\":false}",
            result.Request?.Body
        );
        Assert.DoesNotContain("sensitive-reference-audio", result.Request?.Body);
    }

    [Fact]
    [UpstreamTest(
        Id + "should redact reference audio from API errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_redact_reference_audio_from_API_errors()
    {
        var handler = new ParityHandler(_ =>
            Error(
                HttpStatusCode.BadRequest,
                "{\"object\":\"error\",\"message\":\"The request was rejected.\",\"type\":\"invalid_request_error\",\"param\":\"ref_audio\",\"code\":\"invalid_reference_audio\"}"
            )
        );

        var error = await Assert.ThrowsAsync<BadRequestException>(() =>
            Speech(CreateProvider(handler))
                .DoGenerateAsync(
                    Call(
                        "Hello from the AI SDK!",
                        providerOptions: "{\"mistral\":{\"refAudio\":\"sensitive-reference-audio\"}}"
                    ),
                    CancellationToken.None
                )
        );

        Assert.Equal("The request was rejected.", error.Message);
        Assert.Equal(400, error.StatusCode);
        var values = Assert.IsType<JsonObject>(error.RequestBodyValues);
        Assert.Equal("[redacted]", (string?)values["ref_audio"]);
        Assert.DoesNotContain("sensitive-reference-audio", values.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        Id + "should forward the abort signal",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "The AbortSignal maps to the CancellationToken; the request token follows the caller's token."
    )]
    public async Task Should_forward_the_abort_signal()
    {
        using var abort = new CancellationTokenSource();
        var forwarded = false;
        var handler = new ParityHandler(call =>
        {
            abort.Cancel();
            forwarded = call.CancellationToken.IsCancellationRequested;
            return ParityHandler.Json("{\"audio_data\":\"YXVkaW8=\"}");
        });

        await Speech(CreateProvider(handler))
            .DoGenerateAsync(Call("Hello from the AI SDK!"), abort.Token);

        Assert.True(forwarded);
    }
}
