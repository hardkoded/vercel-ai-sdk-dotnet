// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>
/// Gateway speech model, routed through <c>/speech-model</c>. Maps to <c>GatewaySpeechModel</c> in
/// <c>gateway-speech-model.ts</c>. <c>doGenerate</c> is ported here; the streaming <c>doStream</c> is tracked in #752.
/// </summary>
public sealed class GatewaySpeechModel : ISpeechModel, ISpeechCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a Gateway speech model.</summary>
    public GatewaySpeechModel(GatewayProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc cref="ISpeechModel.Provider" />
    public string Provider => GatewayProvider.ProviderId;

    /// <inheritdoc cref="ISpeechModel.ModelId" />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<SpeechResult> DoGenerateAsync(
        SpeechCallOptions options,
        CancellationToken cancellationToken
    )
    {
        options = options ?? throw new ArgumentNullException(nameof(options));
        var headers = new Dictionary<string, string>();
        if (options.Headers is not null)
        {
            foreach (var pair in options.Headers)
            {
                if (pair.Value is not null)
                {
                    headers[pair.Key] = pair.Value;
                }
            }
        }

        var providerOptions = JsonValues.EmptyObject();
        if (options.ProviderOptions is not null)
        {
            var node = new JsonObject();
            foreach (var pair in options.ProviderOptions)
            {
                node[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }

            providerOptions = JsonDocument.Parse(node.ToJsonString()).RootElement.Clone();
        }

        var result = await DoGenerateAsync(
                new SpeechModelCall(
                    options.Text,
                    options.Voice,
                    options.OutputFormat,
                    options.Instructions,
                    options.Speed,
                    options.Language,
                    providerOptions,
                    headers,
                    cancellationToken
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        return new SpeechResult(
            result.Audio,
            MediaTypes.DetectMediaType(result.Audio, "audio") ?? "audio/mpeg"
        );
    }

    /// <inheritdoc />
    public async Task<SpeechModelResult> DoGenerateAsync(
        SpeechModelCall call,
        CancellationToken cancellationToken
    )
    {
        call = call ?? throw new ArgumentNullException(nameof(call));
        var body = new JsonObject { ["text"] = call.Text };
        if (!string.IsNullOrEmpty(call.Voice))
        {
            body["voice"] = call.Voice;
        }

        if (!string.IsNullOrEmpty(call.OutputFormat))
        {
            body["outputFormat"] = call.OutputFormat;
        }

        if (!string.IsNullOrEmpty(call.Instructions))
        {
            body["instructions"] = call.Instructions;
        }

        if (call.Speed is { } speed)
        {
            body["speed"] = speed;
        }

        if (!string.IsNullOrEmpty(call.Language))
        {
            body["language"] = call.Language;
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        var callHeaders = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            callHeaders[pair.Key] = pair.Value;
        }

        var requestHeaders = await _provider
            .HeadersAsync(
                "ai-speech-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                callHeaders,
                cancellationToken
            )
            .ConfigureAwait(false);
        ProviderTextResponse response;
        try
        {
            response = await _provider
                .Http.SendJsonStringAsync(
                    HttpMethod.Post,
                    _provider.Route("speech-model"),
                    body.ToJsonString(),
                    requestHeaders,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiConnectionException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid Gateway speech response: the body is not valid JSON."
            );
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("audio", out var audio)
            || audio.ValueKind != JsonValueKind.String
        )
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid Gateway speech response: expected audio."
            );
        }

        var warnings = new List<OperationWarning>();
        if (
            root.TryGetProperty("warnings", out var warningArray)
            && warningArray.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var warning in warningArray.EnumerateArray())
            {
                warnings.Add(GatewayImageModel.ReadWarning(warning));
            }
        }

        JsonElement? usage =
            root.TryGetProperty("usage", out var usageElement)
            && usageElement.ValueKind == JsonValueKind.Object
                ? usageElement.Clone()
                : null;
        JsonElement? providerMetadata =
            root.TryGetProperty("providerMetadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
                ? metadata.Clone()
                : null;
        return new SpeechModelResult(
            ByteEncoding.FromBase64(audio.GetString()!),
            warnings,
            providerMetadata,
            new ProviderResponse(
                response.Headers,
                root,
                timestamp: DateTime.UtcNow,
                modelId: ModelId
            ),
            null,
            usage
        );
    }

    /// <summary>
    /// Gateway streaming speech WebSocket URL: the HTTP(S) base upgraded to WS(S), with the model id in
    /// <c>?ai-model-id=</c> (slash-safe for qualified ids like <c>elevenlabs/eleven-flash-v2.5</c>). Maps to <c>toGatewaySpeechUrl</c>.
    /// </summary>
    public static string ToGatewaySpeechUrl(string baseUrl, string modelId)
    {
        var trimmed = baseUrl.TrimEnd('/');
        var wsBase = trimmed.StartsWith("http", StringComparison.Ordinal)
            ? "ws" + trimmed.Substring(4)
            : trimmed;
        return new Uri(wsBase + "/speech-model").AbsoluteUri
            + "?ai-model-id="
            + GatewayRealtimeModel.FormEncode(modelId);
    }
}
