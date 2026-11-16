// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;
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

    /// <summary>Language code forwarded as <c>speechConfig.languageCode</c>. <c>auto</c> lets Gemini detect it.</summary>
    public string? Language { get; set; }

    /// <summary>Output format such as <c>wav</c> or <c>pcm</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Provider options under <c>google</c>, <c>googleVertex</c>, or <c>vertex</c>.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }

    /// <summary>Extra request headers.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }
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
            && config.ValueKind == JsonValueKind.Object
            && config.TryGetProperty("speakerVoiceConfigs", out var speakers)
            && speakers.ValueKind == JsonValueKind.Array)
        {
            foreach (var speaker in speakers.EnumerateArray())
            {
                if (speaker.ValueKind == JsonValueKind.Object
                    && speaker.TryGetProperty("voiceConfig", out var voiceConfig)
                    && voiceConfig.ValueKind == JsonValueKind.Object
                    && voiceConfig.TryGetProperty("voice", out _))
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
public sealed class GoogleSpeechModel : ISpeechModel, ISpeechCaller
{
    private const int DefaultSampleRate = 24000;

    private static readonly Regex SampleRatePattern = new("rate=(\\d+)", RegexOptions.None, TimeSpan.FromSeconds(1));
    private static readonly Regex PcmMimeType = new("^audio/(?:l16|pcm)(?:;|\\z)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

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
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var result = await GenerateAsync(new GoogleSpeechCall(options.Text)
        {
            Voice = options.Voice,
            Instructions = options.Instructions,
            Speed = options.Speed,
            Language = options.Language,
            OutputFormat = options.OutputFormat,
        }, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType, null);
    }

    /// <inheritdoc />
    public async Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var speech = new GoogleSpeechCall(call.Text)
        {
            Voice = call.Voice,
            Instructions = call.Instructions,
            Speed = call.Speed,
            Language = call.Language,
            OutputFormat = call.OutputFormat,
            Headers = call.Headers.ToDictionary(pair => pair.Key, pair => (string?)pair.Value),
        };
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            speech.ProviderOptions = call.ProviderOptions.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
        }

        var result = await GenerateAsync(speech, cancellationToken).ConfigureAwait(false);
        var warnings = result.Warnings
            .Select(warning => new OperationWarning(warning.Type, warning.Message, warning.Feature, warning.Details))
            .ToList();
        var metadata = new JsonObject
        {
            ["google"] = new JsonObject { ["sampleRate"] = result.SampleRate, ["mimeType"] = result.MimeType },
        };
        return new SpeechModelResult(
            result.Audio,
            warnings,
            JsonSerializer.SerializeToElement(metadata),
            new ProviderResponse(result.Headers, result.ResponseBody, timestamp: result.Timestamp.UtcDateTime, modelId: ModelId));
    }

    /// <summary>Synthesizes speech with Gemini-specific options.</summary>
    public async Task<GoogleSpeechResult> GenerateAsync(GoogleSpeechCall call, CancellationToken cancellationToken)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var timestamp = _provider.Clock();
        var prepared = Prepare(ModelId, Provider, call);
        Warnings = prepared.Warnings;
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        if (call.Headers != null)
        {
            foreach (var header in call.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        var requestBody = GoogleJson.Write(prepared.Body);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, GoogleModelPath.Get(ModelId) + ":generateContent"),
            requestBody,
            headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;

        // generateSpeech returns a single audio result and Gemini returns one inline audio part per request,
        // so take the first inline-data part.
        string? base64Audio = null;
        string? mimeType = null;
        if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
        {
            foreach (var candidate in candidates.EnumerateArray())
            {
                if (candidate.ValueKind == JsonValueKind.Object
                    && candidate.TryGetProperty("content", out var content)
                    && content.ValueKind == JsonValueKind.Object
                    && content.TryGetProperty("parts", out var parts)
                    && parts.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.ValueKind == JsonValueKind.Object
                            && part.TryGetProperty("inlineData", out var inline)
                            && inline.ValueKind == JsonValueKind.Object
                            && !string.IsNullOrEmpty(GoogleJson.String(inline, "data")))
                        {
                            base64Audio = GoogleJson.String(inline, "data");
                            mimeType = GoogleJson.String(inline, "mimeType");
                            break;
                        }
                    }
                }

                if (base64Audio != null)
                {
                    break;
                }
            }
        }

        var sampleRate = ParseSampleRate(mimeType) ?? DefaultSampleRate;
        var bytes = base64Audio != null ? Convert.FromBase64String(base64Audio) : Array.Empty<byte>();

        // Older models return PCM, which needs a container for default WAV output.
        // Gemini 3.8 returns WAV itself, and adding another header corrupts that audio.
        var isPcm = PcmMimeType.IsMatch(mimeType ?? string.Empty) || (mimeType == null && !prepared.UsesStructuredSpeech);
        var audio = prepared.OutputFormat == "AUDIO_WAV" && isPcm && bytes.Length > 0 ? AddWavHeader(bytes, sampleRate) : bytes;

        var warnings = new List<GoogleWarning>(prepared.Warnings);
        if (prepared.OutputFormat == "AUDIO_L16" && bytes.Length > 0 && !prepared.UsesStructuredSpeech)
        {
            warnings.Add(GoogleWarning.Unsupported(
                "outputFormat",
                "Returning raw PCM audio (signed 16-bit little-endian, mono, " + sampleRate.ToString(CultureInfo.InvariantCulture) + " Hz). "
                + "These bytes have no container header and are not directly playable; "
                + "see providerMetadata.google for the sample rate and mime type."));
        }

        JsonObject? usage = null;
        if (root.TryGetProperty("usageMetadata", out var usageMetadata) && usageMetadata.ValueKind == JsonValueKind.Object)
        {
            usage = JsonNode.Parse(usageMetadata.GetRawText()) as JsonObject;
        }

        return new GoogleSpeechResult(
            audio,
            mimeType ?? "audio/wav",
            warnings,
            usage,
            requestBody,
            root.Clone(),
            response.Headers,
            timestamp,
            ModelId,
            sampleRate,
            mimeType);
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
            throw new ArgumentException("Custom voices are not supported. Use a prebuilt voice instead.", "voice");
        }

        if (inspection.Text.Length == 0)
        {
            throw new ArgumentException("Speech input must contain a non-empty transcript.", "text");
        }

        var structured = !modelId.StartsWith("gemini-2.5-", StringComparison.Ordinal) && !modelId.StartsWith("gemini-3.1-", StringComparison.Ordinal);
        var voice = string.IsNullOrEmpty(call.Voice) ? "Kore" : call.Voice!;
        var languageCode = call.Language == "auto" ? null : call.Language;
        JsonObject speechConfig;
        if (google.ValueKind == JsonValueKind.Object && google.TryGetProperty("multiSpeakerVoiceConfig", out var multi))
        {
            speechConfig = new JsonObject { ["multiSpeakerVoiceConfig"] = JsonNode.Parse(multi.GetRawText()) };
        }
        else
        {
            speechConfig = new JsonObject { ["voiceConfig"] = new JsonObject { ["prebuiltVoiceConfig"] = new JsonObject { ["voiceName"] = voice } } };
        }

        if (languageCode != null)
        {
            speechConfig["languageCode"] = languageCode;
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

        var part = new JsonObject { ["text"] = prompt };
        var generation = new JsonObject
        {
            ["responseModalities"] = new JsonArray(JsonValue.Create("AUDIO")),
            ["speechConfig"] = speechConfig,
        };
        var formats = structured
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["wav"] = "AUDIO_WAV",
                ["audio/wav"] = "AUDIO_WAV",
                ["pcm"] = "AUDIO_L16",
                ["audio/l16"] = "AUDIO_L16",
                ["mulaw"] = "AUDIO_MULAW",
                ["audio/mulaw"] = "AUDIO_MULAW",
                ["alaw"] = "AUDIO_ALAW",
                ["audio/alaw"] = "AUDIO_ALAW",
            }
            : new Dictionary<string, string>(StringComparer.Ordinal) { ["wav"] = "AUDIO_WAV", ["pcm"] = "AUDIO_L16" };
        var resolvedOutputFormat = "wav";
        if (call.OutputFormat != null && formats.ContainsKey(call.OutputFormat))
        {
            resolvedOutputFormat = call.OutputFormat;
        }
        else if (call.OutputFormat != null)
        {
            warnings.Add(GoogleWarning.Unsupported("outputFormat", "Unsupported output format: " + call.OutputFormat + ". Using wav instead."));
        }

        if (structured && call.OutputFormat != null)
        {
            generation["responseFormat"] = new JsonObject { ["audio"] = new JsonObject { ["mimeType"] = formats[resolvedOutputFormat] } };
        }

        var body = new JsonObject
        {
            ["contents"] = new JsonArray { new JsonObject { ["role"] = "user", ["parts"] = new JsonArray(part) } },
            ["generationConfig"] = generation,
        };
        return new GoogleSpeechPreparation(body, warnings, formats[resolvedOutputFormat], structured);
    }

    /// <summary>Parses the sample rate from a PCM mime type such as <c>audio/L16;rate=24000</c>.</summary>
    private static int? ParseSampleRate(string? mimeType)
    {
        if (mimeType == null)
        {
            return null;
        }

        var match = SampleRatePattern.Match(mimeType);
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var rate) ? rate : null;
    }

    /// <summary>Wraps raw signed 16-bit little-endian mono PCM in a minimal 44-byte WAV (RIFF/WAVE) container.</summary>
    private static byte[] AddWavHeader(byte[] pcm, int sampleRate)
    {
        const int numChannels = 1;
        const int bitsPerSample = 16;
        const int blockAlign = numChannels * bitsPerSample / 8;
        var byteRate = sampleRate * blockAlign;
        var dataSize = pcm.Length;
        var output = new byte[44 + dataSize];
        WriteAscii(output, 0, "RIFF");
        WriteUInt32(output, 4, (uint)(36 + dataSize));
        WriteAscii(output, 8, "WAVE");
        WriteAscii(output, 12, "fmt ");
        WriteUInt32(output, 16, 16);
        WriteUInt16(output, 20, 1);
        WriteUInt16(output, 22, numChannels);
        WriteUInt32(output, 24, (uint)sampleRate);
        WriteUInt32(output, 28, (uint)byteRate);
        WriteUInt16(output, 32, blockAlign);
        WriteUInt16(output, 34, bitsPerSample);
        WriteAscii(output, 36, "data");
        WriteUInt32(output, 40, (uint)dataSize);
        Buffer.BlockCopy(pcm, 0, output, 44, dataSize);
        return output;
    }

    private static void WriteAscii(byte[] buffer, int offset, string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            buffer[offset + i] = (byte)text[i];
        }
    }

    private static void WriteUInt16(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteUInt32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}

/// <summary>A prepared Gemini speech request.</summary>
public sealed class GoogleSpeechPreparation
{
    /// <summary>Creates a preparation.</summary>
    public GoogleSpeechPreparation(JsonObject body, IReadOnlyList<GoogleWarning> warnings, string outputFormat = "AUDIO_WAV", bool usesStructuredSpeech = false)
    {
        Body = body;
        Warnings = warnings;
        OutputFormat = outputFormat;
        UsesStructuredSpeech = usesStructuredSpeech;
    }

    /// <summary>Request JSON.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }

    /// <summary>Resolved Gemini audio format, such as <c>AUDIO_WAV</c>.</summary>
    public string OutputFormat { get; }

    /// <summary>True for Gemini 3.8 style models, which take structured speech metadata.</summary>
    public bool UsesStructuredSpeech { get; }
}

/// <summary>Speech audio plus the response metadata recorded for the call.</summary>
public sealed class GoogleSpeechResult
{
    /// <summary>Creates a speech result.</summary>
    public GoogleSpeechResult(
        byte[] audio,
        string mediaType,
        IReadOnlyList<GoogleWarning> warnings,
        JsonObject? usage,
        string requestBody,
        JsonElement responseBody,
        IReadOnlyDictionary<string, string> headers,
        DateTimeOffset timestamp,
        string modelId,
        int sampleRate,
        string? mimeType)
    {
        Audio = audio;
        MediaType = mediaType;
        Warnings = warnings;
        Usage = usage;
        RequestBody = requestBody;
        ResponseBody = responseBody;
        Headers = headers;
        Timestamp = timestamp;
        ModelId = modelId;
        SampleRate = sampleRate;
        MimeType = mimeType;
    }

    /// <summary>Audio bytes. Raw PCM is wrapped in a WAV container unless <c>pcm</c> was requested.</summary>
    public byte[] Audio { get; }

    /// <summary>Media type of the audio.</summary>
    public string MediaType { get; }

    /// <summary>Warnings produced for the call.</summary>
    public IReadOnlyList<GoogleWarning> Warnings { get; }

    /// <summary>Raw <c>usageMetadata</c> from the response, when present.</summary>
    public JsonObject? Usage { get; }

    /// <summary>Body sent to the provider. Maps to <c>request.body</c>.</summary>
    public string RequestBody { get; }

    /// <summary>Raw response body. Maps to <c>response.body</c>.</summary>
    public JsonElement ResponseBody { get; }

    /// <summary>HTTP response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Timestamp recorded for the call.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id used for the call.</summary>
    public string ModelId { get; }

    /// <summary>Sample rate from the response mime type, or 24000. Maps to <c>providerMetadata.google.sampleRate</c>.</summary>
    public int SampleRate { get; }

    /// <summary>Response mime type, when the provider sent one. Maps to <c>providerMetadata.google.mimeType</c>.</summary>
    public string? MimeType { get; }
}
