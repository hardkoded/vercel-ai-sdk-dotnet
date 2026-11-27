// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Cartesia;

/// <summary>A realtime client secret. Maps to <c>RealtimeModelV4ClientSecretResult</c>.</summary>
public sealed class CartesiaRealtimeClientSecret
{
    /// <summary>Creates a client secret.</summary>
    public CartesiaRealtimeClientSecret(string token, string url, long? expiresAt)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        ExpiresAt = expiresAt;
    }

    /// <summary>Short-lived, STT-scoped access token.</summary>
    public string Token { get; }

    /// <summary>WebSocket URL, without the token.</summary>
    public string Url { get; }

    /// <summary>Expiry as unix seconds, when the caller asked for one.</summary>
    public long? ExpiresAt { get; }
}

/// <summary>Settings of <see cref="CartesiaRealtimeModel"/>. Maps to <c>CartesiaRealtimeModelConfig</c>.</summary>
public sealed class CartesiaRealtimeModelConfig
{
    /// <summary>Creates a config.</summary>
    public CartesiaRealtimeModelConfig(
        string provider,
        string baseUrl,
        string version,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> headers
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>Provider id, for example <c>cartesia.realtime</c>.</summary>
    public string Provider { get; }

    /// <summary>API base URL.</summary>
    public string BaseUrl { get; }

    /// <summary>The Cartesia API version, sent as <c>cartesia_version</c>.</summary>
    public string Version { get; }

    /// <summary>Resolves the request headers, including the credential.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> Headers { get; }

    /// <summary>Maps <c>fetch</c>: the HTTP client used to mint client secrets. Defaults to a shared client.</summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>Maps <c>_internal.currentDate</c>.</summary>
    public Func<DateTime>? CurrentDate { get; set; }
}

/// <summary>Realtime Ink model using Cartesia's STT WebSocket APIs. Maps to <c>CartesiaRealtimeModel</c>.</summary>
public sealed class CartesiaRealtimeModel : IRealtimeModel
{
    private static readonly Lazy<HttpClient> DefaultHttpClient = new(() => new HttpClient());

    private readonly CartesiaRealtimeModelConfig _config;
    private readonly object _gate = new();
    private ManualTranscriptState _manualTranscript = new();
    private bool _sessionCreated;
    private bool _useTurnDetection = true;

    /// <summary>Creates a realtime model.</summary>
    public CartesiaRealtimeModel(string modelId, CartesiaRealtimeModelConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>The realtime model specification version. Maps to <c>specificationVersion</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        throw new AiSdkException(
            "Cartesia realtime connects through the client-secret WebSocket URL."
        );
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException(
            "Cartesia realtime connects through the client-secret WebSocket URL."
        );
    }

    /// <summary>
    /// Posts to <c>/access-token</c> and returns the access token and the WebSocket URL of the matching Ink endpoint.
    /// Maps to <c>doCreateClientSecret</c>.
    /// </summary>
    public async Task<CartesiaRealtimeClientSecret> CreateClientSecretAsync(
        RealtimeModelV4SessionConfig? sessionConfig,
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        if (expiresAfterSeconds is { } expiresIn && (expiresIn <= 0 || expiresIn > 3600))
        {
            throw new AiSdkException(
                "Cartesia realtime client secrets must expire between 1 and 3600 seconds."
            );
        }

        var language = sessionConfig?.InputAudioTranscription?.Language;
        if (language != null && language != "en")
        {
            throw new AiSdkException("Cartesia Ink 2 currently supports English only.");
        }

        var body = new JsonObject { ["grants"] = new JsonObject { ["stt"] = true } };
        if (expiresAfterSeconds is { } expires)
        {
            body["expires_in"] = expires;
        }

        var headers = await _config.Headers(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            ApiKeys.Combine(_config.BaseUrl, "/access-token")
        );
        foreach (var pair in headers)
        {
            if (pair.Value != null)
            {
                request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }
        }

        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8)
        {
            Headers =
            {
                ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"),
            },
        };
        using var response = await (_config.HttpClient ?? DefaultHttpClient.Value)
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);
#if NET
        var text = await response
            .Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);
#else
        var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        if (!response.IsSuccessStatusCode)
        {
            throw new AiSdkException(
                "Cartesia realtime client secret request failed: "
                    + ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture)
                    + " "
                    + text
            );
        }

        string? token = null;
        using (var document = JsonDocument.Parse(text))
        {
            if (
                document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("token", out var tokenElement)
                && tokenElement.ValueKind == JsonValueKind.String
            )
            {
                token = tokenElement.GetString();
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            throw new AiSdkException(
                "Cartesia realtime client secret response did not include a token."
            );
        }

        var useTurnDetection =
            sessionConfig?.TurnDetectionDisabled != true
            && sessionConfig?.TurnDetection?.Type != "disabled";
        var inputAudioFormatType = sessionConfig?.InputAudioFormatType;
        var query = new List<KeyValuePair<string, string>>
        {
            new("model", ModelId),
            new("encoding", ResolveCartesiaEncoding(inputAudioFormatType)),
            new(
                "sample_rate",
                (sessionConfig?.InputAudioFormatRate ?? 24000).ToString(
                    CultureInfo.InvariantCulture
                )
            ),
            new("cartesia_version", _config.Version),
        };
        if (!useTurnDetection && language != null)
        {
            query.Add(new KeyValuePair<string, string>("language", language));
        }

        var baseUri = new Uri(_config.BaseUrl);
        var path = baseUri.AbsolutePath.TrimEnd('/');
        var url =
            (baseUri.Scheme == "http" ? "ws:" : "wss:")
            + "//"
            + baseUri.Authority
            + path
            + (useTurnDetection ? "/stt/turns/websocket" : "/stt/websocket")
            + "?"
            + FormatQuery(query);

        long? expiresAt = null;
        if (expiresAfterSeconds is { } lifetime)
        {
            var now = new DateTimeOffset(
                DateTime.SpecifyKind(
                    _config.CurrentDate?.Invoke() ?? DateTime.UtcNow,
                    DateTimeKind.Utc
                )
            );
            expiresAt = now.ToUnixTimeSeconds() + lifetime;
        }

        return new CartesiaRealtimeClientSecret(token!, url, expiresAt);
    }

    /// <summary>Adds the access token to the WebSocket URL. Maps to <c>getWebSocketConfig</c>.</summary>
    public RealtimeModelV4WebSocketConfig GetWebSocketConfig(string token, string url)
    {
        var uri = new Uri(url);
        lock (_gate)
        {
            _useTurnDetection = uri.AbsolutePath.Contains("/stt/turns/websocket");
        }

        var query = new List<KeyValuePair<string, string>>();
        foreach (
            var part in uri.Query.TrimStart('?').Split(['&'], StringSplitOptions.RemoveEmptyEntries)
        )
        {
            var separator = part.IndexOf('=');
            var name = separator < 0 ? part : part.Substring(0, separator);
            if (name == "access_token")
            {
                continue;
            }

            query.Add(
                new KeyValuePair<string, string>(
                    Uri.UnescapeDataString(name.Replace('+', ' ')),
                    separator < 0
                        ? string.Empty
                        : Uri.UnescapeDataString(part.Substring(separator + 1).Replace('+', ' '))
                )
            );
        }

        query.Add(new KeyValuePair<string, string>("access_token", token));
        var builder = new UriBuilder(uri) { Query = FormatQuery(query) };
        return new RealtimeModelV4WebSocketConfig(builder.Uri.AbsoluteUri);
    }

    /// <summary>Maps one Cartesia event to normalized events. Maps to <c>parseServerEvent</c>.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
    {
        raw = raw.Clone();
        var type = ReadString(raw, "type");
        var requestId = ReadString(raw, "request_id");
        var itemId = requestId ?? "cartesia-realtime";
        var mapped = new List<RealtimeModelV4ServerEvent>();
        lock (_gate)
        {
            switch (type)
            {
                case "connected":
                    _sessionCreated = true;
                    return
                    [
                        new RealtimeModelV4ServerEvent("session-created")
                        {
                            SessionId = requestId,
                            Raw = raw,
                        },
                    ];
                case "turn.start":
                    mapped.Add(Event("speech-started", itemId, raw));
                    break;
                case "turn.resume":
                    mapped.Add(Event("speech-started", itemId, raw));
                    mapped.Add(Custom(type, raw));
                    break;
                case "turn.end":
                    mapped.Add(Event("speech-stopped", itemId, raw));
                    mapped.Add(
                        new RealtimeModelV4ServerEvent("input-transcription-completed")
                        {
                            ItemId = itemId,
                            Transcript = ReadString(raw, "transcript") ?? string.Empty,
                            Raw = raw,
                        }
                    );
                    break;
                case "transcript":
                    if (
                        raw.TryGetProperty("is_final", out var isFinal)
                        && isFinal.ValueKind == JsonValueKind.True
                    )
                    {
                        _manualTranscript.Text += ReadString(raw, "text") ?? string.Empty;
                        _manualTranscript.Duration +=
                            raw.TryGetProperty("duration", out var duration)
                            && duration.ValueKind == JsonValueKind.Number
                                ? duration.GetDouble()
                                : 0;
                        _manualTranscript.Events.Add(raw);
                    }

                    mapped.Add(Custom(type, raw));
                    break;
                case "flush_done":
                    mapped.AddRange(FinishManualTranscript(type, requestId, raw));
                    break;
                case "done":
                    if (_manualTranscript.Text.Length > 0)
                    {
                        mapped.AddRange(FinishManualTranscript(type, requestId, raw));
                    }

                    mapped.Add(Custom(type, raw));
                    break;
                case "error":
                    mapped.Add(
                        new RealtimeModelV4ServerEvent("error")
                        {
                            Message = ReadString(raw, "message")
                                ?? "Unknown Cartesia realtime error",
                            Code = ReadString(raw, "error_code"),
                            Raw = raw,
                        }
                    );
                    break;
                default:
                    mapped.Add(Custom(type, raw));
                    break;
            }

            if (_sessionCreated)
            {
                return mapped;
            }

            _sessionCreated = true;
            mapped.Insert(
                0,
                new RealtimeModelV4ServerEvent("session-created") { SessionId = itemId, Raw = raw }
            );
            return mapped;
        }
    }

    /// <summary>
    /// Serializes a client event: audio becomes a <c>byte[]</c> binary frame, <c>finalize</c> a text frame, and
    /// events Cartesia ignores become null. Maps to <c>serializeClientEvent</c>.
    /// </summary>
    public object? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent)
    {
        clientEvent = clientEvent ?? throw new ArgumentNullException(nameof(clientEvent));
        switch (clientEvent.Type)
        {
            case "session-update":
            case "input-audio-clear":
            case "response-create":
                return null;
            case "input-audio-append":
                return Convert.FromBase64String(clientEvent.Audio ?? string.Empty);
            case "input-audio-commit":
                lock (_gate)
                {
                    return _useTurnDetection ? null : "finalize";
                }

            default:
                throw new AiSdkException(
                    "Cartesia Ink 2 does not support realtime client event \""
                        + clientEvent.Type
                        + "\"."
                );
        }
    }

    /// <summary>Cartesia needs no provider session config. Maps to <c>buildSessionConfig</c>.</summary>
    public JsonNode? BuildSessionConfig(RealtimeModelV4SessionConfig config) => null;

    private static string FormatQuery(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        // URLSearchParams encodes a space as a plus sign.
        return string.Join("&", pairs.Select(pair => Escape(pair.Key) + "=" + Escape(pair.Value)));
    }

    private static string Escape(string value) => Uri.EscapeDataString(value).Replace("%20", "+");

    private static string? ReadString(JsonElement element, string name)
    {
        return
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static RealtimeModelV4ServerEvent Event(string type, string itemId, JsonElement raw)
    {
        return new RealtimeModelV4ServerEvent(type) { ItemId = itemId, Raw = raw };
    }

    private static RealtimeModelV4ServerEvent Custom(string? rawType, JsonElement raw)
    {
        return new RealtimeModelV4ServerEvent("custom") { RawType = rawType, Raw = raw };
    }

    private static string ResolveCartesiaEncoding(string? type)
    {
        return type switch
        {
            null or "audio/pcm" => "pcm_s16le",
            "audio/pcmu" => "pcm_mulaw",
            "audio/pcma" => "pcm_alaw",
            _ => throw new AiSdkException("Unsupported Cartesia realtime audio format: " + type),
        };
    }

    private IReadOnlyList<RealtimeModelV4ServerEvent> FinishManualTranscript(
        string? type,
        string? requestId,
        JsonElement raw
    )
    {
        var state = _manualTranscript;
        _manualTranscript = new ManualTranscriptState();
        if (state.Text.Length == 0)
        {
            return [Custom(type, raw)];
        }

        var events = new JsonArray();
        foreach (var transcriptEvent in state.Events)
        {
            events.Add(JsonNode.Parse(transcriptEvent.GetRawText()));
        }

        var completedRaw = new JsonObject
        {
            ["event"] = JsonNode.Parse(raw.GetRawText()),
            ["transcriptEvents"] = events,
            ["duration"] = state.Duration,
        };
        return
        [
            new RealtimeModelV4ServerEvent("audio-committed") { ItemId = requestId, Raw = raw },
            new RealtimeModelV4ServerEvent("input-transcription-completed")
            {
                ItemId = requestId ?? "cartesia-realtime",
                Transcript = state.Text,
                Raw = JsonSerializer.SerializeToElement(completedRaw),
            },
        ];
    }

    private sealed class ManualTranscriptState
    {
        public double Duration { get; set; }

        public List<JsonElement> Events { get; } = [];

        public string Text { get; set; } = string.Empty;
    }
}

/// <summary>Creates Cartesia realtime models and client secrets. Maps to <c>experimental_realtime</c>.</summary>
public sealed class CartesiaRealtimeFactory
{
    private readonly CartesiaProvider _provider;

    internal CartesiaRealtimeFactory(CartesiaProvider provider)
    {
        _provider = provider;
    }

    /// <summary>Creates a realtime Ink speech-to-text model. Maps to calling the factory with a model id.</summary>
    public CartesiaRealtimeModel Model(string modelId)
    {
        return new CartesiaRealtimeModel(
            modelId,
            new CartesiaRealtimeModelConfig(
                "cartesia.realtime",
                CartesiaProvider.DefaultBaseUrl,
                _provider.Version,
                async cancellationToken =>
                    await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false)
            )
            {
                HttpClient = _provider.HttpClient,
            }
        );
    }

    /// <summary>Server-only: mints a client secret for a browser session. Maps to <c>getToken</c>.</summary>
    public Task<CartesiaRealtimeClientSecret> GetTokenAsync(
        string model,
        RealtimeModelV4SessionConfig? sessionConfig = null,
        int? expiresAfterSeconds = null,
        CancellationToken cancellationToken = default
    )
    {
        return Model(model)
            .CreateClientSecretAsync(sessionConfig, expiresAfterSeconds, cancellationToken);
    }
}
