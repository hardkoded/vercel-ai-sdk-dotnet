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
using Vercel.AI.Util;
using TranscriptSegment = Vercel.AI.Operations.TranscriptSegment;

namespace Vercel.AI.ElevenLabs;

/// <summary>ElevenLabs settings.</summary>
public class ElevenLabsOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Custom WebSocket implementation for streaming speech. Defaults to <c>ClientWebSocket</c>. Maps to <c>webSocket</c>.</summary>
    public WebSocketFactory? WebSocket { get; set; }

    /// <summary>Milliseconds of send inactivity before a streaming speech keepalive. Test seam for <c>KEEPALIVE_INTERVAL_MS</c>.</summary>
    internal int KeepaliveIntervalMs { get; set; } = ElevenLabsSpeechStream.KeepaliveIntervalMs;
}

/// <summary>One string or number in a FastAPI validation issue location.</summary>
public sealed class ElevenLabsErrorLocation
{
    private ElevenLabsErrorLocation(string? text, double? number)
    {
        Text = text;
        Number = number;
    }

    /// <summary>String location component, when the provider sent a string.</summary>
    public string? Text { get; }

    /// <summary>Numeric location component, when the provider sent a number.</summary>
    public double? Number { get; }

    internal static ElevenLabsErrorLocation From(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String
            ? new ElevenLabsErrorLocation(value.GetString(), null)
            : new ElevenLabsErrorLocation(null, value.GetDouble());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Text ?? Number!.Value.ToString(CultureInfo.InvariantCulture);
    }
}

/// <summary>One FastAPI validation issue from an ElevenLabs <c>detail</c> array.</summary>
public sealed class ElevenLabsValidationIssue
{
    internal ElevenLabsValidationIssue(
        string message,
        IReadOnlyList<ElevenLabsErrorLocation>? location,
        string? type,
        IReadOnlyDictionary<string, JsonElement> additionalProperties
    )
    {
        Message = message;
        Location = location;
        Type = type;
        AdditionalProperties = additionalProperties;
    }

    /// <summary>Validation message from <c>msg</c>.</summary>
    public string Message { get; }

    /// <summary>Path components from <c>loc</c>.</summary>
    public IReadOnlyList<ElevenLabsErrorLocation>? Location { get; }

    /// <summary>Validation issue type.</summary>
    public string? Type { get; }

    /// <summary>Loose fields ElevenLabs included in the issue.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; }
}

/// <summary>ElevenLabs API error details.</summary>
public sealed class ElevenLabsErrorDetail
{
    internal ElevenLabsErrorDetail(
        string message,
        string? code,
        string? status,
        string? type,
        string? requestId,
        string? parameter,
        IReadOnlyDictionary<string, JsonElement> additionalProperties
    )
    {
        Message = message;
        Code = code;
        Status = status;
        Type = type;
        RequestId = requestId;
        Parameter = parameter;
        AdditionalProperties = additionalProperties;
    }

    /// <summary>Provider error message.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public string? Code { get; }

    /// <summary>Provider error status.</summary>
    public string? Status { get; }

    /// <summary>Provider error type.</summary>
    public string? Type { get; }

    /// <summary>ElevenLabs request id.</summary>
    public string? RequestId { get; }

    /// <summary>Invalid parameter name.</summary>
    public string? Parameter { get; }

    /// <summary>Loose fields ElevenLabs included in the detail object.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; }
}

/// <summary>Legacy ElevenLabs <c>error</c> object.</summary>
public sealed class ElevenLabsLegacyError
{
    internal ElevenLabsLegacyError(string message, double code)
    {
        Message = message;
        Code = code;
    }

    /// <summary>Provider error message.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public double Code { get; }
}

/// <summary>Parsed ElevenLabs error body.</summary>
public sealed class ElevenLabsErrorData
{
    /// <summary>Creates legacy parsed error data.</summary>
    public ElevenLabsErrorData(string message, int code)
        : this(null, null, null, new ElevenLabsLegacyError(message ?? string.Empty, code)) { }

    private ElevenLabsErrorData(
        ElevenLabsErrorDetail? detail,
        IReadOnlyList<ElevenLabsValidationIssue>? issues,
        string? detailMessage,
        ElevenLabsLegacyError? error
    )
    {
        Detail = detail;
        Issues = issues;
        DetailMessage = detailMessage;
        Error = error;
    }

    /// <summary>Object-valued <c>detail</c>, when present.</summary>
    public ElevenLabsErrorDetail? Detail { get; }

    /// <summary>Array-valued <c>detail</c>, when present.</summary>
    public IReadOnlyList<ElevenLabsValidationIssue>? Issues { get; }

    /// <summary>String-valued <c>detail</c>, when present.</summary>
    public string? DetailMessage { get; }

    /// <summary>Legacy <c>error</c> object, when present.</summary>
    public ElevenLabsLegacyError? Error { get; }

    /// <summary>Legacy provider message. Use <see cref="Error"/> for parsed legacy bodies.</summary>
    [Obsolete("Use Error.Message.")]
    public string Message => Error?.Message ?? string.Empty;

    /// <summary>Legacy integral provider code. Use <see cref="Error"/> for the upstream numeric code.</summary>
    [Obsolete("Use Error.Code.")]
    public int Code => checked((int)(Error?.Code ?? 0));

    internal static ElevenLabsErrorData FromDetail(ElevenLabsErrorDetail detail) =>
        new ElevenLabsErrorData(detail, null, null, null);

    internal static ElevenLabsErrorData FromIssues(
        IReadOnlyList<ElevenLabsValidationIssue> issues
    ) => new ElevenLabsErrorData(null, issues, null, null);

    internal static ElevenLabsErrorData FromMessage(string message) =>
        new ElevenLabsErrorData(null, null, message, null);

    internal static ElevenLabsErrorData FromLegacy(string message, double code) =>
        new ElevenLabsErrorData(null, null, null, new ElevenLabsLegacyError(message, code));
}

/// <summary>Result of parsing an ElevenLabs error body.</summary>
public sealed class ElevenLabsErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public ElevenLabsErrorParseResult(bool success, ElevenLabsErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when the body matched an ElevenLabs error shape.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public ElevenLabsErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public ElevenLabsErrorData? RawValue { get; }
}

/// <summary>Parses ElevenLabs error JSON.</summary>
public static class ElevenLabsError
{
    /// <summary>Parses an error body.</summary>
    public static ElevenLabsErrorParseResult Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ElevenLabsErrorParseResult(false, null);
        }

        try
        {
            using var document = JsonDocument.Parse(json!);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new ElevenLabsErrorParseResult(false, null);
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                return ParseDetail(detail);
            }

            if (
                root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && RequiredString(error, "message") is { } message
                && error.TryGetProperty("code", out var code)
                && code.ValueKind == JsonValueKind.Number
                && code.TryGetDouble(out var codeValue)
            )
            {
                return new ElevenLabsErrorParseResult(
                    true,
                    ElevenLabsErrorData.FromLegacy(message, codeValue)
                );
            }
        }
        catch (JsonException) { }

        return new ElevenLabsErrorParseResult(false, null);
    }

    internal static string ToMessage(ElevenLabsErrorData data)
    {
        if (data.Error is { } error)
        {
            return error.Message;
        }

        if (data.DetailMessage is { } detailMessage)
        {
            return detailMessage;
        }

        if (data.Issues is { } issues)
        {
            return string.Join(
                "; ",
                issues.Select(issue =>
                    issue.Location is { Count: > 0 } location
                        ? string.Join(".", location) + ": " + issue.Message
                        : issue.Message
                )
            );
        }

        var detail = data.Detail!;
        return detail.Code != null ? detail.Message + " (" + detail.Code + ")" : detail.Message;
    }

    private static ElevenLabsErrorParseResult ParseDetail(JsonElement detail)
    {
        if (detail.ValueKind == JsonValueKind.String)
        {
            return new ElevenLabsErrorParseResult(
                true,
                ElevenLabsErrorData.FromMessage(detail.GetString()!)
            );
        }

        if (detail.ValueKind == JsonValueKind.Object)
        {
            var message = RequiredString(detail, "message");
            if (
                message == null
                || !OptionalString(detail, "code", out var code)
                || !OptionalString(detail, "status", out var status)
                || !OptionalString(detail, "type", out var type)
                || !OptionalString(detail, "request_id", out var requestId)
                || !OptionalString(detail, "param", out var parameter)
            )
            {
                return new ElevenLabsErrorParseResult(false, null);
            }

            var value = new ElevenLabsErrorDetail(
                message,
                code,
                status,
                type,
                requestId,
                parameter,
                AdditionalProperties(
                    detail,
                    "message",
                    "code",
                    "status",
                    "type",
                    "request_id",
                    "param"
                )
            );
            return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromDetail(value));
        }

        if (detail.ValueKind != JsonValueKind.Array)
        {
            return new ElevenLabsErrorParseResult(false, null);
        }

        var issues = new List<ElevenLabsValidationIssue>();
        foreach (var issue in detail.EnumerateArray())
        {
            if (
                issue.ValueKind != JsonValueKind.Object
                || RequiredString(issue, "msg") is not { } message
                || !OptionalString(issue, "type", out var type)
                || !TryLocation(issue, out var location)
            )
            {
                return new ElevenLabsErrorParseResult(false, null);
            }

            issues.Add(
                new ElevenLabsValidationIssue(
                    message,
                    location,
                    type,
                    AdditionalProperties(issue, "msg", "loc", "type")
                )
            );
        }

        return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromIssues(issues));
    }

    private static bool TryLocation(
        JsonElement issue,
        out IReadOnlyList<ElevenLabsErrorLocation>? location
    )
    {
        location = null;
        if (!issue.TryGetProperty("loc", out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var result = new List<ElevenLabsErrorLocation>();
        foreach (var component in value.EnumerateArray())
        {
            if (
                component.ValueKind != JsonValueKind.String
                && component.ValueKind != JsonValueKind.Number
            )
            {
                return false;
            }

            result.Add(ElevenLabsErrorLocation.From(component));
        }

        location = result;
        return true;
    }

    private static string? RequiredString(JsonElement value, string name)
    {
        return
            value.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool OptionalString(JsonElement value, string name, out string? result)
    {
        result = null;
        if (
            !value.TryGetProperty(name, out var property)
            || property.ValueKind == JsonValueKind.Null
        )
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        result = property.GetString();
        return true;
    }

    private static IReadOnlyDictionary<string, JsonElement> AdditionalProperties(
        JsonElement value,
        params string[] knownProperties
    )
    {
        var known = new HashSet<string>(knownProperties, StringComparer.Ordinal);
        return value
            .EnumerateObject()
            .Where(property => !known.Contains(property.Name))
            .ToDictionary(
                property => property.Name,
                property => property.Value.Clone(),
                StringComparer.Ordinal
            );
    }
}

/// <summary>Creates .NET API exceptions from ElevenLabs failed JSON responses.</summary>
public static class ElevenLabsFailedResponseHandler
{
    /// <summary>Parses an ElevenLabs error response and preserves the parsed body on <see cref="ApiException.ErrorData"/>.</summary>
    public static ApiException Handle(
        string responseBody,
        int statusCode,
        string url,
        object requestBodyValues,
        IReadOnlyDictionary<string, string>? responseHeaders = null
    )
    {
        var data = ElevenLabsError.Parse(responseBody).Value;
        var message =
            data == null
                ? JsonValues.ExtractErrorMessage(responseBody, statusCode)
                : ElevenLabsError.ToMessage(data);

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

/// <summary>One ElevenLabs text-to-speech call.</summary>
public sealed class ElevenLabsSpeechRequest
{
    /// <summary>Default voice used when the caller does not set one.</summary>
    public const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM";

    /// <summary>Creates a speech request.</summary>
    public ElevenLabsSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id. Null selects <see cref="DefaultVoiceId"/>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format, such as <c>mp3</c> or <c>pcm_44100</c>. Sent as the <c>output_format</c> query parameter.</summary>
    public string OutputFormat { get; set; } = "mp3_44100_128";

    /// <summary>Ignored. ElevenLabs does not accept instructions; a warning is returned.</summary>
    public string? Instructions { get; set; }

    /// <summary>Language code, sent as <c>language_code</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Speaking rate, sent in <c>voice_settings</c>.</summary>
    public double? Speed { get; set; }

    /// <summary>Provider options. The <c>elevenlabs</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Response metadata for one ElevenLabs call.</summary>
public sealed class ElevenLabsResponse
{
    /// <summary>Creates response metadata.</summary>
    public ElevenLabsResponse(
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string> headers,
        JsonElement? body
    )
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

/// <summary>Synthesized ElevenLabs audio.</summary>
public sealed class ElevenLabsSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public ElevenLabsSpeechResult(
        byte[] audio,
        string mediaType,
        IReadOnlyList<ModelWarning> warnings,
        string requestBody,
        ElevenLabsResponse response,
        JsonElement? providerMetadata = null
    )
    {
        ProviderMetadata = providerMetadata;
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        RequestBody = requestBody ?? string.Empty;
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

    /// <summary>Response metadata.</summary>
    public ElevenLabsResponse Response { get; }

    /// <summary>
    /// Provider metadata, <c>{"elevenlabs":{...}}</c>. It holds <c>outputFormat</c>, and with
    /// <c>providerOptions.elevenlabs.withTimestamps</c> also <c>alignment</c> and <c>normalizedAlignment</c>
    /// (<c>characters</c>, <c>characterStartTimesSeconds</c>, <c>characterEndTimesSeconds</c>).
    /// </summary>
    public JsonElement? ProviderMetadata { get; }
}

/// <summary>Provider options and headers for one ElevenLabs transcription.</summary>
public sealed class ElevenLabsTranscriptionRequest
{
    /// <summary>Provider options. The <c>elevenlabs</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>ElevenLabs transcription with word timings and response metadata.</summary>
public sealed class ElevenLabsTranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public ElevenLabsTranscriptionResult(
        string text,
        IReadOnlyList<TranscriptSegment> segments,
        string? language,
        double? durationInSeconds,
        IReadOnlyList<ModelWarning> warnings,
        ElevenLabsResponse response
    )
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<TranscriptSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>One segment per word or spacing item.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>Detected language code.</summary>
    public string? Language { get; }

    /// <summary>End time of the last word, in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public ElevenLabsResponse Response { get; }
}

/// <summary>ElevenLabs provider.</summary>
public sealed class ElevenLabsProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "elevenlabs";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.elevenlabs.io";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public ElevenLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "languageModel");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "embeddingModel");
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw new NoSuchModelError(modelId, "imageModel");
    }

    /// <summary>Creates a provider.</summary>
    public static new ElevenLabsProvider Create(
        OpenAICompatibleOptions? options = null,
        HttpMessageHandler? handler = null
    )
    {
        var client = handler is null
            ? new HttpClient()
            : new HttpClient(handler, disposeHandler: false);
        return new ElevenLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl =
            string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1"
                ? DefaultBaseUrl
                : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ELEVENLABS_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "xi-api-key";
        options.UserAgent = ProviderExchange.UserAgent("elevenlabs");
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) =>
        new ElevenLabsSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) =>
        new ElevenLabsTranscriptionModel(this, modelId);

    private DateTimeOffset Now()
    {
        var clock = (Options as ElevenLabsOptions)?.Clock;
        return clock != null ? clock() : DateTimeOffset.UtcNow;
    }

    private static JsonElement? ElevenLabsObject(JsonElement? providerOptions)
    {
        return
            providerOptions is { ValueKind: JsonValueKind.Object } bag
            && bag.TryGetProperty("elevenlabs", out var value)
            && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static JsonElement? Child(JsonElement? parent, string name)
    {
        return
            parent != null
            && parent.Value.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value
            : null;
    }

    private async Task<ProviderExchangeResult> SendAsync(
        Uri uri,
        HttpContent content,
        object requestBodyValues,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await ProviderExchange
                .SendAsync(_httpClient, HttpMethod.Post, uri, content, headers, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ApiException exception) when (ElevenLabsError.Parse(exception.ResponseBody).Success)
        {
            throw ElevenLabsFailedResponseHandler.Handle(
                exception.ResponseBody!,
                exception.StatusCode,
                uri.ToString(),
                requestBodyValues,
                exception.ResponseHeaders
            );
        }
    }

    /// <summary>ElevenLabs text-to-speech model.</summary>
    public sealed class ElevenLabsSpeechModel : ISpeechModel, ISpeechCaller, IStreamingSpeechCaller
    {
        private static readonly Dictionary<string, string> OutputFormats = new Dictionary<
            string,
            string
        >(StringComparer.Ordinal)
        {
            ["mp3"] = "mp3_44100_128",
            ["mp3_32"] = "mp3_44100_32",
            ["mp3_64"] = "mp3_44100_64",
            ["mp3_96"] = "mp3_44100_96",
            ["mp3_128"] = "mp3_44100_128",
            ["mp3_192"] = "mp3_44100_192",
            ["pcm"] = "pcm_44100",
            ["pcm_16000"] = "pcm_16000",
            ["pcm_22050"] = "pcm_22050",
            ["pcm_24000"] = "pcm_24000",
            ["pcm_44100"] = "pcm_44100",
            ["ulaw"] = "ulaw_8000",
            ["wav"] = "wav_44100",
        };

        private readonly ElevenLabsProvider _provider;

        /// <summary>Creates a speech model.</summary>
        public ElevenLabsSpeechModel(ElevenLabsProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "elevenlabs.speech";

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
                    new ElevenLabsSpeechRequest(options.Text)
                    {
                        Voice = options.Voice,
                        OutputFormat = options.OutputFormat ?? "mp3_44100_128",
                        Instructions = options.Instructions,
                        Language = options.Language,
                        Speed = options.Speed,
                        ProviderOptions = options.ProviderOptions is { Count: > 0 } providerOptions
                            ? JsonSerializer.SerializeToElement(providerOptions)
                            : null,
                        Headers = options.Headers is { } headers
                            ? headers
                                .Where(header => header.Value != null)
                                .ToDictionary(header => header.Key, header => header.Value!)
                            : null,
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType);
        }

        /// <inheritdoc />
        public async Task<SpeechModelResult> DoGenerateAsync(
            SpeechModelCall call,
            CancellationToken cancellationToken
        )
        {
            call = call ?? throw new ArgumentNullException(nameof(call));
            var result = await GenerateAsync(
                    new ElevenLabsSpeechRequest(call.Text)
                    {
                        Voice = call.Voice,
                        OutputFormat = call.OutputFormat ?? "mp3_44100_128",
                        Instructions = call.Instructions,
                        Language = call.Language,
                        Speed = call.Speed,
                        ProviderOptions =
                            call.ProviderOptions.ValueKind == JsonValueKind.Object
                                ? call.ProviderOptions
                                : null,
                        Headers = call.Headers.ToDictionary(pair => pair.Key, pair => pair.Value),
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
            var headers = result.Response.Headers.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase
            );
            headers["Content-Type"] = result.MediaType;
            return new SpeechModelResult(
                result.Audio,
                result.Warnings.Select(ToOperationWarning).ToList(),
                result.ProviderMetadata,
                new ProviderResponse(
                    headers,
                    result.Response.Body,
                    timestamp: result.Response.Timestamp.UtcDateTime,
                    modelId: result.Response.ModelId
                ),
                new ProviderRequest(result.RequestBody)
            );
        }

        /// <inheritdoc />
        public async Task<SpeechModelStreamResult> DoStreamAsync(
            SpeechModelStreamCall call,
            CancellationToken cancellationToken
        )
        {
            call = call ?? throw new ArgumentNullException(nameof(call));
            try
            {
                return await StartStreamAsync(call, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // The provider owns the text stream once doStream is called.
                try
                {
                    await call.Text.CancelAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // The original failure is the one to report.
                }

                throw;
            }
        }

        /// <summary>Maps to <c>startStream</c>.</summary>
        private async Task<SpeechModelStreamResult> StartStreamAsync(
            SpeechModelStreamCall call,
            CancellationToken cancellationToken
        )
        {
            var config = ElevenLabsModelCatalog
                .GetElevenLabsSpeechModelEntry(ModelId)
                .RequestConfig;
            if (config.Music || config.SoundEffects)
            {
                throw new Operations.UnsupportedFunctionalityException(
                    "streaming speech with " + ModelId,
                    "ElevenLabs "
                        + ModelId
                        + " does not support streaming; use generateSpeech instead."
                );
            }

            var currentDate = _provider.Now();
            ElevenLabsSpeechModelOptions.Validate(call.ProviderOptions);
            var elevenlabs = ElevenLabsObject(call.ProviderOptions);
            var dialogue = Child(elevenlabs, "dialogue");
            var streaming = Child(elevenlabs, "streaming");
            var dialogueVoices = Child(dialogue, "voices")
                is { ValueKind: JsonValueKind.Array } voiceList
                ? voiceList.EnumerateArray().Select(voice => voice.GetString()!).ToList()
                : new List<string>();

            if (dialogue != null && !config.Dialogue)
            {
                throw new Operations.UnsupportedFunctionalityException(
                    "streaming dialogue with " + ModelId,
                    "ElevenLabs streaming dialogue requires an eleven_v3 or eleven_v4 model; "
                        + ModelId
                        + " is not one."
                );
            }

            if (dialogue == null && config.Dialogue)
            {
                throw new Operations.UnsupportedFunctionalityException(
                    "streaming speech with " + ModelId,
                    "The ElevenLabs text-to-speech WebSocket does not serve "
                        + ModelId
                        + ". Set providerOptions.elevenlabs.dialogue to stream it as dialogue."
                );
            }

            if (dialogue != null && config.SingleVoiceDialogue && dialogueVoices.Count != 1)
            {
                throw new InvalidArgumentError(
                    "providerOptions.elevenlabs.dialogue.voices",
                    dialogueVoices.Count,
                    "eleven_v3_conversational supports exactly one dialogue voice; received "
                        + dialogueVoices.Count.ToString(CultureInfo.InvariantCulture)
                        + "."
                );
            }

            // The WebSocket API rejects locators without a version, unlike HTTP.
            var locators = Child(elevenlabs, "pronunciationDictionaryLocators")
                is { ValueKind: JsonValueKind.Array } locatorList
                ? locatorList.EnumerateArray().ToList()
                : null;
            var unversionedLocator = locators?.FirstOrDefault(locator =>
                Child(locator, "versionId") is not { } versionId
                || versionId.ValueKind != JsonValueKind.String
                || string.IsNullOrEmpty(versionId.GetString())
            );
            if (unversionedLocator is { ValueKind: JsonValueKind.Object } unversioned)
            {
                throw new InvalidArgumentError(
                    "providerOptions.elevenlabs.pronunciationDictionaryLocators",
                    unversioned.ToString(),
                    "Streaming speech requires a versionId for pronunciation dictionary \""
                        + Child(unversioned, "pronunciationDictionaryId")?.GetString()
                        + "\"."
                );
            }

            var warnings = GetStreamWarnings(call, elevenlabs);
            var outputFormat = Child(streaming, "outputFormat")?.GetString()
                ?? ToElevenLabsStreamOutputFormat(call.OutputFormat);
            var voiceId = call.Voice ?? ElevenLabsSpeechRequest.DefaultVoiceId;
            var path =
                dialogue != null
                    ? "/v1/text-to-dialogue/stream-input"
                    : "/v1/text-to-speech/" + Uri.EscapeDataString(voiceId) + "/stream-input";

            var queryParams = new List<KeyValuePair<string, string?>>
            {
                new("model_id", ModelId),
                new("output_format", outputFormat),
                new(
                    "language_code",
                    call.Language ?? Child(elevenlabs, "languageCode")?.GetString()
                ),
                new("enable_logging", Scalar(Child(elevenlabs, "enableLogging"))),
                // Text-to-Dialogue has a fixed inactivity timeout and no auto/SSML mode.
                new(
                    "inactivity_timeout",
                    dialogue == null ? Scalar(Child(streaming, "inactivityTimeout")) : null
                ),
                new("sync_alignment", Scalar(Child(streaming, "syncAlignment"))),
                new("auto_mode", dialogue == null ? Scalar(Child(streaming, "autoMode")) : null),
                new(
                    "apply_text_normalization",
                    Scalar(Child(elevenlabs, "applyTextNormalization"))
                ),
                new("seed", Scalar(Child(elevenlabs, "seed"))),
                new(
                    "enable_ssml_parsing",
                    dialogue == null ? Scalar(Child(streaming, "enableSsmlParsing")) : null
                ),
            };
            var query = string.Join(
                "&",
                queryParams
                    .Where(pair => pair.Value != null)
                    .Select(pair => pair.Key + "=" + Uri.EscapeDataString(pair.Value!))
            );
            var url = WebSocketUtils.ToWebSocketUrl(
                ApiKeys.Combine(_provider.Options.BaseUrl, path + "?" + query)
            );

            ElevenLabsStreamProtocol protocol;
            if (dialogue != null)
            {
                if (call.Voice != null && !dialogueVoices.Contains(call.Voice))
                {
                    throw new InvalidArgumentError(
                        "voice",
                        call.Voice,
                        "ElevenLabs streaming dialogue voice \""
                            + call.Voice
                            + "\" is not listed in providerOptions.elevenlabs.dialogue.voices."
                    );
                }

                protocol = ElevenLabsSpeechStream.CreateDialogueProtocol(
                    GetDialogueInitMessage(dialogue, elevenlabs),
                    dialogueVoices,
                    call.Voice ?? dialogueVoices[0],
                    // The v3 family accepts a single message per session.
                    config.IncrementalDialogue
                );
            }
            else
            {
                protocol = ElevenLabsSpeechStream.CreateSingleContextProtocol(
                    GetStreamInitMessage(call, elevenlabs),
                    voiceId
                );
            }

            // The API key goes in the init message (not a header) so native WebSockets work; it is kept out of the
            // reported request body.
            var combined = ProviderExchange.Merge(
                await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                call.Headers
            );
            var apiKey = combined.TryGetValue("xi-api-key", out var key) ? key : null;
            combined.Remove("xi-api-key");

            var requestBody = new JsonObject
            {
                ["url"] = url.AbsoluteUri,
                ["initMessage"] = protocol.InitMessage.DeepClone(),
            };
            var keepaliveIntervalMs = (_provider.Options as ElevenLabsOptions)?.KeepaliveIntervalMs
                ?? ElevenLabsSpeechStream.KeepaliveIntervalMs;
            var requestElement = JsonSerializer.SerializeToElement(requestBody);
            return new SpeechModelStreamResult(
                ElevenLabsSpeechStream.CreateElevenLabsSpeechStream(
                    new ElevenLabsSpeechStreamOptions
                    {
                        AbortSignal = call.CancellationToken,
                        ApiKey = apiKey,
                        AudioFormat = ElevenLabsSpeechStream.GetElevenLabsAudioFormat(outputFormat),
                        Headers = combined,
                        IncludeRawChunks = call.IncludeRawChunks,
                        // Text-to-Dialogue has a fixed 20 s inactivity timeout, so it always keeps alive; TTS honors an
                        // explicit inactivityTimeout instead.
                        Keepalive =
                            dialogue != null || Child(streaming, "inactivityTimeout") == null,
                        KeepaliveIntervalMs = keepaliveIntervalMs,
                        Protocol = protocol,
                        Text = call.Text,
                        Url = url,
                        Warnings = warnings,
                        WebSocket = (_provider.Options as ElevenLabsOptions)?.WebSocket,
                    }
                ),
                new SpeechModelStreamResponse
                {
                    Timestamp = currentDate.UtcDateTime,
                    ModelId = ModelId,
                },
                requestElement
            );
        }

        /// <summary>Maps to <c>getDialogueInitMessage</c>.</summary>
        private static JsonObject GetDialogueInitMessage(
            JsonElement? dialogue,
            JsonElement? elevenlabs
        )
        {
            var message = new JsonObject();
            if (Child(Child(dialogue, "voiceSettings"), "stability") is { } stability)
            {
                message["voice_settings"] = new JsonObject
                {
                    ["stability"] = JsonNode.Parse(stability.GetRawText()),
                };
            }

            if (
                Child(elevenlabs, "pronunciationDictionaryLocators") is
                { ValueKind: JsonValueKind.Array } locators
            )
            {
                message["pronunciation_dictionary_locators"] = ToStreamLocators(locators);
            }

            return message;
        }

        /// <summary>Maps to <c>getStreamInitMessage</c>.</summary>
        private static JsonObject GetStreamInitMessage(
            SpeechModelStreamCall call,
            JsonElement? elevenlabs
        )
        {
            var voiceSettings = Child(elevenlabs, "voiceSettings");
            var voiceSettingsMessage = new JsonObject();
            CopyNumber(voiceSettings, "stability", voiceSettingsMessage, "stability");
            CopyNumber(voiceSettings, "similarityBoost", voiceSettingsMessage, "similarity_boost");
            CopyNumber(voiceSettings, "style", voiceSettingsMessage, "style");
            CopyRaw(voiceSettings, "useSpeakerBoost", voiceSettingsMessage, "use_speaker_boost");
            if (call.Speed != null)
            {
                voiceSettingsMessage["speed"] = call.Speed;
            }

            // The protocol requires a single space as the first message text.
            var message = new JsonObject { ["text"] = " " };
            if (voiceSettingsMessage.Count > 0)
            {
                message["voice_settings"] = voiceSettingsMessage;
            }

            if (
                Child(Child(elevenlabs, "streaming"), "chunkLengthSchedule") is
                { ValueKind: JsonValueKind.Array } schedule
            )
            {
                message["generation_config"] = new JsonObject
                {
                    ["chunk_length_schedule"] = JsonNode.Parse(schedule.GetRawText()),
                };
            }

            if (
                Child(elevenlabs, "pronunciationDictionaryLocators") is
                { ValueKind: JsonValueKind.Array } locators
            )
            {
                message["pronunciation_dictionary_locators"] = ToStreamLocators(locators);
            }

            return message;
        }

        private static JsonArray ToStreamLocators(JsonElement locators)
        {
            return new JsonArray(
                locators
                    .EnumerateArray()
                    .Select(locator =>
                        (JsonNode?)
                            new JsonObject
                            {
                                ["pronunciation_dictionary_id"] = Child(
                                    locator,
                                    "pronunciationDictionaryId"
                                )
                                    ?.GetString(),
                                ["version_id"] = Child(locator, "versionId")?.GetString(),
                            }
                    )
                    .ToArray()
            );
        }

        /// <summary>Maps to <c>getStreamWarnings</c>.</summary>
        private static List<OperationWarning> GetStreamWarnings(
            SpeechModelStreamCall call,
            JsonElement? elevenlabs
        )
        {
            var warnings = new List<OperationWarning>();
            if (!string.IsNullOrEmpty(call.Instructions))
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "instructions",
                        "ElevenLabs speech models do not support instructions. Instructions parameter was ignored."
                    )
                );
            }

            // HTTP-only options: neither WebSocket protocol carries them.
            var httpOnly = new[]
            {
                "applyLanguageTextNormalization",
                "withTimestamps",
                "inputs",
                "nextRequestIds",
                "nextText",
                "previousRequestIds",
                "previousText",
            }
                .Concat(ElevenLabsModelCatalog.ElevenLabsMusicOnlyOptions)
                .Concat(ElevenLabsModelCatalog.ElevenLabsSoundEffectsOnlyOptions);
            foreach (var option in httpOnly)
            {
                if (Child(elevenlabs, option) != null)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            "providerOptions.elevenlabs." + option,
                            "ElevenLabs streaming speech does not support " + option + "."
                        )
                    );
                }
            }

            if (Child(elevenlabs, "dialogue") != null)
            {
                // Text-to-Dialogue takes only dialogue.voiceSettings.stability, and has a fixed inactivity timeout and
                // no chunking, auto, or SSML modes.
                var ignored = new List<string>();
                if (call.Speed != null)
                {
                    ignored.Add("speed");
                }

                if (Child(elevenlabs, "voiceSettings") != null)
                {
                    ignored.Add("providerOptions.elevenlabs.voiceSettings");
                }

                foreach (
                    var option in new[]
                    {
                        "autoMode",
                        "chunkLengthSchedule",
                        "enableSsmlParsing",
                        "inactivityTimeout",
                    }
                )
                {
                    if (Child(Child(elevenlabs, "streaming"), option) != null)
                    {
                        ignored.Add("providerOptions.elevenlabs.streaming." + option);
                    }
                }

                foreach (var feature in ignored)
                {
                    warnings.Add(
                        OperationWarning.Unsupported(
                            feature,
                            "ElevenLabs streaming dialogue does not support "
                                + feature
                                + "; it was ignored."
                        )
                    );
                }
            }

            return warnings;
        }

        /// <summary>
        /// Maps a portable output format to an ElevenLabs <c>output_format</c>. Exact native formats (e.g. other MP3
        /// bitrates) use <c>providerOptions.elevenlabs.streaming.outputFormat</c>. Maps to <c>toElevenLabsStreamOutputFormat</c>.
        /// </summary>
        internal static string ToElevenLabsStreamOutputFormat(SpeechAudioFormat? format)
        {
            if (format == null)
            {
                return "mp3_44100_128";
            }

            var rate = format.Rate;
            // MIME types are case-insensitive and may carry parameters.
            var type = format.Type.Split(';')[0].Trim().ToLowerInvariant();
            switch (type)
            {
                case "audio/mpeg":
                case "audio/mp3":
                    if (rate == null || rate == 44100)
                    {
                        return "mp3_44100_128";
                    }

                    if (rate == 22050)
                    {
                        return "mp3_22050_32";
                    }

                    break;
                case "audio/pcm":
                case "audio/l16":
                    var pcmRate = rate ?? 24000;
                    if (
                        Array.IndexOf(new[] { 8000, 16000, 22050, 24000, 44100, 48000 }, pcmRate)
                        >= 0
                    )
                    {
                        return "pcm_" + pcmRate.ToString(CultureInfo.InvariantCulture);
                    }

                    break;
                case "audio/pcmu":
                case "audio/basic":
                    if (rate == null || rate == 8000)
                    {
                        return "ulaw_8000";
                    }

                    break;
                case "audio/pcma":
                    if (rate == null || rate == 8000)
                    {
                        return "alaw_8000";
                    }

                    break;
                case "audio/opus":
                case "audio/ogg":
                    if (rate == null || rate == 48000)
                    {
                        return "opus_48000_64";
                    }

                    break;
            }

            throw new InvalidArgumentError(
                "outputFormat",
                format.Type,
                "ElevenLabs streaming speech does not support the output format "
                    + JsonSerializer.Serialize(
                        new { type = format.Type, rate = format.Rate },
                        new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = System
                                .Text
                                .Json
                                .Serialization
                                .JsonIgnoreCondition
                                .WhenWritingNull,
                        }
                    )
                    + ". Use providerOptions.elevenlabs.streaming.outputFormat for a native ElevenLabs format."
            );
        }

        // Maps `String(value)` for a query value: undefined and null are omitted.
        private static string? Scalar(JsonElement? value)
        {
            return value is not { } element
                ? null
                : element.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Number => element
                        .GetDouble()
                        .ToString("R", CultureInfo.InvariantCulture),
                    JsonValueKind.String => element.GetString(),
                    _ => element.GetRawText(),
                };
        }

        private static OperationWarning ToOperationWarning(ModelWarning warning) =>
            warning switch
            {
                UnsupportedWarning unsupported => new OperationWarning(
                    unsupported.Type,
                    feature: unsupported.Feature,
                    details: unsupported.Details
                ),
                OtherWarning other => new OperationWarning(other.Type, other.Message),
                CompatibilityWarning compatibility => new OperationWarning(
                    compatibility.Type,
                    feature: compatibility.Feature,
                    details: compatibility.Details
                ),
                DeprecatedWarning deprecated => new OperationWarning(
                    deprecated.Type,
                    deprecated.Message,
                    setting: deprecated.Setting
                ),
                _ => new OperationWarning(warning.Type),
            };

        /// <summary>Synthesizes speech and returns the audio with warnings and response metadata.</summary>
        public async Task<ElevenLabsSpeechResult> GenerateAsync(
            ElevenLabsSpeechRequest request,
            CancellationToken cancellationToken
        )
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            var timestamp = _provider.Now();
            var config = ElevenLabsModelCatalog
                .GetElevenLabsSpeechModelEntry(ModelId)
                .RequestConfig;
            var args =
                config.Music || config.SoundEffects
                    ? GetAudioGenerationArgs(request, config)
                    : GetArgs(request, config);
            var body = args.Body;
            var warnings = args.Warnings;
            var withTimestamps = args.WithTimestamps;
            var path =
                args.Path
                + (withTimestamps ? "/with-timestamps" : string.Empty)
                + (args.Query.Count > 0 ? "?" + string.Join("&", args.Query) : string.Empty);
            var json = body.ToJsonString();
            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, path);
            var response = await _provider
                .SendAsync(
                    uri,
                    ProviderExchange.Json(json),
                    body,
                    ProviderExchange.Merge(
                        await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                        request.Headers
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);
            var mediaType = "audio/mpeg";
            response.Headers.TryGetValue("Content-Type", out var contentType);
            if (
                contentType != null
                && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
            )
            {
                var separator = contentType.IndexOf(';');
                mediaType = (
                    separator >= 0 ? contentType.Substring(0, separator) : contentType
                ).Trim();
            }

            var metadata = new JsonObject();
            if (!string.IsNullOrEmpty(args.OutputFormat))
            {
                metadata["outputFormat"] = args.OutputFormat;
            }

            if (args.RequestedDurationMs is { } requestedDurationMs)
            {
                metadata["requestedDurationMs"] = requestedDurationMs;
            }

            var audio = response.Bytes;
            JsonElement? rawBody = null;
            if (args.Path == "/v1/music/detailed")
            {
                var (music, raw) = ElevenLabsMusicResponse.ParseMusicTimestampsResponse(
                    response.Bytes,
                    contentType ?? string.Empty
                );
                rawBody = raw;
                audio = music.Audio;
                mediaType = DetailedMediaType(args.OutputFormat);
                if (music.WordTimestamps != null)
                {
                    metadata["wordTimestamps"] = new JsonArray(
                        music
                            .WordTimestamps.Select(word =>
                                (JsonNode?)
                                    new JsonObject
                                    {
                                        ["word"] = word.Word,
                                        ["startMs"] = word.StartMs,
                                        ["endMs"] = word.EndMs,
                                    }
                            )
                            .ToArray()
                    );
                }
            }
            else if (withTimestamps)
            {
                var envelope = ReadTimestampsEnvelope(response.Body, uri.ToString(), body);
                rawBody = envelope.Raw;
                audio = envelope.Audio;
                mediaType = DetailedMediaType(args.OutputFormat);
                if (envelope.Alignment != null)
                {
                    metadata["alignment"] = envelope.Alignment;
                }

                if (envelope.NormalizedAlignment != null)
                {
                    metadata["normalizedAlignment"] = envelope.NormalizedAlignment;
                }

                if (envelope.VoiceSegments != null)
                {
                    metadata["voiceSegments"] = envelope.VoiceSegments;
                }
            }

            return new ElevenLabsSpeechResult(
                audio,
                mediaType,
                warnings,
                json,
                new ElevenLabsResponse(timestamp, ModelId, response.Headers, rawBody),
                JsonSerializer.SerializeToElement(new JsonObject { ["elevenlabs"] = metadata })
            );
        }

        /// <summary>Text to Dialogue from explicit inputs, or a single text/voice input for v4. Maps to <c>getDialogueArgs</c>.</summary>
        private SpeechArgs GetDialogueArgs(
            ElevenLabsSpeechRequest request,
            ElevenLabsSpeechRequestConfig config,
            JsonElement? elevenlabs,
            List<ModelWarning> warnings
        )
        {
            if (!config.Dialogue)
            {
                throw new Operations.UnsupportedFunctionalityException(
                    "dialogue with " + ModelId,
                    "ElevenLabs dialogue requires an eleven_v3 or eleven_v4 model; "
                        + ModelId
                        + " is not one."
                );
            }

            var explicitInputs = Child(elevenlabs, "inputs");
            if (
                explicitInputs is { } rawInputs
                && (rawInputs.ValueKind != JsonValueKind.Array || rawInputs.GetArrayLength() == 0)
            )
            {
                throw new InvalidArgumentError(
                    "providerOptions.elevenlabs.inputs",
                    rawInputs.ToString(),
                    "inputs must be a non-empty array."
                );
            }

            var inputs = new JsonArray();
            if (explicitInputs is { } given)
            {
                foreach (var input in given.EnumerateArray())
                {
                    if (
                        input.ValueKind != JsonValueKind.Object
                        || Child(input, "text") is not { ValueKind: JsonValueKind.String } inputText
                        || Child(input, "voiceId")
                            is not { ValueKind: JsonValueKind.String } inputVoice
                    )
                    {
                        throw new InvalidArgumentError(
                            "providerOptions.elevenlabs.inputs",
                            input.ToString(),
                            "Each dialogue input needs a text and a voiceId."
                        );
                    }

                    inputs.Add(
                        new JsonObject
                        {
                            ["text"] = inputText.GetString(),
                            ["voice_id"] = inputVoice.GetString(),
                        }
                    );
                }
            }
            else
            {
                inputs.Add(
                    new JsonObject
                    {
                        ["text"] = request.Text,
                        ["voice_id"] = string.IsNullOrEmpty(request.Voice)
                            ? ElevenLabsSpeechRequest.DefaultVoiceId
                            : request.Voice,
                    }
                );
            }

            if (request.Text.Length > 0 && explicitInputs != null)
            {
                warnings.Add(
                    new OtherWarning(
                        "ElevenLabs dialogue uses providerOptions.elevenlabs.inputs; the text prompt was ignored."
                    )
                );
            }

            var voiceSettings = Child(elevenlabs, "voiceSettings");
            var ignored = new List<string>();
            if (!string.IsNullOrEmpty(request.Instructions))
            {
                ignored.Add("instructions");
            }

            if (request.Speed != null)
            {
                ignored.Add("speed");
            }

            if (explicitInputs != null && !string.IsNullOrEmpty(request.Voice))
            {
                ignored.Add("voice");
            }

            if (Child(elevenlabs, "applyLanguageTextNormalization") != null)
            {
                ignored.Add("providerOptions.elevenlabs.applyLanguageTextNormalization");
            }

            if (Child(voiceSettings, "style") != null)
            {
                ignored.Add("providerOptions.elevenlabs.voiceSettings.style");
            }

            if (Child(voiceSettings, "useSpeakerBoost") != null)
            {
                ignored.Add("providerOptions.elevenlabs.voiceSettings.useSpeakerBoost");
            }

            foreach (var feature in ignored)
            {
                warnings.Add(
                    new UnsupportedWarning(
                        feature,
                        "ElevenLabs dialogue does not support " + feature + "; it was ignored."
                    )
                );
            }

            var query = new List<string>
            {
                "output_format="
                    + Uri.EscapeDataString(ToElevenLabsOutputFormat(request.OutputFormat)),
            };
            AddEnableLogging(elevenlabs, query);

            var settings = new JsonObject();
            CopyNumber(voiceSettings, "stability", settings, "stability");

            // Text-to-Dialogue names this setting `similarity`.
            CopyNumber(voiceSettings, "similarityBoost", settings, "similarity");

            var body = new JsonObject { ["inputs"] = inputs, ["model_id"] = ModelId };
            var language = request.Language ?? Child(elevenlabs, "languageCode")?.GetString();
            if (language != null)
            {
                body["language_code"] = language;
            }

            if (settings.Count > 0)
            {
                body["settings"] = settings;
            }

            if (Child(elevenlabs, "previousText") is { ValueKind: JsonValueKind.String } previous)
            {
                body["previous_text"] = previous.GetString();
            }

            if (Child(elevenlabs, "nextText") is { ValueKind: JsonValueKind.String } next)
            {
                body["future_text"] = next.GetString();
            }

            if (
                Child(elevenlabs, "pronunciationDictionaryLocators") is
                { ValueKind: JsonValueKind.Array } locators
            )
            {
                body["pronunciation_dictionary_locators"] = ToLocators(locators);
            }

            CopyNumber(elevenlabs, "seed", body, "seed");
            CopyRaw(elevenlabs, "applyTextNormalization", body, "apply_text_normalization");
            CopyRaw(elevenlabs, "previousRequestIds", body, "previous_request_ids");
            CopyRaw(elevenlabs, "nextRequestIds", body, "next_request_ids");
            return new SpeechArgs(
                "/v1/text-to-dialogue",
                body,
                query,
                ToElevenLabsOutputFormat(request.OutputFormat),
                ReadFlag(elevenlabs, "withTimestamps"),
                null,
                warnings
            );
        }

        /// <summary>Music (<c>/v1/music</c>) and sound effects (<c>/v1/sound-generation</c>). Maps to <c>getAudioGenerationArgs</c>.</summary>
        private SpeechArgs GetAudioGenerationArgs(
            ElevenLabsSpeechRequest request,
            ElevenLabsSpeechRequestConfig config
        )
        {
            var warnings = new List<ModelWarning>();
            var isMusic = config.Music;
            var label = isMusic ? "music" : "sound effects";
            ElevenLabsSpeechModelOptions.Validate(request.ProviderOptions);
            var elevenlabs = ElevenLabsObject(request.ProviderOptions);

            var ignoredCallOptions = new List<string>();
            if (request.Instructions != null)
            {
                ignoredCallOptions.Add("instructions");
            }

            if (request.Language != null)
            {
                ignoredCallOptions.Add("language");
            }

            if (request.Speed != null)
            {
                ignoredCallOptions.Add("speed");
            }

            if (request.Voice != null)
            {
                ignoredCallOptions.Add("voice");
            }

            foreach (var feature in ignoredCallOptions)
            {
                warnings.Add(
                    new UnsupportedWarning(
                        feature,
                        "ElevenLabs "
                            + label
                            + " models do not support "
                            + feature
                            + "; it was ignored."
                    )
                );
            }

            var ignoredProviderOptions = ElevenLabsModelCatalog
                .ElevenLabsTextToSpeechOnlyOptions.Concat(
                    isMusic
                        ? ElevenLabsModelCatalog.ElevenLabsSoundEffectsOnlyOptions
                        : ElevenLabsModelCatalog.ElevenLabsMusicOnlyOptions
                )
                .Concat(isMusic ? [] : new[] { "seed", "withTimestamps" });
            foreach (var option in ignoredProviderOptions)
            {
                if (Child(elevenlabs, option) != null)
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            "providerOptions.elevenlabs." + option,
                            "ElevenLabs "
                                + label
                                + " models do not support "
                                + option
                                + "; it was ignored."
                        )
                    );
                }
            }

            var outputFormat = ToElevenLabsOutputFormat(request.OutputFormat);
            var query = new List<string> { "output_format=" + Uri.EscapeDataString(outputFormat) };
            AddEnableLogging(elevenlabs, query);

            JsonObject body;
            double? requestedDurationMs;
            var withTimestamps = ReadFlag(elevenlabs, "withTimestamps");
            if (isMusic)
            {
                var compositionPlan = Child(elevenlabs, "compositionPlan");
                if (compositionPlan is { } plan)
                {
                    // The API rejects prompt-only options alongside a composition plan.
                    foreach (var option in new[] { "forceInstrumental", "musicLengthMs" })
                    {
                        if (Child(elevenlabs, option) != null)
                        {
                            throw new InvalidArgumentError(
                                "providerOptions.elevenlabs." + option,
                                null,
                                "ElevenLabs music "
                                    + option
                                    + " cannot be combined with compositionPlan."
                            );
                        }
                    }

                    if (request.Text.Length > 0)
                    {
                        warnings.Add(
                            new OtherWarning(
                                "ElevenLabs music uses providerOptions.elevenlabs.compositionPlan; the text prompt was ignored."
                            )
                        );
                    }

                    requestedDurationMs = GetCompositionPlanDurationMs(plan);
                }
                else
                {
                    requestedDurationMs = ReadNumber(elevenlabs, "musicLengthMs");
                }

                body = new JsonObject();
                if (compositionPlan is { } composition)
                {
                    body["composition_plan"] = JsonNode.Parse(composition.GetRawText());
                }
                else
                {
                    body["prompt"] = request.Text;
                }

                body["model_id"] = ModelId;
                CopyNumber(elevenlabs, "musicLengthMs", body, "music_length_ms");
                CopyRaw(elevenlabs, "forceInstrumental", body, "force_instrumental");
                CopyNumber(elevenlabs, "seed", body, "seed");
                if (withTimestamps)
                {
                    body["with_timestamps"] = true;
                }
            }
            else
            {
                var durationSeconds = Child(elevenlabs, "durationSeconds");
                requestedDurationMs = ReadNumber(elevenlabs, "durationSeconds") * 1000;
                body = new JsonObject { ["text"] = request.Text, ["model_id"] = ModelId };
                CopyNumber(elevenlabs, "durationSeconds", body, "duration_seconds");
                if (ReadNumber(elevenlabs, "promptInfluence") is { } promptInfluence)
                {
                    body["prompt_influence"] = promptInfluence;
                }

                if (Child(elevenlabs, "loop") != null)
                {
                    body["loop"] = ReadFlag(elevenlabs, "loop");
                }
            }

            return new SpeechArgs(
                isMusic
                    ? (withTimestamps ? "/v1/music/detailed" : "/v1/music")
                    : "/v1/sound-generation",
                body,
                query,
                outputFormat,
                false,
                requestedDurationMs,
                warnings
            );
        }

        private static bool ReadFlag(JsonElement? options, string name)
        {
            if (Child(options, name) is not { } value)
            {
                return false;
            }

            return value.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? value.GetBoolean()
                : throw new InvalidArgumentError(
                    "providerOptions.elevenlabs." + name,
                    value.ToString(),
                    name + " must be a boolean."
                );
        }

        private static double? ReadNumber(JsonElement? options, string name)
        {
            if (Child(options, name) is not { } value)
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : throw new InvalidArgumentError(
                    "providerOptions.elevenlabs." + name,
                    value.ToString(),
                    name + " must be a number."
                );
        }

        private static string DetailedMediaType(string? outputFormat) =>
            (outputFormat ?? string.Empty).Split('_')[0] switch
            {
                "wav" => "audio/wav",
                "pcm" => "audio/pcm",
                "opus" => "audio/opus",
                "ulaw" or "alaw" => "audio/basic",
                _ => "audio/mpeg",
            };

        private static void AddEnableLogging(JsonElement? elevenlabs, List<string> query)
        {
            if (Child(elevenlabs, "enableLogging") != null)
            {
                query.Add(
                    "enable_logging=" + (ReadFlag(elevenlabs, "enableLogging") ? "true" : "false")
                );
            }
        }

        private static JsonArray ToLocators(JsonElement locators)
        {
            var array = new JsonArray();
            foreach (var locator in locators.EnumerateArray())
            {
                var item = new JsonObject
                {
                    ["pronunciation_dictionary_id"] = Child(locator, "pronunciationDictionaryId")
                        ?.GetString(),
                };
                if (
                    Child(locator, "versionId") is { ValueKind: JsonValueKind.String } version
                    && version.GetString()!.Length > 0
                )
                {
                    item["version_id"] = version.GetString();
                }

                array.Add(item);
            }

            return array;
        }

        private static string? ToOutputFormat(string? requested) =>
            string.IsNullOrEmpty(requested) ? requested : ToElevenLabsOutputFormat(requested!);

        /// <summary>Maps a portable output format to an ElevenLabs <c>output_format</c>. Maps to <c>toElevenLabsOutputFormat</c>.</summary>
        private static string ToElevenLabsOutputFormat(string outputFormat) =>
            OutputFormats.TryGetValue(outputFormat, out var mapped) ? mapped : outputFormat;

        /// <summary>
        /// Total length of a music composition plan: <c>sections</c> (music_v1) or <c>chunks</c> (music_v2+).
        /// Null when the plan has no parts. Maps to <c>getCompositionPlanDurationMs</c>.
        /// </summary>
        private static double? GetCompositionPlanDurationMs(JsonElement plan)
        {
            var durations = new List<double>();
            if (plan.TryGetProperty("sections", out var sections))
            {
                foreach (var section in sections.EnumerateArray())
                {
                    durations.Add(section.GetProperty("duration_ms").GetDouble());
                }
            }
            else if (plan.TryGetProperty("chunks", out var chunks))
            {
                foreach (var chunk in chunks.EnumerateArray())
                {
                    // Audio reference chunks reuse a time range of an earlier song.
                    durations.Add(
                        chunk.TryGetProperty("duration_ms", out var duration)
                            ? duration.GetDouble()
                            : chunk.GetProperty("range").GetProperty("end_ms").GetDouble()
                                - chunk.GetProperty("range").GetProperty("start_ms").GetDouble()
                    );
                }
            }

            return durations.Count == 0 || durations.Any(duration => duration <= 0)
                ? null
                : durations.Sum();
        }

        private sealed record SpeechArgs(
            string Path,
            JsonObject Body,
            List<string> Query,
            string? OutputFormat,
            bool WithTimestamps,
            double? RequestedDurationMs,
            List<ModelWarning> Warnings
        );

        private SpeechArgs GetArgs(
            ElevenLabsSpeechRequest request,
            ElevenLabsSpeechRequestConfig config
        )
        {
            var warnings = new List<ModelWarning>();
            ElevenLabsSpeechModelOptions.Validate(request.ProviderOptions);
            var elevenlabs = ElevenLabsObject(request.ProviderOptions);
            foreach (var option in new[] { "streaming", "dialogue" })
            {
                if (Child(elevenlabs, option) != null)
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            $"providerOptions.elevenlabs.{option}",
                            $"{(option == "streaming" ? "Streaming" : "Dialogue")} options apply only to streaming speech (experimental_streamSpeech) and were ignored."
                        )
                    );
                }
            }

            foreach (
                var option in ElevenLabsModelCatalog.ElevenLabsMusicOnlyOptions.Concat(
                    ElevenLabsModelCatalog.ElevenLabsSoundEffectsOnlyOptions
                )
            )
            {
                if (Child(elevenlabs, option) != null)
                {
                    warnings.Add(
                        new UnsupportedWarning(
                            $"providerOptions.elevenlabs.{option}",
                            $"ElevenLabs speech models do not support {option}; it was ignored."
                        )
                    );
                }
            }

            if (Child(elevenlabs, "inputs") != null || config.DefaultDialogue)
            {
                return GetDialogueArgs(request, config, elevenlabs, warnings);
            }

            var body = new JsonObject { ["text"] = request.Text, ["model_id"] = ModelId };
            var query = new List<string>();
            var requestedOutputFormat = request.OutputFormat;
            if (!string.IsNullOrEmpty(requestedOutputFormat))
            {
                query.Add(
                    "output_format="
                        + Uri.EscapeDataString(
                            OutputFormats.TryGetValue(requestedOutputFormat, out var mapped)
                                ? mapped
                                : requestedOutputFormat
                        )
                );
            }

            if (!string.IsNullOrEmpty(request.Language))
            {
                body["language_code"] = request.Language;
            }

            var voiceSettings = new JsonObject();
            if (request.Speed is { } speed)
            {
                voiceSettings["speed"] = speed;
            }

            var settings = Child(elevenlabs, "voiceSettings");
            CopyNumber(settings, "stability", voiceSettings, "stability");
            CopyNumber(settings, "similarityBoost", voiceSettings, "similarity_boost");
            CopyNumber(settings, "style", voiceSettings, "style");
            if (Child(settings, "useSpeakerBoost") is { } speakerBoost)
            {
                voiceSettings["use_speaker_boost"] = speakerBoost.GetBoolean();
            }

            if (
                body["language_code"] == null
                && Child(elevenlabs, "languageCode")
                    is { ValueKind: JsonValueKind.String } languageCode
                && languageCode.GetString()!.Length > 0
            )
            {
                body["language_code"] = languageCode.GetString();
            }

            if (
                Child(elevenlabs, "pronunciationDictionaryLocators") is
                { ValueKind: JsonValueKind.Array } locators
            )
            {
                var array = new JsonArray();
                foreach (var locator in locators.EnumerateArray())
                {
                    var item = new JsonObject
                    {
                        ["pronunciation_dictionary_id"] = Child(
                            locator,
                            "pronunciationDictionaryId"
                        )
                            ?.GetString(),
                    };
                    if (
                        Child(locator, "versionId") is { ValueKind: JsonValueKind.String } version
                        && version.GetString()!.Length > 0
                    )
                    {
                        item["version_id"] = version.GetString();
                    }

                    array.Add(item);
                }

                body["pronunciation_dictionary_locators"] = array;
            }

            CopyNumber(elevenlabs, "seed", body, "seed");
            CopyText(elevenlabs, "previousText", body, "previous_text");
            CopyText(elevenlabs, "nextText", body, "next_text");
            CopyRaw(elevenlabs, "previousRequestIds", body, "previous_request_ids");
            CopyRaw(elevenlabs, "nextRequestIds", body, "next_request_ids");
            CopyText(elevenlabs, "applyTextNormalization", body, "apply_text_normalization");
            CopyRaw(
                elevenlabs,
                "applyLanguageTextNormalization",
                body,
                "apply_language_text_normalization"
            );
            if (Child(elevenlabs, "enableLogging") != null)
            {
                query.Add(
                    "enable_logging=" + (ReadFlag(elevenlabs, "enableLogging") ? "true" : "false")
                );
            }

            if (voiceSettings.Count > 0)
            {
                body["voice_settings"] = voiceSettings;
            }

            if (!string.IsNullOrEmpty(request.Instructions))
            {
                warnings.Add(
                    new UnsupportedWarning(
                        "instructions",
                        "ElevenLabs speech models do not support instructions. Instructions parameter was ignored."
                    )
                );
            }

            var voice = string.IsNullOrEmpty(request.Voice)
                ? ElevenLabsSpeechRequest.DefaultVoiceId
                : request.Voice!;
            return new SpeechArgs(
                "/v1/text-to-speech/" + Uri.EscapeDataString(voice),
                body,
                query,
                ToOutputFormat(requestedOutputFormat),
                ReadFlag(elevenlabs, "withTimestamps"),
                null,
                warnings
            );
        }

        private static (
            byte[] Audio,
            JsonElement Raw,
            JsonObject? Alignment,
            JsonObject? NormalizedAlignment,
            JsonArray? VoiceSegments
        ) ReadTimestampsEnvelope(string text, string url, object requestBodyValues)
        {
            ApiException Invalid(Exception cause) =>
                new("Invalid JSON response", 200, text, cause)
                {
                    Url = url,
                    RequestBodyValues = requestBodyValues,
                };

            var parsed = JsonParsing.SafeParse(text);
            if (!parsed.Success || parsed.RawValue is not { } raw)
            {
                throw Invalid(
                    parsed.Error
                        ?? new JsonParseException(text, new JsonException("Invalid JSON response"))
                );
            }

            try
            {
                if (
                    raw.ValueKind != JsonValueKind.Object
                    || !raw.TryGetProperty("audio_base64", out var audioBase64)
                    || audioBase64.ValueKind != JsonValueKind.String
                )
                {
                    throw new TypeValidationException(
                        "Type validation failed: audio_base64 must be a string.",
                        raw
                    );
                }

                return (
                    Convert.FromBase64String(audioBase64.GetString()!),
                    raw,
                    ToAlignmentMetadata(raw, "alignment"),
                    ToAlignmentMetadata(raw, "normalized_alignment"),
                    ToVoiceSegments(raw)
                );
            }
            catch (Exception error) when (error is TypeValidationException or FormatException)
            {
                throw Invalid(error);
            }
        }

        private static string SegmentString(JsonElement root, JsonElement segment, string name) =>
            segment.ValueKind == JsonValueKind.Object
            && segment.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()!
                : throw new TypeValidationException(
                    "Type validation failed: voice_segments." + name + " must be a string.",
                    root
                );

        private static double SegmentNumber(JsonElement root, JsonElement segment, string name) =>
            segment.ValueKind == JsonValueKind.Object
            && segment.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : throw new TypeValidationException(
                    "Type validation failed: voice_segments." + name + " must be a number.",
                    root
                );

        private static JsonArray? ToVoiceSegments(JsonElement root)
        {
            if (
                !root.TryGetProperty("voice_segments", out var segments)
                || segments.ValueKind != JsonValueKind.Array
            )
            {
                return null;
            }

            var array = new JsonArray();
            foreach (var segment in segments.EnumerateArray())
            {
                array.Add(
                    new JsonObject
                    {
                        ["voiceId"] = SegmentString(root, segment, "voice_id"),
                        ["startTimeSeconds"] = SegmentNumber(root, segment, "start_time_seconds"),
                        ["endTimeSeconds"] = SegmentNumber(root, segment, "end_time_seconds"),
                        ["characterStartIndex"] = SegmentNumber(
                            root,
                            segment,
                            "character_start_index"
                        ),
                        ["characterEndIndex"] = SegmentNumber(root, segment, "character_end_index"),
                        ["dialogueInputIndex"] = SegmentNumber(
                            root,
                            segment,
                            "dialogue_input_index"
                        ),
                    }
                );
            }

            return array;
        }

        private static JsonObject? ToAlignmentMetadata(JsonElement root, string name)
        {
            if (
                !root.TryGetProperty(name, out var alignment)
                || alignment.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new JsonObject
            {
                ["characters"] = AlignmentArray(
                    root,
                    alignment,
                    "characters",
                    JsonValueKind.String
                ),
                ["characterStartTimesSeconds"] = AlignmentArray(
                    root,
                    alignment,
                    "character_start_times_seconds",
                    JsonValueKind.Number
                ),
                ["characterEndTimesSeconds"] = AlignmentArray(
                    root,
                    alignment,
                    "character_end_times_seconds",
                    JsonValueKind.Number
                ),
            };
        }

        private static JsonArray AlignmentArray(
            JsonElement root,
            JsonElement alignment,
            string name,
            JsonValueKind kind
        )
        {
            if (
                alignment.ValueKind != JsonValueKind.Object
                || !alignment.TryGetProperty(name, out var items)
                || items.ValueKind != JsonValueKind.Array
            )
            {
                throw new TypeValidationException(
                    "Type validation failed: alignment." + name + " must be an array.",
                    root
                );
            }

            var array = new JsonArray();
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != kind)
                {
                    throw new TypeValidationException(
                        "Type validation failed: alignment."
                            + name
                            + " has an item of the wrong type.",
                        root
                    );
                }

                array.Add(JsonNode.Parse(item.GetRawText()));
            }

            return array;
        }

        private static void CopyNumber(
            JsonElement? source,
            string name,
            JsonObject target,
            string targetName
        )
        {
            if (Child(source, name) is { } value)
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }

        private static void CopyText(
            JsonElement? source,
            string name,
            JsonObject target,
            string targetName
        )
        {
            if (
                Child(source, name) is { ValueKind: JsonValueKind.String } value
                && value.GetString()!.Length > 0
            )
            {
                target[targetName] = value.GetString();
            }
        }

        private static void CopyRaw(
            JsonElement? source,
            string name,
            JsonObject target,
            string targetName
        )
        {
            if (Child(source, name) is { } value)
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }
    }

    /// <summary>ElevenLabs batch speech-to-text model.</summary>
    public sealed class ElevenLabsTranscriptionModel : ITranscriptionModel
    {
        private readonly ElevenLabsProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public ElevenLabsTranscriptionModel(ElevenLabsProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        }

        /// <inheritdoc />
        public string Provider => "elevenlabs.transcription";

        /// <inheritdoc />
        public string ModelId { get; }

        private static bool IsRealtimeTranscriptionModelId(string modelId) =>
            ElevenLabsModelCatalog
                .GetElevenLabsTranscriptionModelEntry(modelId)
                .Capabilities.Streaming.IsSupported;

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

        /// <summary>Uploads audio to <c>/v1/speech-to-text</c> and reads the transcript.</summary>
        public async Task<ElevenLabsTranscriptionResult> TranscribeAsync(
            AudioInput audio,
            ElevenLabsTranscriptionRequest? request,
            CancellationToken cancellationToken
        )
        {
            audio = audio ?? throw new ArgumentNullException(nameof(audio));
            if (IsRealtimeTranscriptionModelId(ModelId))
            {
                throw new Operations.UnsupportedFunctionalityException(
                    "non-streaming transcription with " + ModelId,
                    "'non-streaming transcription with "
                        + ModelId
                        + "' functionality not supported."
                );
            }

            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var elevenlabs = ElevenLabsObject(request?.ProviderOptions);
            if (Child(elevenlabs, "streaming") != null)
            {
                warnings.Add(
                    new UnsupportedWarning(
                        "providerOptions.elevenlabs.streaming",
                        "ElevenLabs batch transcription does not support streaming options."
                    )
                );
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(ModelId), "model_id");

            // ElevenLabs accepts exactly one of `file` or `source_url`.
            if (Child(elevenlabs, "sourceUrl") is { ValueKind: JsonValueKind.String } sourceUrl)
            {
                form.Add(new StringContent(sourceUrl.GetString()!), "source_url");
                if (audio.Data.Length > 0)
                {
                    warnings.Add(
                        new OtherWarning(
                            "ElevenLabs transcribes providerOptions.elevenlabs.sourceUrl; the audio input was ignored."
                        )
                    );
                }
            }
            else
            {
                var file = new ByteArrayContent(audio.Data);
                if (!string.IsNullOrEmpty(audio.MediaType))
                {
                    file.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
                }

                form.Add(
                    file,
                    "file",
                    "audio." + ProviderValues.MediaTypeToExtension(audio.MediaType)
                );
            }

            form.Add(
                new StringContent(
                    (elevenlabs != null ? FormValue(Child(elevenlabs, "diarize")) : null) ?? "true"
                ),
                "diarize"
            );
            if (elevenlabs != null)
            {
                // Option defaults apply once the elevenlabs object is present, as in the upstream schema.
                AddField(form, "language_code", FormValue(Child(elevenlabs, "languageCode")));
                AddField(
                    form,
                    "tag_audio_events",
                    FormValue(Child(elevenlabs, "tagAudioEvents")) ?? "true"
                );
                AddField(form, "num_speakers", FormValue(Child(elevenlabs, "numSpeakers")));
                AddField(
                    form,
                    "timestamps_granularity",
                    FormValue(Child(elevenlabs, "timestampsGranularity")) ?? "word"
                );
                AddField(
                    form,
                    "file_format",
                    FormValue(Child(elevenlabs, "fileFormat")) ?? "other"
                );
            }

            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/speech-to-text");
            var response = await _provider
                .SendAsync(
                    uri,
                    form,
                    new Dictionary<string, string> { ["model_id"] = ModelId },
                    ProviderExchange.Merge(
                        await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                        request?.Headers
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            var segments = new List<TranscriptSegment>();
            double? duration = null;
            if (
                root.TryGetProperty("words", out var words)
                && words.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var word in words.EnumerateArray())
                {
                    var end = Child(word, "end")?.GetDouble();
                    segments.Add(
                        new TranscriptSegment(
                            Child(word, "text")?.GetString() ?? string.Empty,
                            Child(word, "start")?.GetDouble() ?? 0,
                            end ?? 0
                        )
                    );
                    duration = end;
                }
            }

            var audioDuration = Child(root, "audio_duration_secs")
                is { ValueKind: JsonValueKind.Number } audioDurationElement
                ? audioDurationElement.GetDouble()
                : (double?)null;

            return new ElevenLabsTranscriptionResult(
                Child(root, "text")?.GetString() ?? string.Empty,
                segments,
                Child(root, "language_code")?.GetString(),
                audioDuration ?? duration,
                warnings,
                new ElevenLabsResponse(timestamp, ModelId, response.Headers, root.Clone())
            );
        }

        private static void AddField(MultipartFormDataContent form, string name, string? value)
        {
            if (value != null)
            {
                form.Add(new StringContent(value), name);
            }
        }

        private static string? FormValue(JsonElement? value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Value.ValueKind switch
            {
                JsonValueKind.String => value.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => value
                    .Value.GetDouble()
                    .ToString(CultureInfo.InvariantCulture),
                _ => value.Value.GetRawText(),
            };
        }
    }
}

/// <summary>Registers ElevenLabs.</summary>
public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddElevenLabs(
        this IServiceCollection services,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        services.AddHttpClient(ElevenLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ElevenLabsProvider(
                sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ElevenLabsProvider.ProviderId),
                options
            );
        });
        return services;
    }
}
