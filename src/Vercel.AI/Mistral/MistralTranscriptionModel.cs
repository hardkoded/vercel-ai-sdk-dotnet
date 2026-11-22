// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral speech-to-text model. Maps to <c>MistralTranscriptionModel</c>.</summary>
public sealed class MistralTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private readonly OpenAICompatibleProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a transcription model. <paramref name="clock"/> sets the response timestamp and defaults to UTC now. Maps to <c>_internal.currentDate</c>.</summary>
    public MistralTranscriptionModel(
        OpenAICompatibleProvider provider,
        string modelId,
        Func<DateTime>? clock = null
    )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc />
    public string Provider => "mistral.transcription";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public bool CanStream => true;

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(
        AudioInput audio,
        CancellationToken cancellationToken
    )
    {
        audio = audio ?? throw new ArgumentNullException(nameof(audio));
        var result = await DoGenerateAsync(
                new TranscriptionModelCall(
                    audio.Data,
                    string.IsNullOrEmpty(audio.MediaType) ? "audio/wav" : audio.MediaType,
                    JsonValues.EmptyObject(),
                    new Dictionary<string, string>(),
                    cancellationToken
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty);
    }

    /// <inheritdoc />
    public async Task<TranscriptionStreamStart?> DoStreamAsync(
        TranscriptionStreamCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var headers = await _provider
            .CreateHeadersAsync(call.Headers, cancellationToken)
            .ConfigureAwait(false);
        return MistralTranscriptionStream.StreamMistralTranscription(
            call,
            ModelId,
            _provider.Options.BaseUrl,
            headers,
            (_provider.Options as MistralOptions)?.WebSocket,
            _clock?.Invoke() ?? DateTime.UtcNow
        );
    }

    /// <inheritdoc />
    public async Task<TranscriptionModelResult> DoGenerateAsync(
        TranscriptionModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var (form, warnings) = GetArgs(call);
        using var _ = form;
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "audio/transcriptions");
        var headers = (
            await _provider
                .CreateHeadersAsync(
                    call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
                    cancellationToken
                )
                .ConfigureAwait(false)
        );
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Post,
                url,
                form,
                headers,
                cancellationToken,
                (status, body) => MistralError.FailedResponse(status, body, url, null)
            )
            .ConfigureAwait(false);

        using var document = ParseResponse(response.Body);
        var root = document.RootElement;
        var segments = new List<TranscriptSegment>();
        var providerSegments = new JsonArray();
        if (
            root.TryGetProperty("segments", out var segmentItems)
            && segmentItems.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var segment in segmentItems.EnumerateArray())
            {
                var text = segment.GetProperty("text").GetString()!;
                var start = segment.GetProperty("start").GetDouble();
                var end = segment.GetProperty("end").GetDouble();
                segments.Add(new TranscriptSegment(text, start, end));
                var type = Present(segment, "type");
                var score = Present(segment, "score");
                var speakerId = Present(segment, "speaker_id");
                if (type != null || score != null || speakerId != null)
                {
                    var providerSegment = new JsonObject
                    {
                        ["text"] = text,
                        ["startSecond"] = start,
                        ["endSecond"] = end,
                    };
                    if (type != null)
                    {
                        providerSegment["type"] = type.Value.GetString();
                    }

                    if (score != null)
                    {
                        providerSegment["score"] = score.Value.GetDouble();
                    }

                    if (speakerId != null)
                    {
                        providerSegment["speakerId"] = speakerId.Value.GetString();
                    }

                    providerSegments.Add(providerSegment);
                }
            }
        }

        var metadata = new JsonObject();
        JsonElement? usage = null;
        double? promptAudioSeconds = null;
        if (Present(root, "usage") is { ValueKind: JsonValueKind.Object } usageElement)
        {
            usage = usageElement.Clone();
            var usageMetadata = new JsonObject();
            AddNumber(usageMetadata, usageElement, "prompt_tokens", "promptTokens");
            AddNumber(usageMetadata, usageElement, "completion_tokens", "completionTokens");
            AddNumber(usageMetadata, usageElement, "total_tokens", "totalTokens");
            AddNumber(usageMetadata, usageElement, "prompt_audio_seconds", "promptAudioSeconds");
            AddNumber(usageMetadata, usageElement, "request_count", "requestCount");
            metadata["usage"] = usageMetadata;
            promptAudioSeconds = Present(usageElement, "prompt_audio_seconds")?.GetDouble();
        }

        if (providerSegments.Count > 0)
        {
            metadata["segments"] = providerSegments;
        }

        return new TranscriptionModelResult(
            root.GetProperty("text").GetString(),
            segments,
            Present(root, "language")?.GetString(),
            promptAudioSeconds
                ?? (segments.Count > 0 ? segments[segments.Count - 1].EndSecond : null),
            warnings,
            metadata.Count > 0
                ? JsonSerializer.SerializeToElement(new JsonObject { ["mistral"] = metadata })
                : null,
            new ProviderResponse(
                response.Headers,
                root.Clone(),
                timestamp: timestamp,
                modelId: root.GetProperty("model").GetString()
            ),
            usage: usage
        );
    }

    private static JsonElement? Present(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value)
            && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            ? value
            : null;
    }

    private static void AddNumber(JsonObject target, JsonElement usage, string field, string name)
    {
        if (Present(usage, field) is { } value)
        {
            target[name] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static bool IsNumber(JsonElement? element) =>
        element is { ValueKind: JsonValueKind.Number } number && number.TryGetDouble(out _);

    private static JsonDocument ParseResponse(string body)
    {
        JsonDocument? document = null;
        try
        {
            document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (
                root.ValueKind != JsonValueKind.Object
                || Present(root, "model")?.ValueKind != JsonValueKind.String
                || Present(root, "text")?.ValueKind != JsonValueKind.String
                || (
                    Present(root, "language") is { } language
                    && language.ValueKind != JsonValueKind.String
                )
            )
            {
                throw new FormatException();
            }

            if (Present(root, "segments") is { } segments)
            {
                if (segments.ValueKind != JsonValueKind.Array)
                {
                    throw new FormatException();
                }

                foreach (var segment in segments.EnumerateArray())
                {
                    if (
                        segment.ValueKind != JsonValueKind.Object
                        || Present(segment, "text")?.ValueKind != JsonValueKind.String
                        || !IsNumber(Present(segment, "start"))
                        || !IsNumber(Present(segment, "end"))
                        || (
                            Present(segment, "type") is { } type
                            && type.ValueKind != JsonValueKind.String
                        )
                        || (Present(segment, "score") is { } score && !IsNumber(score))
                        || (
                            Present(segment, "speaker_id") is { } speaker
                            && speaker.ValueKind != JsonValueKind.String
                        )
                    )
                    {
                        throw new FormatException();
                    }
                }
            }

            if (Present(root, "usage") is { } usage)
            {
                if (usage.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException();
                }

                foreach (
                    var field in new[]
                    {
                        "prompt_tokens",
                        "completion_tokens",
                        "total_tokens",
                        "prompt_audio_seconds",
                        "request_count",
                    }
                )
                {
                    if (
                        Present(usage, field) is { } value
                        && value.ValueKind != JsonValueKind.Number
                    )
                    {
                        throw new FormatException();
                    }
                }
            }

            return document;
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            document?.Dispose();
            throw new InvalidResponseDataException(
                body,
                "Invalid Mistral transcription response: the body does not match the expected shape."
            );
        }
    }

    private (MultipartFormDataContent Form, List<OperationWarning> Warnings) GetArgs(
        TranscriptionModelCall call
    )
    {
        var warnings = new List<OperationWarning>();
        var options = MistralTranscriptionModelOptions.Parse(call.ProviderOptions);
        if (options?.TargetStreamingDelayMs != null)
        {
            warnings.Add(RealtimeOnly("targetStreamingDelayMs"));
        }

        if (options?.SpeakerReferences != null)
        {
            warnings.Add(RealtimeOnly("speakerReferences"));
        }

        if (options?.TurnDetection != null)
        {
            warnings.Add(RealtimeOnly("turnDetection"));
        }

        if (
            !MistralModelCatalog
                .GetMistralTranscriptionModelEntry(ModelId)
                .Transcription.LanguageWithTimestamps
            && options?.Language != null
            && options.TimestampGranularities != null
        )
        {
            throw new InvalidArgumentException(
                "providerOptions",
                null,
                "providerOptions.mistral.language cannot be combined with providerOptions.mistral.timestampGranularities"
            );
        }

        if (options?.Language != null && options.ReturnLanguage == true)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                null,
                "providerOptions.mistral.language cannot be combined with providerOptions.mistral.returnLanguage"
            );
        }

        var form = new MultipartFormDataContent();
        try
        {
            form.Add(new StringContent(ModelId), "model");
            var file = new ByteArrayContent(call.Audio);
            if (!MediaTypeHeaderValue.TryParse(call.MediaType, out var mediaType))
            {
                throw new InvalidArgumentException(
                    "mediaType",
                    call.MediaType,
                    "Invalid media type: " + call.MediaType
                );
            }

            file.Headers.ContentType = mediaType;
            form.Add(file, "file", "audio." + ProviderValues.MediaTypeToExtension(call.MediaType));
            if (options != null)
            {
                AppendFormValue(form, "language", options.Language);
                AppendFormValue(form, "return_language", Format(options.ReturnLanguage));
                AppendFormValue(form, "temperature", Format(options.Temperature));
                foreach (var granularity in options.TimestampGranularities ?? [])
                {
                    AppendFormValue(form, "timestamp_granularities", granularity);
                }

                AppendFormValue(form, "diarize", Format(options.Diarize));
                foreach (var bias in options.ContextBias ?? [])
                {
                    AppendFormValue(form, "context_bias", bias);
                }
            }
        }
        catch
        {
            form.Dispose();
            throw;
        }

        return (form, warnings);
    }

    private static OperationWarning RealtimeOnly(string option)
    {
        return OperationWarning.Unsupported(
            "providerOptions.mistral." + option,
            "This option is only supported for realtime transcription."
        );
    }

    private static string? Format(bool? value) =>
        value is null ? null
        : value.Value ? "true"
        : "false";

    private static string? Format(double? value) =>
        value?.ToString("R", CultureInfo.InvariantCulture);

    private static void AppendFormValue(MultipartFormDataContent form, string key, string? value)
    {
        if (value != null)
        {
            form.Add(new StringContent(value), key);
        }
    }
}
