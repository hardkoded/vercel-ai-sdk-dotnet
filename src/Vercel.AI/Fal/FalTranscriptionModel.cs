// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Fal;

/// <summary>Fal transcription model. Submits the audio to the Fal queue and polls until the transcript is ready.</summary>
public sealed class FalTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private const string InProgress = "Request is still in progress";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly FalProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp.</summary>
    public FalTranscriptionModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="ITranscriptionModel.Provider" />
    public string Provider => "fal.transcription";

    /// <inheritdoc cref="ITranscriptionModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => false;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        if (audio is null)
        {
            throw new ArgumentNullException(nameof(audio));
        }

        var call = new TranscriptionModelCall(audio.Data, audio.MediaType, JsonValues.EmptyObject(), new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var body = CreateBody(FalProvider.FalOptions(call.ProviderOptions));
        body["audio_url"] = "data:" + call.MediaType + ";base64," + ByteEncoding.ToBase64(call.Audio);
        var queued = await _provider.SendAsync(HttpMethod.Post, QueueUri(string.Empty), ProviderExchange.Json(body.ToJsonString()), call.Headers, cancellationToken).ConfigureAwait(false);
        string requestId;
        using (var document = JsonDocument.Parse(queued.Body))
        {
            requestId = document.RootElement.TryGetProperty("request_id", out var id) && id.ValueKind == JsonValueKind.String
                ? id.GetString()!
                : throw new InvalidResponseDataException(queued.Body, "Fal queue response has no request_id.");
        }

        var response = await PollAsync(QueueUri("/requests/" + requestId), call.Headers, cancellationToken).ConfigureAwait(false);
        using var result = JsonDocument.Parse(response.Body);
        var root = result.RootElement;
        var segments = new List<TranscriptSegment>();
        double? duration = null;
        if (root.TryGetProperty("chunks", out var chunks) && chunks.ValueKind == JsonValueKind.Array)
        {
            foreach (var chunk in chunks.EnumerateArray())
            {
                var start = Timestamp(chunk, 0);
                var end = Timestamp(chunk, 1);
                segments.Add(new TranscriptSegment(chunk.GetProperty("text").GetString() ?? string.Empty, start ?? 0, end ?? 0));
                duration = end;
            }
        }

        string? language = null;
        if (root.TryGetProperty("inferred_languages", out var languages) && languages.ValueKind == JsonValueKind.Array && languages.GetArrayLength() > 0)
        {
            language = languages[0].GetString();
        }

        return new TranscriptionModelResult(
            root.GetProperty("text").GetString(),
            segments,
            language,
            duration,
            Array.Empty<OperationWarning>(),
            response: new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp, modelId: ModelId));
    }

    /// <inheritdoc />
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
    }

    // The defaults below apply only when providerOptions.fal is set.
    private static JsonObject CreateBody(JsonElement? fal)
    {
        var body = new JsonObject
        {
            ["task"] = "transcribe",
            ["diarize"] = true,
            ["chunk_level"] = "word",
        };
        if (fal is not { } options)
        {
            return body;
        }

        body["language"] = options.TryGetProperty("language", out var language) ? JsonNode.Parse(language.GetRawText()) : "en";
        SetUnlessNull(body, "version", options, "version", "3");
        SetUnlessNull(body, "batch_size", options, "batchSize", 64);
        SetUnlessNull(body, "num_speakers", options, "numSpeakers", null);
        if (options.TryGetProperty("diarize", out var diarize) && (diarize.ValueKind == JsonValueKind.True || diarize.ValueKind == JsonValueKind.False))
        {
            body["diarize"] = diarize.GetBoolean();
        }

        if (!options.TryGetProperty("chunkLevel", out var chunkLevel))
        {
            body["chunk_level"] = "segment";
        }
        else if (chunkLevel.ValueKind == JsonValueKind.String && chunkLevel.GetString()!.Length > 0)
        {
            body["chunk_level"] = chunkLevel.GetString();
        }

        return body;
    }

    // A missing option takes its default; an explicit null leaves the field out.
    private static void SetUnlessNull(JsonObject body, string field, JsonElement options, string option, JsonNode? fallback)
    {
        if (!options.TryGetProperty(option, out var value))
        {
            if (fallback != null)
            {
                body[field] = fallback;
            }
        }
        else if (value.ValueKind != JsonValueKind.Null)
        {
            body[field] = JsonNode.Parse(value.GetRawText());
        }
    }

    private async Task<ProviderExchangeResult> PollAsync(Uri statusUri, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
    {
        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                return await _provider.SendAsync(HttpMethod.Get, statusUri, null, headers, cancellationToken).ConfigureAwait(false);
            }
            catch (ApiException error) when (IsInProgress(error.ResponseBody))
            {
            }

            if (elapsed.Elapsed > Timeout)
            {
                throw new AiSdkException("Transcription request timed out after 60 seconds");
            }

            await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool IsInProgress(string? body)
    {
        try
        {
            using var document = JsonDocument.Parse(body ?? string.Empty);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("detail", out var detail)
                && detail.ValueKind == JsonValueKind.String
                && detail.GetString() == InProgress;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static double? Timestamp(JsonElement chunk, int index)
    {
        return chunk.TryGetProperty("timestamp", out var timestamp) && timestamp.ValueKind == JsonValueKind.Array && index < timestamp.GetArrayLength() && timestamp[index].ValueKind == JsonValueKind.Number
            ? timestamp[index].GetDouble()
            : null;
    }

    private Uri QueueUri(string path)
    {
        return new Uri("https://queue.fal.run/fal-ai/" + ModelId + path);
    }
}
