// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Xai;

/// <summary>Client secret returned by <c>/realtime/client_secrets</c>.</summary>
public sealed class XaiClientSecret
{
    /// <summary>Creates a client secret.</summary>
    public XaiClientSecret(string token, string url, long? expiresAt)
    {
        Token = token ?? string.Empty;
        Url = url ?? string.Empty;
        ExpiresAt = expiresAt;
    }

    /// <summary>Secret token.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL. The model is a query parameter.</summary>
    public string Url { get; }

    /// <summary>Unix expiration, when the response includes one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>xAI realtime voice model.</summary>
public sealed class XaiRealtimeModel : IRealtimeModel
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly Func<IReadOnlyDictionary<string, string?>> _headers;

    /// <summary>Creates a realtime model.</summary>
    public XaiRealtimeModel(string modelId, HttpClient httpClient, string baseUrl, Func<IReadOnlyDictionary<string, string?>>? headers)
    {
        ModelId = modelId ?? string.Empty;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = string.IsNullOrEmpty(baseUrl) ? XaiProvider.DefaultBaseUrl : baseUrl;
        _headers = headers ?? (() => new Dictionary<string, string?>());
    }

    /// <inheritdoc />
    public string Provider
    {
        get { return "xai.realtime"; }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        return new Uri(WebSocketUrl());
    }

    /// <summary>WebSocket sessions are opened by the caller with <see cref="XaiClientSecret.Url"/>.</summary>
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException("xAI realtime connects through the client-secret WebSocket URL.");
    }

    /// <summary>Posts to <c>/realtime/client_secrets</c> and returns a WebSocket URL that includes the model.</summary>
    public async Task<XaiClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
    {
        var body = new JsonObject();
        if (expiresAfterSeconds is { } seconds)
        {
            body["expires_after"] = new JsonObject { ["seconds"] = seconds };
        }

        var response = await ProviderExchange.SendAsync(
            _httpClient,
            HttpMethod.Post,
            ApiKeys.Combine(_baseUrl, "/realtime/client_secrets"),
            ProviderExchange.Json(body.ToJsonString()),
            _headers(),
            cancellationToken).ConfigureAwait(false);
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body))
        {
            var token = document.RootElement.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
            long? expires = null;
            if (document.RootElement.TryGetProperty("expires_at", out var expiresValue) && expiresValue.ValueKind == JsonValueKind.Number)
            {
                expires = expiresValue.GetInt64();
            }

            return new XaiClientSecret(token, WebSocketUrl(), expires);
        }
    }

    /// <summary>
    /// Serializes a client event. <c>conversation-item-truncate</c> is dropped because xAI ignores it over WebSocket.
    /// </summary>
    public string? SerializeClientEvent(string type)
    {
        if (string.Equals(type, "conversation-item-truncate", StringComparison.Ordinal))
        {
            return null;
        }

        return type ?? string.Empty;
    }

    private string WebSocketUrl()
    {
        var host = new Uri(_baseUrl).Host;
        return "wss://" + host + "/v1/realtime?model=" + Uri.EscapeDataString(ModelId);
    }
}
