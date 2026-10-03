// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Cloud Text-to-Speech <c>text:synthesize</c> for Chirp 3 HD voices.</summary>
public sealed class GoogleVertexCloudSpeechModel : ISpeechModel
{
    /// <summary>Synthesize endpoint. Cloud TTS uses one non-regional host.</summary>
    public const string SynthesizeUrl = "https://texttospeech.googleapis.com/v1/text:synthesize";

    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Cloud TTS model.</summary>
    public GoogleVertexCloudSpeechModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.speech"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Warnings from the last call.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; private set; } = Array.Empty<GoogleWarning>();

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options.Text, options.Voice, null, null, null);
        Warnings = prepared.Warnings;
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, new Uri(SynthesizeUrl), GoogleJson.Write(prepared.Body), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        var audio = Array.Empty<byte>();
        if (document.RootElement.TryGetProperty("audioContent", out var content) && content.ValueKind == System.Text.Json.JsonValueKind.String && !string.IsNullOrEmpty(content.GetString()))
        {
            audio = Convert.FromBase64String(content.GetString()!);
        }

        return new SpeechResult(audio, "audio/wav", null);
    }

    /// <summary>Builds the synthesize body.</summary>
    public static GoogleSpeechPreparation Prepare(string text, string? voice, string? language, double? speed, string? instructions)
    {
        var warnings = new List<GoogleWarning>();
        const string infix = "Chirp3-HD";
        var selected = string.IsNullOrEmpty(voice) ? "Kore" : voice!;
        string voiceName;
        string languageCode;
        if (selected.IndexOf(infix, StringComparison.Ordinal) >= 0)
        {
            voiceName = selected;
            var prefix = selected.Substring(0, selected.IndexOf(infix, StringComparison.Ordinal)).TrimEnd('-');
            languageCode = language ?? (prefix.Length == 0 ? "en-US" : prefix);
        }
        else
        {
            languageCode = language ?? "en-US";
            voiceName = languageCode + "-" + infix + "-" + selected;
        }

        if (!string.IsNullOrEmpty(instructions))
        {
            warnings.Add(GoogleWarning.Unsupported("instructions", "Google Cloud Text-to-Speech Chirp 3: HD voices do not support the instructions option. It was ignored."));
        }

        var audio = new JsonObject { ["audioEncoding"] = "LINEAR16" };
        if (speed != null)
        {
            audio["speakingRate"] = speed.Value;
        }

        var body = new JsonObject
        {
            ["input"] = new JsonObject { ["text"] = text ?? string.Empty },
            ["voice"] = new JsonObject { ["languageCode"] = languageCode, ["name"] = voiceName },
            ["audioConfig"] = audio,
        };
        return new GoogleSpeechPreparation(body, warnings);
    }
}

/// <summary>Cloud Speech-to-Text recognize.</summary>
public sealed class GoogleVertexSpeechTranscriptionModel : ITranscriptionModel
{
    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public GoogleVertexSpeechTranscriptionModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.transcription"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var url = GoogleVertexEndpoints.RecognizeUrl(_provider.Vertex.Project, _provider.Vertex.Region);
        var body = Request(ModelId, audio.Data, null);
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, new Uri(url), GoogleJson.Write(body), _provider.Headers(), cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(ReadTranscript(document.RootElement), null);
    }

    /// <summary>Builds the recognize body. Audio bytes are base64-encoded.</summary>
    public static JsonObject Request(string modelId, byte[] audio, string[]? languageCodes)
    {
        var codes = new JsonArray();
        if (languageCodes == null || languageCodes.Length == 0)
        {
            codes.Add(JsonValue.Create("auto"));
        }
        else
        {
            foreach (var code in languageCodes)
            {
                codes.Add(JsonValue.Create(code));
            }
        }

        return new JsonObject
        {
            ["config"] = new JsonObject
            {
                ["model"] = modelId,
                ["languageCodes"] = codes,
                ["autoDecodingConfig"] = new JsonObject(),
                ["features"] = new JsonObject { ["enableWordTimeOffsets"] = true, ["enableAutomaticPunctuation"] = true },
            },
            ["content"] = Convert.ToBase64String(audio ?? Array.Empty<byte>()),
        };
    }

    /// <summary>Maps a BCP-47 tag onto a two-letter ISO-639-1 code.</summary>
    public static string? ToIso6391(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        var language = tag!;
        var dash = language.IndexOf('-');
        if (dash > 0)
        {
            language = language.Substring(0, dash);
        }

        return language.Length == 2 ? language.ToLowerInvariant() : null;
    }

    /// <summary>Parses a duration such as <c>1.200s</c>.</summary>
    public static double? ParseDuration(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var text = value!.EndsWith("s", StringComparison.Ordinal) ? value.Substring(0, value.Length - 1) : value;
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) ? seconds : null;
    }

    private static string ReadTranscript(System.Text.Json.JsonElement root)
    {
        if (!root.TryGetProperty("results", out var results))
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var result in results.EnumerateArray())
        {
            if (result.TryGetProperty("alternatives", out var alternatives) && alternatives.GetArrayLength() > 0)
            {
                var transcript = GoogleJson.String(alternatives[0], "transcript");
                if (!string.IsNullOrEmpty(transcript))
                {
                    parts.Add(transcript!);
                }
            }
        }

        return string.Join(" ", parts).Trim();
    }

}

/// <summary>Vertex Gemini transcription through generateContent.</summary>
public sealed class GoogleVertexGeminiTranscriptionModel : ITranscriptionModel
{
    private readonly GoogleVertexProvider _provider;

    /// <summary>Creates a Gemini transcription model.</summary>
    public GoogleVertexGeminiTranscriptionModel(GoogleVertexProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "google.vertex.transcription"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        if (GoogleTranscriptionModel.IsLive(ModelId))
        {
            throw new ArgumentException("Model '" + ModelId + "' only supports streaming transcription.", nameof(ModelId));
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["parts"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["inlineData"] = new JsonObject
                            {
                                ["mimeType"] = audio.MediaType,
                                ["data"] = Convert.ToBase64String(audio.Data),
                            },
                        },
                    },
                },
            },
            ["generationConfig"] = new JsonObject { ["audioTranscriptionConfig"] = new JsonObject() },
        };
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            GoogleJson.Write(body),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        var text = string.Empty;
        if (document.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            foreach (var part in candidates[0].GetProperty("content").GetProperty("parts").EnumerateArray())
            {
                text += GoogleJson.String(part, "text");
            }
        }

        return new TranscriptionResult(text, null);
    }
}
