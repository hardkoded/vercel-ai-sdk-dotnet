// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.RevAI;

/// <summary>Rev.ai settings.</summary>
public class RevAIOptions : OpenAICompatibleOptions
{
    /// <summary>Clock used for response timestamps. Defaults to UTC now.</summary>
    public Func<DateTimeOffset>? Clock { get; set; }

    /// <summary>Wait between job status polls. Defaults to one second.</summary>
    public Func<CancellationToken, Task>? PollDelay { get; set; }
}

/// <summary>Parsed Rev.ai <c>error</c> object.</summary>
public sealed class RevAIErrorData
{
    /// <summary>Creates parsed error data.</summary>
    public RevAIErrorData(string message, int code)
    {
        Message = message ?? string.Empty;
        Code = code;
    }

    /// <summary>Provider message. Nested JSON is left as text.</summary>
    public string Message { get; }

    /// <summary>Provider error code.</summary>
    public int Code { get; }
}

/// <summary>Result of parsing a Rev.ai error body.</summary>
public sealed class RevAIErrorParseResult
{
    /// <summary>Creates a parse result.</summary>
    public RevAIErrorParseResult(bool success, RevAIErrorData? value)
    {
        Success = success;
        Value = value;
        RawValue = value;
    }

    /// <summary>True when <c>error.message</c> and <c>error.code</c> were present.</summary>
    public bool Success { get; }

    /// <summary>Parsed error.</summary>
    public RevAIErrorData? Value { get; }

    /// <summary>Same payload as <see cref="Value"/>.</summary>
    public RevAIErrorData? RawValue { get; }
}

/// <summary>Parses Rev.ai error JSON.</summary>
public static class RevAIError
{
    /// <summary>Parses a resource error body.</summary>
    public static RevAIErrorParseResult Parse(string json)
    {
        if (ProviderExchange.TryParseError(json, out var message, out var code))
        {
            return new RevAIErrorParseResult(true, new RevAIErrorData(message, code));
        }

        return new RevAIErrorParseResult(false, null);
    }
}

/// <summary>Extra fields and headers for one Rev.ai transcription.</summary>
public sealed class RevAITranscriptionRequest
{
    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>Additional JSON merged into the multipart <c>config</c> object.</summary>
    public JsonObject? Config { get; set; }
}

/// <summary>One timed transcript fragment.</summary>
public sealed class RevAITranscriptionSegment
{
    /// <summary>Creates a segment.</summary>
    public RevAITranscriptionSegment(string text, double startSecond, double endSecond)
    {
        Text = text ?? string.Empty;
        StartSecond = startSecond;
        EndSecond = endSecond;
    }

    /// <summary>Segment text.</summary>
    public string Text { get; }

    /// <summary>Start time in seconds.</summary>
    public double StartSecond { get; }

    /// <summary>End time in seconds.</summary>
    public double EndSecond { get; }
}

/// <summary>Metadata returned with a transcription.</summary>
public sealed class RevAITranscriptionResponse
{
    /// <summary>Creates response metadata.</summary>
    public RevAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
    {
        Timestamp = timestamp;
        ModelId = modelId ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>();
        Body = body ?? string.Empty;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id sent as <c>transcriber</c>.</summary>
    public string ModelId { get; }

    /// <summary>Headers from the transcript response.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw transcript JSON.</summary>
    public string Body { get; }
}

/// <summary>Rev.ai transcription, including segments and response metadata.</summary>
public sealed class RevAITranscriptionResult
{
    /// <summary>Creates a transcription result.</summary>
    public RevAITranscriptionResult(
        string text,
        IReadOnlyList<RevAITranscriptionSegment> segments,
        string? language,
        double durationInSeconds,
        RevAITranscriptionResponse response)
    {
        Text = text ?? string.Empty;
        Segments = segments ?? Array.Empty<RevAITranscriptionSegment>();
        Language = language;
        DurationInSeconds = durationInSeconds;
        Response = response;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Timed fragments.</summary>
    public IReadOnlyList<RevAITranscriptionSegment> Segments { get; }

    /// <summary>Language reported on the job, when present.</summary>
    public string? Language { get; }

    /// <summary>Latest word end time, in seconds.</summary>
    public double DurationInSeconds { get; }

    /// <summary>Response metadata.</summary>
    public RevAITranscriptionResponse Response { get; }
}

/// <summary>Rev.ai speech-to-text provider.</summary>
public sealed class RevAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "revai";

    /// <summary>Default API origin.</summary>
    public const string DefaultBaseUrl = "https://api.rev.ai";

    private readonly HttpClient _httpClient;

    /// <summary>Creates a provider.</summary>
    public RevAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
        : base(Prepare(options), httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Creates a provider.</summary>
    public static new RevAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new RevAIProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        throw new AiSdkException("Rev.ai does not provide language models.");
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new RevAITranscriptionModel(this, modelId);
    }

    private static OpenAICompatibleOptions Prepare(OpenAICompatibleOptions? options)
    {
        options ??= new OpenAICompatibleOptions();
        options.ProviderName = ProviderId;
        options.BaseUrl = string.IsNullOrEmpty(options.BaseUrl) || options.BaseUrl == "https://api.openai.com/v1" ? DefaultBaseUrl : options.BaseUrl;
        options.ApiKeyEnvironmentVariable = "REVAI_API_KEY";
        options.SupportsEmbeddings = false;
        options.SupportsImages = false;
        options.ApiKeyStyle = ApiKeyStyle.Bearer;
        options.UserAgent = ProviderExchange.UserAgent("revai");
        return options;
    }

    /// <summary>Rev.ai asynchronous transcription model.</summary>
    public sealed class RevAITranscriptionModel : ITranscriptionModel
    {
        private readonly RevAIProvider _provider;

        /// <summary>Creates a transcription model.</summary>
        public RevAITranscriptionModel(RevAIProvider provider, string modelId)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ModelId = modelId ?? string.Empty;
        }

        /// <inheritdoc />
        public string Provider
        {
            get { return "revai.transcription"; }
        }

        /// <inheritdoc />
        public string ModelId { get; }

        /// <inheritdoc />
        public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
        {
            var result = await TranscribeAsync(audio, null, cancellationToken).ConfigureAwait(false);
            return new TranscriptionResult(result.Text);
        }

        /// <summary>Submits a job, polls until it is transcribed, and reads the transcript.</summary>
        public async Task<RevAITranscriptionResult> TranscribeAsync(AudioInput audio, RevAITranscriptionRequest? request, CancellationToken cancellationToken)
        {
            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio));
            }

            var options = _provider.Options as RevAIOptions;
            var started = options?.Clock != null ? options.Clock() : DateTimeOffset.UtcNow;
            var headers = Headers(request);
            var config = new JsonObject { ["transcriber"] = ModelId };
            if (request?.Config != null)
            {
                foreach (var pair in request.Config)
                {
                    config[pair.Key] = pair.Value == null ? null : JsonNode.Parse(pair.Value.ToJsonString());
                }
            }

            var form = new MultipartFormDataContent();
            var media = new ByteArrayContent(audio.Data);
            if (!string.IsNullOrEmpty(audio.MediaType))
            {
                media.Headers.ContentType = MediaTypeHeaderValue.Parse(audio.MediaType);
            }

            form.Add(media, "media", "audio." + Extension(audio.MediaType));
            form.Add(new StringContent(config.ToJsonString(), Encoding.UTF8), "config");

            var submission = await ProviderExchange.SendAsync(
                _provider._httpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "/speechtotext/v1/jobs"),
                form,
                headers,
                cancellationToken).ConfigureAwait(false);
            using (var submitted = JsonDocument.Parse(string.IsNullOrWhiteSpace(submission.Body) ? "{}" : submission.Body))
            {
                var job = submitted.RootElement;
                var status = ReadString(job, "status");
                if (string.Equals(status, "failed", StringComparison.Ordinal))
                {
                    throw new AiSdkException("Transcription job failed.");
                }

                var language = ReadString(job, "language");
                var transcriptBody = submission.Body;
                var transcriptHeaders = submission.Headers;
                if (!HasMonologues(job))
                {
                    var jobId = ReadString(job, "id");
                    if (!string.IsNullOrEmpty(jobId))
                    {
                        if (string.Equals(status, "in_progress", StringComparison.Ordinal))
                        {
                            job = await PollAsync(jobId, headers, cancellationToken).ConfigureAwait(false);
                            language = ReadString(job, "language") ?? language;
                            status = ReadString(job, "status");
                            if (string.Equals(status, "failed", StringComparison.Ordinal))
                            {
                                throw new AiSdkException("Transcription job failed.");
                            }
                        }

                        var transcript = await ProviderExchange.SendAsync(
                            _provider._httpClient,
                            HttpMethod.Get,
                            ApiKeys.Combine(_provider.Options.BaseUrl, "/speechtotext/v1/jobs/" + jobId + "/transcript"),
                            null,
                            headers,
                            cancellationToken).ConfigureAwait(false);
                        transcriptBody = transcript.Body;
                        transcriptHeaders = transcript.Headers;
                    }
                }

                using (var transcriptDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(transcriptBody) ? "{}" : transcriptBody))
                {
                    var parsed = ReadTranscript(transcriptDocument.RootElement);
                    return new RevAITranscriptionResult(
                        parsed.Text,
                        parsed.Segments,
                        language,
                        parsed.Duration,
                        new RevAITranscriptionResponse(started, ModelId, transcriptHeaders, transcriptBody));
                }
            }
        }

        private async Task<JsonElement> PollAsync(string jobId, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
        {
            var options = _provider.Options as RevAIOptions;
            var started = DateTime.UtcNow;
            while (true)
            {
                if (DateTime.UtcNow - started > TimeSpan.FromSeconds(60))
                {
                    throw new AiSdkException("Transcription job polling timed out.");
                }

                var statusResponse = await ProviderExchange.SendAsync(
                    _provider._httpClient,
                    HttpMethod.Get,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "/speechtotext/v1/jobs/" + jobId),
                    null,
                    headers,
                    cancellationToken).ConfigureAwait(false);
                using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(statusResponse.Body) ? "{}" : statusResponse.Body))
                {
                    var status = ReadString(document.RootElement, "status");
                    if (string.Equals(status, "transcribed", StringComparison.Ordinal) || string.Equals(status, "failed", StringComparison.Ordinal))
                    {
                        return document.RootElement.Clone();
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

        private Dictionary<string, string?> Headers(RevAITranscriptionRequest? request)
        {
            var headers = ProviderExchange.Merge(_provider.CreateHeaders(), request?.Headers);
            ProviderExchange.AddUserAgent(headers, "revai");
            return headers;
        }
    }

    private static bool HasMonologues(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("monologues", out var monologues)
            && monologues.ValueKind == JsonValueKind.Array;
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

    private static TranscriptRead ReadTranscript(JsonElement root)
    {
        var text = new StringBuilder();
        var segments = new List<RevAITranscriptionSegment>();
        var duration = 0d;
        if (!root.TryGetProperty("monologues", out var monologues) || monologues.ValueKind != JsonValueKind.Array)
        {
            if (root.TryGetProperty("text", out var plain) && plain.ValueKind == JsonValueKind.String)
            {
                return new TranscriptRead(plain.GetString() ?? string.Empty, segments, duration);
            }

            return new TranscriptRead(string.Empty, segments, duration);
        }

        var first = true;
        foreach (var monologue in monologues.EnumerateArray())
        {
            if (!first)
            {
                text.Append(' ');
            }

            first = false;
            var monologueText = new StringBuilder();
            if (!monologue.TryGetProperty("elements", out var elements) || elements.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var current = new StringBuilder();
            var segmentStart = 0d;
            var startedSegment = false;
            foreach (var element in elements.EnumerateArray())
            {
                var value = element.TryGetProperty("value", out var raw) && raw.ValueKind == JsonValueKind.String
                    ? raw.GetString() ?? string.Empty
                    : string.Empty;
                monologueText.Append(value);
                current.Append(value);
                var type = element.TryGetProperty("type", out var typeValue) ? typeValue.GetString() : null;
                if (!string.Equals(type, "text", StringComparison.Ordinal))
                {
                    continue;
                }

                if (element.TryGetProperty("end_ts", out var endValue) && endValue.TryGetDouble(out var end) && end > duration)
                {
                    duration = end;
                }

                if (!startedSegment && element.TryGetProperty("ts", out var startValue) && startValue.TryGetDouble(out var start))
                {
                    segmentStart = start;
                    startedSegment = true;
                }

                if (startedSegment && element.TryGetProperty("end_ts", out var endTs) && endTs.TryGetDouble(out var segmentEnd))
                {
                    var segmentText = current.ToString().Trim();
                    if (segmentText.Length > 0)
                    {
                        segments.Add(new RevAITranscriptionSegment(segmentText, segmentStart, segmentEnd));
                    }

                    current.Clear();
                    startedSegment = false;
                }
            }

            if (startedSegment && current.ToString().Trim().Length > 0)
            {
                var endSecond = duration > segmentStart ? duration : segmentStart + 1;
                segments.Add(new RevAITranscriptionSegment(current.ToString().Trim(), segmentStart, endSecond));
            }

            text.Append(monologueText);
        }

        return new TranscriptRead(text.ToString(), segments, duration);
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

        if (value.IndexOf("flac", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "flac";
        }

        if (value.IndexOf("ogg", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "ogg";
        }

        if (value.IndexOf("webm", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "webm";
        }

        if (value.IndexOf("mp4", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "mp4";
        }

        return "bin";
    }

    private sealed class TranscriptRead
    {
        public TranscriptRead(string text, List<RevAITranscriptionSegment> segments, double duration)
        {
            Text = text;
            Segments = segments;
            Duration = duration;
        }

        public string Text { get; }

        public List<RevAITranscriptionSegment> Segments { get; }

        public double Duration { get; }
    }
}

/// <summary>Registers Rev.ai.</summary>
public static class RevAIServiceCollectionExtensions
{
    /// <summary>Adds the provider.</summary>
    public static IServiceCollection AddRevAI(this IServiceCollection services, Action<OpenAICompatibleOptions>? configure = null)
    {
        services.AddHttpClient(RevAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAICompatibleOptions();
            configure?.Invoke(options);
            return new RevAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(RevAIProvider.ProviderId), options);
        });
        return services;
    }
}
