// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Hume;

/// <summary>A warning produced while building a Hume speech request.</summary>
public sealed class HumeSpeechWarning
{
    /// <summary>Creates a warning.</summary>
    public HumeSpeechWarning(string type, string feature, string details)
    {
        Type = type ?? string.Empty;
        Feature = feature ?? string.Empty;
        Details = details ?? string.Empty;
    }

    /// <summary>Warning category, such as <c>unsupported</c>.</summary>
    public string Type { get; }

    /// <summary>Feature that was ignored.</summary>
    public string Feature { get; }

    /// <summary>Explanation.</summary>
    public string Details { get; }
}

/// <summary>Text sent to Hume text-to-speech.</summary>
public sealed class HumeSpeechRequest
{
    /// <summary>Default Hume voice used when <see cref="Voice"/> is empty.</summary>
    public const string DefaultVoiceId = "d8ab67c6-953d-4bd8-9370-8fa53a0f1453";

    /// <summary>Creates a speech request.</summary>
    public HumeSpeechRequest(string text)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text to speak.</summary>
    public string Text { get; }

    /// <summary>Voice id. Empty selects <see cref="DefaultVoiceId"/>.</summary>
    public string? Voice { get; set; }

    /// <summary>Output format: <c>mp3</c>, <c>pcm</c>, or <c>wav</c>.</summary>
    public string? OutputFormat { get; set; }

    /// <summary>Speaking rate.</summary>
    public double? Speed { get; set; }

    /// <summary>Utterance description.</summary>
    public string? Instructions { get; set; }

    /// <summary>Ignored. Hume speech does not select a language.</summary>
    public string? Language { get; set; }

    /// <summary>Headers merged over the provider headers for this call.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>Hume speech result.</summary>
public sealed class HumeSpeechGeneration
{
    /// <summary>Creates a result.</summary>
    public HumeSpeechGeneration(byte[] audio, string modelId, DateTimeOffset timestamp, IReadOnlyDictionary<string, string> responseHeaders, IReadOnlyList<HumeSpeechWarning> warnings)
    {
        Audio = audio ?? Array.Empty<byte>();
        ModelId = modelId ?? string.Empty;
        Timestamp = timestamp;
        ResponseHeaders = responseHeaders ?? new Dictionary<string, string>();
        Warnings = warnings ?? Array.Empty<HumeSpeechWarning>();
    }

    /// <summary>Audio bytes.</summary>
    public byte[] Audio { get; }

    /// <summary>Model id. Hume speech uses an empty id.</summary>
    public string ModelId { get; }

    /// <summary>Clock value captured when the call started.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Response headers, including the audio content type.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; }

    /// <summary>Warnings for ignored settings.</summary>
    public IReadOnlyList<HumeSpeechWarning> Warnings { get; }
}

/// <summary>Hume text-to-speech at <c>POST /v0/tts/file</c>.</summary>
public sealed class HumeSpeechModel : ISpeechModel
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a speech model.</summary>
    public HumeSpeechModel(HttpClient http, string modelId, string baseUrl, Func<IReadOnlyDictionary<string, string?>> headers)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        ModelId = modelId ?? string.Empty;
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? HumeProvider.DefaultBaseUrl : baseUrl.TrimEnd('/');
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <inheritdoc />
    public string Provider { get; set; } = "hume.speech";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Clock used for <see cref="HumeSpeechGeneration.Timestamp"/>.</summary>
    public Func<DateTimeOffset> Clock { get; set; } = () => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var request = new HumeSpeechRequest(options.Text) { Voice = options.Voice };
        var result = await GenerateAsync(request, cancellationToken).ConfigureAwait(false);
        var mediaType = "audio/mpeg";
        if (result.ResponseHeaders.TryGetValue("content-type", out var contentType) && !string.IsNullOrEmpty(contentType))
        {
            mediaType = contentType;
        }

        return new SpeechResult(result.Audio, mediaType);
    }

    /// <summary>Synthesizes speech and returns the audio bytes plus response metadata.</summary>
    public async Task<HumeSpeechGeneration> GenerateAsync(HumeSpeechRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var timestamp = Clock();
        var warnings = new List<HumeSpeechWarning>();
        var format = "mp3";
        if (!string.IsNullOrEmpty(request.OutputFormat))
        {
            if (request.OutputFormat == "mp3" || request.OutputFormat == "pcm" || request.OutputFormat == "wav")
            {
                format = request.OutputFormat!;
            }
            else
            {
                warnings.Add(new HumeSpeechWarning(
                    "unsupported",
                    "outputFormat",
                    "Unsupported output format: " + request.OutputFormat + ". Using mp3 instead."));
            }
        }

        if (!string.IsNullOrEmpty(request.Language))
        {
            warnings.Add(new HumeSpeechWarning(
                "unsupported",
                "language",
                "Hume speech models do not support language selection. Language parameter \"" + request.Language + "\" was ignored."));
        }

        var voice = string.IsNullOrEmpty(request.Voice) ? HumeSpeechRequest.DefaultVoiceId : request.Voice!;
        var utterance = new JsonObject
        {
            ["text"] = request.Text,
        };
        if (request.Speed is double speed)
        {
            utterance["speed"] = speed;
        }

        if (!string.IsNullOrEmpty(request.Instructions))
        {
            utterance["description"] = request.Instructions;
        }

        utterance["voice"] = new JsonObject
        {
            ["id"] = voice,
            ["provider"] = "HUME_AI",
        };
        var body = new JsonObject
        {
            ["utterances"] = new JsonArray(utterance),
            ["format"] = new JsonObject { ["type"] = format },
        };

        var response = await MediaExchange.SendAsync(
            _http,
            HttpMethod.Post,
            ApiKeys.Combine(_baseUrl, "/v0/tts/file"),
            MediaExchange.Json(body.ToJsonString()),
            Merge(request.Headers),
            cancellationToken).ConfigureAwait(false);
        if (response.StatusCode < 200 || response.StatusCode > 299)
        {
            throw ProviderHttp.MapStatus(response.StatusCode, response.Text);
        }

        return new HumeSpeechGeneration(response.Body, ModelId, timestamp, response.Headers, warnings);
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
