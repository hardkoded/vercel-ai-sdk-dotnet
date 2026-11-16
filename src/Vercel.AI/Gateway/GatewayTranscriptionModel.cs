// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway transcription model, routed through <c>/transcription-model</c>. Maps to <c>GatewayTranscriptionModel</c>.
/// Streaming transcription is not ported yet, so <see cref="CanStream"/> is false.
/// </summary>
public sealed class GatewayTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway transcription model.</summary>
    /// <param name="provider">Gateway provider that sends the requests.</param>
    /// <param name="modelId">Model id.</param>
    /// <param name="providerName">Value of <see cref="Provider"/> (upstream <c>GatewayConfig.provider</c>). Defaults to <c>gateway</c>.</param>
    public GatewayTranscriptionModel(GatewayProvider provider, string modelId, string? providerName = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = providerName ?? GatewayProvider.ProviderId;
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => false;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        audio = audio ?? throw new ArgumentNullException(nameof(audio));
        var result = await DoGenerateAsync(new TranscriptionModelCall(audio.Data, string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType, OperationJson.Parse("{}"), new Dictionary<string, string>(), cancellationToken), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var body = new JsonObject
        {
            ["audio"] = call.AudioBase64 ?? ByteEncoding.ToBase64(call.Audio),
            ["mediaType"] = call.MediaType,
        };
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        ProviderTextResponse response;
        try
        {
            response = await _provider.Http.SendJsonStringAsync(
                HttpMethod.Post,
                _provider.Route("transcription-model"),
                body.ToJsonString(),
                _provider.Headers("ai-transcription-model-specification-version", "4", "ai-model-id", ModelId, null, headers),
                cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new InvalidResponseDataException(response.Body, "Invalid Gateway transcription response: the body is not valid JSON.");
        }

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String)
        {
            throw new InvalidResponseDataException(response.Body, "Invalid Gateway transcription response: expected text.");
        }

        var segments = ReadSegments(root, "segments", response.Body) ?? new List<TranscriptSegment>();
        var words = ReadSegments(root, "words", response.Body);
        var warnings = new List<OperationWarning>();
        if (root.TryGetProperty("warnings", out var warningArray) && warningArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var warning in warningArray.EnumerateArray())
            {
                warnings.Add(GatewayImageModel.ReadWarning(warning));
            }
        }

        if (call.TimestampGranularities is { Count: > 0 })
        {
            warnings.Add(new OperationWarning(
                "unsupported",
                feature: "timestampGranularities",
                details: "The AI Gateway adapter does not forward the shared timestampGranularities option."));
        }

        if (IsXaiTranscriptionModel(ModelId) && RequestsXaiDiarization(call.ProviderOptions) && !ContainsSpeaker(root))
        {
            warnings.Add(new OperationWarning(
                "unsupported",
                feature: "providerOptions.xai.diarize",
                details: "AI Gateway does not currently expose xAI speaker diarization metadata."));
        }

        JsonElement? usage = root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object ? usageElement.Clone() : null;
        JsonElement? providerMetadata = root.TryGetProperty("providerMetadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object ? metadata.Clone() : null;
        return new TranscriptionModelResult(
            textElement.GetString(),
            segments,
            root.TryGetProperty("language", out var language) && language.ValueKind == JsonValueKind.String ? language.GetString() : null,
            root.TryGetProperty("durationInSeconds", out var duration) && duration.ValueKind == JsonValueKind.Number ? duration.GetDouble() : null,
            warnings,
            providerMetadata,
            new ProviderResponse(response.Headers, root, timestamp: DateTime.UtcNow, modelId: ModelId),
            words,
            usage);
    }

    /// <inheritdoc />
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
    }

    private static List<TranscriptSegment>? ReadSegments(JsonElement root, string name, string body)
    {
        if (!root.TryGetProperty(name, out var items) || items.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (items.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidResponseDataException(body, "Invalid Gateway transcription response: " + name + " must be an array.");
        }

        var result = new List<TranscriptSegment>();
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("text", out var text) || text.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("startSecond", out var start) || start.ValueKind != JsonValueKind.Number
                || !item.TryGetProperty("endSecond", out var end) || end.ValueKind != JsonValueKind.Number)
            {
                throw new InvalidResponseDataException(body, "Invalid Gateway transcription response: an entry of " + name + " is malformed.");
            }

            result.Add(new TranscriptSegment(text.GetString()!, start.GetDouble(), end.GetDouble()));
        }

        return result;
    }

    private static bool IsXaiTranscriptionModel(string modelId)
    {
        return modelId.StartsWith("spacexai/", StringComparison.Ordinal) || modelId.StartsWith("xai/", StringComparison.Ordinal);
    }

    private static bool RequestsXaiDiarization(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var name in new[] { "xai", "spacexai" })
        {
            if (providerOptions.TryGetProperty(name, out var options)
                && options.ValueKind == JsonValueKind.Object
                && options.TryGetProperty("diarize", out var diarize)
                && diarize.ValueKind == JsonValueKind.True)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsSpeaker(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Array:
                return value.EnumerateArray().Any(ContainsSpeaker);
            case JsonValueKind.Object:
                return value.EnumerateObject().Any(property =>
                    (property.Name == "speaker" && property.Value.ValueKind != JsonValueKind.Null)
                    || ContainsSpeaker(property.Value));
            default:
                return false;
        }
    }
}
