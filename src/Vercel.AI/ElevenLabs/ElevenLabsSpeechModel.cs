// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.ElevenLabs;

/// <summary>A setting ElevenLabs speech ignored.</summary>
public sealed class ElevenLabsSpeechWarning
{
    /// <summary>Creates a warning.</summary>
    public ElevenLabsSpeechWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning category.</summary>
    public string Type { get; }

    /// <summary>Feature that was ignored.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>ElevenLabs text-to-speech request.</summary>
public sealed class ElevenLabsSpeechRequest
{
    /// <summary>Default voice used when <see cref="Voice"/> is empty.</summary>
    public const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM";

    /// <summary>Creates a request.</summary>
    public ElevenLabsSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id. Empty uses <see cref="DefaultVoiceId"/>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format. Empty uses <c>mp3_44100_128</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>BCP-47 language code sent as <c>language_code</c>.</summary>
    public string? Language { get; set; }

    /// <summary>Speaking rate stored under <c>voice_settings.speed</c>.</summary>
    public double? Speed { get; set; }

    /// <summary>Ignored. Produces a warning.</summary>
    public string? Instructions { get; set; }

    /// <summary>ElevenLabs provider options. A wrapping <c>elevenlabs</c> object is accepted.</summary>
    public JsonElement? ProviderOptions { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>ElevenLabs speech audio.</summary>
public sealed class ElevenLabsSpeechGeneration
{
    /// <summary>Creates a result.</summary>
    public ElevenLabsSpeechGeneration(byte[] audio, string mediaType, string modelId, DateTimeOffset timestamp, IReadOnlyDictionary<string, string> responseHeaders, IReadOnlyList<ElevenLabsSpeechWarning> warnings)
    {
        Audio = audio ?? Array.Empty<byte>();
        MediaType = mediaType ?? "audio/mpeg";
        ModelId = modelId ?? string.Empty;
        Timestamp = timestamp;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        Warnings = warnings ?? Array.Empty<ElevenLabsSpeechWarning>();
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Media type from the response.</summary>
    public string MediaType { get; }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Warnings for unsupported settings.</summary>
    public IReadOnlyList<ElevenLabsSpeechWarning> Warnings { get; }
}

/// <summary>ElevenLabs speech model. Posts JSON to <c>/v1/text-to-speech/{voice}</c>.</summary>
public sealed class ElevenLabsSpeechModel : ISpeechModel
{
    private static readonly Dictionary<string, string> Formats = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["mp3"] = "mp3_44100_128",
        ["mp3_32"] = "mp3_44100_32",
        ["mp3_64"] = "mp3_44100_64",
        ["mp3_96"] = "mp3_44100_96",
        ["mp3_128"] = "mp3_44100_128",
        ["mp3_192"] = "mp3_44100_192",
        ["pcm"] = "pcm_44100",
        ["pcm_16000"] = "pcm_16000",
        ["pcm_22050"] = "pcm_22050",
        ["pcm_24000"] = "pcm_24000",
        ["pcm_44100"] = "pcm_44100",
        ["ulaw"] = "ulaw_8000",
    };

    private readonly HttpClient _http;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a speech model.</summary>
    public ElevenLabsSpeechModel(HttpClient http, string modelId, string provider, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = string.IsNullOrEmpty(provider) ? "elevenlabs.speech" : provider;
        BaseUrl = string.IsNullOrEmpty(baseUrl) ? ElevenLabsProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>API origin without a trailing slash.</summary>
    public string BaseUrl { get; }

    /// <summary>Clock used for <see cref="ElevenLabsSpeechGeneration.Timestamp"/>.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var result = await GenerateAsync(new ElevenLabsSpeechRequest(options.Text) { Voice = options.Voice }, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, result.MediaType);
    }

    /// <summary>Synthesizes speech and returns the audio bytes.</summary>
    public async Task<ElevenLabsSpeechGeneration> GenerateAsync(ElevenLabsSpeechRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var warnings = new List<ElevenLabsSpeechWarning>();
        if (!string.IsNullOrEmpty(request.Instructions))
        {
            warnings.Add(new ElevenLabsSpeechWarning(
                "unsupported",
                "instructions",
                "ElevenLabs speech models do not support instructions. Instructions parameter was ignored."));
        }

        var voice = string.IsNullOrEmpty(request.Voice) ? ElevenLabsSpeechRequest.DefaultVoiceId : request.Voice!;
        var format = string.IsNullOrEmpty(request.OutputFormat) ? "mp3_44100_128" : request.OutputFormat!;
        if (!Formats.TryGetValue(format, out var mapped) || mapped is null)
        {
            mapped = format;
        }

        var body = new JsonObject
        {
            ["text"] = request.Text,
            ["model_id"] = ModelId,
        };
        if (!string.IsNullOrEmpty(request.Language))
        {
            body["language_code"] = request.Language;
        }

        var options = Unwrap(request.ProviderOptions);
        var settings = new JsonObject();
        if (request.Speed is double speed)
        {
            settings["speed"] = speed;
        }

        string? languageFromOptions = null;
        if (options is { } element)
        {
            ApplyOptions(element, body, settings, out languageFromOptions);
        }

        if (body["language_code"] == null && !string.IsNullOrEmpty(languageFromOptions))
        {
            body["language_code"] = languageFromOptions;
        }

        if (settings.Count > 0)
        {
            body["voice_settings"] = settings;
        }

        var url = ApiKeys.Combine(BaseUrl, "/v1/text-to-speech/" + Uri.EscapeDataString(voice));
        var withQuery = new Uri(url.AbsoluteUri + "?output_format=" + Uri.EscapeDataString(mapped));
        var response = await MediaExchange.SendAsync(_http, HttpMethod.Post, withQuery, MediaExchange.Json(body.ToJsonString()), Merge(request.Headers), cancellationToken).ConfigureAwait(false);
        if (response.StatusCode < 200 || response.StatusCode > 299)
        {
            if (ElevenLabsError.TryParse(response.Text, out var error) && error != null)
            {
                throw new AiSdkException(error.Message);
            }

            throw new AiSdkException(JsonValues.ExtractErrorMessage(response.Text, response.StatusCode));
        }

        var mediaType = "audio/mpeg";
        if (response.Headers.TryGetValue("content-type", out var contentType) && !string.IsNullOrEmpty(contentType))
        {
            var separator = contentType.IndexOf(';');
            mediaType = separator < 0 ? contentType : contentType.Substring(0, separator);
        }

        return new ElevenLabsSpeechGeneration(response.Body, mediaType, ModelId, Clock(), response.Headers, warnings);
    }

    private static void ApplyOptions(JsonElement options, JsonObject body, JsonObject settings, out string? language)
    {
        language = null;
        if (options.TryGetProperty("voiceSettings", out var voice) && voice.ValueKind == JsonValueKind.Object)
        {
            CopyNumber(voice, "stability", settings, "stability");
            CopyNumber(voice, "similarityBoost", settings, "similarity_boost");
            CopyNumber(voice, "style", settings, "style");
            CopyBool(voice, "useSpeakerBoost", settings, "use_speaker_boost");
        }

        if (options.TryGetProperty("languageCode", out var code) && code.ValueKind == JsonValueKind.String)
        {
            language = code.GetString();
        }

        if (options.TryGetProperty("seed", out var seed) && seed.TryGetInt32(out var seedValue))
        {
            body["seed"] = seedValue;
        }

        CopyString(options, "previousText", body, "previous_text");
        CopyString(options, "nextText", body, "next_text");
        CopyRaw(options, "previousRequestIds", body, "previous_request_ids");
        CopyRaw(options, "nextRequestIds", body, "next_request_ids");
        CopyString(options, "applyTextNormalization", body, "apply_text_normalization");
        CopyBool(options, "applyLanguageTextNormalization", body, "apply_language_text_normalization");
        if (options.TryGetProperty("pronunciationDictionaryLocators", out var locators) && locators.ValueKind == JsonValueKind.Array)
        {
            var mapped = new JsonArray();
            foreach (var locator in locators.EnumerateArray())
            {
                var item = new JsonObject();
                if (locator.TryGetProperty("pronunciationDictionaryId", out var id) && id.ValueKind == JsonValueKind.String)
                {
                    item["pronunciation_dictionary_id"] = id.GetString();
                }

                if (locator.TryGetProperty("versionId", out var version) && version.ValueKind == JsonValueKind.String)
                {
                    item["version_id"] = version.GetString();
                }

                mapped.Add(item);
            }

            body["pronunciation_dictionary_locators"] = mapped;
        }
    }

    private static void CopyNumber(JsonElement source, string from, JsonObject target, string to)
    {
        if (source.TryGetProperty(from, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            target[to] = value.GetDouble();
        }
    }

    private static void CopyBool(JsonElement source, string from, JsonObject target, string to)
    {
        if (source.TryGetProperty(from, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            target[to] = value.GetBoolean();
        }
    }

    private static void CopyString(JsonElement source, string from, JsonObject target, string to)
    {
        if (source.TryGetProperty(from, out var value) && value.ValueKind == JsonValueKind.String)
        {
            target[to] = value.GetString();
        }
    }

    private static void CopyRaw(JsonElement source, string from, JsonObject target, string to)
    {
        if (source.TryGetProperty(from, out var value) && value.ValueKind != JsonValueKind.Null && value.ValueKind != JsonValueKind.Undefined)
        {
            target[to] = JsonNode.Parse(value.GetRawText());
        }
    }

    private static JsonElement? Unwrap(JsonElement? options)
    {
        if (options is not { } element || element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.TryGetProperty("elevenlabs", out var nested) && nested.ValueKind == JsonValueKind.Object)
        {
            return nested;
        }

        return element;
    }

    private Dictionary<string, string?> Merge(IDictionary<string, string>? requestHeaders)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in _headers())
        {
            headers[pair.Key] = pair.Value;
        }

        if (requestHeaders != null)
        {
            foreach (var pair in requestHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}
