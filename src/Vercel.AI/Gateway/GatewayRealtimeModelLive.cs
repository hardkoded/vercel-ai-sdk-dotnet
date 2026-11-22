// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Gateway;

/// <summary>Configuration of a <see cref="GatewayRealtimeModelLive"/>. Maps to <c>GatewayRealtimeModelLiveConfig</c>.</summary>
public sealed class GatewayRealtimeModelLiveConfig
{
    /// <summary>Creates a config.</summary>
    public GatewayRealtimeModelLiveConfig(
        string provider,
        string baseUrl,
        Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> getHeaders,
        Func<
            string,
            int?,
            string,
            CancellationToken,
            Task<GatewayMintedClientSecret>
        > createClientSecret,
        string? teamIdOrSlug = null
    )
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        GetHeaders = getHeaders ?? throw new ArgumentNullException(nameof(getHeaders));
        CreateClientSecret = createClientSecret
            ?? throw new ArgumentNullException(nameof(createClientSecret));
        TeamIdOrSlug = teamIdOrSlug;
    }

    /// <summary>Provider id.</summary>
    public string Provider { get; }

    /// <summary>HTTP(S) base URL of the Gateway V4 routes.</summary>
    public string BaseUrl { get; }

    /// <summary>Optional Vercel team scope.</summary>
    public string? TeamIdOrSlug { get; }

    /// <summary>Resolves the auth headers for a server-side connection.</summary>
    public Func<CancellationToken, Task<IReadOnlyDictionary<string, string?>>> GetHeaders { get; }

    /// <summary>
    /// Mints a Live-bound client secret (<c>vcst_</c>) server-side. Arguments are the model id, <c>expiresAfterSeconds</c>
    /// and the route kind (always <c>live</c>).
    /// </summary>
    public Func<
        string,
        int?,
        string,
        CancellationToken,
        Task<GatewayMintedClientSecret>
    > CreateClientSecret { get; }
}

/// <summary>
/// Live model backed by the AI Gateway. Live is a continuous conversation API: one <c>session-start</c> opens the
/// session, PCM audio streams in both directions, and <c>session-close</c> ends it with final usage. The client speaks
/// the Gateway's normalized Live protocol on <c>/live-model</c>, and the Gateway runs the provider mapping server-side,
/// so server events arrive already normalized. Maps to <c>GatewayRealtimeModelLive</c>.
/// </summary>
public sealed class GatewayRealtimeModelLive : IServerWebSocketRealtimeModel
{
    private static readonly HashSet<string> s_supportedOpenAILiveOptions =
    [
        "delegation",
        "input",
        "store",
        "voice",
    ];

    private static readonly HashSet<string> s_contextChannels =
    [
        "commentary",
        "instructions",
        "thinking",
    ];

    private readonly GatewayRealtimeModelLiveConfig _config;

    /// <summary>Creates a Live model.</summary>
    public GatewayRealtimeModelLive(string modelId, GatewayRealtimeModelLiveConfig config)
    {
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>Specification version. Always <c>v4</c>.</summary>
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public string Provider => _config.Provider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>The conversation mode and the connection kinds this model supports. Maps to <c>capabilities</c>.</summary>
    public RealtimeCapabilities Capabilities { get; } =
        new()
        {
            Conversation = "continuous",
            Transports = ["websocket"],
            Connections = ["client-secret-websocket", "server-websocket"],
            Startup = "session-start",
            Finalization = "session-close",
        };

    /// <summary>The Gateway Live WebSocket URL for this model.</summary>
    public Uri BuildUri()
    {
        return new Uri(ToGatewayLiveUrl(_config.BaseUrl, ModelId));
    }

    /// <summary>The Gateway connects through the client-secret or server WebSocket config, not through a session.</summary>
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
        throw new AiSdkException(
            "AI Gateway Live connects through the WebSocket config from GetWebSocketConfig or GetServerWebSocketConfigAsync."
        );
    }

    /// <summary>
    /// Mints a short-lived client secret bound to this Live model. The browser opens the Live WebSocket with it through
    /// the <c>ai-gateway-auth.&lt;token&gt;</c> subprotocol; the session configuration is sent later in
    /// <c>session-start</c>. Maps to <c>doCreateClientSecret</c>.
    /// </summary>
    public async Task<GatewayRealtimeClientSecret> DoCreateClientSecretAsync(
        int? expiresAfterSeconds,
        CancellationToken cancellationToken
    )
    {
        var secret = await _config
            .CreateClientSecret(ModelId, expiresAfterSeconds, "live", cancellationToken)
            .ConfigureAwait(false);
        return new GatewayRealtimeClientSecret(
            secret.Token,
            ToGatewayLiveUrl(_config.BaseUrl, ModelId),
            secret.ExpiresAt
        );
    }

    /// <summary>Returns the URL and the subprotocols that carry the token. Maps to <c>getWebSocketConfig</c>.</summary>
    public GatewayRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
    {
        return new GatewayRealtimeWebSocketConfig(
            url,
            GatewayRealtimeAuth.GetGatewayRealtimeProtocols(token, _config.TeamIdOrSlug)
        );
    }

    /// <inheritdoc />
    public async Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(
        RealtimeModelV4SessionIntent? intent,
        CancellationToken cancellationToken
    )
    {
        AssertConversationIntent(intent);

        var headers = await _config.GetHeaders(cancellationToken).ConfigureAwait(false);
        var defined = new Dictionary<string, string>();
        foreach (var pair in headers)
        {
            if (pair.Value is not null)
            {
                defined[pair.Key] = pair.Value;
            }
        }

        return new RealtimeModelV4ServerConnection(
            ToGatewayLiveUrl(_config.BaseUrl, ModelId),
            defined
        );
    }

    /// <summary>
    /// Passes a normalized server event through. A Gateway-originated <c>error</c> frame carries no provider payload,
    /// so the frame is kept as <c>Raw</c>. A value that is not an event with a string <c>type</c> becomes an
    /// <c>invalid_server_event</c> error. Maps to <c>parseServerEvent</c>.
    /// </summary>
    public RealtimeModelV4ServerEvent ParseServerEvent(object? raw)
    {
        switch (raw)
        {
            case RealtimeModelV4ServerEvent typed:
                if (typed.Type == "error" && typed.Raw is null)
                {
                    typed.Raw = JsonSerializer.SerializeToElement(
                        new JsonObject
                        {
                            ["type"] = "error",
                            ["code"] = typed.Code,
                            ["message"] = typed.Message,
                            ["clientEventId"] = typed.ClientEventId,
                        }
                    );
                }

                return typed;
            case JsonElement { ValueKind: JsonValueKind.Object } element
                when element.TryGetProperty("type", out var type)
                    && type.ValueKind == JsonValueKind.String:
                return ReadServerEvent(element, type.GetString()!);
            default:
                return new RealtimeModelV4ServerEvent("error")
                {
                    Code = "invalid_server_event",
                    Message = "Invalid AI Gateway Live server event.",
                    Raw = raw switch
                    {
                        JsonElement element => element.Clone(),
                        null => JsonSerializer.SerializeToElement<object?>(null),
                        _ => JsonSerializer.SerializeToElement(raw),
                    },
                };
        }
    }

    /// <summary>Maps a normalized client event to the Gateway Live wire format. Maps to <c>serializeClientEvent</c>.</summary>
    public JsonNode SerializeClientEvent(
        RealtimeModelV4ClientEvent @event,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        AssertConversationIntent(intent);
        JsonObject result;
        switch (@event.Type)
        {
            case "session-start":
                result = new JsonObject
                {
                    ["type"] = "session-start",
                    ["modelId"] = ModelId,
                    ["config"] = ToJson(
                        BuildSessionConfig(@event.Config ?? new RealtimeModelV4SessionConfig())
                    ),
                };
                break;
            case "session-close":
            case "input-audio-mute":
            case "input-audio-unmute":
                result = new JsonObject { ["type"] = @event.Type };
                break;
            case "input-audio-append":
                result = new JsonObject
                {
                    ["type"] = "input-audio-append",
                    ["audio"] = @event.Audio,
                };
                break;
            case "context-append":
                ValidateContextOptions(@event.ProviderOptions);
                result = new JsonObject
                {
                    ["type"] = "context-append",
                    ["content"] = @event.Content,
                    ["delegationId"] = @event.DelegationId,
                };
                if (@event.ProviderOptions is not null)
                {
                    result["providerOptions"] = @event.ProviderOptions.DeepClone();
                }

                break;
            default:
                throw new UnsupportedFunctionalityException(
                    "AI Gateway Live client event: " + @event.Type,
                    "'AI Gateway Live client event: "
                        + @event.Type
                        + "' functionality not supported."
                );
        }

        if (@event.EventId is not null)
        {
            result["eventId"] = @event.EventId;
        }

        return result;
    }

    /// <summary>
    /// Validates a normalized session config against what the Gateway Live route accepts and returns it. Maps to
    /// <c>buildSessionConfig</c>.
    /// </summary>
    public RealtimeModelV4SessionConfig BuildSessionConfig(
        RealtimeModelV4SessionConfig config,
        RealtimeModelV4SessionIntent? intent = null
    )
    {
        AssertConversationIntent(intent);
        foreach (
            var (key, isSet) in new (string, bool)[]
            {
                ("outputModalities", config.OutputModalities is not null),
                ("inputAudioTranscription", config.InputAudioTranscription is not null),
                ("outputAudioTranscription", config.OutputAudioTranscription is not null),
                ("turnDetection", config.TurnDetection is not null),
                ("tools", config.Tools is not null),
            }
        )
        {
            if (isSet)
            {
                throw Unsupported("AI Gateway Live session setting: " + key);
            }
        }

        ValidateSessionProviderOptions(config.ProviderOptions);

        var hasInput =
            config.InputAudioFormatType is not null || config.InputAudioFormatRate is not null;
        var hasOutput =
            config.OutputAudioFormatType is not null || config.OutputAudioFormatRate is not null;
        foreach (
            var (argument, present, type, rate) in new (string, bool, string?, int?)[]
            {
                (
                    "inputAudioFormat",
                    hasInput,
                    config.InputAudioFormatType,
                    config.InputAudioFormatRate
                ),
                (
                    "outputAudioFormat",
                    hasOutput,
                    config.OutputAudioFormatType,
                    config.OutputAudioFormatRate
                ),
            }
        )
        {
            if (present && !IsSupportedAudioFormat(type, rate))
            {
                throw Unsupported(
                    "AI Gateway Live "
                        + argument
                        + " "
                        + type
                        + " at "
                        + (rate?.ToString(CultureInfo.InvariantCulture) ?? "an unspecified")
                        + " Hz; use audio/pcm at 16000 or 24000 Hz, or audio/pcma or audio/pcmu at 8000 Hz"
                );
            }
        }

        if (
            hasInput
            && hasOutput
            && (
                config.InputAudioFormatType != config.OutputAudioFormatType
                || config.InputAudioFormatRate != config.OutputAudioFormatRate
            )
        )
        {
            throw new InvalidArgumentException(
                "outputAudioFormat",
                config.OutputAudioFormatType,
                "AI Gateway Live requires the same input and output audio format."
            );
        }

        return config;
    }

    private static JsonObject ToJson(RealtimeModelV4SessionConfig config)
    {
        var result = new JsonObject();
        if (config.Instructions is not null)
        {
            result["instructions"] = config.Instructions;
        }

        if (config.Voice is not null)
        {
            result["voice"] = config.Voice;
        }

        if (config.InputAudioFormatType is not null || config.InputAudioFormatRate is not null)
        {
            result["inputAudioFormat"] = Format(
                config.InputAudioFormatType,
                config.InputAudioFormatRate
            );
        }

        if (config.OutputAudioFormatType is not null || config.OutputAudioFormatRate is not null)
        {
            result["outputAudioFormat"] = Format(
                config.OutputAudioFormatType,
                config.OutputAudioFormatRate
            );
        }

        if (config.ProviderOptions is not null)
        {
            result["providerOptions"] = RemoveUndefined(config.ProviderOptions);
        }

        return result;
    }

    private static JsonObject Format(string? type, int? rate)
    {
        var format = new JsonObject { ["type"] = type };
        if (rate is not null)
        {
            format["rate"] = rate;
        }

        return format;
    }

    // JS drops undefined entries when the options are serialized; a C# JsonObject has no undefined, so nothing to drop.
    private static JsonObject RemoveUndefined(JsonObject value)
    {
        return (JsonObject)value.DeepClone();
    }

    /// <summary>Gateway options stay service-owned; OpenAI Live options are WebSocket-only.</summary>
    private static void ValidateSessionProviderOptions(JsonObject? providerOptions)
    {
        if (providerOptions is null)
        {
            return;
        }

        foreach (var pair in providerOptions)
        {
            var ns = pair.Key;
            var options = pair.Value;
            if (ns != "gateway" && ns != "openai")
            {
                throw Unsupported("AI Gateway Live provider options namespace: " + ns);
            }

            if (options is not JsonObject)
            {
                throw new InvalidArgumentException(
                    "providerOptions." + ns,
                    options?.ToJsonString(),
                    "providerOptions." + ns + " must be an object."
                );
            }
        }

        if (providerOptions["openai"] is not JsonObject openai)
        {
            return;
        }

        foreach (var pair in openai)
        {
            var key = pair.Key;
            var value = pair.Value;
            if (!s_supportedOpenAILiveOptions.Contains(key))
            {
                throw Unsupported("AI Gateway Live OpenAI option: " + key);
            }

            if (
                key == "delegation"
                && value is not null
                && !(HasExactKeys(value, "type") && IsString(((JsonObject)value)["type"], "client"))
            )
            {
                throw Unsupported(
                    "AI Gateway Live delegation other than { type: \"client\" } or null"
                );
            }

            if (!IsValidOpenAILiveOption(key, value))
            {
                throw new InvalidArgumentException(
                    "providerOptions.openai." + key,
                    value?.ToJsonString(),
                    OpenAILiveOptionMessage(key)
                );
            }
        }
    }

    private static string OpenAILiveOptionMessage(string key)
    {
        return key switch
        {
            "delegation" => "delegation must be { type: \"client\" } or null.",
            "input" =>
                "input must be at most 128 messages, each { type: \"message\", role, content: [{ type, text }] }.",
            "store" => "store must be a boolean.",
            _ => "voice must be { id: string } with a nonempty id.",
        };
    }

    private static bool IsValidOpenAILiveOption(string key, JsonNode? value)
    {
        switch (key)
        {
            case "delegation":
                return true;
            case "store":
                return value is JsonValue store
                    && store.GetValueKind() is JsonValueKind.True or JsonValueKind.False;
            case "voice":
                return HasExactKeys(value, "id")
                    && ((JsonObject)value!)["id"] is JsonValue id
                    && id.TryGetValue<string>(out var voiceId)
                    && voiceId.Length > 0;
            case "input":
                return value is JsonArray input && input.Count <= 128 && input.All(IsInputMessage);
            default:
                return false;
        }
    }

    private static bool IsInputMessage(JsonNode? message)
    {
        if (
            !HasExactKeys(message, "type", "role", "content")
            || message is not JsonObject obj
            || !IsString(obj["type"], "message")
            || obj["content"] is not JsonArray { Count: 1 } content
            || !HasExactKeys(content[0], "type", "text")
            || content[0] is not JsonObject part
            || !(part["text"] is JsonValue text && text.TryGetValue<string>(out _))
        )
        {
            return false;
        }

        var partType = part["type"]?.GetValue<string>();
        var role =
            obj["role"] is JsonValue r && r.TryGetValue<string>(out var roleValue)
                ? roleValue
                : null;
        return role == "assistant"
            ? partType is "text" or "output_text"
            : role is "developer" or "user" && partType == "input_text";
    }

    private static bool IsString(JsonNode? node, string expected)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var s) && s == expected;
    }

    private static bool HasExactKeys(JsonNode? value, params string[] keys)
    {
        return value is JsonObject obj && obj.Count == keys.Length && keys.All(obj.ContainsKey);
    }

    private static void ValidateContextOptions(JsonObject? providerOptions)
    {
        if (providerOptions is null)
        {
            return;
        }

        var valid = providerOptions.All(pair =>
            pair.Key == "openai"
            && pair.Value is JsonObject options
            && options.All(option =>
                option.Key == "channel"
                && option.Value is JsonValue channel
                && channel.TryGetValue<string>(out var name)
                && s_contextChannels.Contains(name)
            )
        );
        if (!valid)
        {
            throw Unsupported(
                "AI Gateway Live context options other than openai.channel commentary, instructions, or thinking"
            );
        }
    }

    private static bool IsSupportedAudioFormat(string? type, int? rate)
    {
        return type == "audio/pcm"
            ? rate is 16000 or 24000
            : type is "audio/pcma" or "audio/pcmu" && rate == 8000;
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }

    private static void AssertConversationIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent is { } value && value != RealtimeModelV4SessionIntent.Conversation)
        {
            throw new InvalidOperationException(
                "AI Gateway Live does not support the '"
                    + value.ToString().ToLowerInvariant()
                    + "' session intent."
            );
        }
    }

    private static RealtimeModelV4ServerEvent ReadServerEvent(JsonElement element, string type)
    {
        string? Text(JsonElement parent, string name) =>
            parent.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
                ? v.GetString()
                : null;
        double? Number(JsonElement parent, string name) =>
            parent.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number
                ? v.GetDouble()
                : null;
        int? Integer(JsonElement parent, string name) =>
            parent.TryGetProperty(name, out var v) && v.TryGetInt32(out var n) ? n : null;
        var @event = new RealtimeModelV4ServerEvent(type)
        {
            SessionId = Text(element, "sessionId"),
            ResponseId = Text(element, "responseId"),
            ItemId = Text(element, "itemId"),
            PreviousItemId = Text(element, "previousItemId"),
            CallId = Text(element, "callId"),
            Name = Text(element, "name"),
            Delta = Text(element, "delta"),
            Arguments = Text(element, "arguments"),
            Transcript = Text(element, "transcript"),
            Text = Text(element, "text"),
            Status = Text(element, "status"),
            Message = Text(element, "message"),
            Code = Text(element, "code"),
            ClientEventId = Text(element, "clientEventId"),
            RawType = Text(element, "rawType"),
            Reason = Text(element, "reason"),
            DelegationMode = Text(element, "delegationMode"),
            Speaker = Text(element, "speaker"),
            DelegationId = Text(element, "delegationId"),
            Target = Text(element, "target"),
            Command = Text(element, "command"),
            ContextWindowUsageRatio = Number(element, "contextWindowUsageRatio"),
            StartMs = Number(element, "startMs"),
            EndMs = Number(element, "endMs"),
            OffsetMs = Number(element, "offsetMs"),
            Item = element.TryGetProperty("item", out var item) ? item.Clone() : null,
            Raw = element.TryGetProperty("raw", out var raw) ? raw.Clone() : null,
        };
        if (
            element.TryGetProperty("usage", out var usage)
            && usage.ValueKind == JsonValueKind.Object
        )
        {
            @event.Usage = new RealtimeModelV4Usage
            {
                InputTokens = Integer(usage, "inputTokens"),
                OutputTokens = Integer(usage, "outputTokens"),
                CachedInputTokens = Integer(usage, "cachedInputTokens"),
                InputTextTokens = Integer(usage, "inputTextTokens"),
                InputAudioTokens = Integer(usage, "inputAudioTokens"),
                OutputTextTokens = Integer(usage, "outputTextTokens"),
                OutputAudioTokens = Integer(usage, "outputAudioTokens"),
                CachedInputTextTokens = Integer(usage, "cachedInputTextTokens"),
                CachedInputAudioTokens = Integer(usage, "cachedInputAudioTokens"),
                Seconds = Number(usage, "seconds"),
            };
        }

        // Gateway-originated errors carry no provider payload; keep the frame as `raw`.
        if (type == "error" && @event.Raw is null)
        {
            @event.Raw = element.Clone();
        }

        return @event;
    }

    // Live WebSocket URL; the model id rides ?ai-model-id= like the realtime route.
    private static string ToGatewayLiveUrl(string baseUrl, string modelId)
    {
        var wsBase = baseUrl.StartsWith("http", StringComparison.Ordinal)
            ? "ws" + baseUrl.Substring(4)
            : baseUrl;
        var url = new Uri(wsBase + "/live-model").AbsoluteUri;
        return url + "?ai-model-id=" + GatewayRealtimeModel.FormEncode(modelId);
    }
}
