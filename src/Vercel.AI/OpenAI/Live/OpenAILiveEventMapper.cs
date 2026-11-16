// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Maps between normalized realtime events and the OpenAI Live wire format. Maps to <c>openai-live-event-mapper.ts</c>.</summary>
internal static class OpenAILiveEventMapper
{
    private static readonly HashSet<string> s_knownTypes = new(StringComparer.Ordinal)
    {
        "session.started",
        "session.closed",
        "session.usage.updated",
        "session.output_audio.delta",
        "session.input_transcript.delta",
        "session.output_transcript.delta",
        "session.delegation.created",
        "session.updated",
        "session.input_audio.muted",
        "session.input_audio.unmuted",
        "session.instructions.appended",
        "session.thinking.appended",
        "session.commentary.appended",
        "error",
    };

    /// <summary>Returns a parser for server events. Maps to <c>createOpenAILiveServerEventParser</c>.</summary>
    public static Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CreateOpenAILiveServerEventParser()
    {
        return ParseOpenAILiveServerEvent;
    }

    /// <summary>Parses a raw Live server event. Maps to <c>parseOpenAILiveServerEvent</c>.</summary>
    public static IReadOnlyList<RealtimeModelV4ServerEvent> ParseOpenAILiveServerEvent(JsonElement raw)
    {
        return [ParseServerEvent(raw)];
    }

    /// <summary>Serializes a normalized client event into the Live wire format. Maps to <c>serializeOpenAILiveClientEvent</c>.</summary>
    public static JsonNode SerializeOpenAILiveClientEvent(RealtimeModelV4ClientEvent @event, string modelId)
    {
        JsonObject WithEventId(JsonObject target)
        {
            if (@event.EventId != null)
            {
                target["event_id"] = @event.EventId;
            }

            return target;
        }

        switch (@event.Type)
        {
            case "session-start":
                return WithEventId(new JsonObject
                {
                    ["type"] = "session.start",
                    ["session"] = OpenAILiveSessionConfig.BuildOpenAILiveSessionConfig(@event.Config ?? new RealtimeModelV4SessionConfig(), modelId),
                });
            case "session-update":
                throw Unsupported("OpenAI Live session-update; startup settings are immutable; use context-append or input-audio-mute/input-audio-unmute");
            case "session-close":
                return WithEventId(new JsonObject { ["type"] = "session.close" });
            case "input-audio-append":
                return WithEventId(new JsonObject { ["type"] = "session.input_audio.append", ["audio"] = @event.Audio });
            case "input-audio-mute":
                return WithEventId(new JsonObject { ["type"] = "session.input_audio.mute" });
            case "input-audio-unmute":
                return WithEventId(new JsonObject { ["type"] = "session.input_audio.unmute" });
            case "context-append":
                if (@event.Content == null)
                {
                    throw new InvalidArgumentException("content", null, "context-append requires content.");
                }

                if (@event.DelegationId is { Length: 0 })
                {
                    throw new InvalidArgumentException("delegationId", @event.DelegationId, "delegationId must be null or a non-empty string.");
                }

                var channel = ReadChannel(@event.ProviderOptions);
                return WithEventId(new JsonObject
                {
                    ["type"] = "session." + channel + ".append",
                    ["content"] = @event.Content,
                    ["delegation_id"] = @event.DelegationId,
                });
            default:
                throw Unsupported("OpenAI Live command: " + @event.Type + "; use continuous audio and context-append instead of voice-turn commands");
        }
    }

    private static string ReadChannel(JsonObject? providerOptions)
    {
        JsonNode? openai = null;
        providerOptions?.TryGetPropertyValue("openai", out openai);
        if (openai == null)
        {
            return "thinking";
        }

        if (openai is not JsonObject options)
        {
            throw new InvalidArgumentException("providerOptions", openai, "providerOptions.openai must be an object.");
        }

        foreach (var pair in options)
        {
            if (pair.Key != "channel")
            {
                throw new InvalidArgumentException("providerOptions", pair.Key, "providerOptions.openai has unrecognized key '" + pair.Key + "'.");
            }
        }

        if (!options.TryGetPropertyValue("channel", out var channelNode) || channelNode == null)
        {
            return "thinking";
        }

        if (channelNode is JsonValue value
            && value.TryGetValue<string>(out var channel)
            && (channel == "instructions" || channel == "thinking" || channel == "commentary"))
        {
            return channel;
        }

        throw new InvalidArgumentException("providerOptions", channelNode, "providerOptions.openai.channel must be instructions, thinking or commentary.");
    }

    private static RealtimeModelV4ServerEvent ParseServerEvent(JsonElement raw)
    {
        if (raw.ValueKind == JsonValueKind.Object
            && raw.TryGetProperty("type", out var typeElement)
            && typeElement.ValueKind == JsonValueKind.String
            && !s_knownTypes.Contains(typeElement.GetString()!))
        {
            return new RealtimeModelV4ServerEvent("custom") { RawType = typeElement.GetString(), Raw = raw };
        }

        try
        {
            return Read(raw);
        }
        catch (FormatException exception)
        {
            return new RealtimeModelV4ServerEvent("error")
            {
                Code = "invalid_server_event",
                Message = exception.Message,
                Raw = raw,
            };
        }
    }

    private static RealtimeModelV4ServerEvent Read(JsonElement raw)
    {
        const string InvalidEvent = "Invalid OpenAI Live server event.";
        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException(InvalidEvent);
        }

        var type = RequireString(raw, "type");
        var @event = new RealtimeModelV4ServerEvent("custom") { Raw = raw };
        RealtimeModelV4ServerEvent Tagged(string tag)
        {
            return new RealtimeModelV4ServerEvent(tag) { Raw = raw };
        }

        switch (type)
        {
            case "session.started":
                var started = RequireObject(raw, "session");
                RequireNonEmpty(started, "id");
                var startedDelegation = OptionalObject(started, "delegation");
                string? delegationType = null;
                if (startedDelegation is { } startedDelegationValue)
                {
                    delegationType = RequireEnum(startedDelegationValue, "type", "client", "responses");
                }

                @event = Tagged("session-started");
                @event.SessionId = RequireNonEmpty(started, "id");
                @event.DelegationMode = delegationType == "responses" ? "provider" : "client";
                return @event;

            case "session.closed":
                var closedSession = OptionalObject(raw, "session");
                if (closedSession is { } closedSessionValue)
                {
                    RequireNonEmpty(closedSessionValue, "id");
                }

                var closedUsage = RequireUsage(raw);
                var reason = RequireString(raw, "reason");
                @event = Tagged("session-closed");
                @event.SessionId = closedSession is { } session ? RequireNonEmpty(session, "id") : null;
                @event.Usage = new RealtimeModelV4Usage { Seconds = closedUsage };
                @event.Reason = reason;
                return @event;

            case "session.usage.updated":
                var usage = RequireUsage(raw);
                double? ratio = null;
                if (OptionalObject(raw, "context_window") is { } window)
                {
                    ratio = RequireNumber(window, "usage_ratio");
                    if (ratio < 0 || ratio > 1)
                    {
                        throw new FormatException(InvalidEvent);
                    }
                }

                @event = Tagged("session-usage");
                @event.Usage = new RealtimeModelV4Usage { Seconds = usage };
                @event.ContextWindowUsageRatio = ratio;
                return @event;

            case "session.output_audio.delta":
                @event = Tagged("audio-chunk");
                @event.Delta = RequireString(raw, "delta");
                return @event;

            case "session.input_transcript.delta":
            case "session.output_transcript.delta":
                var delta = RequireString(raw, "delta");
                var start = RequireNonNegative(raw, "start_ms");
                var end = RequireNonNegative(raw, "end_ms");
                RequireInterval(start, end);
                @event = Tagged("transcript-fragment");
                @event.Speaker = type == "session.input_transcript.delta" ? "user" : "assistant";
                @event.Delta = delta;
                @event.StartMs = start;
                @event.EndMs = end;
                return @event;

            case "session.delegation.created":
                var offset = OptionalNonNegative(raw, "offset_ms");
                var delegation = RequireObject(raw, "delegation");
                var delegationId = RequireNonEmpty(delegation, "id");
                var target = OptionalEnum(delegation, "target", "client", "responses");
                var responseId = OptionalNonEmpty(delegation, "response_id");
                @event = Tagged("delegation-created");
                @event.DelegationId = delegationId;
                @event.Target = target == "responses" ? "provider" : target;
                @event.OffsetMs = offset;
                @event.ResponseId = responseId;
                return @event;

            case "error":
                var error = RequireObject(raw, "error");
                var message = RequireString(error, "message");
                @event = Tagged("error");
                @event.Message = message;
                @event.Code = OptionalString(error, "code");
                @event.ClientEventId = OptionalString(error, "client_event_id");
                return @event;

            case "session.updated":
                RequireNonEmpty(RequireObject(raw, "session"), "id");
                return Acknowledged(Tagged("command-acknowledged"), raw, "session.update");

            case "session.input_audio.muted":
            case "session.input_audio.unmuted":
                return Acknowledged(Tagged("command-acknowledged"), raw, type.Substring(0, type.Length - 1));

            default:
                // session.instructions.appended, session.thinking.appended and session.commentary.appended
                var appendStart = RequireNonNegative(raw, "start_ms");
                var appendEnd = RequireNonNegative(raw, "end_ms");
                RequireInterval(appendStart, appendEnd);
                return Acknowledged(Tagged("command-acknowledged"), raw, type.Substring(0, type.Length - 2));
        }
    }

    private static RealtimeModelV4ServerEvent Acknowledged(RealtimeModelV4ServerEvent @event, JsonElement raw, string command)
    {
        @event.Command = command;
        @event.ClientEventId = OptionalString(raw, "client_event_id");
        return @event;
    }

    private static void RequireInterval(double start, double end)
    {
        if (end < start)
        {
            throw new FormatException("Invalid OpenAI Live event time interval.");
        }
    }

    private static double RequireUsage(JsonElement parent)
    {
        return RequireNonNegative(RequireObject(parent, "usage"), "seconds");
    }

    private static JsonElement RequireObject(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object)
        {
            return value;
        }

        throw Invalid();
    }

    private static JsonElement? OptionalObject(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Object ? value : throw Invalid();
    }

    private static string RequireString(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString()!;
        }

        throw Invalid();
    }

    private static string? OptionalString(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : throw Invalid();
    }

    private static string RequireNonEmpty(JsonElement parent, string name)
    {
        var value = RequireString(parent, name);
        return value.Length > 0 ? value : throw Invalid();
    }

    private static string? OptionalNonEmpty(JsonElement parent, string name)
    {
        var value = OptionalString(parent, name);
        return value is { Length: 0 } ? throw Invalid() : value;
    }

    private static string RequireEnum(JsonElement parent, string name, params string[] allowed)
    {
        var value = RequireString(parent, name);
        return Array.IndexOf(allowed, value) >= 0 ? value : throw Invalid();
    }

    private static string? OptionalEnum(JsonElement parent, string name, params string[] allowed)
    {
        var value = OptionalString(parent, name);
        return value != null && Array.IndexOf(allowed, value) < 0 ? throw Invalid() : value;
    }

    private static double RequireNumber(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDouble();
        }

        throw Invalid();
    }

    private static double RequireNonNegative(JsonElement parent, string name)
    {
        var value = RequireNumber(parent, name);
        return value >= 0 ? value : throw Invalid();
    }

    private static double? OptionalNonNegative(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return RequireNonNegative(parent, name);
    }

    private static FormatException Invalid()
    {
        return new FormatException("Invalid OpenAI Live server event.");
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(functionality, "'" + functionality + "' functionality not supported.");
    }
}
