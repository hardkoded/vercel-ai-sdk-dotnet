// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.FishAudio;

/// <summary>FishAudio provider.</summary>
public sealed class FishAudioProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "fish-audio";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.fish.audio";

    /// <summary>Key of the Fish Audio entry in provider options.</summary>
    public const string ProviderOptionsKey = "fishAudio";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public FishAudioProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Schema of a Fish Audio error body.</summary>
    public static JsonObject ErrorSchema { get; } = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["status"] = new JsonObject { ["type"] = new JsonArray("number", "null") },
            ["message"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
        },
    };

    /// <summary>Specification version reported by the Fish Audio provider.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Fish Audio does not provide language models.");
    }

    /// <summary>Creates a speech model.</summary>
    public ISpeechModel Speech(string modelId)
    {
        return SpeechModel(modelId);
    }

    /// <summary>Creates the default transcription model <c>transcribe-1</c>.</summary>
    public ITranscriptionModel Transcription()
    {
        return TranscriptionModel("transcribe-1");
    }

    /// <summary>Creates a provider.</summary>
    public static new FishAudioProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new FishAudioProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "FISH_AUDIO_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent(ProviderId);
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new FishSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new FishTranscriptionModel(this, string.IsNullOrEmpty(modelId) ? "transcribe-1" : modelId);

    private Task<ProviderExchangeResult> SendAsync(string path, HttpContent content, IEnumerable<KeyValuePair<string, string>>? modelHeaders, IReadOnlyDictionary<string, string> callHeaders, CancellationToken cancellationToken)
    {
        var headers = ProviderExchange.Merge(ProviderExchange.Merge(CreateHeaders(), modelHeaders), callHeaders);
        return ProviderExchange.SendAsync(_httpClient, HttpMethod.Post, ApiKeys.Combine(Options.BaseUrl, path), content, headers, cancellationToken);
    }

    private static JsonElement? FishOptions(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind == JsonValueKind.Object
            && providerOptions.TryGetProperty(ProviderOptionsKey, out var fish)
            && fish.ValueKind == JsonValueKind.Object)
        {
            return fish;
        }

        return null;
    }

    private static JsonElement? Option(JsonElement? options, string name)
    {
        return options is { } value && value.TryGetProperty(name, out var property) && property.ValueKind != JsonValueKind.Null
            ? property
            : null;
    }

    /// <summary>Fish Audio speech model. Posts to <c>/v1/tts</c> and selects the model with a <c>model</c> header.</summary>
    public sealed class FishSpeechModel : ISpeechModel, ISpeechCaller
    {
        private static readonly string[] Formats = { "wav", "pcm", "mp3", "opus" };

        // Provider option names copied to the request body as-is.
        private static readonly KeyValuePair<string, string>[] PassThrough =
        {
            new KeyValuePair<string, string>("sampleRate", "sample_rate"),
            new KeyValuePair<string, string>("latency", "latency"),
            new KeyValuePair<string, string>("temperature", "temperature"),
            new KeyValuePair<string, string>("topP", "top_p"),
            new KeyValuePair<string, string>("chunkLength", "chunk_length"),
            new KeyValuePair<string, string>("minChunkLength", "min_chunk_length"),
            new KeyValuePair<string, string>("normalize", "normalize"),
            new KeyValuePair<string, string>("maxNewTokens", "max_new_tokens"),
            new KeyValuePair<string, string>("repetitionPenalty", "repetition_penalty"),
            new KeyValuePair<string, string>("conditionOnPreviousChunks", "condition_on_previous_chunks"),
            new KeyValuePair<string, string>("earlyStopThreshold", "early_stop_threshold"),
            new KeyValuePair<string, string>("features", "features"),
        };

        private readonly FishAudioProvider _provider;
        private readonly Func<DateTime>? _clock;

        /// <summary>Creates a speech model. <paramref name="clock"/> sets the response timestamp.</summary>
        public FishSpeechModel(FishAudioProvider provider, string modelId, Func<DateTime>? clock = null)
        {
            _provider = provider;
            ModelId = modelId;
            _clock = clock;
        }

        /// <summary>Specification version.</summary>
        public string SpecificationVersion => "v4";

        /// <inheritdoc cref="ISpeechModel.Provider" />
        public string Provider => "fish-audio.speech";

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
            var fish = FishOptions(call.ProviderOptions);
            var warnings = new List<OperationWarning>();
            var format = Format(call.OutputFormat, warnings);
            var body = new JsonObject { ["text"] = call.Text, ["format"] = format };
            if (Option(fish, "referenceId") is { } referenceId)
            {
                body["reference_id"] = JsonNode.Parse(referenceId.GetRawText());
            }
            else if (call.Voice != null)
            {
                body["reference_id"] = call.Voice;
            }

            var prosody = new JsonObject();
            if (call.Speed is { } speed)
            {
                if (speed >= 0.5 && speed <= 2)
                {
                    prosody["speed"] = speed;
                }
                else
                {
                    warnings.Add(OperationWarning.Unsupported("speed", "Fish Audio speed must be between 0.5 and 2. The speed option was ignored."));
                }
            }

            if (Option(fish, "volume") is { } volume)
            {
                prosody["volume"] = JsonNode.Parse(volume.GetRawText());
            }

            if (Option(fish, "normalizeLoudness") is { } normalizeLoudness)
            {
                // Fish Audio accepts normalize_loudness on s1 but ignores it.
                if (ModelId == "s1")
                {
                    warnings.Add(OperationWarning.Unsupported(
                        "providerOptions.fishAudio.normalizeLoudness",
                        "Fish Audio ignores normalizeLoudness on s1. It is supported by the S2 family (s2-pro, s2.1-pro)."));
                }
                else
                {
                    prosody["normalize_loudness"] = normalizeLoudness.GetBoolean();
                }
            }

            if (prosody.Count > 0)
            {
                body["prosody"] = prosody;
            }

            if (call.Language != null)
            {
                warnings.Add(OperationWarning.Unsupported(
                    "language",
                    "Fish Audio infers the language from the input text and the selected voice, and has no language parameter. The language option was ignored."));
            }

            if (call.Instructions != null)
            {
                warnings.Add(OperationWarning.Unsupported("instructions", "Fish Audio does not support instructions. The instructions option was ignored."));
            }

            CopyBitrate(fish, body, "mp3Bitrate", "mp3_bitrate", "mp3", format, warnings);
            CopyBitrate(fish, body, "opusBitrate", "opus_bitrate", "opus", format, warnings);
            foreach (var pair in PassThrough)
            {
                if (Option(fish, pair.Key) is { } value)
                {
                    body[pair.Value] = JsonNode.Parse(value.GetRawText());
                }
            }

            var modelHeader = new Dictionary<string, string> { ["model"] = ModelId };
            var response = await _provider.SendAsync("/v1/tts", ProviderExchange.Json(body.ToJsonString()), modelHeader, call.Headers, cancellationToken).ConfigureAwait(false);
            return new SpeechModelResult(response.Bytes, warnings, response: new ProviderResponse(response.Headers, timestamp: timestamp, modelId: ModelId));
        }

        private static string Format(string? outputFormat, List<OperationWarning> warnings)
        {
            if (outputFormat == null)
            {
                return "mp3";
            }

            var normalized = outputFormat.ToLowerInvariant();
            if (Array.IndexOf(Formats, normalized) >= 0)
            {
                return normalized;
            }

            warnings.Add(OperationWarning.Unsupported(
                "outputFormat",
                "Fish Audio does not support the output format \"" + outputFormat + "\". Falling back to mp3. Supported formats are " + string.Join(", ", Formats) + "."));
            return "mp3";
        }

        private static void CopyBitrate(JsonElement? fish, JsonObject body, string option, string field, string target, string format, List<OperationWarning> warnings)
        {
            if (Option(fish, option) is not { } bitrate)
            {
                return;
            }

            if (format == target)
            {
                body[field] = JsonNode.Parse(bitrate.GetRawText());
            }
            else
            {
                warnings.Add(OperationWarning.Unsupported(
                    "providerOptions.fishAudio." + option,
                    option + " only applies to " + target + " output. The option was ignored for " + format + " output."));
            }
        }
    }

    /// <summary>Fish Audio transcription model. Posts multipart audio to <c>/v1/asr</c>.</summary>
    public sealed class FishTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
    {
        private readonly FishAudioProvider _provider;
        private readonly Func<DateTime>? _clock;

        /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp.</summary>
        public FishTranscriptionModel(FishAudioProvider provider, string modelId, Func<DateTime>? clock = null)
        {
            _provider = provider;
            ModelId = modelId;
            _clock = clock;
        }

        /// <inheritdoc cref="ITranscriptionCaller.SpecificationVersion" />
        public string SpecificationVersion => "v4";

        /// <inheritdoc cref="ITranscriptionModel.Provider" />
        public string Provider => "fish-audio.transcription";

        /// <inheritdoc cref="ITranscriptionModel.ModelId" />
        public string ModelId { get; }

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
            var fish = FishOptions(call.ProviderOptions);
            var form = new MultipartFormDataContent();
            var audio = new ByteArrayContent(call.Audio);
            audio.Headers.ContentType = MediaTypeHeaderValue.Parse(call.MediaType);
            form.Add(audio, "audio", "audio." + ProviderValues.MediaTypeToExtension(call.MediaType));
            if (Option(fish, "language") is { } language)
            {
                form.Add(new StringContent(language.GetString() ?? string.Empty), "language");
            }

            // Fish Audio defaults ignore_timestamps to true, which returns no segments.
            var ignoreTimestamps = Option(fish, "ignoreTimestamps")?.GetBoolean() ?? false;
            form.Add(new StringContent(ignoreTimestamps ? "true" : "false"), "ignore_timestamps");

            var response = await _provider.SendAsync("/v1/asr", form, null, call.Headers, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            var segments = new List<TranscriptSegment>();
            if (Option(root, "segments") is { ValueKind: JsonValueKind.Array } items)
            {
                foreach (var segment in items.EnumerateArray())
                {
                    segments.Add(new TranscriptSegment(segment.GetProperty("text").GetString() ?? string.Empty, segment.GetProperty("start").GetDouble(), segment.GetProperty("end").GetDouble()));
                }
            }

            JsonElement? metadata = null;
            if (Option(root, "language") is { } displayLanguage)
            {
                metadata = JsonValues.ParseOrEmpty(new JsonObject
                {
                    [ProviderOptionsKey] = new JsonObject { ["language"] = displayLanguage.GetString() },
                }.ToJsonString());
            }

            return new TranscriptionModelResult(
                root.GetProperty("text").GetString(),
                segments,
                Option(root, "language_code")?.GetString(),
                Option(root, "duration")?.GetDouble(),
                Array.Empty<OperationWarning>(),
                metadata,
                new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp, modelId: ModelId));
        }

        /// <inheritdoc />
        public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
        {
            return Task.FromResult<TranscriptionStreamStart?>(null);
        }
    }
}

/// <summary>Registers FishAudio.</summary>
public static class FishAudioServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddFishAudio(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(FishAudioProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new FishAudioProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(FishAudioProvider.ProviderId), options);
        });
        return services;
    }
}
