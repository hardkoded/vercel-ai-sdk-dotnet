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

namespace Vercel.AI.ElevenLabs;

/// <summary>ElevenLabs settings.</summary>
public class ElevenLabsOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }
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
        IReadOnlyDictionary<string, JsonElement> additionalProperties)
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
        IReadOnlyDictionary<string, JsonElement> additionalProperties)
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
        : this(null, null, null, new ElevenLabsLegacyError(message ?? string.Empty, code))
    {
    }

    private ElevenLabsErrorData(
        ElevenLabsErrorDetail? detail,
        IReadOnlyList<ElevenLabsValidationIssue>? issues,
        string? detailMessage,
        ElevenLabsLegacyError? error)
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

    internal static ElevenLabsErrorData FromDetail(ElevenLabsErrorDetail detail) => new ElevenLabsErrorData(detail, null, null, null);

    internal static ElevenLabsErrorData FromIssues(IReadOnlyList<ElevenLabsValidationIssue> issues) => new ElevenLabsErrorData(null, issues, null, null);

    internal static ElevenLabsErrorData FromMessage(string message) => new ElevenLabsErrorData(null, null, message, null);

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

            if (root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && RequiredString(error, "message") is { } message
                && error.TryGetProperty("code", out var code)
                && code.ValueKind == JsonValueKind.Number
                && code.TryGetDouble(out var codeValue))
            {
                return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromLegacy(message, codeValue));
            }
        }
        catch (JsonException)
        {
        }

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
            return string.Join("; ", issues.Select(issue =>
                issue.Location is { Count: > 0 } location
                    ? string.Join(".", location) + ": " + issue.Message
                    : issue.Message));
        }

        var detail = data.Detail!;
        return detail.Code != null ? detail.Message + " (" + detail.Code + ")" : detail.Message;
    }

    private static ElevenLabsErrorParseResult ParseDetail(JsonElement detail)
    {
        if (detail.ValueKind == JsonValueKind.String)
        {
            return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromMessage(detail.GetString()!));
        }

        if (detail.ValueKind == JsonValueKind.Object)
        {
            var message = RequiredString(detail, "message");
            if (message == null
                || !OptionalString(detail, "code", out var code)
                || !OptionalString(detail, "status", out var status)
                || !OptionalString(detail, "type", out var type)
                || !OptionalString(detail, "request_id", out var requestId)
                || !OptionalString(detail, "param", out var parameter))
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
                AdditionalProperties(detail, "message", "code", "status", "type", "request_id", "param"));
            return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromDetail(value));
        }

        if (detail.ValueKind != JsonValueKind.Array)
        {
            return new ElevenLabsErrorParseResult(false, null);
        }

        var issues = new List<ElevenLabsValidationIssue>();
        foreach (var issue in detail.EnumerateArray())
        {
            if (issue.ValueKind != JsonValueKind.Object
                || RequiredString(issue, "msg") is not { } message
                || !OptionalString(issue, "type", out var type)
                || !TryLocation(issue, out var location))
            {
                return new ElevenLabsErrorParseResult(false, null);
            }

            issues.Add(new ElevenLabsValidationIssue(
                message,
                location,
                type,
                AdditionalProperties(issue, "msg", "loc", "type")));
        }

        return new ElevenLabsErrorParseResult(true, ElevenLabsErrorData.FromIssues(issues));
    }

    private static bool TryLocation(JsonElement issue, out IReadOnlyList<ElevenLabsErrorLocation>? location)
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
            if (component.ValueKind != JsonValueKind.String && component.ValueKind != JsonValueKind.Number)
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
        return value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool OptionalString(JsonElement value, string name, out string? result)
    {
        result = null;
        if (!value.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
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

    private static IReadOnlyDictionary<string, JsonElement> AdditionalProperties(JsonElement value, params string[] knownProperties)
    {
        var known = new HashSet<string>(knownProperties, StringComparer.Ordinal);
        return value.EnumerateObject()
            .Where(property => !known.Contains(property.Name))
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);
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
        IReadOnlyDictionary<string, string>? responseHeaders = null)
    {
        var data = ElevenLabsError.Parse(responseBody).Value;
        var message = data == null
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
    public ElevenLabsResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, JsonElement? body)
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
    public ElevenLabsSpeechResult(byte[] audio, string mediaType, IReadOnlyList<ModelWarning> warnings, string requestBody, ElevenLabsResponse response)
    {
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
        ElevenLabsResponse response)
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
        throw new AiSdkException("ElevenLabs does not provide language models.");
    }

    /// <summary>Creates a provider.</summary>
    public static new ElevenLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new ElevenLabsProvider(client, options);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options = options?.Copy() ?? new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "ELEVENLABS_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "xi-api-key";
        options.UserAgent = ProviderExchange.UserAgent("elevenlabs");
        return options;
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId) => new ElevenLabsSpeechModel(this, modelId);

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId) => new ElevenLabsTranscriptionModel(this, modelId);

    private DateTimeOffset Now()
    {
        var clock = (Options as ElevenLabsOptions)?.Clock;
        return clock != null ? clock() : DateTimeOffset.UtcNow;
    }

    private static JsonElement? ElevenLabsObject(JsonElement? providerOptions)
    {
        return providerOptions is { ValueKind: JsonValueKind.Object } bag && bag.TryGetProperty("elevenlabs", out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static JsonElement? Child(JsonElement? parent, string name)
    {
        return parent != null && parent.Value.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;
    }

    private async Task<ProviderExchangeResult> SendAsync(
        Uri uri,
        HttpContent content,
        object requestBodyValues,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ProviderExchange.SendAsync(_httpClient, HttpMethod.Post, uri, content, headers, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception) when (ElevenLabsError.Parse(exception.ResponseBody).Success)
        {
            throw ElevenLabsFailedResponseHandler.Handle(
                exception.ResponseBody!,
                exception.StatusCode,
                uri.ToString(),
                requestBodyValues,
                exception.ResponseHeaders);
        }
    }

    /// <summary>ElevenLabs text-to-speech model.</summary>
    public sealed class ElevenLabsSpeechModel : ISpeechModel
    {
        private static readonly Dictionary<string, string> OutputFormats = new Dictionary<string, string>(StringComparer.Ordinal)
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
        public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
            var result = await GenerateAsync(new ElevenLabsSpeechRequest(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
            return new SpeechResult(result.Audio, result.MediaType);
        }

        /// <summary>Synthesizes speech and returns the audio with warnings and response metadata.</summary>
        public async Task<ElevenLabsSpeechResult> GenerateAsync(ElevenLabsSpeechRequest request, CancellationToken cancellationToken)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var elevenlabs = ElevenLabsObject(request.ProviderOptions);
            var body = new JsonObject { ["text"] = request.Text, ["model_id"] = ModelId };
            var query = new List<string>();
            if (!string.IsNullOrEmpty(request.OutputFormat))
            {
                query.Add("output_format=" + Uri.EscapeDataString(OutputFormats.TryGetValue(request.OutputFormat, out var mapped) ? mapped : request.OutputFormat));
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

            if (body["language_code"] == null && Child(elevenlabs, "languageCode") is { ValueKind: JsonValueKind.String } languageCode && languageCode.GetString()!.Length > 0)
            {
                body["language_code"] = languageCode.GetString();
            }

            if (Child(elevenlabs, "pronunciationDictionaryLocators") is { ValueKind: JsonValueKind.Array } locators)
            {
                var array = new JsonArray();
                foreach (var locator in locators.EnumerateArray())
                {
                    var item = new JsonObject { ["pronunciation_dictionary_id"] = Child(locator, "pronunciationDictionaryId")?.GetString() };
                    if (Child(locator, "versionId") is { ValueKind: JsonValueKind.String } version && version.GetString()!.Length > 0)
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
            CopyRaw(elevenlabs, "applyLanguageTextNormalization", body, "apply_language_text_normalization");
            if (Child(elevenlabs, "enableLogging") is { } enableLogging)
            {
                query.Add("enable_logging=" + (enableLogging.GetBoolean() ? "true" : "false"));
            }

            if (voiceSettings.Count > 0)
            {
                body["voice_settings"] = voiceSettings;
            }

            if (!string.IsNullOrEmpty(request.Instructions))
            {
                warnings.Add(new UnsupportedWarning("instructions", "ElevenLabs speech models do not support instructions. Instructions parameter was ignored."));
            }

            var voice = string.IsNullOrEmpty(request.Voice) ? ElevenLabsSpeechRequest.DefaultVoiceId : request.Voice!;
            var path = "/v1/text-to-speech/" + Uri.EscapeDataString(voice) + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var json = body.ToJsonString();
            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, path);
            var response = await _provider.SendAsync(
                uri,
                ProviderExchange.Json(json),
                body,
                ProviderExchange.Merge(_provider.CreateHeaders(), request.Headers),
                cancellationToken).ConfigureAwait(false);
            var mediaType = "audio/mpeg";
            if (response.Headers.TryGetValue("Content-Type", out var contentType) && contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var separator = contentType.IndexOf(';');
                mediaType = (separator >= 0 ? contentType.Substring(0, separator) : contentType).Trim();
            }

            return new ElevenLabsSpeechResult(response.Bytes, mediaType, warnings, json, new ElevenLabsResponse(timestamp, ModelId, response.Headers, null));
        }

        private static void CopyNumber(JsonElement? source, string name, JsonObject target, string targetName)
        {
            if (Child(source, name) is { } value)
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }

        private static void CopyText(JsonElement? source, string name, JsonObject target, string targetName)
        {
            if (Child(source, name) is { ValueKind: JsonValueKind.String } value && value.GetString()!.Length > 0)
            {
                target[targetName] = value.GetString();
            }
        }

        private static void CopyRaw(JsonElement? source, string name, JsonObject target, string targetName)
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

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Uploads audio to <c>/v1/speech-to-text</c> and reads the transcript.</summary>
        public async Task<ElevenLabsTranscriptionResult> TranscribeAsync(AudioInput audio, ElevenLabsTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            audio = audio ?? throw new ArgumentNullException(nameof(audio));
            if (ModelId == "scribe_v2_realtime")
            {
                throw new Operations.UnsupportedFunctionalityException("non-streaming transcription with " + ModelId, "'non-streaming transcription with " + ModelId + "' functionality not supported.");
            }

            var timestamp = _provider.Now();
            var warnings = new List<ModelWarning>();
            var elevenlabs = ElevenLabsObject(request?.ProviderOptions);
            if (Child(elevenlabs, "streaming") != null)
            {
                warnings.Add(new UnsupportedWarning("providerOptions.elevenlabs.streaming", "ElevenLabs batch transcription does not support streaming options."));
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(ModelId), "model_id");
            var file = new ByteArrayContent(audio.Data);
            if (!string.IsNullOrEmpty(audio.MediaType))
            {
                file.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
            }

            form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(audio.MediaType));
            form.Add(new StringContent((elevenlabs != null ? FormValue(Child(elevenlabs, "diarize")) : null) ?? "true"), "diarize");
            if (elevenlabs != null)
            {
                // Option defaults apply once the elevenlabs object is present, as in the upstream schema.
                AddField(form, "language_code", FormValue(Child(elevenlabs, "languageCode")));
                AddField(form, "tag_audio_events", FormValue(Child(elevenlabs, "tagAudioEvents")) ?? "true");
                AddField(form, "num_speakers", FormValue(Child(elevenlabs, "numSpeakers")));
                AddField(form, "timestamps_granularity", FormValue(Child(elevenlabs, "timestampsGranularity")) ?? "word");
                AddField(form, "file_format", FormValue(Child(elevenlabs, "fileFormat")) ?? "other");
            }

            var uri = ApiKeys.Combine(_provider.Options.BaseUrl, "/v1/speech-to-text");
            var response = await _provider.SendAsync(
                uri,
                form,
                new Dictionary<string, string> { ["model_id"] = ModelId },
                ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers),
                cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(response.Body);
            var root = document.RootElement;
            var segments = new List<TranscriptSegment>();
            double? duration = null;
            if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
            {
                foreach (var word in words.EnumerateArray())
                {
                    var end = Child(word, "end")?.GetDouble();
                    segments.Add(new TranscriptSegment(Child(word, "text")?.GetString() ?? string.Empty, Child(word, "start")?.GetDouble() ?? 0, end ?? 0));
                    duration = end;
                }
            }

            var audioDuration = Child(root, "audio_duration_secs") is { ValueKind: JsonValueKind.Number } audioDurationElement
                ? audioDurationElement.GetDouble()
                : (double?)null;

            return new ElevenLabsTranscriptionResult(
                Child(root, "text")?.GetString() ?? string.Empty,
                segments,
                Child(root, "language_code")?.GetString(),
                audioDuration ?? duration,
                warnings,
                new ElevenLabsResponse(timestamp, ModelId, response.Headers, root.Clone()));
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
                JsonValueKind.Number => value.Value.GetDouble().ToString(CultureInfo.InvariantCulture),
                _ => value.Value.GetRawText(),
            };
        }
    }
}

/// <summary>Registers ElevenLabs.</summary>
public static class ElevenLabsServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddElevenLabs(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(ElevenLabsProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new ElevenLabsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(ElevenLabsProvider.ProviderId), options);
        });
        return services;
    }
}
