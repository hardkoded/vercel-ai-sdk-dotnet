// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Gemini transcription. Unary models post to the Interactions API.</summary>
public sealed class GoogleTranscriptionModel : ITranscriptionModel
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
    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        if (IsLive(ModelId))
        {
            throw new ArgumentException("Model '" + ModelId + "' only supports streaming transcription. Use a unary model such as 'gemini-3.5-transcribe'.", nameof(ModelId));
        }

        var body = BuildRequest(ModelId, audio, null);
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "interactions"),
            GoogleJson.Write(body),
            _provider.Headers(),
            cancellationToken).ConfigureAwait(false);
        return new TranscriptionResult(ReadText(document.RootElement), null);
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

    private static string ReadText(System.Text.Json.JsonElement root)
    {
        var text = string.Empty;
        if (!root.TryGetProperty("steps", out var steps) || steps.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return text;
        }

        foreach (var step in steps.EnumerateArray())
        {
            if (!step.TryGetProperty("content", out var content) || content.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in content.EnumerateArray())
            {
                if (GoogleJson.String(part, "type") == "text")
                {
                    text += GoogleJson.String(part, "text");
                }
            }
        }

        return text;
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
