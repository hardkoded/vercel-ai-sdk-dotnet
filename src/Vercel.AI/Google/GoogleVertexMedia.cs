// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
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
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, new Uri(SynthesizeUrl), GoogleJson.Write(prepared.Body), headers, cancellationToken).ConfigureAwait(false);
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
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        using var document = await _provider.Http.SendJsonAsync(HttpMethod.Post, new Uri(url), GoogleJson.Write(body), headers, cancellationToken).ConfigureAwait(false);
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
public sealed class GoogleVertexGeminiTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private static readonly string[] OptionNamespaces = { "googleVertex", "vertex", "google" };
    private static readonly string[] ConfigFields = { "languageCodes", "customVocabulary", "wordTimestamp", "diarization", "mode" };

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
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>False. The Vertex Live WebSocket is not opened by this port.</summary>
    public bool CanStream
    {
        get { return false; }
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var call = new TranscriptionModelCall(audio.Data, audio.MediaType, default, new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text ?? string.Empty, null);
    }

    /// <summary>
    /// Transcribes audio. Options are read from <c>googleVertex</c>, then <c>vertex</c>, then <c>google</c>.
    /// Text parts win over the <c>audioTranscription</c> text, which also carries the language and word timings.
    /// </summary>
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
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
                                ["mimeType"] = call.MediaType,
                                ["data"] = Convert.ToBase64String(call.Audio),
                            },
                        },
                    },
                },
            },
        };
        if (TranscriptionConfig(call.ProviderOptions) is { } config)
        {
            body["generationConfig"] = new JsonObject { ["audioTranscriptionConfig"] = config };
        }

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            GoogleJson.Write(body),
            headers,
            cancellationToken).ConfigureAwait(false);
        var root = document.RootElement;
        var plainText = string.Empty;
        var transcriptionText = string.Empty;
        string? language = null;
        var segments = new List<TranscriptSegment>();
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0
            && GoogleJson.TryObject(candidates[0], "content", out var content)
            && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                plainText += GoogleJson.String(part, "text");
                if (!GoogleJson.TryObject(part, "audioTranscription", out var transcription))
                {
                    continue;
                }

                transcriptionText += GoogleJson.String(transcription, "text");
                language ??= GoogleJson.String(transcription, "languageCode");
                if (!transcription.TryGetProperty("words", out var words) || words.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var word in words.EnumerateArray())
                {
                    var text = GoogleJson.String(word, "word");
                    var start = GoogleVertexSpeechTranscriptionModel.ParseDuration(GoogleJson.String(word, "startOffset"));
                    var end = GoogleVertexSpeechTranscriptionModel.ParseDuration(GoogleJson.String(word, "endOffset"));
                    if (text != null && start != null && end != null)
                    {
                        segments.Add(new TranscriptSegment(text, start.Value, end.Value));
                    }
                }
            }
        }

        JsonElement? metadata = null;
        if (GoogleJson.TryObject(root, "usageMetadata", out var usage))
        {
            metadata = JsonSerializer.SerializeToElement(new JsonObject { ["google"] = new JsonObject { ["usageMetadata"] = JsonNode.Parse(usage.GetRawText()) } });
        }

        return new TranscriptionModelResult(
            plainText.Length > 0 ? plainText : transcriptionText,
            segments,
            language,
            providerMetadata: metadata,
            response: new ProviderResponse(body: root.Clone(), timestamp: DateTime.UtcNow, modelId: ModelId));
    }

    /// <summary>Returns null. The Vertex Live WebSocket is not opened by this port.</summary>
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
    }

    private static JsonObject? TranscriptionConfig(JsonElement providerOptions)
    {
        foreach (var name in OptionNamespaces)
        {
            if (!GoogleJson.TryObject(providerOptions, name, out var options))
            {
                continue;
            }

            var config = new JsonObject();
            foreach (var field in ConfigFields)
            {
                if (options.TryGetProperty(field, out var value))
                {
                    GoogleJson.Set(config, field, GoogleJson.Clone(value));
                }
            }

            return config.Count > 0 ? config : null;
        }

        return null;
    }
}
