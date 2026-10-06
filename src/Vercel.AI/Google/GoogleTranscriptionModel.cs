// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini transcription. Unary models post to the Interactions API.</summary>
public sealed class GoogleTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates a transcription model.</summary>
    public GoogleTranscriptionModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return _provider.ModelProvider; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>True when the model id is a Live API transcription model.</summary>
    public static bool IsLive(string modelId)
    {
        return modelId != null && modelId.IndexOf("-live", StringComparison.Ordinal) >= 0;
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "v4"; }
    }

    /// <summary>False. The Gemini Live WebSocket is not opened by this port.</summary>
    public bool CanStream
    {
        get { return false; }
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        var call = new TranscriptionModelCall(audio.Data, audio.MediaType, default, new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(result.Text, null);
    }

    /// <summary>
    /// Transcribes audio through the Interactions API. <c>google</c> provider options become the
    /// <c>transcription_config</c>, and <c>word_info</c> annotations become word segments.
    /// </summary>
    public async Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
    {
        if (IsLive(ModelId))
        {
            throw new ArgumentException("Model '" + ModelId + "' only supports streaming transcription. Use a unary model such as 'gemini-3.5-transcribe'.", nameof(ModelId));
        }

        var timestamp = _provider.Clock();
        var body = BuildRequest(ModelId, new AudioInput(call.Audio, call.MediaType, null), TranscriptionConfig(call.ProviderOptions));
        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var header in call.Headers)
        {
            headers[header.Key] = header.Value;
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "interactions"),
            GoogleJson.Write(body),
            headers,
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var text = string.Empty;
        var segments = new List<TranscriptSegment>();
        if (root.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
        {
            foreach (var step in steps.EnumerateArray())
            {
                if (!step.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in content.EnumerateArray())
                {
                    if (GoogleJson.String(part, "type") != "text" || GoogleJson.String(part, "text") is not { } partText)
                    {
                        continue;
                    }

                    text += partText;
                    if (!part.TryGetProperty("annotations", out var annotations) || annotations.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var annotation in annotations.EnumerateArray())
                    {
                        var word = GoogleJson.String(annotation, "text");
                        var start = GoogleVertexSpeechTranscriptionModel.ParseDuration(GoogleJson.String(annotation, "start_offset"));
                        var end = GoogleVertexSpeechTranscriptionModel.ParseDuration(GoogleJson.String(annotation, "end_offset"));
                        if (GoogleJson.String(annotation, "type") == "word_info" && word != null && start != null && end != null)
                        {
                            segments.Add(new TranscriptSegment(word, start.Value, end.Value));
                        }
                    }
                }
            }
        }

        JsonElement? metadata = null;
        if (GoogleJson.TryObject(root, "usage", out var usage))
        {
            metadata = JsonSerializer.SerializeToElement(new JsonObject { ["google"] = new JsonObject { ["usage"] = JsonNode.Parse(usage.GetRawText()) } });
        }

        return new TranscriptionModelResult(
            text,
            segments,
            providerMetadata: metadata,
            response: new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp.UtcDateTime, modelId: ModelId));
    }

    /// <summary>Returns null. The Gemini Live WebSocket is not opened by this port.</summary>
    public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
    {
        return Task.FromResult<TranscriptionStreamStart?>(null);
    }

    /// <summary>Builds the Interactions transcription body.</summary>
    public static JsonObject BuildRequest(string modelId, AudioInput audio, JsonObject? transcriptionConfig)
    {
        var body = new JsonObject
        {
            ["model"] = modelId,
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
        if (transcriptionConfig != null)
        {
            body["generation_config"] = new JsonObject { ["transcription_config"] = transcriptionConfig };
        }

        return body;
    }

    /// <summary>Builds the Live API setup message. Live models reject a unary call.</summary>
    public static JsonObject BuildLiveSetup(string modelId)
    {
        if (!IsLive(modelId))
        {
            throw new ArgumentException("Model '" + modelId + "' does not support streaming transcription.", nameof(modelId));
        }

        return new JsonObject
        {
            ["model"] = GoogleModelPath.Get(modelId),
            ["inputAudioTranscription"] = new JsonObject(),
        };
    }

    /// <summary>Maps <c>google</c> options onto <c>transcription_config</c>, or null when none are set.</summary>
    private static JsonObject? TranscriptionConfig(JsonElement providerOptions)
    {
        if (!GoogleJson.TryObject(providerOptions, "google", out var options))
        {
            return null;
        }

        var config = new JsonObject();
        if (options.TryGetProperty("languageCodes", out var languages))
        {
            config["language_codes"] = GoogleJson.Clone(languages);
        }

        if (options.TryGetProperty("customVocabulary", out var vocabulary))
        {
            config["custom_vocabulary"] = GoogleJson.Clone(vocabulary);
        }

        var mode = GoogleJson.String(options, "mode");
        var diarization = options.TryGetProperty("diarization", out var diarizationValue) && diarizationValue.ValueKind == JsonValueKind.True;
        var wordTimestamp = options.TryGetProperty("wordTimestamp", out var wordValue) && wordValue.ValueKind == JsonValueKind.True;
        if (mode != null || diarization || wordTimestamp)
        {
            var modeObject = new JsonObject { ["type"] = (mode ?? "VERBATIM").ToLowerInvariant() };
            if (diarization)
            {
                modeObject["diarization_mode"] = "speaker";
            }

            if (wordTimestamp)
            {
                modeObject["timestamp_granularities"] = new JsonArray("word");
            }

            config["mode"] = modeObject;
        }

        return config.Count > 0 ? config : null;
    }
}

/// <summary>Speech translation configuration for the Gemini Live API. The socket itself is not opened here.</summary>
public static class GoogleSpeechTranslation
{
    /// <summary>Live translation WebSocket path.</summary>
    public const string LivePath = "google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";

    /// <summary>Builds a <c>wss</c> URL and strips a trailing <c>/v1beta</c> or <c>/v1alpha</c> segment.</summary>
    public static Uri WebSocketUrl(string baseUrl, string? apiKey)
    {
        var builder = LiveWebSocketUrl(baseUrl, LivePath);
        if (!string.IsNullOrEmpty(apiKey))
        {
            builder.Query = "key=" + Uri.EscapeDataString(apiKey!);
        }

        return builder.Uri;
    }

    /// <summary>The Live API base URL: <paramref name="baseUrl"/> without a trailing <c>/v1beta</c> or <c>/v1alpha</c>.</summary>
    internal static UriBuilder LiveBaseUrl(string baseUrl)
    {
        var url = new Uri(baseUrl, UriKind.Absolute);
        var path = url.AbsolutePath.TrimEnd('/');
        if (path.EndsWith("/v1beta", StringComparison.Ordinal) || path.EndsWith("/v1alpha", StringComparison.Ordinal))
        {
            path = path.Substring(0, path.LastIndexOf('/'));
        }

        return new UriBuilder(url) { Path = path };
    }

    /// <summary>A <c>wss</c> (or <c>ws</c> for http) Live API URL for the given bidi service path.</summary>
    internal static UriBuilder LiveWebSocketUrl(string baseUrl, string servicePath)
    {
        var builder = LiveBaseUrl(baseUrl);
        builder.Scheme = builder.Scheme == "https" ? "wss" : "ws";
        builder.Path = builder.Path.TrimEnd('/') + "/ws/" + servicePath;
        return builder;
    }

    /// <summary>Warnings for options the Live translation API does not accept.</summary>
    public static IReadOnlyList<GoogleWarning> WarningsFor(string? sourceLanguage, string? outputAudioFormat)
    {
        var warnings = new List<GoogleWarning>();
        if (!string.IsNullOrEmpty(sourceLanguage))
        {
            warnings.Add(GoogleWarning.Unsupported("sourceLanguage"));
        }

        if (!string.IsNullOrEmpty(outputAudioFormat))
        {
            warnings.Add(GoogleWarning.Unsupported("outputAudioFormat"));
        }

        return warnings;
    }
}
