// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.AssemblyAI;

/// <summary>AssemblyAI settings.</summary>
public class AssemblyAIOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Wait between transcript status polls. Defaults to three seconds.</summary>
    public Func<CancellationToken, Task>? PollDelay { get; set; }
}

/// <summary>
/// Parsed AssemblyAI error. AssemblyAI documents a flat <c>{ "error": "Invalid API key" }</c> body for 4xx and 5xx responses.
/// The nested <c>{ "error": { "message": "...", "code": 429 } }</c> body is still read.
/// </summary>
public sealed class AssemblyAIErrorData
{
    /// <summary>Creates parsed error data from the flat shape. <see cref="Code"/> is null.</summary>
    public AssemblyAIErrorData(string message)
    {
        Message = message ?? string.Empty;
    }

    /// <summary>Creates parsed error data from the nested shape.</summary>
    public AssemblyAIErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message. Nested JSON is left as text.</summary>
    public string Message { get; }

    /// <summary>Provider error code. Null for the flat shape, which has none.</summary>
    public int? Code { get; }
}

/// <summary>Result of parsing an AssemblyAI error body.</summary>
public sealed class AssemblyAIErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public AssemblyAIErrorParseResult(bool success, AssemblyAIErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when the body had a flat <c>error</c> string, or a nested <c>error.message</c> and <c>error.code</c>.</summary>
    public bool Success { get; }

    /// <summary>Parsed error. Keys other than <c>error</c>, such as <c>status</c>, are dropped.</summary>
    public AssemblyAIErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public AssemblyAIErrorData? RawValue { get; }
}

/// <summary>Parses AssemblyAI error JSON.</summary>
public static class AssemblyAIError
{
    /// <summary>Parses an error body in the flat or the nested shape.</summary>
    public static AssemblyAIErrorParseResult Parse(string? json)
    {
        if (json == null)
        {
            return new AssemblyAIErrorParseResult(false, null);
        }

        try
        {
            using (var document = JsonDocument.Parse(json))
            {
                if (
                    document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("error", out var error)
                    && error.ValueKind == JsonValueKind.String
                )
                {
                    return new AssemblyAIErrorParseResult(
                        true,
                        new AssemblyAIErrorData(error.GetString() ?? string.Empty)
                    );
                }
            }
        }
        catch (JsonException)
        {
            return new AssemblyAIErrorParseResult(false, null);
        }

        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new AssemblyAIErrorParseResult(true, new AssemblyAIErrorData(message, code));
        }

        return new AssemblyAIErrorParseResult(false, null);
    }

    /// <summary>Maps parsed error data to the most informative message: the flat <c>error</c> string, or the nested <c>error.message</c>.</summary>
    public static string ToMessage(AssemblyAIErrorData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        return data.Message;
    }
}

/// <summary>Creates .NET API exceptions from AssemblyAI failed JSON responses.</summary>
public static class AssemblyAIFailedResponseHandler
{
    /// <summary>
    /// Parses an AssemblyAI error response and preserves the parsed body on <see cref="ApiException.ErrorData"/>.
    /// A body that is not recognized falls back to <paramref name="statusText"/>, then to the body.
    /// </summary>
    public static ApiException Handle(
        string responseBody,
        int statusCode,
        string url,
        object? requestBodyValues,
        IReadOnlyDictionary<string, string>? responseHeaders = null,
        string? statusText = null
    )
    {
        var data = AssemblyAIError.Parse(responseBody).Value;
        var message =
            data != null ? AssemblyAIError.ToMessage(data)
            : !string.IsNullOrEmpty(statusText) ? statusText!
            : JsonValues.ExtractErrorMessage(responseBody, statusCode);

        return statusCode switch
        {
            400 => new BadRequestException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            401 => new AuthenticationException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            403 => new PermissionDeniedException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            404 => new NotFoundException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            422 => new UnprocessableEntityException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            429 => new RateLimitException(message, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            >= 500 and <= 599 => new InternalServerException(message, statusCode, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
            _ => new ApiException(message, statusCode, responseBody)
            {
                Url = url,
                RequestBodyValues = requestBodyValues,
                ResponseHeaders = responseHeaders,
                ErrorData = data,
            },
        };
    }
}

/// <summary>Provider options and headers for one AssemblyAI transcription.</summary>
public sealed class AssemblyAITranscriptionRequest
{
    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>Provider options. The <c>assemblyai</c> object is read, with camelCase keys.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
}

/// <summary>One AssemblyAI text-to-speech call.</summary>
public sealed class AssemblyAISpeechRequest
{
    /// <summary>Creates a speech request.</summary>
    public AssemblyAISpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id, sent as <c>voice_id</c>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format: <c>mp3</c> (default), <c>wav</c>, <c>pcm</c>, <c>mulaw</c>, or <c>alaw</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Ignored. AssemblyAI does not accept instructions; a warning is returned.</summary>
    public string? Instructions { get; set; }

    /// <summary>Speaking rate. Only the default speed is supported; another value returns a warning.</summary>
    public double? Speed { get; set; }

    /// <summary>ISO 639-1 code (<c>de</c>, <c>en</c>, <c>es</c>, <c>fr</c>, <c>it</c>, <c>pt</c>) or the full language name.</summary>
    public string? Language { get; set; }

    /// <summary>Provider options. The <c>assemblyai</c> object is read: <c>encoding</c>, <c>container</c>, <c>sampleRate</c>, <c>bitRate</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Metadata returned with synthesized AssemblyAI speech.</summary>
public sealed class AssemblyAISpeechResponse
{
    /// <summary>Creates response metadata.</summary>
    public AssemblyAISpeechResponse(
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string> headers
    )
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Synthesized AssemblyAI audio.</summary>
public sealed class AssemblyAISpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public AssemblyAISpeechResult(
        byte[] audio,
        string mediaType,
        IReadOnlyList<ModelWarning> warnings,
        string requestBody,
        AudioUsage? usage,
        JsonElement? providerMetadata,
        AssemblyAISpeechResponse response
    )
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        RequestBody = requestBody ?? string.Empty;
        Usage = usage;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Response media type.</summary>
    public string MediaType { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>JSON request body.</summary>
    public string RequestBody { get; }

    /// <summary>Billed characters, when the response reports them.</summary>
    public AudioUsage? Usage { get; }

    /// <summary>Response details under <c>assemblyai</c>: request id, model, voice, language, encoding, sample rate, duration, billed characters.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public AssemblyAISpeechResponse Response { get; }
}

/// <summary>One transcribed word.</summary>
public sealed class AssemblyAITranscriptionSegment
{
    /// <summary>Creates a segment.</summary>
    public AssemblyAITranscriptionSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Word text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>Metadata returned with a transcription.</summary>
public sealed class AssemblyAITranscriptionResponse
{
    /// <summary>Creates response metadata.</summary>
    public AssemblyAITranscriptionResponse(
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string> headers,
        string body
    )
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body ?? string.Empty;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the final transcript poll.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw transcript JSON from the final poll.</summary>
    public string Body { get; }
}

/// <summary>AssemblyAI transcription, including word segments and audio-intelligence metadata.</summary>
public sealed class AssemblyAITranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public AssemblyAITranscriptionResult(
        string text,
        IReadOnlyList<AssemblyAITranscriptionSegment> segments,
        string? language,
        double? durationInSeconds,
        IReadOnlyList<ModelWarning> warnings,
        JsonElement? providerMetadata,
        AssemblyAITranscriptionResponse response
    )
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<AssemblyAITranscriptionSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Words with timings in seconds.</summary>
    public IReadOnlyList<AssemblyAITranscriptionSegment> Segments { get; }

    /// <summary>Language code reported on the transcript.</summary>
    public string? Language { get; }

    /// <summary>Audio duration, or the last word end when the duration is missing.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings produced while building the request.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>
    /// Diarization and audio-intelligence results under <c>assemblyai</c>, when the transcript has any.
    /// Timings inside these objects stay in milliseconds, as AssemblyAI returns them.
    /// </summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public AssemblyAITranscriptionResponse Response { get; }
}

/// <summary>AssemblyAI provider.</summary>
public sealed class AssemblyAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "assemblyai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.assemblyai.com";

    /// <summary>Default text-to-speech API origin.</summary>
    public const string DefaultSpeechBaseUrl = "https://tts.us.assemblyai.com";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public AssemblyAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("AssemblyAI does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new AssemblyAIProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new AssemblyAIProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? DefaultBaseUrl
                : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ASSEMBLYAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.RawAuthorization;
        options.UserAgent = ProviderExchange.UserAgent("assemblyai");
        return options;
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) =>
        Transcription(modelId);

    private static JsonElement? AssemblyAIObject(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        return
            providerOptions != null
            && providerOptions.TryGetValue(ProviderId, out var value)
            && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => Speech(modelId);

    /// <summary>Creates the typed speech model.</summary>
    public AssemblyAISpeechModel Speech(string modelId) => new AssemblyAISpeechModel(this, modelId);

    /// <summary>Creates the typed transcription model.</summary>
    public AssemblyAITranscriptionModel Transcription(string modelId) =>
        new AssemblyAITranscriptionModel(this, modelId);

    /// <summary>AssemblyAI text-to-speech model. Requests are buffered, not streamed.</summary>
    public sealed class AssemblyAISpeechModel : ISpeechModel
    {
        private static readonly Dictionary<
            string,
            (string Encoding, string? Container, int? SampleRate)
        > OutputFormats = new(StringComparer.Ordinal)
        {
            ["alaw"] = ("pcm_alaw", "raw", 8000),
            ["mp3"] = ("mp3", null, null),
            ["mulaw"] = ("pcm_mulaw", "raw", 8000),
            ["pcm"] = ("pcm_s16le", "raw", null),
            ["wav"] = ("pcm_s16le", "wav", null),
        };

        // The API names languages in full, so ISO 639-1 codes are translated.
        private static readonly Dictionary<string, string> Languages = new(StringComparer.Ordinal)
        {
            ["de"] = "german",
            ["en"] = "english",
            ["es"] = "spanish",
            ["fr"] = "french",
            ["it"] = "italian",
            ["pt"] = "portuguese",
        };

        private static readonly string[] Encodings =
        {
            "pcm_s16le",
            "pcm_mulaw",
            "pcm_alaw",
            "mp3",
        };
        private static readonly string[] Containers = { "raw", "wav" };
        private static readonly int[] SampleRates = { 8000, 16000, 22050, 24000, 44100, 48000 };
        private static readonly int[] BitRates = { 32000, 48000, 64000, 96000, 128000 };

        private readonly AssemblyAIProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public AssemblyAISpeechModel(AssemblyAIProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "assemblyai.speech";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<SpeechResult> DoGenerateAsync(
            SpeechCallOptions options,
            CancellationToken cancellationToken
        )
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
            var result = await GenerateAsync(
                    new AssemblyAISpeechRequest(options.Text)
                    {
                        Voice = options.Voice,
                        OutputFormat = options.OutputFormat,
                        Instructions = options.Instructions,
                        Speed = options.Speed,
                        Language = options.Language,
                        ProviderOptions = options.ProviderOptions,
                        Headers = options.Headers is null
                            ? null
                            : options
                                .Headers.Where(pair => pair.Value != null)
                                .ToDictionary(pair => pair.Key, pair => pair.Value!),
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType, result.Usage);
        }

        /// <summary>Synthesizes speech and returns the audio with warnings, usage, and response metadata.</summary>
        public async Task<AssemblyAISpeechResult> GenerateAsync(
            AssemblyAISpeechRequest request,
            CancellationToken cancellationToken
        )
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            var options = _provider.Options as AssemblyAIOptions;
            var timestamp = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var (body, warnings) = Args(request);
            var json = body.ToJsonString();
            var uri = ApiKeys.Combine(DefaultSpeechBaseUrl, "/v1/speech");
            ProviderExchangeResult response;
            try
            {
                response = await ProviderExchange
                    .SendAsync(
                        _provider._httpClient,
                        HttpMethod.Post,
                        uri,
                        ProviderExchange.Json(json),
                        ProviderExchange.Merge(
                            await _provider
                                .CreateHeadersAsync(cancellationToken)
                                .ConfigureAwait(false),
                            request.Headers
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            catch (ApiException exception)
                when (AssemblyAIError.Parse(exception.ResponseBody).Success)
            {
                throw AssemblyAIFailedResponseHandler.Handle(
                    exception.ResponseBody!,
                    exception.StatusCode,
                    uri.ToString(),
                    body,
                    exception.ResponseHeaders
                );
            }

            var mediaType = "audio/mpeg";
            if (
                response.Headers.TryGetValue("Content-Type", out var contentType)
                && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
            )
            {
                var separator = contentType.IndexOf(';');
                mediaType = (
                    separator >= 0 ? contentType.Substring(0, separator) : contentType
                ).Trim();
            }

            var billedCharacters = HeaderNumber(response.Headers, "x-tts-billed-characters");
            var audioDurationMs = HeaderNumber(response.Headers, "x-tts-audio-duration-ms");
            var sampleRate = HeaderNumber(response.Headers, "x-tts-sample-rate");
            var metadata = new JsonObject();
            AddHeader(metadata, "requestId", response.Headers, "x-request-id");
            AddHeader(metadata, "model", response.Headers, "x-tts-model");
            AddHeader(metadata, "voiceId", response.Headers, "x-tts-voice-id");
            AddHeader(metadata, "language", response.Headers, "x-tts-language");
            AddHeader(metadata, "encoding", response.Headers, "x-tts-encoding");
            if (sampleRate != null)
            {
                metadata["sampleRate"] = sampleRate;
            }

            if (audioDurationMs != null)
            {
                metadata["audioDurationMs"] = audioDurationMs;
            }

            if (billedCharacters != null)
            {
                metadata["billedCharacters"] = billedCharacters;
            }

            JsonElement providerMetadata;
            using (
                var document = JsonDocument.Parse(
                    new JsonObject { [ProviderId] = metadata }.ToJsonString()
                )
            )
            {
                providerMetadata = document.RootElement.Clone();
            }

            return new AssemblyAISpeechResult(
                response.Bytes,
                mediaType,
                warnings,
                json,
                billedCharacters is { } characters
                    ? new AudioUsage(characters: (int)Math.Min(characters, int.MaxValue))
                    : null,
                providerMetadata,
                new AssemblyAISpeechResponse(timestamp, ModelId, response.Headers)
            );
        }

        private static double? HeaderNumber(
            IReadOnlyDictionary<string, string> headers,
            string name
        )
        {
            foreach (var header in headers)
            {
                if (string.Equals(header.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    return double.TryParse(
                        header.Value,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var parsed
                    )
                        ? parsed
                        : null;
                }
            }

            return null;
        }

        private static void AddHeader(
            JsonObject target,
            string name,
            IReadOnlyDictionary<string, string> headers,
            string header
        )
        {
            foreach (var pair in headers)
            {
                if (
                    string.Equals(pair.Key, header, StringComparison.OrdinalIgnoreCase)
                    && pair.Value.Length > 0
                )
                {
                    target[name] = pair.Value;
                    return;
                }
            }
        }

        private static string? OptionString(JsonElement? options, string name, string[] allowed)
        {
            if (
                options is { } source
                && source.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null
            )
            {
                var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
                if (text == null || Array.IndexOf(allowed, text) < 0)
                {
                    throw new InvalidArgumentException(
                        "providerOptions.assemblyai." + name,
                        value.ToString(),
                        "Expected one of: " + string.Join(", ", allowed) + "."
                    );
                }

                return text;
            }

            return null;
        }

        private static int? OptionNumber(JsonElement? options, string name, int[] allowed)
        {
            if (
                options is { } source
                && source.TryGetProperty(name, out var value)
                && value.ValueKind != JsonValueKind.Null
            )
            {
                if (
                    value.ValueKind != JsonValueKind.Number
                    || !value.TryGetInt32(out var number)
                    || Array.IndexOf(allowed, number) < 0
                )
                {
                    throw new InvalidArgumentException(
                        "providerOptions.assemblyai." + name,
                        value.ToString(),
                        "Expected one of: " + string.Join(", ", allowed) + "."
                    );
                }

                return number;
            }

            return null;
        }

        private (JsonObject Body, List<ModelWarning> Warnings) Args(AssemblyAISpeechRequest request)
        {
            var warnings = new List<ModelWarning>();
            var assemblyai = AssemblyAIObject(request.ProviderOptions);
            var outputFormat = request.OutputFormat ?? "mp3";
            if (!OutputFormats.TryGetValue(outputFormat.ToLowerInvariant(), out var format))
            {
                warnings.Add(
                    new UnsupportedWarning(
                        "outputFormat",
                        "Unknown output format \""
                            + outputFormat
                            + "\". Falling back to mp3. Use providerOptions.assemblyai to configure encoding, container, and sampleRate directly."
                    )
                );
                format = OutputFormats["mp3"];
            }

            var encoding = OptionString(assemblyai, "encoding", Encodings) ?? format.Encoding;
            // Container and sample rate implied by outputFormat only apply to the encoding they were derived for.
            var formatApplies = encoding == format.Encoding;
            var body = new JsonObject
            {
                ["text"] = request.Text,
                ["model_id"] = ModelId,
                ["encoding"] = encoding,
                // Buffered responses report the final billed character count and are never cut mid-body.
                ["stream"] = false,
            };
            if (!string.IsNullOrEmpty(request.Voice))
            {
                body["voice_id"] = request.Voice;
            }

            var container = OptionString(assemblyai, "container", Containers)
                ?? (formatApplies ? format.Container : null);
            if (container != null)
            {
                if (encoding == "mp3")
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            "providerOptions.assemblyai.container",
                            "AssemblyAI MP3 output has no container. The container option was ignored."
                        )
                    );
                }
                else
                {
                    body["container"] = container;
                }
            }

            var sampleRate = OptionNumber(assemblyai, "sampleRate", SampleRates)
                ?? (formatApplies ? format.SampleRate : null);
            if (sampleRate != null)
            {
                body["sample_rate"] = sampleRate;
            }

            if (OptionNumber(assemblyai, "bitRate", BitRates) is { } bitRate)
            {
                if (encoding == "mp3")
                {
                    body["bit_rate"] = bitRate;
                }
                else
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            "providerOptions.assemblyai.bitRate",
                            "AssemblyAI only accepts a bit rate for MP3 output. The bitRate option was ignored."
                        )
                    );
                }
            }

            if (!string.IsNullOrEmpty(request.Language))
            {
                var normalized = request.Language!.ToLowerInvariant();
                if (Languages.TryGetValue(normalized, out var mapped))
                {
                    body["language"] = mapped;
                }
                else if (Languages.ContainsValue(normalized))
                {
                    body["language"] = normalized;
                }
                else
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            "language",
                            normalized == "auto"
                                ? "AssemblyAI text-to-speech does not detect the language. The voice speaks its own language."
                                : "AssemblyAI text-to-speech does not support language \""
                                    + request.Language
                                    + "\" (supported: "
                                    + string.Join(", ", Languages.Keys)
                                    + "). The language option was ignored."
                        )
                    );
                }
            }

            if (request.Speed is { } speed && speed != 1)
            {
                warnings.Add(
                    new UnsupportedWarning(
                        "speed",
                        "AssemblyAI text-to-speech only supports the default speed. The speed option was ignored."
                    )
                );
            }

            if (!string.IsNullOrEmpty(request.Instructions))
            {
                warnings.Add(
                    new UnsupportedWarning(
                        "instructions",
                        "AssemblyAI text-to-speech does not support instructions. Instructions parameter was ignored."
                    )
                );
            }

            return (body, warnings);
        }
    }

    /// <summary>Uploads audio, submits a transcript, and polls until it completes.</summary>
    public sealed class AssemblyAITranscriptionModel : ITranscriptionModel
    {
        private const string SpeechModelDocs =
            "https://www.assemblyai.com/docs/pre-recorded-audio/select-the-speech-model";

        // Provider options copied to the request with snake_case names. Nested objects list their own keys.
        private static readonly string[] FlatOptions =
        {
            "audioEndAt",
            "audioStartFrom",
            "autoChapters",
            "autoHighlights",
            "boostParam",
            "contentSafety",
            "contentSafetyConfidence",
            "customSpelling",
            "disfluencies",
            "entityDetection",
            "filterProfanity",
            "formatText",
            "iabCategories",
            "languageCode",
            "languageConfidenceThreshold",
            "languageDetection",
            "multichannel",
            "punctuate",
            "redactPii",
            "redactPiiAudio",
            "redactPiiAudioQuality",
            "redactPiiPolicies",
            "redactPiiSub",
            "sentimentAnalysis",
            "speakerLabels",
            "speakersExpected",
            "speechThreshold",
            "summarization",
            "summaryModel",
            "summaryType",
            "webhookAuthHeaderName",
            "webhookAuthHeaderValue",
            "webhookUrl",
            "wordBoost",
            "keytermsPrompt",
            "prompt",
            "temperature",
            "removeAudioTags",
            "domain",
            "redactPiiReturnUnredacted",
            "redactStaticEntities",
        };

        private static readonly KeyValuePair<string, string[]>[] NestedOptions =
        {
            new("speakerOptions", new[] { "minSpeakersExpected", "maxSpeakersExpected" }),
            new(
                "languageDetectionOptions",
                new[]
                {
                    "expectedLanguages",
                    "fallbackLanguage",
                    "codeSwitching",
                    "codeSwitchingConfidenceThreshold",
                }
            ),
            new(
                "redactPiiAudioOptions",
                new[] { "returnRedactedNoSpeechAudio", "overrideAudioRedactionMethod" }
            ),
        };

        // Transcript fields that the segment shape cannot carry, surfaced as provider metadata.
        private static readonly KeyValuePair<string, string>[] MetadataFields =
        {
            new("utterances", "utterances"),
            new("sentiment_analysis_results", "sentimentAnalysisResults"),
            new("entities", "entities"),
            new("content_safety_labels", "contentSafetyLabels"),
            new("iab_categories_result", "iabCategoriesResult"),
            new("auto_highlights_result", "autoHighlightsResult"),
        };

        private readonly AssemblyAIProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public AssemblyAITranscriptionModel(AssemblyAIProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <inheritdoc />
        public string Provider => "assemblyai.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(
            AudioInput audio,
            CancellationToken cancellationToken
        )
        {
            var result = await TranscribeAsync(audio, null, cancellationToken)
                .ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Uploads audio, submits a transcript, and polls until it completes.</summary>
        public async Task<AssemblyAITranscriptionResult> TranscribeAsync(
            AudioInput audio,
            AssemblyAITranscriptionRequest? request,
            CancellationToken cancellationToken
        )
        {
            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio));
            }

            var options = _provider.Options as AssemblyAIOptions;
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = ProviderExchange.Merge(
                await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                request?.Headers
            );
            var uploadContent = new ByteArrayContent(audio.Data);
            uploadContent.Headers.ContentType = new MediaTypeHeaderValue(
                "application/octet-stream"
            );
            var uploaded = await SendAsync(
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/upload"),
                    uploadContent,
                    headers,
                    null,
                    cancellationToken
                )
                .ConfigureAwait(false);
            string uploadUrl;
            using (var uploadDocument = JsonDocument.Parse(uploaded.Body))
            {
                uploadUrl = RequiredString(uploadDocument.RootElement, "upload_url");
            }

            var warnings = new List<ModelWarning>();
            var body = Body(AssemblyAIObject(request?.ProviderOptions), warnings);
            body["audio_url"] = uploadUrl;
            var submitted = await SendAsync(
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/transcript"),
                    ProviderExchange.Json(body.ToJsonString()),
                    headers,
                    body,
                    cancellationToken
                )
                .ConfigureAwait(false);
            string transcriptId;
            using (var submitDocument = JsonDocument.Parse(submitted.Body))
            {
                transcriptId = RequiredString(submitDocument.RootElement, "id");
            }

            var completed = await WaitForCompletionAsync(
                    transcriptId,
                    headers,
                    options,
                    cancellationToken
                )
                .ConfigureAwait(false);
            using (var document = JsonDocument.Parse(completed.Body))
            {
                var root = document.RootElement;
                var segments = new List<AssemblyAITranscriptionSegment>();
                if (
                    root.TryGetProperty("words", out var words)
                    && words.ValueKind == JsonValueKind.Array
                )
                {
                    foreach (var word in words.EnumerateArray())
                    {
                        segments.Add(
                            new AssemblyAITranscriptionSegment(
                                ReadString(word, "text") ?? string.Empty,
                                word.GetProperty("start").GetDouble() / 1000,
                                word.GetProperty("end").GetDouble() / 1000
                            )
                        );
                    }
                }

                double? duration =
                    root.TryGetProperty("audio_duration", out var audioDuration)
                    && audioDuration.ValueKind == JsonValueKind.Number
                        ? audioDuration.GetDouble()
                    : segments.Count > 0 ? segments[segments.Count - 1].EndSecond
                    : (double?)null;
                return new AssemblyAITranscriptionResult(
                    ReadString(root, "text") ?? string.Empty,
                    segments,
                    ReadString(root, "language_code"),
                    duration,
                    warnings,
                    Metadata(root),
                    new AssemblyAITranscriptionResponse(
                        started,
                        ModelId,
                        completed.Headers,
                        completed.Body
                    )
                );
            }
        }

        private async Task<ProviderExchangeResult> SendAsync(
            HttpMethod method,
            Uri uri,
            HttpContent? content,
            IReadOnlyDictionary<string, string?> headers,
            object? requestBodyValues,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await ProviderExchange
                    .SendAsync(
                        _provider._httpClient,
                        method,
                        uri,
                        content,
                        headers,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            catch (ApiException exception)
                when (AssemblyAIError.Parse(exception.ResponseBody).Success)
            {
                throw AssemblyAIFailedResponseHandler.Handle(
                    exception.ResponseBody!,
                    exception.StatusCode,
                    uri.ToString(),
                    requestBodyValues,
                    exception.ResponseHeaders
                );
            }
        }

        private async Task<ProviderExchangeResult> WaitForCompletionAsync(
            string transcriptId,
            IReadOnlyDictionary<string, string?> headers,
            AssemblyAIOptions? options,
            CancellationToken cancellationToken
        )
        {
            var url = ApiKeys.Combine(
                _provider.Options.BaseUrl,
                "/v2/transcript/" + Uri.EscapeDataString(transcriptId)
            );
            while (true)
            {
                var response = await SendAsync(
                        HttpMethod.Get,
                        url,
                        null,
                        headers,
                        null,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
                using (var document = JsonDocument.Parse(response.Body))
                {
                    var status = ReadString(document.RootElement, "status");
                    switch (status)
                    {
                        case "completed":
                            return response;
                        case "error":
                            throw new AiSdkException(
                                "Transcription failed: "
                                    + (ReadString(document.RootElement, "error") ?? "Unknown error")
                            );
                        case "queued":
                        case "processing":
                            break;
                        default:
                            throw new AiSdkException(
                                "Unexpected AssemblyAI transcript status: "
                                    + (status ?? "missing")
                                    + "."
                            );
                    }
                }

                if (options?.PollDelay != null)
                {
                    await options.PollDelay(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private JsonObject Body(JsonElement? assemblyai, List<ModelWarning> warnings)
        {
            var body = new JsonObject();

            // Every model id, including ones AssemblyAI no longer accepts, goes in speech_models unchanged.
            body["speech_models"] = new JsonArray(ModelId);
            if (ModelId == "universal-3-5-pro")
            {
                warnings.Add(
                    new DeprecatedWarning(
                        "model 'universal-3-5-pro'",
                        "AssemblyAI has deprecated 'universal-3-5-pro'. Use 'assemblyai-stt-3-7' instead. See documentation: "
                            + SpeechModelDocs
                    )
                );
            }
            else if (ModelId == "universal-2")
            {
                warnings.Add(
                    new OtherWarning(
                        "'assemblyai-stt-3-7' is AssemblyAI's latest flagship model; 'universal-2' remains available for languages it does not support. See "
                            + SpeechModelDocs
                    )
                );
            }

            if (assemblyai == null)
            {
                return body;
            }

            var source = assemblyai.Value;
            foreach (var name in FlatOptions)
            {
                if (Present(source, name, out var value))
                {
                    body[SnakeCase(name)] = JsonNode.Parse(value.GetRawText());
                }
            }

            foreach (var nested in NestedOptions)
            {
                if (!Present(source, nested.Key, out var value))
                {
                    continue;
                }

                var target = new JsonObject();
                foreach (var name in nested.Value)
                {
                    if (Present(value, name, out var inner))
                    {
                        target[SnakeCase(name)] = JsonNode.Parse(inner.GetRawText());
                    }
                }

                body[SnakeCase(nested.Key)] = target;
            }

            var deprecatedBoost = new List<string>();
            if (Present(source, "wordBoost", out _))
            {
                deprecatedBoost.Add("wordBoost");
            }

            if (Present(source, "boostParam", out _))
            {
                deprecatedBoost.Add("boostParam");
            }

            if (deprecatedBoost.Count > 0)
            {
                warnings.Add(
                    new DeprecatedWarning(
                        string.Join(", ", deprecatedBoost),
                        "'wordBoost' and 'boostParam' are deprecated and are rejected by 'assemblyai-stt-3-7' and 'universal-3-5-pro'. Use 'keytermsPrompt' instead."
                    )
                );
            }

            // These options only work alongside a prerequisite option. Warn rather than change the request.
            if (
                (
                    Present(source, "redactPiiReturnUnredacted", out _)
                    || Present(source, "redactStaticEntities", out _)
                ) && !IsTrue(source, "redactPii")
            )
            {
                warnings.Add(
                    new OtherWarning(
                        "'redactPiiReturnUnredacted' and 'redactStaticEntities' require 'redactPii' to be enabled; AssemblyAI rejects the request otherwise."
                    )
                );
            }

            if (
                Present(source, "redactPiiAudioOptions", out _) && !IsTrue(source, "redactPiiAudio")
            )
            {
                warnings.Add(
                    new OtherWarning(
                        "'redactPiiAudioOptions' only applies when 'redactPiiAudio' is enabled; it is otherwise ignored."
                    )
                );
            }

            if (Present(source, "languageCode", out _) && IsTrue(source, "languageDetection"))
            {
                warnings.Add(
                    new OtherWarning(
                        "'languageDetection' cannot be combined with an explicit 'languageCode'; AssemblyAI rejects requests that set both."
                    )
                );
            }

            return body;
        }

        private static JsonElement? Metadata(JsonElement root)
        {
            var assemblyai = new JsonObject();
            foreach (var field in MetadataFields)
            {
                if (Present(root, field.Key, out var value))
                {
                    assemblyai[field.Value] = JsonNode.Parse(value.GetRawText());
                }
            }

            if (assemblyai.Count == 0)
            {
                return null;
            }

            using (
                var document = JsonDocument.Parse(
                    new JsonObject { ["assemblyai"] = assemblyai }.ToJsonString()
                )
            )
            {
                return document.RootElement.Clone();
            }
        }

        private static bool Present(JsonElement element, string name, out JsonElement value)
        {
            return element.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null;
        }

        private static bool IsTrue(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.True;
        }

        private static string SnakeCase(string name)
        {
            var builder = new StringBuilder(name.Length + 8);
            foreach (var character in name)
            {
                if (char.IsUpper(character))
                {
                    builder.Append('_').Append(char.ToLowerInvariant(character));
                }
                else
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static string RequiredString(JsonElement element, string name)
        {
            return ReadString(element, name)
                ?? throw new AiSdkException("AssemblyAI response is missing '" + name + "'.");
        }

        private static string? ReadString(JsonElement element, string name)
        {
            if (
                element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.String
            )
            {
                return value.GetString();
            }

            return null;
        }
    }
}

/// <summary>Registers AssemblyAI.</summary>
public static class AssemblyAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddAssemblyAI(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(AssemblyAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new AssemblyAIProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(AssemblyAIProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
