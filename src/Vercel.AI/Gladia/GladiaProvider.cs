// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gladia;

/// <summary>Gladia settings.</summary>
public class GladiaOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Wait between result polls. Defaults to one second.</summary>
    public Func<CancellationToken, Task>? PollDelay { get; set; }
}

/// <summary>Parsed Gladia error payload.</summary>
public sealed class GladiaErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public GladiaErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message.</summary>
    public string Message { get; }

    /// <summary>Provider code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing a Gladia error body.</summary>
public sealed class GladiaErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public GladiaErrorParseResult(bool success, GladiaErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when the error object was present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public GladiaErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public GladiaErrorData? RawValue { get; }
}

/// <summary>Parses Gladia error JSON.</summary>
public static class GladiaError
{
    /// <summary>Parses a resource error body.</summary>
    public static GladiaErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new GladiaErrorParseResult(true, new GladiaErrorData(message, code));
        }

        return new GladiaErrorParseResult(false, null);
    }
}

/// <summary>Headers and extra pre-recorded fields for one Gladia transcription.</summary>
public sealed class GladiaTranscriptionRequest
{
    /// <summary>Headers merged over the provider headers.</summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>Extra JSON fields sent with <c>audio_url</c>.</summary>
    public JsonObject? Body { get; set; }
}

/// <summary>One Gladia utterance mapped to a segment.</summary>
public sealed class GladiaTranscriptionSegment
{
    /// <summary>Creates a segment.</summary>
    public GladiaTranscriptionSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Utterance text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>Gladia response metadata.</summary>
public sealed class GladiaTranscriptionResponse
{
    /// <summary>Creates response metadata.</summary>
    public GladiaTranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id. Gladia pre-recorded calls use <c>default</c>.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the result response.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}

/// <summary>Gladia transcription, including utterance metadata.</summary>
public sealed class GladiaTranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public GladiaTranscriptionResult(
        string text,
        IReadOnlyList<GladiaTranscriptionSegment> segments,
        string? language,
        double? durationInSeconds,
        JsonElement providerMetadata,
        IReadOnlyList<string> warnings,
        GladiaTranscriptionResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<GladiaTranscriptionSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        ProviderMetadata = providerMetadata;
        Warnings = warnings ?? Array.Empty<string>();
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Utterances.</summary>
    public IReadOnlyList<GladiaTranscriptionSegment> Segments { get; }

    /// <summary>First detected language.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Raw Gladia result object.</summary>
    public JsonElement ProviderMetadata { get; }

    /// <summary>Warnings produced while building the request.</summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>Response metadata.</summary>
    public GladiaTranscriptionResponse Response { get; }
}

/// <summary>Gladia pre-recorded transcription provider.</summary>
public sealed class GladiaProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "gladia";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.gladia.io";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public GladiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new GladiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new GladiaProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Gladia does not provide language models.");
    }

    /// <summary>Creates the pre-recorded transcription model.</summary>
    public GladiaTranscriptionModel Transcription()
    {
        return new GladiaTranscriptionModel(this, "default");
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new GladiaTranscriptionModel(this, string.IsNullOrEmpty(modelId) ? "default" : modelId);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "GLADIA_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.CustomHeader;
        options.ApiKeyHeaderName = "x-gladia-key";
        options.UserAgent = ProviderExchange.UserAgent("gladia");
        return options;
    }

    /// <summary>Uploads audio, starts a pre-recorded job, and reads the result.</summary>
    public sealed class GladiaTranscriptionModel : ITranscriptionModel
    {
        private readonly GladiaProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public GladiaTranscriptionModel(GladiaProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = string.IsNullOrEmpty(modelId) ? "default" : modelId;
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "gladia.transcription"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Transcribes audio through upload, pre-recorded init, and result polling.</summary>
        public async Task<GladiaTranscriptionResult> TranscribeAsync(AudioInput audio, GladiaTranscriptionRequest? request, CancellationToken cancellationToken)
        {
            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio));
            }

            var options = _provider.Options as GladiaOptions;
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = Headers(request, includeKey: true);
            var form = new MultipartFormDataContent();
            var media = new ByteArrayContent(audio.Data);
            if (!string.IsNullOrEmpty(audio.MediaType))
            {
                media.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
            }

            form.Add(media, "audio", "audio." + Extension(audio.MediaType));
            var upload = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/upload"),
                form,
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var uploaded = JsonDocument.Parse(string.IsNullOrWhiteSpace(upload.Body) ? "{}" : upload.Body))
            {
                var audioUrl = ReadString(uploaded.RootElement, "audio_url");
                if (string.IsNullOrEmpty(audioUrl))
                {
                    var text = ReadString(uploaded.RootElement, "text") ?? string.Empty;
                    return new GladiaTranscriptionResult(
                        text,
                        Array.Empty<GladiaTranscriptionSegment>(),
                        null,
                        null,
                        uploaded.RootElement.Clone(),
                        Array.Empty<string>(),
                        new GladiaTranscriptionResponse(started, ModelId, upload.Headers));
                }

                var body = request?.Body == null ? new JsonObject() : JsonNode.Parse(request.Body.ToJsonString()) as JsonObject ?? new JsonObject();
                body["audio_url"] = audioUrl;
                var initiated = await ProviderExchange.SendAsync(
                    _provider._httpClient,
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "/v2/pre-recorded"),
                    ProviderExchange.Json(body.ToJsonString()),
                    headers,
                    cancellationToken).ConfigureAwait(false);
                using (var initDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(initiated.Body) ? "{}" : initiated.Body))
                {
                    var resultUrl = ReadString(initDocument.RootElement, "result_url");
                    if (string.IsNullOrEmpty(resultUrl))
                    {
                        throw new AiSdkException("Gladia did not return a result URL.");
                    }

                    var result = await PollAsync(resultUrl!, headers, cancellationToken).ConfigureAwait(false);
                    return Parse(result.Body, result.Headers, started);
                }
            }
        }

        private GladiaTranscriptionResult Parse(string body, IReadOnlyDictionary<string, string> headers, DateTimeOffset started)
        {
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body))
            {
                var root = document.RootElement;
                var text = string.Empty;
                string? language = null;
                double? duration = null;
                var segments = new List<GladiaTranscriptionSegment>();
                if (root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object)
                {
                    if (result.TryGetProperty("metadata", out var metadata)
                        && metadata.TryGetProperty("audio_duration", out var audioDuration)
                        && audioDuration.TryGetDouble(out var seconds))
                    {
                        duration = seconds;
                    }

                    if (result.TryGetProperty("transcription", out var transcription) && transcription.ValueKind == JsonValueKind.Object)
                    {
                        text = ReadString(transcription, "full_transcript") ?? string.Empty;
                        if (transcription.TryGetProperty("languages", out var languages)
                            && languages.ValueKind == JsonValueKind.Array
                            && languages.GetArrayLength() > 0
                            && languages[0].ValueKind == JsonValueKind.String)
                        {
                            language = languages[0].GetString();
                        }

                        if (transcription.TryGetProperty("utterances", out var utterances) && utterances.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var utterance in utterances.EnumerateArray())
                            {
                                var start = utterance.TryGetProperty("start", out var startValue) && startValue.TryGetDouble(out var startSecond) ? startSecond : 0;
                                var end = utterance.TryGetProperty("end", out var endValue) && endValue.TryGetDouble(out var endSecond) ? endSecond : start;
                                segments.Add(new GladiaTranscriptionSegment(ReadString(utterance, "text") ?? string.Empty, start, end));
                            }
                        }
                    }
                }

                using (var wrapped = JsonDocument.Parse("{\"gladia\":" + root.GetRawText() + "}"))
                {
                    return new GladiaTranscriptionResult(
                        text,
                        segments,
                        language,
                        duration,
                        wrapped.RootElement.Clone(),
                        Array.Empty<string>(),
                        new GladiaTranscriptionResponse(started, ModelId, headers));
                }
            }
        }

        private async Task<ProviderExchangeResult> PollAsync(string resultUrl, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
        {
            var options = _provider.Options as GladiaOptions;
            var origin = new Uri(_provider.Options.BaseUrl, UriKind.Absolute);
            var target = new Uri(resultUrl, UriKind.Absolute);
            IReadOnlyDictionary<string, string?> callHeaders = headers;
            if (!string.Equals(origin.Host, target.Host, StringComparison.OrdinalIgnoreCase))
            {
                var stripped = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in headers)
                {
                    if (pair.Key.Equals("x-gladia-key", StringComparison.OrdinalIgnoreCase)
                        || pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    stripped[pair.Key] = pair.Value;
                }

                callHeaders = stripped;
            }

            var started = DateTime.UtcNow;
            while (true)
            {
                if (DateTime.UtcNow - started > TimeSpan.FromSeconds(60))
                {
                    throw new AiSdkException("Transcription job polling timed out.");
                }

                var response = await ProviderExchange.SendAsync(
                    _provider._httpClient,
                    HttpMethod.Get,
                    target,
                    null,
                    callHeaders,
                    cancellationToken).ConfigureAwait(false);
                using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
                {
                    var status = ReadString(document.RootElement, "status");
                    if (string.Equals(status, "error", StringComparison.Ordinal))
                    {
                        throw new AiSdkException("Transcription job failed.");
                    }

                    if (string.Equals(status, "done", StringComparison.Ordinal) || string.IsNullOrEmpty(status))
                    {
                        return response;
                    }
                }

                if (options?.PollDelay != null)
                {
                    await options.PollDelay(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private Dictionary<string, string?> Headers(GladiaTranscriptionRequest? request, bool includeKey)
        {
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers);
            if (!includeKey)
            {
                headers.Remove("x-gladia-key");
            }

            ProviderExchange.AddUserAgent(headers, "gladia");
            return headers;
        }
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static string Extension(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return "bin";
        }

        var value = mediaType!;
        if (value.IndexOf("wav", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "wav";
        }

        if (value.IndexOf("mpeg", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("mp3", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "mp3";
        }

        return "bin";
    }
}

/// <summary>Registers Gladia.</summary>
public static class GladiaServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddGladia(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(GladiaProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new GladiaProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(GladiaProvider.ProviderId), options);
        });
        return services;
    }
}
