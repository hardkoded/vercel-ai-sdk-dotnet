// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.RevAI;

/// <summary>Audio submitted to Rev.ai asynchronous transcription.</summary>
public sealed class RevAITranscriptionRequest
{
    /// <summary>Creates a transcription request.</summary>
    public RevAITranscriptionRequest(byte[] audio, string mediaType)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        MediaType = string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType;
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>IANA media type of <see cref="Audio"/>.</summary>
    public string MediaType { get; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>One timed piece of a Rev.ai transcript.</summary>
public sealed class RevAITranscriptSegment
{
    /// <summary>Creates a segment.</summary>
    public RevAITranscriptSegment(string text, double startSecond, double endSecond)
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

/// <summary>Rev.ai transcription result, including the transcript response metadata.</summary>
public sealed class RevAITranscriptionResult
{
    /// <summary>Creates a result.</summary>
    public RevAITranscriptionResult(
        string text,
        string modelId,
        DateTimeOffset timestamp,
        IReadOnlyList<RevAITranscriptSegment> segments,
        string? language,
        double durationSeconds,
        IReadOnlyDictionary<string, string> responseHeaders,
        string responseBody)
    {
        Text = text ?? string.Empty;
        ModelId = modelId ?? string.Empty;
        Timestamp = timestamp;
        Segments = segments ?? Array.Empty<RevAITranscriptSegment>();
        Language = language;
        DurationSeconds = durationSeconds;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        ResponseBody = responseBody ?? string.Empty;
    }

    /// <summary>Joined transcript text.</summary>
    public string Text { get; }

    /// <summary>Model id sent as <c>transcriber</c>.</summary>
    public string ModelId { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Timed segments derived from monologue elements.</summary>
    public IReadOnlyList<RevAITranscriptSegment> Segments { get; }

    /// <summary>Language reported on the submitted job.</summary>
    public string? Language { get; }

    /// <summary>Duration in seconds, from the latest word end time.</summary>
    public double DurationSeconds { get; }

    /// <summary>Headers from the transcript response.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Raw transcript JSON.</summary>
    public string ResponseBody { get; }
}

/// <summary>
/// Rev.ai asynchronous transcription. Posts multipart audio to
/// <c>/speechtotext/v1/jobs</c>, polls the job, then reads the transcript.
/// </summary>
public sealed class RevAITranscriptionModel : ITranscriptionModel
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a transcription model.</summary>
    public RevAITranscriptionModel(HttpClient http, string modelId, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? string.Empty;
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? RevAIProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string Provider { get; set; } = "revai.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Clock used for <see cref="RevAITranscriptionResult.Timestamp"/>. Defaults to UTC now.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <summary>Delay between job status polls. Zero skips the wait.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var result = await TranscribeAsync(new RevAITranscriptionRequest(audio.Data, audio.MediaType), cancellationToken).ConfigureAwait(false);
        AudioUsage? usage = result.DurationSeconds > 0 ? new AudioUsage(seconds: result.DurationSeconds) : null;
        return new TranscriptionResult(result.Text, usage);
    }

    /// <summary>Submits audio and returns the transcript plus response metadata.</summary>
    public async Task<RevAITranscriptionResult> TranscribeAsync(RevAITranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var timestamp = Clock();
        var headers = Merge(request.Headers);
        var submission = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            Url("/speechtotext/v1/jobs"),
            Form(request),
            headers,
            cancellationToken).ConfigureAwait(false);
        EnsureSuccess(submission);

        string? jobId = null;
        string? status = null;
        string? language = null;
        using (var submitted = JsonDocument.Parse(string.IsNullOrWhiteSpace(submission.Text) ? "{}" : submission.Text))
        {
            var root = submitted.RootElement;
            jobId = StringOf(root, "id");
            status = StringOf(root, "status");
            language = StringOf(root, "language");
        }

        if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            throw new AiSdkException("Transcription job failed.");
        }

        var started = DateTime.UtcNow;
        while (string.Equals(status, "in_progress", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(jobId))
        {
            if (DateTime.UtcNow - started > TimeSpan.FromSeconds(60))
            {
                throw new AiSdkException("Transcription job polling timed out.");
            }

            if (PollInterval > TimeSpan.Zero)
            {
                await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
            }

            var polled = await MediaExchange.SendAsync(
                _http,
                HttpMethod.Get,
                Url("/speechtotext/v1/jobs/" + jobId),
                null,
                headers,
                cancellationToken).ConfigureAwait(false);
            EnsureSuccess(polled);
            using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(polled.Text) ? "{}" : polled.Text))
            {
                status = StringOf(document.RootElement, "status");
                var polledLanguage = StringOf(document.RootElement, "language");
                if (!string.IsNullOrEmpty(polledLanguage))
                {
                    language = polledLanguage;
                }
            }

            if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
            {
                throw new AiSdkException("Transcription job failed.");
            }
        }

        if (string.IsNullOrEmpty(jobId))
        {
            return new RevAITranscriptionResult(string.Empty, ModelId, timestamp, Array.Empty<RevAITranscriptSegment>(), language, 0, submission.Headers, submission.Text);
        }

        var transcript = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Get,
            Url("/speechtotext/v1/jobs/" + jobId + "/transcript"),
            null,
            headers,
            cancellationToken).ConfigureAwait(false);
        EnsureSuccess(transcript);
        var parsed = ParseTranscript(transcript.Text);
        return new RevAITranscriptionResult(
            parsed.Text,
            ModelId,
            timestamp,
            parsed.Segments,
            language,
            parsed.DurationSeconds,
            transcript.Headers,
            transcript.Text);
    }

    private HttpContent Form(RevAITranscriptionRequest request)
    {
        var form = new MultipartFormDataContent();
        var audio = new ByteArrayContent(request.Audio);
        audio.Headers.ContentType = new MediaTypeHeaderValue(request.MediaType);
        form.Add(audio, "media", "audio." + Extension(request.MediaType));
        QuoteDisposition(audio, "media", "audio." + Extension(request.MediaType));
        var config = new JsonObjectWriter();
        config.Add("transcriber", ModelId);
        var configContent = new ByteArrayContent(Encoding.UTF8.GetBytes(config.ToJson()));
        configContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(configContent, "config");
        QuoteDisposition(configContent, "config", null);
        return form;
    }

    private static void QuoteDisposition(HttpContent content, string name, string? fileName)
    {
        content.Headers.Remove("Content-Disposition");
        var value = fileName == null
            ? "form-data; name=\"" + name + "\""
            : "form-data; name=\"" + name + "\"; filename=\"" + fileName + "\"";
        content.Headers.TryAddWithoutValidation("Content-Disposition", value);
    }

    private Dictionary<string, string?> Merge(IDictionary<string, string>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    private Uri Url(string path)
    {
        return ApiKeys.Combine(_baseUrl, path);
    }

    private static void EnsureSuccess(MediaResponse response)
    {
        if (response.StatusCode >= 200 && response.StatusCode <= 299)
        {
            return;
        }

        throw ProviderHttp.MapStatus(response.StatusCode, response.Text);
    }

    private static ParsedTranscript ParseTranscript(string json)
    {
        var segments = new List<RevAITranscriptSegment>();
        var monologues = new List<string>();
        double duration = 0;
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ParsedTranscript(string.Empty, segments, duration);
        }

        using (var document = JsonDocument.Parse(json))
        {
            if (!document.RootElement.TryGetProperty("monologues", out var list) || list.ValueKind != JsonValueKind.Array)
            {
                return new ParsedTranscript(string.Empty, segments, duration);
            }

            foreach (var monologue in list.EnumerateArray())
            {
                if (!monologue.TryGetProperty("elements", out var elements) || elements.ValueKind != JsonValueKind.Array)
                {
                    monologues.Add(string.Empty);
                    continue;
                }

                var text = new StringBuilder();
                var current = new StringBuilder();
                double segmentStart = 0;
                var started = false;
                foreach (var element in elements.EnumerateArray())
                {
                    var value = StringOf(element, "value") ?? string.Empty;
                    text.Append(value);
                    current.Append(value);
                    var type = StringOf(element, "type");
                    if (!string.Equals(type, "text", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    double? end = NumberOf(element, "end_ts");
                    double? start = NumberOf(element, "ts");
                    if (end is double endSeconds && endSeconds > duration)
                    {
                        duration = endSeconds;
                    }

                    if (!started && start is double startSeconds)
                    {
                        segmentStart = startSeconds;
                        started = true;
                    }

                    if (end is double completed && started)
                    {
                        var segmentText = current.ToString().Trim();
                        if (segmentText.Length > 0)
                        {
                            segments.Add(new RevAITranscriptSegment(segmentText, segmentStart, completed));
                        }

                        current.Clear();
                        started = false;
                    }
                }

                if (started)
                {
                    var remaining = current.ToString().Trim();
                    if (remaining.Length > 0)
                    {
                        var endSecond = duration > segmentStart ? duration : segmentStart + 1;
                        segments.Add(new RevAITranscriptSegment(remaining, segmentStart, endSecond));
                    }
                }

                monologues.Add(text.ToString());
            }
        }

        return new ParsedTranscript(string.Join(" ", monologues), segments, duration);
    }

    private static string? StringOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }

    private static double? NumberOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.GetDouble();
    }

    private static string Extension(string mediaType)
    {
        var slash = mediaType.IndexOf('/');
        var subtype = slash >= 0 ? mediaType.Substring(slash + 1) : mediaType;
        var semicolon = subtype.IndexOf(';');
        if (semicolon >= 0)
        {
            subtype = subtype.Substring(0, semicolon);
        }

        if (string.Equals(subtype, "mpeg", StringComparison.OrdinalIgnoreCase) || string.Equals(subtype, "mp3", StringComparison.OrdinalIgnoreCase))
        {
            return "mp3";
        }

        if (string.Equals(subtype, "x-wav", StringComparison.OrdinalIgnoreCase))
        {
            return "wav";
        }

        return subtype;
    }

    private sealed class ParsedTranscript
    {
        public ParsedTranscript(string text, List<RevAITranscriptSegment> segments, double durationSeconds)
        {
            Text = text;
            Segments = segments;
            DurationSeconds = durationSeconds;
        }

        public string Text { get; }

        public List<RevAITranscriptSegment> Segments { get; }

        public double DurationSeconds { get; }
    }

    /// <summary>Writes a flat JSON object without a dependency on insertion helpers.</summary>
    private sealed class JsonObjectWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private bool _started;

        public void Add(string name, string? value)
        {
            if (value == null)
            {
                return;
            }

            if (!_started)
            {
                _builder.Append('{');
                _started = true;
            }
            else
            {
                _builder.Append(',');
            }

            _builder.Append('"').Append(name).Append('"').Append(':');
            _builder.Append(JsonSerializer.Serialize(value));
        }

        public string ToJson()
        {
            if (!_started)
            {
                return "{}";
            }

            _builder.Append('}');
            return _builder.ToString();
        }
    }
}
