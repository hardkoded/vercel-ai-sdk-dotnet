// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cartesia;

/// <summary>Cartesia settings.</summary>
public class CartesiaOptions : OpenAICompatibleOptions
{
    /// <summary>The Cartesia API version, sent as the <c>Cartesia-Version</c> header. Defaults to <c>2026-03-01</c>.</summary>
    public string? Version { get; set; }

    /// <summary>Custom WebSocket implementation for Ink 2 streaming transcription. Defaults to <c>ClientWebSocket</c>.</summary>
    public WebSocketFactory? WebSocket { get; set; }
}

/// <summary>Cartesia provider.</summary>
public sealed class CartesiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "cartesia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.cartesia.ai";

    /// <summary>Default Cartesia API version. Maps to <c>CARTESIA_API_VERSION</c>.</summary>
    public const string DefaultVersion = "2026-03-01";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public CartesiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Schema of a Cartesia error body.</summary>
    public static JsonObject ErrorSchema { get; } = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["error_code"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
            ["title"] = new JsonObject { ["type"] = "string" },
            ["message"] = new JsonObject { ["type"] = "string" },
            ["request_id"] = new JsonObject { ["type"] = "string" },
            ["doc_url"] = new JsonObject { ["type"] = "string" },
        },
        ["required"] = new JsonArray("title", "message", "request_id"),
    };

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Cartesia does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new CartesiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new CartesiaProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "CARTESIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.Headers["Cartesia-Version"] = (options as CartesiaOptions)?.Version ?? DefaultVersion;
        options.UserAgent = ProviderExchange.UserAgent("cartesia");
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new CartesiaSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new CartesiaTranscriptionModel(this, modelId);

    /// <summary>Synthesizes speech. <paramref name="headers"/> override the provider headers.</summary>
    public async Task<SpeechResult> GenerateSpeechAsync(string modelId, SpeechCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var model = new CartesiaSpeechModel(this, modelId);
        var call = new SpeechModelCall(options.Text, options.Voice, null, null, null, null, JsonValues.EmptyObject(), new Dictionary<string, string>(headers ?? new Dictionary<string, string>()), cancellationToken);
        var result = await model.DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, "audio/mpeg");
    }

    /// <summary>The Cartesia API version sent with every request.</summary>
    internal string Version => (Options as CartesiaOptions)?.Version ?? DefaultVersion;

    /// <summary>The WebSocket factory for streaming transcription.</summary>
    internal WebSocketFactory? WebSocket => (Options as CartesiaOptions)?.WebSocket;

    internal async Task<ProviderExchangeResult> SendAsync(string path, HttpContent content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var merged = ProviderExchange.Merge(CreateHeaders(), headers);
        return await ProviderExchange.SendAsync(_httpClient, HttpMethod.Post, ApiKeys.Combine(Options.BaseUrl, path), content, merged, cancellationToken).ConfigureAwait(false);
    }

    internal static JsonElement? CartesiaOptions(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind == JsonValueKind.Object
            && providerOptions.TryGetProperty(ProviderId, out var cartesia)
            && cartesia.ValueKind == JsonValueKind.Object)
        {
            return cartesia;
        }

        return null;
    }

    internal static string? ReadString(JsonElement? element, string name)
    {
        return element is { } value && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    internal static double? ReadNumber(JsonElement? element, string name)
    {
        return element is { } value && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : null;
    }
}

/// <summary>Cartesia text-to-speech model. Posts to <c>/tts/bytes</c>.</summary>
public sealed class CartesiaSpeechModel : ISpeechModel, ISpeechCaller
{
    private static readonly int[] SampleRates = { 8000, 16000, 22050, 24000, 44100, 48000 };

    private static readonly string[] Emotions =
    {
        "neutral", "happy", "excited", "enthusiastic", "elated", "euphoric", "triumphant", "amazed", "surprised", "flirtatious",
        "curious", "content", "peaceful", "serene", "calm", "grateful", "affectionate", "trust", "sympathetic", "anticipation",
        "mysterious", "angry", "mad", "outraged", "frustrated", "agitated", "threatened", "disgusted", "contempt", "envious",
        "sarcastic", "ironic", "sad", "dejected", "melancholic", "disappointed", "hurt", "guilty", "bored", "tired",
        "rejected", "nostalgic", "wistful", "apologetic", "hesitant", "insecure", "confused", "resigned", "anxious", "panicked",
        "alarmed", "scared", "proud", "confident", "distant", "skeptical", "contemplative", "determined",
    };

    private readonly CartesiaProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a speech model. <paramref name="clock"/> sets the response timestamp.</summary>
    public CartesiaSpeechModel(CartesiaProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="ISpeechModel.Provider" />
    public string Provider => "cartesia.speech";

    /// <inheritdoc cref="ISpeechModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var call = new SpeechModelCall(options.Text, options.Voice, null, null, null, null, JsonValues.EmptyObject(), new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, "audio/mpeg");
    }

    /// <inheritdoc />
    public async Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var cartesia = CartesiaProvider.CartesiaOptions(call.ProviderOptions);
        if (string.IsNullOrEmpty(call.Voice))
        {
            throw new AiSdkException("Cartesia speech models require a `voice` to be set.");
        }

        ValidateOptions(cartesia);
        var warnings = new List<OperationWarning>();
        var body = new JsonObject
        {
            ["model_id"] = ModelId,
            ["transcript"] = call.Text,
            ["voice"] = new JsonObject { ["mode"] = "id", ["id"] = call.Voice },
            ["output_format"] = OutputFormat(call.OutputFormat ?? "mp3", cartesia, warnings),
        };
        var providerLocale = CartesiaProvider.ReadString(cartesia, "locale");
        var providerLanguage = CartesiaProvider.ReadString(cartesia, "language");
        if (providerLocale != null && providerLanguage != null)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                cartesia?.GetRawText(),
                "providerOptions.cartesia.locale cannot be combined with providerOptions.cartesia.language");
        }

        if (providerLocale != null)
        {
            body["locale"] = providerLocale;
            if (!string.IsNullOrEmpty(call.Language))
            {
                warnings.Add(OperationWarning.Unsupported("language", "Cartesia does not support both locale and language. The language option was ignored in favor of providerOptions.cartesia.locale."));
            }
        }
        else if (providerLanguage != null)
        {
            body["language"] = providerLanguage;
        }
        else if (!string.IsNullOrEmpty(call.Language))
        {
            body["language"] = call.Language;
        }

        if (CartesiaProvider.ReadString(cartesia, "normalization") is { } normalization)
        {
            body["normalization"] = normalization;
        }

        if (CartesiaProvider.ReadString(cartesia, "pronunciationDictionaryId") is { } pronunciationDictionaryId)
        {
            body["pronunciation_dict_id"] = pronunciationDictionaryId;
        }

        var generationConfig = new JsonObject();
        if ((CartesiaProvider.ReadNumber(cartesia, "speed") ?? call.Speed) is { } speed)
        {
            if (speed >= 0.6 && speed <= 1.5)
            {
                generationConfig["speed"] = speed;
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported("speed", "Cartesia speed must be between 0.6 and 1.5. The speed option was ignored."));
            }
        }

        if (CartesiaProvider.ReadNumber(cartesia, "volume") is { } volume)
        {
            generationConfig["volume"] = volume;
        }

        if (CartesiaProvider.ReadString(cartesia, "emotion") is { } emotion)
        {
            generationConfig["emotion"] = emotion;
        }

        if (generationConfig.Count > 0)
        {
            body["generation_config"] = generationConfig;
        }

        if (!string.IsNullOrEmpty(call.Instructions))
        {
            warnings.Add(OperationWarning.Unsupported("instructions", "Cartesia speech models do not support instructions. Instructions parameter was ignored."));
        }

        var response = await _provider.SendAsync("/tts/bytes", ProviderExchange.Json(body.ToJsonString()), call.Headers, cancellationToken).ConfigureAwait(false);
        return new SpeechModelResult(response.Bytes, warnings, response: new ProviderResponse(response.Headers, timestamp: timestamp, modelId: ModelId));
    }

    private static void ValidateOptions(JsonElement? cartesia)
    {
        if (cartesia is not { } values)
        {
            return;
        }

        foreach (var property in values.EnumerateObject())
        {
            var value = property.Value;
            var valid = property.Name switch
            {
                "volume" => value.ValueKind == JsonValueKind.Number && value.GetDouble() is >= 0.5 and <= 2,
                "emotion" => value.ValueKind == JsonValueKind.String && Array.IndexOf(Emotions, value.GetString()) >= 0,
                "locale" or "normalization" or "pronunciationDictionaryId" => value.ValueKind == JsonValueKind.String,
                _ => true,
            };
            if (!valid)
            {
                throw new InvalidArgumentException("providerOptions", values.GetRawText(), "Invalid cartesia provider options: " + property.Name + ".");
            }
        }
    }

    private static JsonObject OutputFormat(string outputFormat, JsonElement? cartesia, List<OperationWarning> warnings)
    {
        var parts = outputFormat.ToLowerInvariant().Split('_');
        var mapped = Mapped(parts[0]);
        var resolved = mapped ?? Mapped("mp3")!;
        if (mapped == null)
        {
            warnings.Add(OperationWarning.Unsupported(
                "outputFormat",
                "Unknown output format \"" + outputFormat + "\". Falling back to mp3. Use providerOptions.cartesia to configure container, encoding, and sampleRate directly."));
        }
        else if (parts.Length > 1)
        {
            if (parts.Length == 2 && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var rate) && Array.IndexOf(SampleRates, rate) >= 0)
            {
                resolved.SampleRate = rate;
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported(
                    "outputFormat",
                    "Unsupported Cartesia sample rate in output format \"" + outputFormat + "\". Using " + resolved.SampleRate.ToString(CultureInfo.InvariantCulture) + " Hz instead."));
            }
        }

        var container = CartesiaProvider.ReadString(cartesia, "container") ?? resolved.Container;
        var sampleRate = (int?)CartesiaProvider.ReadNumber(cartesia, "sampleRate") ?? resolved.SampleRate;
        var encoding = CartesiaProvider.ReadString(cartesia, "encoding");
        var bitRate = (int?)CartesiaProvider.ReadNumber(cartesia, "bitRate");
        if (container == "mp3")
        {
            if (encoding != null)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "providerOptions.cartesia.encoding",
                    "Cartesia MP3 output does not accept an encoding. The encoding option was ignored."));
            }

            return new JsonObject
            {
                ["container"] = container,
                ["sample_rate"] = sampleRate,
                ["bit_rate"] = bitRate ?? 128000,
            };
        }

        if (bitRate != null)
        {
            warnings.Add(OperationWarning.Unsupported(
                "providerOptions.cartesia.bitRate",
                "Cartesia raw and WAV output do not accept a bit rate. The bitRate option was ignored."));
        }

        return new JsonObject
        {
            ["container"] = container,
            ["encoding"] = encoding ?? resolved.Encoding ?? (container == "wav" ? "pcm_s16le" : "pcm_f32le"),
            ["sample_rate"] = sampleRate,
        };
    }

    private static Format? Mapped(string name)
    {
        switch (name)
        {
            case "alaw":
                return new Format("raw", "pcm_alaw", 8000);
            case "mp3":
                return new Format("mp3", null, 44100);
            case "mulaw":
                return new Format("raw", "pcm_mulaw", 8000);
            case "pcm":
            case "raw":
                return new Format("raw", "pcm_f32le", 44100);
            case "wav":
                return new Format("wav", "pcm_s16le", 44100);
            default:
                return null;
        }
    }

    private sealed class Format
    {
        public Format(string container, string? encoding, int sampleRate)
        {
            Container = container;
            Encoding = encoding;
            SampleRate = sampleRate;
        }

        public string Container { get; }

        public string? Encoding { get; }

        public int SampleRate { get; set; }
    }
}

/// <summary>Cartesia batch transcription model. Posts multipart audio to <c>/stt</c>.</summary>
public sealed class CartesiaTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private readonly CartesiaProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp.</summary>
    public CartesiaTranscriptionModel(CartesiaProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="ITranscriptionModel.Provider" />
    public string Provider => "cartesia.transcription";

    /// <inheritdoc cref="ITranscriptionModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        if (audio is null)
        {
            throw new ArgumentNullException(nameof(audio));
        }

        var call = new TranscriptionModelCall(audio.Data, audio.MediaType, JsonValues.EmptyObject(), new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (IsStreamingModel)
        {
            throw Unsupported("non-streaming transcription with " + ModelId);
        }

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var cartesia = CartesiaProvider.CartesiaOptions(call.ProviderOptions);
        CartesiaStreamingTranscription.ValidateProviderOptions(cartesia);
        var warnings = new List<OperationWarning>();
        if (cartesia is { } options && options.TryGetProperty("streaming", out var streaming) && streaming.ValueKind != JsonValueKind.Null)
        {
            warnings.Add(OperationWarning.Unsupported("providerOptions.cartesia.streaming", "Cartesia batch transcription does not support streaming options."));
        }

        var form = new MultipartFormDataContent();
        form.Add(new StringContent(ModelId), "model");
        var file = new ByteArrayContent(call.Audio);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(call.MediaType);
        form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(call.MediaType));
        if (CartesiaProvider.ReadString(cartesia, "language") is { } language)
        {
            form.Add(new StringContent(language), "language");
        }

        if (cartesia is { } granularOptions && granularOptions.TryGetProperty("timestampGranularities", out var granularities) && granularities.ValueKind == JsonValueKind.Array)
        {
            foreach (var granularity in granularities.EnumerateArray())
            {
                form.Add(new StringContent(granularity.GetString() ?? string.Empty), "timestamp_granularities[]");
            }
        }

        var response = await _provider.SendAsync("/stt", form, call.Headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var segments = new List<TranscriptSegment>();
        if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
        {
            foreach (var word in words.EnumerateArray())
            {
                segments.Add(new TranscriptSegment(word.GetProperty("word").GetString() ?? string.Empty, word.GetProperty("start").GetDouble(), word.GetProperty("end").GetDouble()));
            }
        }

        return new TranscriptionModelResult(
            root.GetProperty("text").GetString(),
            segments,
            CartesiaProvider.ReadString(root, "language"),
            CartesiaProvider.ReadNumber(root, "duration"),
            warnings,
            response: new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp, modelId: ModelId));
    }

    /// <inheritdoc />
    public async Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (!IsStreamingModel)
        {
            throw Unsupported("streaming transcription with " + ModelId);
        }

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        return await CartesiaStreamingTranscription.StreamAsync(ModelId, _provider, call, timestamp, CreateStreamingAccessTokenAsync, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates a short-lived, STT-scoped access token so the API key stays out of the WebSocket URL.</summary>
    internal async Task<string> CreateStreamingAccessTokenAsync(IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        var request = headers.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value!);
        var response = await _provider.SendAsync("/access-token", ProviderExchange.Json("{\"grants\":{\"stt\":true}}"), request, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        if (document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("token", out var token)
            && token.ValueKind == JsonValueKind.String
            && !string.IsNullOrEmpty(token.GetString()))
        {
            return token.GetString()!;
        }

        throw new AiSdkException("Cartesia access token response is missing a token.");
    }

    private bool IsStreamingModel => CartesiaModelCatalog.GetCartesiaTranscriptionModelEntry(ModelId).Streaming;

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(functionality, "'" + functionality + "' functionality not supported.");
    }
}

/// <summary>Registers Cartesia.</summary>
public static class CartesiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddCartesia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(CartesiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new CartesiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(CartesiaProvider.ProviderId), options);
        });
        return services;
    }
}
