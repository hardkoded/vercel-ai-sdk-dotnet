// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Speech request settings beyond the shared text and voice fields.</summary>
public sealed class GoogleSpeechCall
{
    /// <summary>Creates a speech call.</summary>
    public GoogleSpeechCall(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>Transcript.</summary>
    public string Text { get; }

    /// <summary>Prebuilt voice. Defaults to Kore.</summary>
    public string? Voice { get; set; }

    /// <summary>Style directions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Speaking rate. Gemini TTS ignores it.</summary>
    public double? Speed { get; set; }

    /// <summary>Language. Gemini TTS ignores it.</summary>
    public string? Language { get; set; }

    /// <summary>Output format such as <c>wav</c> or <c>pcm</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Provider options under <c>google</c>, <c>googleVertex</c>, or <c>vertex</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
}

/// <summary>Inspects a speech transcript before a model call.</summary>
public static class GoogleSpeechInput
{
    /// <summary>Joins structured turns and detects custom voices, which Gemini rejects.</summary>
    public static GoogleSpeechInspection Inspect(string text, string? voice, JsonElement? google)
    {
        var transcript = text ?? string.Empty;
        if (google is { ValueKind: JsonValueKind.Object } options && options.TryGetProperty("turns", out var turns) && turns.ValueKind == JsonValueKind.Array && turns.GetArrayLength() > 0)
        {
            var parts = new List<string>();
            var complete = true;
            foreach (var turn in turns.EnumerateArray())
            {
                if (turn.ValueKind != JsonValueKind.Object || !turn.TryGetProperty("text", out var value) || value.ValueKind != JsonValueKind.String)
                {
                    complete = false;
                    break;
                }

                parts.Add(value.GetString() ?? string.Empty);
            }

            if (complete)
            {
                transcript = string.Concat(parts);
            }
        }

        var custom = voice != null && (voice.StartsWith("voice_", StringComparison.Ordinal) || voice.StartsWith("voicekey_", StringComparison.Ordinal));
        if (!custom && google is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty("multiSpeakerVoiceConfig", out var config)
            && config.TryGetProperty("speakerVoiceConfigs", out var speakers)
            && speakers.ValueKind == JsonValueKind.Array)
        {
            foreach (var speaker in speakers.EnumerateArray())
            {
                if (speaker.TryGetProperty("voiceConfig", out var voiceConfig) && voiceConfig.TryGetProperty("voice", out _))
                {
                    custom = true;
                }
            }
        }

        return new GoogleSpeechInspection(transcript, custom);
    }
}

/// <summary>Transcript text and whether the voice is a custom voice.</summary>
public sealed class GoogleSpeechInspection
{
    /// <summary>Creates an inspection.</summary>
    public GoogleSpeechInspection(string text, bool usesCustomVoice)
    {
        Text = text ?? string.Empty;
        UsesCustomVoice = usesCustomVoice;
    }

    /// <summary>Transcript that will be spoken.</summary>
    public string Text { get; }

    /// <summary>True when the voice is a custom voice Gemini does not accept.</summary>
    public bool UsesCustomVoice { get; }
}

/// <summary>Gemini text-to-speech. Posts to <c>generateContent</c> with an audio response modality.</summary>
public sealed class GoogleSpeechModel : ISpeechModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates a speech model.</summary>
    public GoogleSpeechModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider
    {
        get
        {
            return _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.speech"
                : _provider.ModelProvider;
        }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Warnings from the last call.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; private set; } = Array.Empty<GoogleWarning>();

    /// <inheritdoc />
    public Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        return GenerateAsync(new GoogleSpeechCall(options.Text) { Voice = options.Voice }, cancellationToken);
    }

    /// <summary>Synthesizes speech with Gemini-specific options.</summary>
    public async Task<SpeechResult> GenerateAsync(GoogleSpeechCall call, CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, Provider, call);
        Warnings = prepared.Warnings;
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            GoogleJson.Write(prepared.Body),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        var audio = Array.Empty<byte>();
        var media = "audio/wav";
        if (document.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out var inline))
                {
                    media = GoogleJson.String(inline, "mimeType") ?? media;
                    var data = GoogleJson.String(inline, "data");
                    if (!string.IsNullOrEmpty(data))
                    {
                        audio = Convert.FromBase64String(data!);
                    }
                }
            }
        }

        return new SpeechResult(audio, media, null);
    }

    /// <summary>Builds the speech request body and warnings.</summary>
    public static GoogleSpeechPreparation Prepare(string modelId, string provider, GoogleSpeechCall call)
    {
        var warnings = new List<GoogleWarning>();
        var vertex = provider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0;
        var names = vertex ? new[] { "googleVertex", "vertex", "google" } : new[] { "google" };
        JsonElement google = default;
        if (call.ProviderOptions != null)
        {
            foreach (var name in names)
            {
                if (call.ProviderOptions.TryGetValue(name, out var value))
                {
                    google = value;
                    break;
                }
            }
        }

        var inspection = GoogleSpeechInput.Inspect(call.Text, call.Voice, google.ValueKind == JsonValueKind.Object ? google : null);
        if (inspection.UsesCustomVoice)
        {
            throw new ArgumentException("Custom voices are not supported. Use a prebuilt voice instead.", nameof(call));
        }

        if (inspection.Text.Length == 0)
        {
            throw new ArgumentException("Speech input must contain a non-empty transcript.", nameof(call));
        }

        var structured = !modelId.StartsWith("gemini-2.5-", StringComparison.Ordinal) && !modelId.StartsWith("gemini-3.1-", StringComparison.Ordinal);
        var voice = string.IsNullOrEmpty(call.Voice) ? "Kore" : call.Voice!;
        JsonObject speechConfig;
        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("multiSpeakerVoiceConfig", out var multi))
        {
            speechConfig = new JsonObject { ["multiSpeakerVoiceConfig"] = JsonNode.Parse(multi.GetRawText()) };
        }
        else
        {
            speechConfig = new JsonObject { ["voiceConfig"] = new JsonObject { ["prebuiltVoiceConfig"] = new JsonObject { ["voiceName"] = voice } } };
        }

        var prompt = call.Text;
        if (!string.IsNullOrEmpty(call.Instructions) && !structured)
        {
            if (speechConfig.ContainsKey("multiSpeakerVoiceConfig"))
            {
                warnings.Add(GoogleWarning.Unsupported("instructions", "Google Gemini TTS ignores instructions when multiSpeakerVoiceConfig is set."));
            }
            else
            {
                prompt = call.Instructions + ": " + call.Text;
            }
        }

        if (call.Speed != null)
        {
            warnings.Add(GoogleWarning.Unsupported("speed", "Google Gemini TTS models do not support the speed option. It was ignored."));
        }

        if (call.Language != null)
        {
            warnings.Add(GoogleWarning.Unsupported("language", "Google Gemini TTS models do not support the language option."));
        }

        var part = new JsonObject { ["text"] = prompt };
        var generation = new JsonObject
        {
            ["responseModalities"] = new JsonArray(JsonValue.Create("AUDIO")),
            ["speechConfig"] = speechConfig,
        };
        if (structured && call.OutputFormat != null)
        {
            var mime = call.OutputFormat == "pcm" || call.OutputFormat == "audio/l16" ? "AUDIO_L16" : "AUDIO_WAV";
            if (call.OutputFormat != "wav" && call.OutputFormat != "audio/wav" && call.OutputFormat != "pcm" && call.OutputFormat != "audio/l16")
            {
                warnings.Add(GoogleWarning.Unsupported("outputFormat", "Unsupported output format: " + call.OutputFormat + ". Using wav instead."));
            }
            else
            {
                generation["responseFormat"] = new JsonObject { ["audio"] = new JsonObject { ["mimeType"] = mime } };
            }
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray { new JsonObject { ["role"] = "user", ["parts"] = new JsonArray(part) } },
            ["generationConfig"] = generation,
        };
        return new GoogleSpeechPreparation(body, warnings);
    }
}

/// <summary>A prepared Gemini speech request.</summary>
public sealed class GoogleSpeechPreparation
{
    /// <summary>Creates a preparation.</summary>
    public GoogleSpeechPreparation(JsonObject body, IReadOnlyList<GoogleWarning> warnings)
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>Request JSON.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }
}
