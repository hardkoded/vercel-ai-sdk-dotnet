// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using TranscriptSegment = Vercel.AI.Operations.TranscriptSegment;

namespace Vercel.AI.Deepgram;

/// <summary>Deepgram settings.</summary>
public class DeepgramOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }
}

/// <summary>Parsed Deepgram error body.</summary>
public sealed class DeepgramErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public DeepgramErrorData(string errCode, string errMsg, string? requestId)
    {
        ErrCode = errCode ?? string.Empty;
        ErrMsg = errMsg ?? string.Empty;
        RequestId = requestId;
    }

    /// <summary>Error code, such as <c>INVALID_AUTH</c>.</summary>
    public string ErrCode { get; }

    /// <summary>Error message.</summary>
    public string ErrMsg { get; }

    /// <summary>Request id, when Deepgram sends one.</summary>
    public string? RequestId { get; }
}

/// <summary>Result of parsing a Deepgram error body.</summary>
public sealed class DeepgramErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public DeepgramErrorParseResult(bool success, DeepgramErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when <c>err_code</c> and <c>err_msg</c> were present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public DeepgramErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public DeepgramErrorData? RawValue { get; }
}

/// <summary>Parses Deepgram error JSON.</summary>
public static class DeepgramError
{
    /// <summary>Parses an <c>err_code</c>/<c>err_msg</c> body.</summary>
    public static DeepgramErrorParseResult Parse(string? json)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                using var document = JsonDocument.Parse(json!);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("err_code", out var code) && code.ValueKind == JsonValueKind.String
                    && root.TryGetProperty("err_msg", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    var requestId = root.TryGetProperty("request_id", out var id) && id.ValueKind == JsonValueKind.String ? id.GetString() : null;
                    return new DeepgramErrorParseResult(true, new DeepgramErrorData(code.GetString()!, message.GetString()!, requestId));
                }
            }
            catch (JsonException)
            {
            }
        }

        return new DeepgramErrorParseResult(false, null);
    }
}

/// <summary>Response metadata for one Deepgram call.</summary>
public sealed class DeepgramResponse
{
    /// <summary>Creates response metadata.</summary>
    public DeepgramResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, JsonElement? body)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>JSON response body. Speech calls return audio, so this is null for them.</summary>
    public JsonElement? Body { get; }
}

/// <summary>One Deepgram text-to-speech call.</summary>
public sealed class DeepgramSpeechRequest
{
    /// <summary>Creates a speech request.</summary>
    public DeepgramSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice. Required for the <c>aura</c> and <c>aura-2</c> family ids, where it becomes part of the model id.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format, such as <c>mp3</c>, <c>wav</c>, or <c>linear16_16000</c>.</summary>
    public string OutputFormat { get; set; } = "mp3";

    /// <summary>Speaking rate, sent as the <c>speed</c> query parameter.</summary>
    public double? Speed { get; set; }

    /// <summary>Language. Used to compose family model ids. <c>auto</c> falls back to <c>en</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Ignored. Deepgram does not accept instructions; a warning is returned.</summary>
    public string? Instructions { get; set; }

    /// <summary>Provider options. The <c>deepgram</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Synthesized Deepgram audio.</summary>
public sealed class DeepgramSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public DeepgramSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, DeepgramResponse response, JsonElement providerMetadata)
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        RequestBody = requestBody ?? string.Empty;
        Response = response;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Response media type.</summary>
    public string MediaType { get; }

    /// <summary>Warnings for ignored or adjusted settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>JSON request body.</summary>
    public string RequestBody { get; }

    /// <summary>Response metadata.</summary>
    public DeepgramResponse Response { get; }

    /// <summary><c>{ "deepgram": { ... } }</c> with model and usage details from the <c>dg-*</c> response headers.</summary>
    public JsonElement ProviderMetadata { get; }
}

/// <summary>Provider options and headers for one Deepgram transcription.</summary>
public sealed class DeepgramTranscriptionRequest
{
    /// <summary>Provider options. The <c>deepgram</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Deepgram transcription with word timings and response metadata.</summary>
public sealed class DeepgramTranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public DeepgramTranscriptionResult(string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<ModelWarning> warnings, DeepgramResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<TranscriptSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        Response = response;
    }

    /// <summary>Transcript of the first channel.</summary>
    public string Text { get; }

    /// <summary>One segment per word.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>Detected language, when language detection ran.</summary>
    public string? Language { get; }

    /// <summary>Audio duration from the response metadata.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public DeepgramResponse Response { get; }
}

/// <summary>Deepgram provider.</summary>
public sealed class DeepgramProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "deepgram";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.deepgram.com";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public DeepgramProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Deepgram does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new DeepgramProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new DeepgramProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "DEEPGRAM_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Token;
        options.UserAgent = ProviderExchange.UserAgent("deepgram");
        return options;
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new DeepgramTranscriptionModel(this, modelId);

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new DeepgramSpeechModel(this, modelId);

    /// <summary>Posts raw audio to <c>/v1/listen?model=</c>.</summary>
    public Task<DeepgramTranscriptionResult> TranscribeAsync(string modelId, AudioInput audio, DeepgramTranscriptionRequest? request, CancellationToken cancellationToken)
    {
        return new DeepgramTranscriptionModel(this, modelId).TranscribeAsync(audio, request, cancellationToken);
    }

    private DateTimeOffset Now()
    {
        var clock = (Options as DeepgramOptions)?.Clock;
        return clock != null ? clock() : DateTimeOffset.UtcNow;
    }

    private async Task<ProviderExchangeResult> SendAsync(string path, IReadOnlyList<KeyValuePair<string, string>> query, HttpContent content, IDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        var uri = new Uri(ApiKeys.Combine(Options.BaseUrl, path).AbsoluteUri + "?" + string.Join("&", query.Select(pair => pair.Key + "=" + Uri.EscapeDataString(pair.Value))));
        try
        {
            return await ProviderExchange.SendAsync(_httpClient, HttpMethod.Post, uri, content, ProviderExchange.Merge(CreateHeaders(), headers), cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception) when (DeepgramError.Parse(exception.ResponseBody).Value is { } error)
        {
            throw OpenAICompatibleChat.WithMessage(exception, error.ErrMsg);
        }
    }

    private static JsonElement? DeepgramObject(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty("deepgram", out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static JsonElement? Child(JsonElement? parent, string name)
    {
        return parent != null && parent.Value.ValueKind == JsonValueKind.Object && parent.Value.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
    }

    private static string QueryValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()!,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonValueKind.Array => string.Join(",", value.EnumerateArray().Select(QueryValue)),
            _ => value.GetRawText(),
        };
    }

    /// <summary>Deepgram Aura text-to-speech model.</summary>
    public sealed class DeepgramSpeechModel : ISpeechModel
    {
        private static readonly string[] Pcm = { "linear16", "mulaw", "alaw" };
        private static readonly string[] NoContainer = { "mp3", "flac", "aac" };
        private static readonly string[] FixedRate = { "mp3", "opus", "aac" };
        private static readonly string[] NoBitRate = { "linear16", "mulaw", "alaw", "flac" };

        private readonly DeepgramProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public DeepgramSpeechModel(DeepgramProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "deepgram.speech";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
            var result = await GenerateAsync(new DeepgramSpeechRequest(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType);
        }

        /// <summary>Synthesizes speech with <c>/v1/speak</c>.</summary>
        public async Task<DeepgramSpeechResult> GenerateAsync(DeepgramSpeechRequest request, CancellationToken cancellationToken)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var deepgram = DeepgramObject(request.ProviderOptions);

            // Deepgram embeds voice and language in the model id (aura-2-thalia-en). Family ids compose it from the call.
            var upstreamModelId = ModelId;
            if (ModelId == "aura" || ModelId == "aura-2")
            {
                var voice = request.Voice?.Trim();
                if (string.IsNullOrEmpty(voice))
                {
                    throw new AiSdkException("Deepgram speech model \"" + ModelId + "\" requires a `voice` to be set (e.g. voice: 'thalia').");
                }

                if (request.Language == "auto")
                {
                    warnings.Add(new CompatibilityWarning("language", "Deepgram TTS models do not support automatic language detection. Language \"en\" was used instead."));
                }

                upstreamModelId = ModelId + "-" + voice + "-" + (!string.IsNullOrEmpty(request.Language) && request.Language != "auto" ? request.Language : "en");
            }

            var query = new Dictionary<string, string> { ["model"] = upstreamModelId };
            ApplyOutputFormat(query, request.OutputFormat);
            if (deepgram != null)
            {
                ApplyProviderOptions(query, deepgram.Value, warnings);
            }

            if (upstreamModelId == ModelId && !string.IsNullOrEmpty(request.Voice) && request.Voice != ModelId)
            {
                warnings.Add(new UnsupportedWarning("voice", "Deepgram TTS models embed the voice in the model ID. The voice parameter \"" + request.Voice + "\" was ignored. Use the model ID to select a voice (e.g., \"aura-2-helena-en\")."));
            }

            if (request.Speed is { } speed)
            {
                query["speed"] = speed.ToString(CultureInfo.InvariantCulture);
            }

            if (upstreamModelId == ModelId && !string.IsNullOrEmpty(request.Language))
            {
                warnings.Add(new UnsupportedWarning("language", "Deepgram TTS models are language-specific via the model ID. Language parameter \"" + request.Language + "\" was ignored. Select a model with the appropriate language suffix (e.g., \"-en\" for English)."));
            }

            if (!string.IsNullOrEmpty(request.Instructions))
            {
                warnings.Add(new UnsupportedWarning("instructions", "Deepgram TTS REST API does not support instructions. Instructions parameter was ignored."));
            }

            var json = new JsonObject { ["text"] = request.Text }.ToJsonString();
            var response = await _provider.SendAsync("/v1/speak", query.ToList(), ProviderExchange.Json(json), request.Headers, cancellationToken).ConfigureAwait(false);
            var mediaType = "audio/mpeg";
            if (response.Headers.TryGetValue("Content-Type", out var contentType) && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var separator = contentType.IndexOf(';');
                mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            }

            return new DeepgramSpeechResult(response.Bytes, mediaType, warnings, json, new DeepgramResponse(timestamp, ModelId, response.Headers, null), Metadata(response.Headers));
        }

        private static void ApplyOutputFormat(Dictionary<string, string> query, string? outputFormat)
        {
            if (string.IsNullOrEmpty(outputFormat))
            {
                return;
            }

            var format = outputFormat!.ToLowerInvariant();
            (string? Encoding, string? Container)? mapped = format switch
            {
                "mp3" => ("mp3", null),
                "wav" or "linear16" => ("linear16", "wav"),
                "mulaw" => ("mulaw", "wav"),
                "alaw" => ("alaw", "wav"),
                "opus" or "ogg" => ("opus", "ogg"),
                "flac" => ("flac", null),
                "aac" => ("aac", null),
                "pcm" => ("linear16", "none"),
                _ => null,
            };
            if (mapped != null)
            {
                query["encoding"] = mapped.Value.Encoding!;
                if (mapped.Value.Container != null)
                {
                    query["container"] = mapped.Value.Container;
                }

                return;
            }

            // Formats such as linear16_24000 or wav_44100.
            var parts = format.Split('_');
            if (parts.Length < 2)
            {
                return;
            }

            var encoding = parts[0];
            var hasRate = int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var rate);
            if (Array.IndexOf(new[] { "linear16", "mulaw", "alaw", "mp3", "opus", "flac", "aac" }, encoding) >= 0)
            {
                query["encoding"] = encoding;
                if (Array.IndexOf(Pcm, encoding) >= 0)
                {
                    query["container"] = "wav";
                }
                else if (encoding == "opus")
                {
                    query["container"] = "ogg";
                }

                if (hasRate && SupportsRate(encoding, rate))
                {
                    query["sample_rate"] = rate.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (encoding == "wav" || encoding == "ogg")
            {
                query["container"] = encoding;
                query["encoding"] = encoding == "wav" ? "linear16" : "opus";
                if (hasRate)
                {
                    query["sample_rate"] = rate.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private static void ApplyProviderOptions(Dictionary<string, string> query, JsonElement deepgram, List<ModelWarning> warnings)
        {
            var container = Child(deepgram, "container")?.GetString();
            if (Child(deepgram, "encoding")?.GetString() is { Length: > 0 } requested)
            {
                var encoding = requested.ToLowerInvariant();
                query["encoding"] = encoding;
                if (!string.IsNullOrEmpty(container))
                {
                    if (Array.IndexOf(Pcm, encoding) >= 0)
                    {
                        if (container!.ToLowerInvariant() is "wav" or "none")
                        {
                            query["container"] = container.ToLowerInvariant();
                        }
                        else
                        {
                            warnings.Add(new UnsupportedWarning("providerOptions", "Encoding \"" + encoding + "\" only supports containers \"wav\" or \"none\". Container \"" + container + "\" was ignored."));
                        }
                    }
                    else if (encoding == "opus")
                    {
                        query["container"] = "ogg";
                    }
                    else if (Array.IndexOf(NoContainer, encoding) >= 0)
                    {
                        warnings.Add(new UnsupportedWarning("providerOptions", "Encoding \"" + encoding + "\" does not support container parameter. Container \"" + container + "\" was ignored."));
                        query.Remove("container");
                    }
                }
                else if (Array.IndexOf(NoContainer, encoding) >= 0)
                {
                    query.Remove("container");
                }
                else if (Array.IndexOf(Pcm, encoding) >= 0)
                {
                    if (!query.ContainsKey("container"))
                    {
                        query["container"] = "wav";
                    }
                }
                else if (encoding == "opus")
                {
                    query["container"] = "ogg";
                }

                RemoveIncompatible(query, encoding);
            }
            else if (!string.IsNullOrEmpty(container))
            {
                var lower = container!.ToLowerInvariant();
                query.TryGetValue("encoding", out var oldEncoding);
                string? newEncoding = null;
                if (lower is "wav" or "none")
                {
                    query["container"] = lower;
                    newEncoding = "linear16";
                }
                else if (lower == "ogg")
                {
                    query["container"] = "ogg";
                    newEncoding = "opus";
                }

                if (newEncoding != null && newEncoding != oldEncoding?.ToLowerInvariant())
                {
                    query["encoding"] = newEncoding;
                    RemoveIncompatible(query, newEncoding);
                }
            }

            if (Child(deepgram, "sampleRate") is { } sampleRateValue)
            {
                var sampleRate = sampleRateValue.GetDouble();
                var encoding = query.TryGetValue("encoding", out var current) ? current.ToLowerInvariant() : string.Empty;
                var text = sampleRate.ToString(CultureInfo.InvariantCulture);
                if (encoding == "linear16" || encoding == "mulaw" || encoding == "alaw" || encoding == "flac")
                {
                    if (SupportsRate(encoding, sampleRate))
                    {
                        query["sample_rate"] = text;
                    }
                    else
                    {
                        var rates = encoding == "linear16" ? "8000, 16000, 24000, 32000, 48000" : encoding == "flac" ? "8000, 16000, 22050, 32000, 48000" : "8000, 16000";
                        warnings.Add(new UnsupportedWarning("providerOptions", "Encoding \"" + encoding + "\" only supports sample rates: " + rates + ". Sample rate " + text + " was ignored."));
                    }
                }
                else if (Array.IndexOf(FixedRate, encoding) >= 0)
                {
                    warnings.Add(new UnsupportedWarning("providerOptions", "Encoding \"" + encoding + "\" has a fixed sample rate and does not support sample_rate parameter. Sample rate " + text + " was ignored."));
                }
                else
                {
                    query["sample_rate"] = text;
                }
            }

            if (Child(deepgram, "bitRate") is { } bitRateValue)
            {
                var text = QueryValue(bitRateValue);
                var encoding = query.TryGetValue("encoding", out var current) ? current.ToLowerInvariant() : string.Empty;
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var bitRate);
                if (encoding == "mp3")
                {
                    AddBitRate(query, warnings, text, bitRate == 32000 || bitRate == 48000, "Encoding \"mp3\" only supports bit rates: 32000, 48000. Bit rate " + text + " was ignored.");
                }
                else if (encoding == "opus")
                {
                    AddBitRate(query, warnings, text, bitRate >= 4000 && bitRate <= 650000, "Encoding \"opus\" supports bit rates between 4000 and 650000. Bit rate " + text + " was ignored.");
                }
                else if (encoding == "aac")
                {
                    AddBitRate(query, warnings, text, bitRate >= 4000 && bitRate <= 192000, "Encoding \"aac\" supports bit rates between 4000 and 192000. Bit rate " + text + " was ignored.");
                }
                else if (Array.IndexOf(NoBitRate, encoding) >= 0)
                {
                    warnings.Add(new UnsupportedWarning("providerOptions", "Encoding \"" + encoding + "\" does not support bit_rate parameter. Bit rate " + text + " was ignored."));
                }
                else
                {
                    query["bit_rate"] = text;
                }
            }

            if (Child(deepgram, "callback")?.GetString() is { Length: > 0 } callback)
            {
                query["callback"] = callback;
            }

            if (Child(deepgram, "callbackMethod")?.GetString() is { Length: > 0 } callbackMethod)
            {
                query["callback_method"] = callbackMethod;
            }

            if (Child(deepgram, "mipOptOut") is { } mipOptOut)
            {
                query["mip_opt_out"] = QueryValue(mipOptOut);
            }

            if (Child(deepgram, "tag") is { } tag && QueryValue(tag).Length > 0)
            {
                query["tag"] = QueryValue(tag);
            }
        }

        private static void AddBitRate(Dictionary<string, string> query, List<ModelWarning> warnings, string text, bool valid, string warning)
        {
            if (valid)
            {
                query["bit_rate"] = text;
            }
            else
            {
                warnings.Add(new UnsupportedWarning("providerOptions", warning));
            }
        }

        private static void RemoveIncompatible(Dictionary<string, string> query, string encoding)
        {
            if (Array.IndexOf(FixedRate, encoding) >= 0)
            {
                query.Remove("sample_rate");
            }

            if (Array.IndexOf(NoBitRate, encoding) >= 0)
            {
                query.Remove("bit_rate");
            }
        }

        private static bool SupportsRate(string encoding, double rate)
        {
            return encoding switch
            {
                "linear16" => rate is 8000 or 16000 or 24000 or 32000 or 48000,
                "mulaw" or "alaw" => rate is 8000 or 16000,
                "flac" => rate is 8000 or 16000 or 22050 or 32000 or 48000,
                _ => false,
            };
        }

        private static JsonElement Metadata(IReadOnlyDictionary<string, string> headers)
        {
            // dg-project-id is left out because it identifies the account.
            var deepgram = new JsonObject();
            AddText(deepgram, headers, "dg-model-name", "modelName");
            AddText(deepgram, headers, "dg-model-uuid", "modelUuid");
            if (headers.TryGetValue("dg-additional-model-uuids", out var additional))
            {
                deepgram["additionalModelUuids"] = new JsonArray(additional.Split(',').Select(uuid => (JsonNode?)JsonValue.Create(uuid)).ToArray());
            }

            AddNumber(deepgram, headers, "dg-char-count", "charCount");
            AddNumber(deepgram, headers, "dg-breaks-applied", "breaksApplied");
            AddNumber(deepgram, headers, "dg-pronunciations-applied", "pronunciationsApplied");
            AddText(deepgram, headers, "dg-pronunciation-warnings", "pronunciationWarnings");
            AddText(deepgram, headers, "dg-request-id", "requestId");
            using var document = JsonDocument.Parse(new JsonObject { ["deepgram"] = deepgram }.ToJsonString());
            return document.RootElement.Clone();
        }

        private static void AddText(JsonObject target, IReadOnlyDictionary<string, string> headers, string header, string name)
        {
            if (headers.TryGetValue(header, out var value) && value.Length > 0)
            {
                target[name] = value;
            }
        }

        private static void AddNumber(JsonObject target, IReadOnlyDictionary<string, string> headers, string header, string name)
        {
            if (headers.TryGetValue(header, out var value) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                target[name] = number;
            }
        }
    }

    /// <summary>Deepgram pre-recorded transcription model.</summary>
    public sealed class DeepgramTranscriptionModel : ITranscriptionModel
    {
        private static readonly (string Option, string Query)[] QueryOptions =
        {
            ("detectEntities", "detect_entities"),
            ("detectLanguage", "detect_language"),
            ("diarize", "diarize"),
            ("fillerWords", "filler_words"),
            ("intents", "intents"),
            ("keyterm", "keyterm"),
            ("language", "language"),
            ("paragraphs", "paragraphs"),
            ("punctuate", "punctuate"),
            ("redact", "redact"),
            ("replace", "replace"),
            ("search", "search"),
            ("sentiment", "sentiment"),
            ("smartFormat", "smart_format"),
            ("summarize", "summarize"),
            ("topics", "topics"),
            ("utterances", "utterances"),
            ("uttSplit", "utt_split"),
        };

        private readonly DeepgramProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public DeepgramTranscriptionModel(DeepgramProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "deepgram.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Posts raw audio to <c>/v1/listen</c>. Provider options become query parameters.</summary>
        public async Task<DeepgramTranscriptionResult> TranscribeAsync(AudioInput audio, DeepgramTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            audio = audio ?? throw new ArgumentNullException(nameof(audio));
            var timestamp = _provider.Now();
            var deepgram = DeepgramObject(request?.ProviderOptions);
            var query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("model", ModelId) };
            foreach (var (option, name) in QueryOptions)
            {
                if (Child(deepgram, option) is { } value)
                {
                    query.Add(new KeyValuePair<string, string>(name, QueryValue(value)));
                }
            }

            var content = new ByteArrayContent(audio.Data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrEmpty(audio.MediaType) ? "application/octet-stream" : audio.MediaType);
            var response = await _provider.SendAsync("/v1/listen", query, content, request?.Headers, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            var channel = Child(root, "results") is { } results && Child(results, "channels") is { ValueKind: JsonValueKind.Array } channels && channels.GetArrayLength() > 0
                ? channels[0]
                : (JsonElement?)null;
            var alternative = Child(channel, "alternatives") is { ValueKind: JsonValueKind.Array } alternatives && alternatives.GetArrayLength() > 0
                ? alternatives[0]
                : (JsonElement?)null;
            var segments = new List<TranscriptSegment>();
            if (Child(alternative, "words") is { ValueKind: JsonValueKind.Array } words)
            {
                foreach (var word in words.EnumerateArray())
                {
                    segments.Add(new TranscriptSegment(Child(word, "word")?.GetString() ?? string.Empty, Child(word, "start")?.GetDouble() ?? 0, Child(word, "end")?.GetDouble() ?? 0));
                }
            }

            return new DeepgramTranscriptionResult(
                Child(alternative, "transcript")?.GetString() ?? string.Empty,
                segments,
                Child(channel, "detected_language")?.GetString(),
                Child(Child(root, "metadata"), "duration")?.GetDouble(),
                Array.Empty<ModelWarning>(),
                new DeepgramResponse(timestamp, ModelId, response.Headers, root.Clone()));
        }
    }
}

/// <summary>Registers Deepgram.</summary>
public static class DeepgramServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddDeepgram(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(DeepgramProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new DeepgramProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(DeepgramProvider.ProviderId), options);
        });
        return services;
    }
}
