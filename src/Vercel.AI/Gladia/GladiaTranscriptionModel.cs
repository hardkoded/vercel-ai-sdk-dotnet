// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gladia;

/// <summary>Audio submitted to Gladia pre-recorded transcription.</summary>
public sealed class GladiaTranscriptionRequest
{
    /// <summary>Creates a transcription request.</summary>
    public GladiaTranscriptionRequest(byte[] audio, string mediaType)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        MediaType = string.IsNullOrEmpty(mediaType) ? "application/octet-stream" : mediaType;
    }

    /// <summary>Audio bytes uploaded to <c>/v2/upload</c>.</summary>
    public byte[] Audio { get; }

    /// <summary>IANA media type of <see cref="Audio"/>.</summary>
    public string MediaType { get; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>One Gladia utterance mapped to a transcript segment.</summary>
public sealed class GladiaTranscriptSegment
{
    /// <summary>Creates a segment.</summary>
    public GladiaTranscriptSegment(string text, double startSecond, double endSecond)
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

/// <summary>Gladia transcription result.</summary>
public sealed class GladiaTranscriptionResult
{
    /// <summary>Creates a result.</summary>
    public GladiaTranscriptionResult(
        string text,
        string modelId,
        DateTimeOffset timestamp,
        IReadOnlyList<GladiaTranscriptSegment> segments,
        string? language,
        double durationSeconds,
        IReadOnlyDictionary<string, string> responseHeaders,
        JsonNode? providerMetadata)
    {
        Text = text ?? string.Empty;
        ModelId = modelId ?? "default";
        Timestamp = timestamp;
        Segments = segments ?? Array.Empty<GladiaTranscriptSegment>();
        Language = language;
        DurationSeconds = durationSeconds;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        ProviderMetadata = providerMetadata;
    }

    /// <summary>Full transcript.</summary>
    public string Text { get; }

    /// <summary>Model id. Gladia pre-recorded calls report <c>default</c>.</summary>
    public string ModelId { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Utterances as timed segments.</summary>
    public IReadOnlyList<GladiaTranscriptSegment> Segments { get; }

    /// <summary>First detected language, when the result lists one.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double DurationSeconds { get; }

    /// <summary>Headers from the completed result response.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Raw result document exposed as Gladia provider metadata.</summary>
    public JsonNode? ProviderMetadata { get; }

    /// <summary>Warnings. Gladia pre-recorded transcription does not emit any for a normal call.</summary>
    public IReadOnlyList<string> Warnings
    {
        get { return Array.Empty<string>(); }
    }
}

/// <summary>
/// Gladia pre-recorded transcription. Uploads audio, starts a job at
/// <c>/v2/pre-recorded</c>, and polls the result URL. Credentials are omitted
/// when that URL is on another origin.
/// </summary>
public sealed class GladiaTranscriptionModel : ITranscriptionModel
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a transcription model.</summary>
    public GladiaTranscriptionModel(HttpClient http, string modelId, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = string.IsNullOrEmpty(modelId) ? "default" : modelId;
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? GladiaProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string Provider { get; set; } = "gladia.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Clock used for <see cref="GladiaTranscriptionResult.Timestamp"/>.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <summary>Delay between result polls. Zero skips the wait.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var result = await TranscribeAsync(new GladiaTranscriptionRequest(audio.Data, audio.MediaType), cancellationToken).ConfigureAwait(false);
        AudioUsage? usage = result.DurationSeconds > 0 ? new AudioUsage(seconds: result.DurationSeconds) : null;
        return new TranscriptionResult(result.Text, usage);
    }

    /// <summary>Uploads audio, starts transcription, and polls until the job is done.</summary>
    public async Task<GladiaTranscriptionResult> TranscribeAsync(GladiaTranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var timestamp = Clock();
        var headers = Merge(request.Headers);
        var upload = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            Url("/v2/upload"),
            Form(request),
            headers,
            cancellationToken).ConfigureAwait(false);
        EnsureSuccess(upload);

        string? audioUrl = null;
        using (var uploaded = JsonDocument.Parse(Blank(upload.Text)))
        {
            audioUrl = StringOf(uploaded.RootElement, "audio_url");
            if (string.IsNullOrEmpty(audioUrl))
            {
                var text = StringOf(uploaded.RootElement, "text") ?? string.Empty;
                return new GladiaTranscriptionResult(text, ModelId, timestamp, Array.Empty<GladiaTranscriptSegment>(), null, 0, upload.Headers, null);
            }
        }

        var initiateBody = new JsonObject { ["audio_url"] = audioUrl };
        var initiated = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            Url("/v2/pre-recorded"),
            MediaExchange.Json(initiateBody.ToJsonString()),
            headers,
            cancellationToken).ConfigureAwait(false);
        EnsureSuccess(initiated);

        string? resultUrl;
        using (var document = JsonDocument.Parse(Blank(initiated.Text)))
        {
            resultUrl = StringOf(document.RootElement, "result_url");
        }

        if (string.IsNullOrEmpty(resultUrl))
        {
            throw new AiSdkException("Gladia transcription did not return a result URL.");
        }

        var pollHeaders = headers;
        if (!SameOrigin(resultUrl))
        {
            pollHeaders = new Dictionary<string, string?>(headers, StringComparer.OrdinalIgnoreCase);
            pollHeaders.Remove("Authorization");
            pollHeaders.Remove("x-gladia-key");
        }

        var started = DateTime.UtcNow;
        while (true)
        {
            if (DateTime.UtcNow - started > TimeSpan.FromSeconds(60))
            {
                throw new AiSdkException("Transcription job polling timed out.");
            }

            var polled = await MediaExchange.SendAsync(
                _http,
                HttpMethod.Get,
                new Uri(resultUrl, UriKind.Absolute),
                null,
                pollHeaders,
                cancellationToken).ConfigureAwait(false);
            EnsureSuccess(polled);
            using (var document = JsonDocument.Parse(Blank(polled.Text)))
            {
                var status = StringOf(document.RootElement, "status");
                if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AiSdkException("Transcription job failed.");
                }

                if (!string.Equals(status, "done", StringComparison.OrdinalIgnoreCase))
                {
                    if (PollInterval > TimeSpan.Zero)
                    {
                        await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
                    }

                    continue;
                }

                if (!document.RootElement.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object)
                {
                    throw new AiSdkException("Transcription result is empty.");
                }

                var text = string.Empty;
                string? language = null;
                double duration = 0;
                var segments = new List<GladiaTranscriptSegment>();
                if (result.TryGetProperty("metadata", out var metadata) && metadata.TryGetProperty("audio_duration", out var durationElement) && durationElement.ValueKind == JsonValueKind.Number)
                {
                    duration = durationElement.GetDouble();
                }

                if (result.TryGetProperty("transcription", out var transcription))
                {
                    text = StringOf(transcription, "full_transcript") ?? string.Empty;
                    if (transcription.TryGetProperty("languages", out var languages) && languages.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in languages.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                language = item.GetString();
                                break;
                            }
                        }
                    }

                    if (transcription.TryGetProperty("utterances", out var utterances) && utterances.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var utterance in utterances.EnumerateArray())
                        {
                            var start = utterance.TryGetProperty("start", out var startElement) && startElement.ValueKind == JsonValueKind.Number ? startElement.GetDouble() : 0;
                            var end = utterance.TryGetProperty("end", out var endElement) && endElement.ValueKind == JsonValueKind.Number ? endElement.GetDouble() : 0;
                            segments.Add(new GladiaTranscriptSegment(StringOf(utterance, "text") ?? string.Empty, start, end));
                        }
                    }
                }

                return new GladiaTranscriptionResult(
                    text,
                    "default",
                    timestamp,
                    segments,
                    language,
                    duration,
                    polled.Headers,
                    JsonNode.Parse(polled.Text));
            }
        }
    }

    private HttpContent Form(GladiaTranscriptionRequest request)
    {
        var form = new MultipartFormDataContent();
        var audio = new ByteArrayContent(request.Audio);
        audio.Headers.ContentType = new MediaTypeHeaderValue(request.MediaType);
        form.Add(audio, "audio", "audio." + Extension(request.MediaType));
        audio.Headers.Remove("Content-Disposition");
        audio.Headers.TryAddWithoutValidation(
            "Content-Disposition",
            "form-data; name=\"audio\"; filename=\"audio." + Extension(request.MediaType) + "\"");
        return form;
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

    private bool SameOrigin(string resultUrl)
    {
        if (!Uri.TryCreate(resultUrl, UriKind.Absolute, out var uri))
        {
            return true;
        }

        var api = new Uri(_baseUrl, UriKind.Absolute);
        return string.Equals(uri.Host, api.Host, StringComparison.OrdinalIgnoreCase);
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

    private static string Blank(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "{}" : text;
    }

    private static string? StringOf(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
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

        if (string.Equals(subtype, "mpeg", StringComparison.OrdinalIgnoreCase))
        {
            return "mp3";
        }

        return subtype;
    }
}
