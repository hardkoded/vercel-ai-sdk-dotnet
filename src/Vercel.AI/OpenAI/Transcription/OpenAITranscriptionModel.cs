// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Transcription settings beyond the audio payload.</summary>
public sealed class OpenAITranscriptionCall
{
    /// <summary>Creates a transcription call.</summary>
    public OpenAITranscriptionCall(AudioInput audio)
    {
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
    }

    /// <summary>Audio to transcribe.</summary>
    public AudioInput Audio { get; }

    /// <summary>Provider options. The <c>openai</c> object is read.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Extra HTTP headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
}

/// <summary>Response metadata recorded for a transcription.</summary>
public sealed class OpenAITranscriptionResponse
{
    internal OpenAITranscriptionResponse(DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string> headers, string body)
    {
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
        Body = body;
    }

    /// <summary>Timestamp captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id used for the call.</summary>
    public string ModelId { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Raw response JSON.</summary>
    public string Body { get; }
}

/// <summary>Transcript with timed segments and response metadata.</summary>
public sealed class OpenAITranscriptionResult
{
    internal OpenAITranscriptionResult(
        string text,
        IReadOnlyList<TranscriptSegment> segments,
        string? language,
        double? durationInSeconds,
        JsonElement? providerMetadata,
        OpenAITranscriptionResponse response)
    {
        Text = text;
        Segments = segments;
        Language = language;
        DurationInSeconds = durationInSeconds;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Transcript text.</summary>
    public string Text { get; }

    /// <summary>Segments, or words when the response has no segments.</summary>
    public IReadOnlyList<TranscriptSegment> Segments { get; }

    /// <summary>ISO-639-1 code, when OpenAI reports a known language name.</summary>
    public string? Language { get; }

    /// <summary>Audio duration in seconds.</summary>
    public double? DurationInSeconds { get; }

    /// <summary>Speaker segments under <c>openai.segments</c> for diarized responses.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata.</summary>
    public OpenAITranscriptionResponse Response { get; }
}

/// <summary>OpenAI audio transcription model.</summary>
public sealed class OpenAITranscriptionModel : ITranscriptionModel
{
    // OpenAI reports the detected language by name. Only these names map to ISO-639-1 codes.
    private static readonly Dictionary<string, string> LanguageCodes = new(StringComparer.Ordinal)
    {
        ["afrikaans"] = "af",
        ["arabic"] = "ar",
        ["armenian"] = "hy",
        ["azerbaijani"] = "az",
        ["belarusian"] = "be",
        ["bosnian"] = "bs",
        ["bulgarian"] = "bg",
        ["catalan"] = "ca",
        ["chinese"] = "zh",
        ["croatian"] = "hr",
        ["czech"] = "cs",
        ["danish"] = "da",
        ["dutch"] = "nl",
        ["english"] = "en",
        ["estonian"] = "et",
        ["finnish"] = "fi",
        ["french"] = "fr",
        ["galician"] = "gl",
        ["german"] = "de",
        ["greek"] = "el",
        ["hebrew"] = "he",
        ["hindi"] = "hi",
        ["hungarian"] = "hu",
        ["icelandic"] = "is",
        ["indonesian"] = "id",
        ["italian"] = "it",
        ["japanese"] = "ja",
        ["kannada"] = "kn",
        ["kazakh"] = "kk",
        ["korean"] = "ko",
        ["latvian"] = "lv",
        ["lithuanian"] = "lt",
        ["macedonian"] = "mk",
        ["malay"] = "ms",
        ["marathi"] = "mr",
        ["maori"] = "mi",
        ["nepali"] = "ne",
        ["norwegian"] = "no",
        ["persian"] = "fa",
        ["polish"] = "pl",
        ["portuguese"] = "pt",
        ["romanian"] = "ro",
        ["russian"] = "ru",
        ["serbian"] = "sr",
        ["slovak"] = "sk",
        ["slovenian"] = "sl",
        ["spanish"] = "es",
        ["swahili"] = "sw",
        ["swedish"] = "sv",
        ["tagalog"] = "tl",
        ["tamil"] = "ta",
        ["thai"] = "th",
        ["turkish"] = "tr",
        ["ukrainian"] = "uk",
        ["urdu"] = "ur",
        ["vietnamese"] = "vi",
        ["welsh"] = "cy",
    };

    private readonly OpenAIProvider _provider;
    private readonly string _path;
    private readonly Func<DateTimeOffset>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp and defaults to UTC now.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, Func<DateTimeOffset>? clock = null)
        : this(provider, modelId, "audio/transcriptions", clock)
    {
    }

    /// <summary>Creates a transcription or translation model for <paramref name="path"/>.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, string path, Func<DateTimeOffset>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _path = path;
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => _provider.Name + ".transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>True when <paramref name="modelId"/> must use the realtime transcription socket.</summary>
    public static bool IsRealtimeWhisper(string modelId)
    {
        return modelId == "gpt-realtime-whisper" || modelId.StartsWith("gpt-realtime-whisper-", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var result = await TranscribeAsync(new OpenAITranscriptionCall(audio), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text);
    }

    /// <summary>Transcribes audio and returns segments, language, duration, and response metadata.</summary>
    public async Task<OpenAITranscriptionResult> TranscribeAsync(OpenAITranscriptionCall call, CancellationToken cancellationToken)
    {
        if (IsRealtimeWhisper(ModelId))
        {
            throw new AiSdkException("gpt-realtime-whisper requires a streaming transcription.");
        }

        var timestamp = OpenAIClock.Now(_clock);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiKeys.Combine(_provider.Options.BaseUrl, _path));
        request.Content = Build(call);
        OpenAIJson.ApplyHeaders(request, _provider.CreateOpenAIHeaders(call.Headers));
        using var response = await _provider.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
        if (!response.IsSuccessStatusCode)
        {
            throw ProviderHttp.MapStatus((int)response.StatusCode, body);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var segments = new List<TranscriptSegment>();
        var speakers = new JsonArray();
        if (root.TryGetProperty("segments", out var segmentItems) && segmentItems.ValueKind == JsonValueKind.Array)
        {
            foreach (var segment in segmentItems.EnumerateArray())
            {
                var text = OpenAIJson.String(segment, "text") ?? string.Empty;
                var start = segment.GetProperty("start").GetDouble();
                var end = segment.GetProperty("end").GetDouble();
                segments.Add(new TranscriptSegment(text, start, end));
                if (segment.TryGetProperty("speaker", out var speaker))
                {
                    speakers.Add(new JsonObject { ["text"] = text, ["startSecond"] = start, ["endSecond"] = end, ["speaker"] = JsonNode.Parse(speaker.GetRawText()) });
                }
            }
        }
        else if (root.TryGetProperty("words", out var words) && words.ValueKind == JsonValueKind.Array)
        {
            foreach (var word in words.EnumerateArray())
            {
                segments.Add(new TranscriptSegment(OpenAIJson.String(word, "word") ?? string.Empty, word.GetProperty("start").GetDouble(), word.GetProperty("end").GetDouble()));
            }
        }

        var languageName = OpenAIJson.String(root, "language");
        return new OpenAITranscriptionResult(
            OpenAIJson.String(root, "text") ?? string.Empty,
            segments,
            languageName != null && LanguageCodes.TryGetValue(languageName, out var code) ? code : null,
            root.TryGetProperty("duration", out var duration) && duration.ValueKind == JsonValueKind.Number ? duration.GetDouble() : null,
            speakers.Count > 0 ? SpeakerMetadata(speakers) : null,
            new OpenAITranscriptionResponse(timestamp, ModelId, OpenAIJson.CopyHeaders(response), body));
    }

    /// <summary>Builds the multipart body so tests can inspect field names.</summary>
    public MultipartFormDataContent Build(OpenAITranscriptionCall call)
    {
        var content = new MultipartFormDataContent();
        var mediaType = string.IsNullOrEmpty(call.Audio.MediaType) ? "audio/mpeg" : call.Audio.MediaType;
        var file = new ByteArrayContent(call.Audio.Data);
        file.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(file, "file", "audio." + OpenAIJson.Extension(mediaType));
        content.Add(new StringContent(ModelId), "model");
        var openai = OpenAIJson.OpenAIObject(call.ProviderOptions);
        var diarize = ModelId == "gpt-4o-transcribe-diarize";
        if (ModelId == "whisper-1")
        {
            content.Add(new StringContent("verbose_json"), "response_format");
        }

        var chunking = Chunking(openai, diarize);
        if (openai != null && ModelId != "whisper-1")
        {
            var format = OpenAIJson.String(openai, "responseFormat");
            if (format == null)
            {
                format = diarize
                    ? "diarized_json"
                    : ModelId == "gpt-4o-transcribe" || ModelId == "gpt-4o-mini-transcribe" ? "json" : "verbose_json";
            }

            content.Add(new StringContent(format), "response_format");
        }
        else if (openai == null && diarize)
        {
            content.Add(new StringContent("diarized_json"), "response_format");
        }

        if (openai != null)
        {
            Add(content, "language", OpenAIJson.String(openai, "language"));
            Add(content, "prompt", OpenAIJson.String(openai, "prompt"));
            var temperature = OpenAIJson.Double(openai, "temperature") ?? 0d;
            content.Add(new StringContent(temperature.ToString(CultureInfo.InvariantCulture)), "temperature");
            foreach (var granularity in Granularities(openai.Value))
            {
                content.Add(new StringContent(granularity), "timestamp_granularities[]");
            }
        }

        if (chunking != null)
        {
            content.Add(new StringContent(chunking), "chunking_strategy");
        }

        return content;
    }

    private static JsonElement SpeakerMetadata(JsonArray segments)
    {
        var metadata = new JsonObject { ["openai"] = new JsonObject { ["segments"] = segments } };
        using var document = JsonDocument.Parse(metadata.ToJsonString());
        return document.RootElement.Clone();
    }

    private static void Add(MultipartFormDataContent content, string name, string? value)
    {
        if (value != null)
        {
            content.Add(new StringContent(value), name);
        }
    }

    private static string? Chunking(JsonElement? openai, bool diarize)
    {
        var chunking = OpenAIJson.Child(openai, "chunkingStrategy");
        if (chunking == null)
        {
            return diarize ? "auto" : null;
        }

        if (chunking.Value.ValueKind == JsonValueKind.String)
        {
            return chunking.Value.GetString();
        }

        if (chunking.Value.ValueKind != JsonValueKind.Object)
        {
            return diarize ? "auto" : null;
        }

        var json = new System.Text.Json.Nodes.JsonObject();
        if (chunking.Value.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String)
        {
            json["type"] = type.GetString();
        }

        if (chunking.Value.TryGetProperty("threshold", out var threshold) && threshold.ValueKind == JsonValueKind.Number)
        {
            json["threshold"] = threshold.GetDouble();
        }

        if (chunking.Value.TryGetProperty("prefixPaddingMs", out var prefix) && prefix.TryGetInt32(out var prefixMs))
        {
            json["prefix_padding_ms"] = prefixMs;
        }

        if (chunking.Value.TryGetProperty("silenceDurationMs", out var silence) && silence.TryGetInt32(out var silenceMs))
        {
            json["silence_duration_ms"] = silenceMs;
        }

        return json.ToJsonString();
    }

    private static IReadOnlyList<string> Granularities(JsonElement openai)
    {
        var granularities = OpenAIJson.Child(openai, "timestampGranularities");
        if (granularities == null || granularities.Value.ValueKind != JsonValueKind.Array)
        {
            return new[] { "segment" };
        }

        var values = new List<string>();
        foreach (var item in granularities.Value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                values.Add(item.GetString() ?? string.Empty);
            }
        }

        return values;
    }
}
