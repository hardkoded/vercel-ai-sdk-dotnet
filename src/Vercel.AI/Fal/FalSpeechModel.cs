// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Fal;

/// <summary>Fal text-to-speech model. Posts to <c>https://fal.run/{modelId}</c> and downloads the returned audio.</summary>
public sealed class FalSpeechModel : ISpeechModel, ISpeechCaller
{
    private readonly FalProvider _provider;
    private readonly Func<DateTime>? _clock;

    /// <summary>Creates a speech model. <paramref name="clock"/> sets the response timestamp.</summary>
    public FalSpeechModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _clock = clock;
    }

    /// <inheritdoc cref="ISpeechModel.Provider" />
    public string Provider => "fal.speech";

    /// <inheritdoc cref="ISpeechModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var call = new SpeechModelCall(options.Text, options.Voice, null, null, null, null, JsonValues.EmptyObject(), new Dictionary<string, string>(), cancellationToken);
        var result = await DoGenerateAsync(call, cancellationToken).ConfigureAwait(false);
        return new SpeechResult(result.Audio, MediaTypes.DetectMediaType(result.Audio, "audio") ?? "audio/mpeg");
    }

    /// <inheritdoc />
    public async Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
    {
        if (call is null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var timestamp = _clock?.Invoke() ?? DateTime.UtcNow;
        var warnings = new List<OperationWarning>();
        var body = new JsonObject
        {
            ["text"] = call.Text,
            ["output_format"] = call.OutputFormat == "hex" ? "hex" : "url",
        };
        if (call.Voice != null)
        {
            body["voice"] = call.Voice;
        }

        if (call.Speed is { } speed)
        {
            body["speed"] = speed;
        }

        if (FalProvider.FalOptions(call.ProviderOptions) is { } fal)
        {
            foreach (var property in fal.EnumerateObject())
            {
                body[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        if (!string.IsNullOrEmpty(call.Language))
        {
            warnings.Add(OperationWarning.Unsupported("language", "fal speech models don't support 'language' directly; consider providerOptions.fal.language_boost"));
        }

        if (!string.IsNullOrEmpty(call.OutputFormat) && call.OutputFormat != "url" && call.OutputFormat != "hex")
        {
            warnings.Add(OperationWarning.Unsupported("outputFormat", "Unsupported outputFormat: " + call.OutputFormat + ". Using 'url' instead."));
        }

        var requestUrl = "https://fal.run/" + ModelId;
        var response = await _provider.SendAsync(HttpMethod.Post, new Uri(requestUrl), ProviderExchange.Json(body.ToJsonString()), call.Headers, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var audioUrl = root.TryGetProperty("audio", out var audio) && audio.ValueKind == JsonValueKind.Object && audio.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String
            ? url.GetString()!
            : throw new InvalidResponseDataException(response.Body, "Fal speech response has no audio url.");
        var bytes = await _provider.DownloadAsync(audioUrl, requestUrl, cancellationToken).ConfigureAwait(false);
        return new SpeechModelResult(bytes, warnings, response: new ProviderResponse(response.Headers, root.Clone(), timestamp: timestamp, modelId: ModelId));
    }
}
