// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

/// <summary>OpenAI audio transcription model.</summary>
public sealed class OpenAITranscriptionModel : ITranscriptionModel
{
    private readonly OpenAIProvider _provider;
    private readonly string _path;

    /// <summary>Creates a transcription model.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId)
        : this(provider, modelId, "audio/transcriptions")
    {
    }

    /// <summary>Creates a transcription or translation model for <paramref name="path"/>.</summary>
    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, string path)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _path = path;
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
    public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        return TranscribeAsync(new OpenAITranscriptionCall(audio), cancellationToken);
    }

    /// <summary>Transcribes audio.</summary>
    public async Task<TranscriptionResult> TranscribeAsync(OpenAITranscriptionCall call, CancellationToken cancellationToken)
    {
        if (IsRealtimeWhisper(ModelId))
        {
            throw new AiSdkException("gpt-realtime-whisper requires a streaming transcription.");
        }

        using var content = Build(call);
        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, _path),
            content,
            _provider.CreateOpenAIHeaders(call.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
        var text = document.RootElement.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String
            ? textElement.GetString() ?? string.Empty
            : string.Empty;
        return new TranscriptionResult(text);
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
