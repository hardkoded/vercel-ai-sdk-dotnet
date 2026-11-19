// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>xAI text-to-speech model, over <c>/tts</c>. Maps to <c>XaiSpeechModel</c>.</summary>
public sealed class XaiSpeechModel : ISpeechModel, ISpeechCaller
{
    private static readonly JsonSerializerOptions BodyOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly XaiProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a model. xAI has one speech model, so the model id is empty. <paramref name="clock"/> sets the response timestamp and defaults to UTC now. Maps to <c>_internal.currentDate</c>.</summary>
    public XaiSpeechModel(XaiProvider provider, string modelId = "", Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <summary>Specification version.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc cref="ISpeechModel.Provider" />
    public string Provider => "xai.speech";

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
                    options.ProviderOptions is { } providerOptions
                        ? JsonSerializer.SerializeToElement(providerOptions)
                        : JsonValues.EmptyObject(),
                    (options.Headers ?? new Dictionary<string, string?>())
                        .Where(pair => pair.Value != null)
                        .ToDictionary(pair => pair.Key, pair => pair.Value!),
                    cancellationToken
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        var codec = JsonNode.Parse(result.Request!.Body!)!["output_format"]![
            "codec"
        ]!.GetValue<string>();
        return new SpeechResult(
            result.Audio,
            codec switch
            {
                "wav" => "audio/wav",
                "pcm" => "audio/pcm",
                "mulaw" => "audio/basic",
                "alaw" => "audio/alaw",
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
        var (requestBody, warnings, withTimestamps) = GetArgs(call);
        var requestJson = requestBody.ToJsonString(BodyOptions);

        // With `with_timestamps` the API returns a JSON envelope carrying base64-encoded audio plus
        // character-level timings instead of raw audio bytes.
        var response = await ProviderExchange
            .SendAsync(
                _provider.SharedHttpClient,
                HttpMethod.Post,
                ApiKeys.Combine(_provider.Options.BaseUrl, "tts"),
                new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json"),
                _provider.CreateHeaders(
                    call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value)
                ),
                cancellationToken,
                XaiError.FailedResponseHandler
            )
            .ConfigureAwait(false);

        byte[] audio;
        JsonElement? raw = null;
        JsonElement? envelope = null;
        if (withTimestamps)
        {
            try
            {
                using var document = JsonDocument.Parse(response.Body);
                envelope = document.RootElement.Clone();
                raw = envelope;
                audio = Text(envelope.Value, "audio") is { } encoded
                    ? Convert.FromBase64String(encoded)
                    : [];
            }
            catch (Exception error) when (error is JsonException or FormatException)
            {
                throw new InvalidResponseDataException(
                    response.Body,
                    "Invalid xAI speech response."
                );
            }
        }
        else
        {
            audio = response.Bytes;
        }

        // xAI returns a trace id on every response (success and error).
        var metadata = new JsonObject();
        if (
            response.Headers.FirstOrDefault(pair =>
                string.Equals(pair.Key, "x-trace-id", StringComparison.OrdinalIgnoreCase)
            ) is
            { Key: not null } trace
        )
        {
            metadata["traceId"] = trace.Value;
        }

        if (envelope is { } value)
        {
            if (
                value.TryGetProperty("duration", out var duration)
                && duration.ValueKind == JsonValueKind.Number
            )
            {
                metadata["duration"] = JsonNode.Parse(duration.GetRawText());
            }

            if (Text(value, "content_type") is { } contentType)
            {
                metadata["contentType"] = contentType;
            }

            if (
                value.TryGetProperty("audio_timestamps", out var timestamps)
                && timestamps.ValueKind == JsonValueKind.Object
            )
            {
                metadata["audioTimestamps"] = new JsonObject
                {
                    ["graphChars"] = JsonNode.Parse(
                        timestamps.GetProperty("graph_chars").GetRawText()
                    ),
                    ["graphTimes"] = JsonNode.Parse(
                        timestamps.GetProperty("graph_times").GetRawText()
                    ),
                };
            }
        }

        return new SpeechModelResult(
            audio,
            warnings,
            OperationJson.Parse(
                new JsonObject { [XaiProvider.ProviderId] = metadata }.ToJsonString()
            ),
            new ProviderResponse(response.Headers, raw, timestamp: timestamp, modelId: ModelId),
            new ProviderRequest(requestJson)
        );
    }

    private static string? Text(JsonElement item, string name)
    {
        return item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static (
        JsonObject RequestBody,
        List<OperationWarning> Warnings,
        bool WithTimestamps
    ) GetArgs(SpeechModelCall call)
    {
        var defaults = XaiModelCatalog.GetXaiSpeechModelEntry().RequestConfig;
        var voice = call.Voice ?? defaults.Voice;
        var outputFormat = call.OutputFormat ?? defaults.OutputFormat;
        var language = call.Language ?? defaults.Language;
        var warnings = new List<OperationWarning>();
        var xaiOptions = XaiSpeechModelOptions.Parse(call.ProviderOptions);

        var speech = XaiModelCatalog.GetXaiSpeechModelEntry().Capabilities;
        var codec = defaults.OutputFormat;
        if (speech.OutputFormats.Contains(outputFormat))
        {
            codec = outputFormat;
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
                    "xAI speech models do not support the `instructions` option. "
                        + "Use xAI speech tags in `text` to control delivery."
                )
            );
        }

        var format = new JsonObject { ["codec"] = codec };
        if (xaiOptions?.SampleRate != null)
        {
            format["sample_rate"] = xaiOptions.SampleRate;
        }

        if (xaiOptions?.BitRate != null)
        {
            if (codec == "mp3")
            {
                format["bit_rate"] = xaiOptions.BitRate;
            }
            else
            {
                warnings.Add(
                    OperationWarning.Unsupported(
                        "providerOptions",
                        "xAI `bitRate` is supported only for mp3 output. It was ignored."
                    )
                );
            }
        }

        var body = new JsonObject
        {
            ["text"] = call.Text,
            ["voice_id"] = voice,
            ["language"] = language,
            ["output_format"] = format,
        };
        if (call.Speed != null)
        {
            body["speed"] = call.Speed;
        }

        if (xaiOptions?.OptimizeStreamingLatency != null)
        {
            body["optimize_streaming_latency"] = xaiOptions.OptimizeStreamingLatency;
        }

        if (xaiOptions?.TextNormalization != null)
        {
            body["text_normalization"] = xaiOptions.TextNormalization;
        }

        if (xaiOptions?.WithTimestamps != null)
        {
            body["with_timestamps"] = xaiOptions.WithTimestamps;
        }

        if (xaiOptions?.Replace != null)
        {
            body["replace"] = JsonSerializer.SerializeToNode(xaiOptions.Replace, BodyOptions);
        }

        return (body, warnings, xaiOptions?.WithTimestamps == true);
    }
}
