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

/// <summary>Cartesia provider.</summary>
public sealed class CartesiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "cartesia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.cartesia.ai";

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
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "CARTESIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.Headers["Cartesia-Version"] = "2026-03-01";
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

        var warnings = new List<OperationWarning>();
        var body = new JsonObject
        {
            ["model_id"] = ModelId,
            ["transcript"] = call.Text,
            ["voice"] = new JsonObject { ["mode"] = "id", ["id"] = call.Voice },
            ["output_format"] = OutputFormat(call.OutputFormat ?? "mp3", cartesia, warnings),
        };
        var language = CartesiaProvider.ReadString(cartesia, "language") ?? (string.IsNullOrEmpty(call.Language) ? null : call.Language);
        if (language != null)
        {
            body["language"] = language;
        }

        if ((CartesiaProvider.ReadNumber(cartesia, "speed") ?? call.Speed) is { } speed)
        {
            if (speed >= 0.6 && speed <= 1.5)
            {
                body["generation_config"] = new JsonObject { ["speed"] = speed };
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported("speed", "Cartesia speed must be between 0.6 and 1.5. The speed option was ignored."));
            }
        }

        if (!string.IsNullOrEmpty(call.Instructions))
        {
            warnings.Add(OperationWarning.Unsupported("instructions", "Cartesia speech models do not support instructions. Instructions parameter was ignored."));
        }

        var response = await _provider.SendAsync("/tts/bytes", ProviderExchange.Json(body.ToJsonString()), call.Headers, cancellationToken).ConfigureAwait(false);
        return new SpeechModelResult(response.Bytes, warnings, response: new ProviderResponse(response.Headers, timestamp: timestamp, modelId: ModelId));
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
    public bool CanStream => false;

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

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var cartesia = CartesiaProvider.CartesiaOptions(call.ProviderOptions);
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
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
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
