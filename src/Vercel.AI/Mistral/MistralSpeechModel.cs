// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral text-to-speech model. Maps to <c>MistralSpeechModel</c>.</summary>
public sealed class MistralSpeechModel : ISpeechModel, ISpeechCaller
{
    private static readonly JsonSerializerOptions BodyOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly OpenAICompatibleProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a speech model. <paramref name="clock"/> sets the response timestamp and defaults to UTC now. Maps to <c>_internal.currentDate</c>.</summary>
    public MistralSpeechModel(
        OpenAICompatibleProvider provider,
        string modelId,
        Func<DateTime>? clock = null
    )
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="ISpeechModel.Provider" />
    public string Provider => "mistral.speech";

    /// <inheritdoc cref="ISpeechModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(
        SpeechCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var result = await DoGenerateAsync(
                new SpeechModelCall(
                    options.Text,
                    options.Voice,
                    options.OutputFormat,
                    options.Instructions,
                    options.Speed,
                    options.Language,
                    JsonValues.EmptyObject(),
                    new Dictionary<string, string>(),
                    cancellationToken
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        var format = JsonNode.Parse(result.Request!.Body!)!["response_format"]!.GetValue<string>();
        return new SpeechResult(
            result.Audio,
            format switch
            {
                "wav" => "audio/wav",
                "pcm" => "audio/pcm",
                "flac" => "audio/flac",
                "opus" => "audio/opus",
                _ => "audio/mpeg",
            }
        );
    }

    /// <inheritdoc />
    public async Task<SpeechModelResult> DoGenerateAsync(
        SpeechModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var (requestBody, requestBodyValues, warnings) = GetArgs(call);
        var url = ApiKeys.Combine(_provider.Options.BaseUrl, "audio/speech");
        var headers = (await _provider.CreateHeadersAsync(call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value), cancellationToken).ConfigureAwait(false));
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Post,
                url,
                ProviderExchange.Json(requestBody),
                headers,
                cancellationToken,
                (status, body) => MistralError.FailedResponse(status, body, url, requestBodyValues)
            )
            .ConfigureAwait(false);

        JsonElement raw;
        string audioData;
        byte[] audioBytes;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            raw = document.RootElement.Clone();
            if (
                raw.ValueKind != JsonValueKind.Object
                || !raw.TryGetProperty("audio_data", out var audio)
                || audio.ValueKind != JsonValueKind.String
            )
            {
                throw new FormatException();
            }

            audioData = audio.GetString()!;
            audioBytes = Convert.FromBase64String(audioData);
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid Mistral speech response: expected audio_data."
            );
        }

        return new SpeechModelResult(
            audioBytes,
            warnings,
            response: new ProviderResponse(
                response.Headers,
                raw,
                timestamp: timestamp,
                modelId: ModelId
            ),
            request: new ProviderRequest(requestBodyValues.ToJsonString(BodyOptions))
        );
    }

    private (
        string RequestBody,
        JsonObject RequestBodyValues,
        List<OperationWarning> Warnings
    ) GetArgs(SpeechModelCall call)
    {
        var warnings = new List<OperationWarning>();
        var mistralOptions = MistralSpeechModelOptions.Parse(call.ProviderOptions);
        var speech = MistralModelCatalog.GetMistralSpeechModelEntry(ModelId).Speech;
        var outputFormat = call.OutputFormat ?? "mp3";
        var responseFormat = "mp3";
        if (speech.OutputFormats.Contains(outputFormat))
        {
            responseFormat = outputFormat;
        }
        else
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "outputFormat",
                    "Unsupported output format: " + outputFormat + ". Using mp3 instead."
                )
            );
        }

        if (
            speech.Instructions is { IsSupported: false, IsUnknown: false, Condition: null }
            && call.Instructions != null
        )
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "instructions",
                    "Mistral speech models do not support the `instructions` option. "
                        + "Use a reference audio clip to guide delivery."
                )
            );
        }

        if (call.Speed != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "speed",
                    "Mistral speech models do not support the `speed` option. It was ignored."
                )
            );
        }

        if (call.Language != null)
        {
            warnings.Add(
                OperationWarning.Unsupported(
                    "language",
                    "Mistral speech models do not support the `language` option. "
                        + "Language is inferred from the input text and voice."
                )
            );
        }

        var refAudio = mistralOptions?.RefAudio;
        var body = new JsonObject { ["model"] = ModelId, ["input"] = call.Text };
        if (refAudio == null)
        {
            if (call.Voice != null)
            {
                body["voice_id"] = call.Voice;
            }
        }
        else
        {
            body["ref_audio"] = refAudio;
        }

        if (mistralOptions?.Expressiveness is { } expressiveness)
        {
            body["expressiveness"] = expressiveness;
        }

        if (mistralOptions?.PhonemeHints is { } hints)
        {
            body["phoneme_hints"] = JsonSerializer.SerializeToNode(hints);
        }

        body["response_format"] = responseFormat;
        body["stream"] = false;

        var values = (JsonObject)body.DeepClone();
        if (refAudio != null)
        {
            values["ref_audio"] = "[redacted]";
        }

        return (body.ToJsonString(BodyOptions), values, warnings);
    }
}
