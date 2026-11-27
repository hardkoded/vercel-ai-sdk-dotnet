// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Cartesia;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.Upstream.Cartesia.CartesiaTranscriptionModel;

/// <summary>Port of <c>cartesia-transcription-model.test.ts</c> &gt; <c>doGenerate &gt; transcription</c> and its response metadata tests.</summary>
public sealed class DoGenerateTests
{
    private const string Id =
        "packages/cartesia/src/cartesia-transcription-model.test.ts::doGenerate > ";

    private const string TranscriptionJson =
        "{\"text\":\"Hello from the Vercel AI SDK.\",\"language\":\"en\",\"duration\":2.479,\"words\":[{\"word\":\"Hello\",\"start\":0.199,\"end\":0.479},{\"word\":\"from\",\"start\":0.5,\"end\":0.639},{\"word\":\"the\",\"start\":0.66,\"end\":0.759},{\"word\":\"Vercel\",\"start\":0.759,\"end\":1.12},{\"word\":\"AI\",\"start\":1.2,\"end\":1.519},{\"word\":\"SDK.\",\"start\":1.58,\"end\":2.479}]}";

    [Fact]
    [UpstreamTest(Id + "transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task sends_the_model_in_the_form()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("https://api.cartesia.ai/stt", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("ink-whisper", FormValue(handler.Calls[0], "model"));
    }

    [Fact]
    [UpstreamTest(
        Id + "transcription::routes dated Ink Whisper snapshots to the batch endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task routes_dated_Ink_Whisper_snapshots_to_the_batch_endpoint()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson));
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new global::Vercel.AI.Cartesia.CartesiaTranscriptionModel(
            provider,
            "ink-whisper-2025-06-04"
        );

        await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);

        Assert.Equal("https://api.cartesia.ai/stt", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("ink-whisper-2025-06-04", FormValue(handler.Calls[0], "model"));
    }

    [Fact]
    [UpstreamTest(Id + "transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task sends_auth_version_custom_headers_and_user_agent()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var model = new global::Vercel.AI.Cartesia.CartesiaTranscriptionModel(
            CartesiaProvider.Create(options, handler),
            "ink-whisper"
        );
        await model.DoGenerateAsync(
            TranscriptionCall(
                headers: new Dictionary<string, string>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            ),
            CancellationToken.None
        );
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.False(string.IsNullOrEmpty(call.Header("Cartesia-Version")));
        Assert.StartsWith(
            "multipart/form-data; boundary=",
            call.Header("Content-Type"),
            StringComparison.Ordinal
        );
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains(
            "ai-sdk-cartesia/" + AiSdkVersion.Version,
            call.Header("User-Agent"),
            StringComparison.Ordinal
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "transcription::should extract the transcription text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task returns_the_text()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("Hello from the Vercel AI SDK.", result.Text);
    }

    [Fact]
    [UpstreamTest(
        Id + "transcription::should extract segments, language and duration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task returns_word_segments_language_and_duration()
    {
        var (model, _) = TranscriptionModel();
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal("en", result.Language);
        Assert.Equal(2.479, result.DurationInSeconds);
        Assert.Equal("Hello", result.Segments[0].Text);
        Assert.Equal(0.199, result.Segments[0].StartSecond);
        Assert.Equal(0.479, result.Segments[0].EndSecond);
    }

    [Fact]
    [UpstreamTest(
        Id + "transcription::should pass language and timestamp granularities",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task sends_language_and_timestamp_granularities()
    {
        var (model, handler) = TranscriptionModel();
        await model.DoGenerateAsync(
            TranscriptionCall(
                providerOptions: "{\"cartesia\":{\"language\":\"en\",\"timestampGranularities\":[\"word\"]}}"
            ),
            CancellationToken.None
        );
        var call = handler.Calls[0];
        Assert.Contains("filename=audio.wav", call.Text, StringComparison.Ordinal);
        Assert.Equal("ink-whisper", FormValue(call, "model"));
        Assert.Equal("en", FormValue(call, "language"));
        Assert.Equal("word", FormValue(call, "\"timestamp_granularities[]\""));
    }

    [Fact]
    [UpstreamTest(
        Id + "response metadata::should include response data with timestamp and modelId",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task returns_the_timestamp_and_model()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson));
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        var model = new global::Vercel.AI.Cartesia.CartesiaTranscriptionModel(
            provider,
            "ink-whisper",
            () => DateTime.UnixEpoch
        );
        var result = await model.DoGenerateAsync(TranscriptionCall(), CancellationToken.None);
        Assert.Equal(DateTime.UnixEpoch, result.Response.Timestamp);
        Assert.Equal("ink-whisper", result.Response.ModelId);
    }

    private static (
        global::Vercel.AI.Cartesia.CartesiaTranscriptionModel Model,
        ParityHandler Handler
    ) TranscriptionModel()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(TranscriptionJson));
        var provider = CartesiaProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            handler
        );
        return (
            new global::Vercel.AI.Cartesia.CartesiaTranscriptionModel(provider, "ink-whisper"),
            handler
        );
    }

    private static TranscriptionModelCall TranscriptionCall(
        string providerOptions = "{}",
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        return new TranscriptionModelCall(
            new byte[] { 1, 2, 3 },
            "audio/wav",
            Json(providerOptions),
            headers ?? new Dictionary<string, string>(),
            CancellationToken.None
        );
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string? FormValue(ParityCall call, string name)
    {
        var match = Regex.Match(
            call.Text,
            "name=" + Regex.Escape(name) + "\r\n(?:[^\r\n]+\r\n)*\r\n([^\r\n]*)\r\n"
        );
        return match.Success ? match.Groups[1].Value : null;
    }
}
