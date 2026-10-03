// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Hume;

/// <summary>Hume settings.</summary>
public class HumeOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }
}

/// <summary>Parsed Hume error payload.</summary>
public sealed class HumeErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public HumeErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message.</summary>
    public string Message { get; }

    /// <summary>Provider code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing a Hume error body.</summary>
public sealed class HumeErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public HumeErrorParseResult(bool success, HumeErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when the error object was present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public HumeErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public HumeErrorData? RawValue { get; }
}

/// <summary>Parses Hume error JSON.</summary>
public static class HumeError
{
    /// <summary>Parses a resource error body.</summary>
    public static HumeErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new HumeErrorParseResult(true, new HumeErrorData(message, code));
        }

        return new HumeErrorParseResult(false, null);
    }
}

/// <summary>A warning produced while building a Hume speech request.</summary>
public sealed class HumeWarning
{
    /// <summary>Creates a warning.</summary>
    public HumeWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning type, such as <c>unsupported</c>.</summary>
    public string Type { get; }

    /// <summary>Feature that was ignored.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>Hume speech request.</summary>
public sealed class HumeSpeechRequest
{
    /// <summary>Default Hume voice used when the caller does not set one.</summary>
    public const string DefaultVoiceId = "d8ab67c6-953d-4bd8-9370-8fa53a0f1453";

    /// <summary>Creates a speech request.</summary>
    public HumeSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id. Null selects <see cref="DefaultVoiceId"/>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output container. <c>mp3</c>, <c>pcm</c>, and <c>wav</c> are sent as-is.</summary>
    public string OutputFormat { get; set; } = "mp3";

    /// <summary>Speaking rate, when set.</summary>
    public double? Speed { get; set; }

    /// <summary>Ignored. Hume does not accept a language.</summary>
    public string? Language { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Hume speech response metadata.</summary>
public sealed class HumeSpeechResponse
{
    /// <summary>Creates response metadata.</summary>
    public HumeSpeechResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id. Hume speech uses an empty id.</summary>
    public string ModelId { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Synthesized Hume audio.</summary>
public sealed class HumeSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public HumeSpeechResult(byte[] audio, string mediaType, IReadOnlyList<HumeWarning> warnings, HumeSpeechResponse response)
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        Warnings = warnings ?? Array.Empty<HumeWarning>();
        Response = response;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Response media type.</summary>
    public string MediaType { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<HumeWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public HumeSpeechResponse Response { get; }
}

/// <summary>Hume text-to-speech provider.</summary>
public sealed class HumeProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "hume";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.hume.ai";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public HumeProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new HumeProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new HumeProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Hume does not provide language models.");
    }

    /// <summary>Creates the speech model. Hume does not take a model id.</summary>
    public HumeSpeechModel Speech()
    {
        return new HumeSpeechModel(this);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new HumeSpeechModel(this);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "HUME_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "X-Hume-Api-Key";
        options.UserAgent = ProviderExchange.UserAgent("hume");
        return options;
    }

    /// <summary>Posts utterances to <c>/v0/tts/file</c>.</summary>
    public sealed class HumeSpeechModel : ISpeechModel
    {
        private readonly HumeProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public HumeSpeechModel(HumeProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "hume.speech"; }
        }

        /// <inheritdoc />
        public string ModelId
        {
            get { return string.Empty; }
        }

        /// <inheritdoc />
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            var request = new HumeSpeechRequest(options.Text) { Voice = options.Voice };
            var result = await GenerateAsync(request, cancellationToken).ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType);
        }

        /// <summary>Synthesizes speech and returns the audio bytes with response metadata.</summary>
        public async Task<HumeSpeechResult> GenerateAsync(HumeSpeechRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var warnings = new List<HumeWarning>();
            var format = request.OutputFormat ?? "mp3";
            if (!string.Equals(format, "mp3", StringComparison.Ordinal)
                && !string.Equals(format, "pcm", StringComparison.Ordinal)
                && !string.Equals(format, "wav", StringComparison.Ordinal))
            {
                warnings.Add(new HumeWarning("unsupported", "outputFormat", "Unsupported output format: " + format + ". Using mp3 instead."));
                format = "mp3";
            }

            if (!string.IsNullOrEmpty(request.Language))
            {
                warnings.Add(new HumeWarning(
                    "unsupported",
                    "language",
                    "Hume speech models do not support language selection. Language parameter \"" + request.Language + "\" was ignored."));
            }

            var voice = string.IsNullOrEmpty(request.Voice) ? HumeSpeechRequest.DefaultVoiceId : request.Voice;
            var utterance = new JsonObject
            {
                ["text"] = request.Text,
                ["voice"] = new JsonObject { ["id"] = voice, ["provider"] = "HUME_AI" },
            };
            if (request.Speed is { } speed)
            {
                utterance["speed"] = speed;
            }

            var body = new JsonObject
            {
                ["utterances"] = new JsonArray(utterance),
                ["format"] = new JsonObject { ["type"] = format },
            };
            var options = _provider.Options as HumeOptions;
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers);
            ProviderExchange.AddUserAgent(headers, "hume");
            var response = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/v0/tts/file"),
                ProviderExchange.Json(body.ToJsonString()),
                headers,
                cancellationToken).ConfigureAwait(false);
            var mediaType = "audio/" + format;
            if (response.Headers.TryGetValue("Content-Type", out var contentType) && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var separator = contentType.IndexOf(';');
                mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            }

            return new HumeSpeechResult(response.Bytes, mediaType, warnings, new HumeSpeechResponse(started, string.Empty, response.Headers));
        }
    }
}

/// <summary>Registers Hume.</summary>
public static class HumeServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddHume(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(HumeProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new HumeProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HumeProvider.ProviderId), options);
        });
        return services;
    }
}
