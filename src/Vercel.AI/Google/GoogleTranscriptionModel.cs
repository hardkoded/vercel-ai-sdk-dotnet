// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>
/// Gemini transcription. Unary model ids post to the Interactions API.
/// Live model ids are rejected here because this port has no Live API socket.
/// </summary>
public sealed class GoogleTranscriptionModel : ITranscriptionModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public GoogleTranscriptionModel(GoogleProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.Name; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        return DoTranscribeAsync(audio, null, cancellationToken);
    }

    /// <summary>Transcribes audio with Google transcription provider options.</summary>
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, JsonElement? googleOptions, CancellationToken cancellationToken)
    {
        if (audio == null)
        {
            throw new ArgumentNullException(nameof(audio));
        }

        if (ModelId.IndexOf("-live", StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                "Model '" + ModelId + "' only supports streaming transcription. " +
                "Use experimental_streamTranscribe, or a unary model such as 'gemini-3.5-transcribe'.");
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "audio",
                    ["data"] = Convert.ToBase64String(audio.Data),
                    ["mime_type"] = audio.MediaType,
                },
            },
        };
        var config = TranscriptionConfig(googleOptions);
        if (config != null)
        {
            body["generation_config"] = new JsonObject { ["transcription_config"] = config };
        }

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "interactions"),
            body.ToJsonString(),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        var text = new System.Text.StringBuilder();
        if (document.RootElement.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
        {
            foreach (var step in steps.EnumerateArray())
            {
                if (!step.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in content.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var type) && type.GetString() == "text" && part.TryGetProperty("text", out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        text.Append(value.GetString());
                    }
                }
            }
        }

        AudioUsage? usage = null;
        if (document.RootElement.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            usage = new AudioUsage(
                inputTokens: Int(usageElement, "total_input_tokens"),
                outputTokens: Int(usageElement, "total_output_tokens"),
                totalTokens: Int(usageElement, "total_tokens"));
        }

        return new TranscriptionResult(text.ToString(), usage);
    }

    private static JsonObject? TranscriptionConfig(JsonElement? googleOptions)
    {
        if (googleOptions is not { ValueKind: JsonValueKind.Object } options)
        {
            return null;
        }

        var config = new JsonObject();
        if (options.TryGetProperty("languageCodes", out var languages) && languages.ValueKind == JsonValueKind.Array)
        {
            config["language_codes"] = GoogleJson.Clone(languages);
        }

        if (options.TryGetProperty("customVocabulary", out var vocabulary) && vocabulary.ValueKind == JsonValueKind.Array)
        {
            config["custom_vocabulary"] = GoogleJson.Clone(vocabulary);
        }

        var mode = options.TryGetProperty("mode", out var modeElement) && modeElement.ValueKind == JsonValueKind.String
            ? modeElement.GetString()
            : null;
        var diarization = options.TryGetProperty("diarization", out var diarizationElement) && diarizationElement.ValueKind == JsonValueKind.True;
        var words = options.TryGetProperty("wordTimestamp", out var wordElement) && wordElement.ValueKind == JsonValueKind.True;
        if (mode != null || diarization || words)
        {
            var modeObject = new JsonObject { ["type"] = (mode ?? "VERBATIM").ToLowerInvariant() };
            if (diarization)
            {
                modeObject["diarization_mode"] = "speaker";
            }

            if (words)
            {
                modeObject["timestamp_granularities"] = new JsonArray { "word" };
            }

            config["mode"] = modeObject;
        }

        return config.Count == 0 ? null : config;
    }

    private static int? Int(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return value.TryGetInt32(out var number) ? number : null;
    }
}
