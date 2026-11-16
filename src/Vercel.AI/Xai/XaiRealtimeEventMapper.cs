// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Xai;

/// <summary>
/// Maps xAI realtime wire events to normalized events and back. Maps to the exported functions of
/// <c>xai-realtime-event-mapper.ts</c>: <c>parseXaiRealtimeServerEvent</c>, <c>serializeXaiRealtimeClientEvent</c>
/// and <c>buildXaiSessionConfig</c>.
/// </summary>
public static class XaiRealtimeEventMapper
{
    /// <summary>Maps one xAI server event. Maps to <c>parseXaiRealtimeServerEvent</c>.</summary>
    public static RealtimeModelV4ServerEvent ParseXaiRealtimeServerEvent(JsonElement raw)
    {
        var clone = raw.Clone();
        var type = String(clone, "type");

        switch (type)
        {
            case "session.created":
                return new RealtimeModelV4ServerEvent("session-created")
                {
                    SessionId = Object(clone, "session") is { } session ? String(session, "id") : null,
                    Raw = clone,
                };

            case "session.updated":
                return new RealtimeModelV4ServerEvent("session-updated") { Raw = clone };

            case "conversation.created":
                return Custom(type, clone);

            case "input_audio_buffer.speech_started":
                return new RealtimeModelV4ServerEvent("speech-started") { ItemId = String(clone, "item_id"), Raw = clone };

            case "input_audio_buffer.speech_stopped":
                return new RealtimeModelV4ServerEvent("speech-stopped") { ItemId = String(clone, "item_id"), Raw = clone };

            case "input_audio_buffer.committed":
                return new RealtimeModelV4ServerEvent("audio-committed")
                {
                    ItemId = String(clone, "item_id"),
                    PreviousItemId = String(clone, "previous_item_id"),
                    Raw = clone,
                };

            case "conversation.item.added":
                return new RealtimeModelV4ServerEvent("conversation-item-added")
                {
                    ItemId = ItemId(clone),
                    Item = Object(clone, "item"),
                    Raw = clone,
                };

            case "conversation.item.input_audio_transcription.completed":
                return new RealtimeModelV4ServerEvent("input-transcription-completed")
                {
                    ItemId = String(clone, "item_id"),
                    Transcript = String(clone, "transcript") ?? string.Empty,
                    Usage = MapTranscriptionUsage(Object(clone, "usage")),
                    Raw = clone,
                };

            case "response.created":
                return new RealtimeModelV4ServerEvent("response-created") { ResponseId = ResponseId(clone), Raw = clone };

            case "response.done":
            {
                var response = Object(clone, "response");
                return new RealtimeModelV4ServerEvent("response-done")
                {
                    ResponseId = ResponseId(clone),
                    Status = (response != null ? String(response.Value, "status") : null) ?? "completed",
                    Usage = MapResponseUsage(Object(clone, "usage") ?? (response != null ? Object(response.Value, "usage") : null)),
                    Raw = clone,
                };
            }

            case "response.output_item.added":
                return new RealtimeModelV4ServerEvent("output-item-added") { ResponseId = String(clone, "response_id"), ItemId = ItemId(clone), Raw = clone };

            case "response.output_item.done":
                return new RealtimeModelV4ServerEvent("output-item-done") { ResponseId = String(clone, "response_id"), ItemId = ItemId(clone), Raw = clone };

            case "response.content_part.added":
                return Response("content-part-added", clone);

            case "response.content_part.done":
                return Response("content-part-done", clone);

            case "response.output_audio.delta":
                return Response("audio-delta", clone, delta: String(clone, "delta"));

            case "response.output_audio.done":
                return Response("audio-done", clone);

            case "response.output_audio_transcript.delta":
                return Response("audio-transcript-delta", clone, delta: String(clone, "delta"));

            case "response.output_audio_transcript.done":
                return Response("audio-transcript-done", clone, transcript: String(clone, "transcript"));

            case "response.text.delta":
                return Response("text-delta", clone, delta: String(clone, "delta"));

            case "response.text.done":
                return Response("text-done", clone, text: String(clone, "text"));

            case "response.function_call_arguments.delta":
                return Response("function-call-arguments-delta", clone, delta: String(clone, "delta"), callId: String(clone, "call_id"));

            case "response.function_call_arguments.done":
                return Response("function-call-arguments-done", clone, callId: String(clone, "call_id"), name: String(clone, "name"), arguments: String(clone, "arguments"));

            case "mcp_list_tools.in_progress":
            case "mcp_list_tools.completed":
            case "mcp_list_tools.failed":
            case "response.mcp_call_arguments.delta":
            case "response.mcp_call_arguments.done":
            case "response.mcp_call.in_progress":
            case "response.mcp_call.completed":
            case "response.mcp_call.failed":
                return Custom(type, clone);

            case "error":
            {
                var error = Object(clone, "error");
                return new RealtimeModelV4ServerEvent("error")
                {
                    Message = (error != null ? String(error.Value, "message") : null) ?? String(clone, "message") ?? "Unknown error",
                    Code = (error != null ? String(error.Value, "code") : null) ?? String(clone, "code"),
                    Raw = clone,
                };
            }

            default:
                return Custom(type, clone);
        }
    }

    /// <summary>Maps one normalized client event to the xAI wire event. Returns null when xAI has no equivalent. Maps to <c>serializeXaiRealtimeClientEvent</c>.</summary>
    public static JsonObject? SerializeXaiRealtimeClientEvent(RealtimeModelV4ClientEvent clientEvent)
    {
        switch (clientEvent.Type)
        {
            case "session-update":
                return new JsonObject
                {
                    ["type"] = "session.update",
                    ["session"] = BuildXaiSessionConfig(clientEvent.Config ?? new RealtimeModelV4SessionConfig()),
                };

            case "input-audio-append":
                return new JsonObject { ["type"] = "input_audio_buffer.append", ["audio"] = clientEvent.Audio };

            case "input-audio-commit":
                return new JsonObject { ["type"] = "input_audio_buffer.commit" };

            case "input-audio-clear":
                return new JsonObject { ["type"] = "input_audio_buffer.clear" };

            case "conversation-item-create":
            {
                var item = clientEvent.Item;
                switch (item?.Type)
                {
                    case "text-message":
                        return ConversationItemCreate(Message(item.Role, new JsonObject { ["type"] = "input_text", ["text"] = item.Text }));
                    case "audio-message":
                        return ConversationItemCreate(Message(item.Role, new JsonObject { ["type"] = "input_audio", ["audio"] = item.Audio }));
                    case "function-call-output":
                        return ConversationItemCreate(new JsonObject
                        {
                            ["type"] = "function_call_output",
                            ["call_id"] = item.CallId,
                            ["output"] = item.Output,
                        });
                    default:
                        return null;
                }
            }

            case "conversation-item-truncate":
                // xAI does not support `conversation.item.truncate` over WebSocket (it is
                // silently ignored by the server). Barge-in still works because the SDK
                // stops local playback when `speech_started` fires, so dropping the event
                // here just avoids sending a no-op.
                return null;

            case "response-create":
            {
                var create = new JsonObject { ["type"] = "response.create" };
                if (clientEvent.Options != null)
                {
                    var response = new JsonObject();
                    if (clientEvent.Options.Modalities != null)
                    {
                        response["modalities"] = new JsonArray([.. clientEvent.Options.Modalities.Select(m => (JsonNode?)JsonValue.Create(m))]);
                    }

                    if (clientEvent.Options.Instructions != null)
                    {
                        response["instructions"] = clientEvent.Options.Instructions;
                    }

                    create["response"] = response;
                }

                return create;
            }

            case "response-cancel":
                return new JsonObject { ["type"] = "response.cancel" };

            default:
                return null;
        }
    }

    /// <summary>Builds the xAI <c>session</c> object from a normalized session config. Maps to <c>buildXaiSessionConfig</c>.</summary>
    public static JsonObject BuildXaiSessionConfig(RealtimeModelV4SessionConfig config)
    {
        var session = new JsonObject();

        if (config.Instructions != null)
        {
            session["instructions"] = config.Instructions;
        }

        if (config.Voice != null)
        {
            session["voice"] = config.Voice;
        }

        var audio = new JsonObject();
        if (config.InputAudioFormatType != null || config.InputAudioFormatRate != null)
        {
            audio["input"] = new JsonObject { ["format"] = Format(config.InputAudioFormatType, config.InputAudioFormatRate) };
        }

        if (config.OutputAudioFormatType != null || config.OutputAudioFormatRate != null)
        {
            audio["output"] = new JsonObject { ["format"] = Format(config.OutputAudioFormatType, config.OutputAudioFormatRate) };
        }

        if (audio.Count > 0)
        {
            session["audio"] = audio;
        }

        if (config.TurnDetection != null)
        {
            if (config.TurnDetection.Type == "disabled")
            {
                session["turn_detection"] = null;
            }
            else
            {
                var turnDetection = new JsonObject { ["type"] = "server_vad" };
                if (config.TurnDetection.Threshold != null)
                {
                    turnDetection["threshold"] = config.TurnDetection.Threshold;
                }

                if (config.TurnDetection.SilenceDurationMs != null)
                {
                    turnDetection["silence_duration_ms"] = config.TurnDetection.SilenceDurationMs;
                }

                if (config.TurnDetection.PrefixPaddingMs != null)
                {
                    turnDetection["prefix_padding_ms"] = config.TurnDetection.PrefixPaddingMs;
                }

                session["turn_detection"] = turnDetection;
            }
        }

        if (config.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in config.Tools)
            {
                var definition = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
                if (tool.Description != null)
                {
                    definition["description"] = tool.Description;
                }

                definition["parameters"] = JsonNode.Parse(tool.Parameters.GetRawText());
                tools.Add(definition);
            }

            session["tools"] = tools;
        }

        if (config.ProviderOptions != null)
        {
            if (config.ProviderOptions["tools"] is JsonArray extraTools)
            {
                var merged = new JsonArray();
                if (session["tools"] is JsonArray existingTools)
                {
                    foreach (var tool in existingTools)
                    {
                        merged.Add(tool?.DeepClone());
                    }
                }

                foreach (var tool in extraTools)
                {
                    merged.Add(tool?.DeepClone());
                }

                session["tools"] = merged;
            }

            foreach (var pair in config.ProviderOptions)
            {
                if (pair.Key != "tools")
                {
                    session[pair.Key] = pair.Value?.DeepClone();
                }
            }
        }

        return session;
    }

    /// <summary>
    /// Maps xAI realtime <c>response.done</c> usage to normalized usage. xAI speaks the OpenAI realtime wire shape,
    /// so input buckets are gross (cache-inclusive) with the cached portion surfaced separately.
    /// </summary>
    private static RealtimeModelV4Usage? MapResponseUsage(JsonElement? usage)
    {
        if (usage == null)
        {
            return null;
        }

        var input = Object(usage.Value, "input_token_details");
        var output = Object(usage.Value, "output_token_details");
        var cached = input != null ? Object(input.Value, "cached_tokens_details") : null;

        return CompactUsage(new RealtimeModelV4Usage
        {
            InputTokens = Count(usage.Value, "input_tokens"),
            OutputTokens = Count(usage.Value, "output_tokens"),
            CachedInputTokens = input != null ? Count(input.Value, "cached_tokens") : null,
            InputAudioTokens = input != null ? Count(input.Value, "audio_tokens") : null,
            InputTextTokens = input != null ? Count(input.Value, "text_tokens") : null,
            OutputAudioTokens = output != null ? Count(output.Value, "audio_tokens") : null,
            OutputTextTokens = output != null ? Count(output.Value, "text_tokens") : null,
            CachedInputAudioTokens = cached != null ? Count(cached.Value, "audio_tokens") : null,
            CachedInputTextTokens = cached != null ? Count(cached.Value, "text_tokens") : null,
        });
    }

    /// <summary>
    /// Maps xAI realtime transcription-completed usage to normalized usage. Duration-billed transcription reports
    /// <c>seconds</c>; token-billed reports tokens.
    /// </summary>
    private static RealtimeModelV4Usage? MapTranscriptionUsage(JsonElement? usage)
    {
        if (usage == null)
        {
            return null;
        }

        if (String(usage.Value, "type") == "duration")
        {
            return usage.Value.TryGetProperty("seconds", out var seconds) && seconds.ValueKind == JsonValueKind.Number
                ? new RealtimeModelV4Usage { Seconds = seconds.GetDouble() }
                : null;
        }

        var input = Object(usage.Value, "input_token_details");
        return CompactUsage(new RealtimeModelV4Usage
        {
            InputTokens = Count(usage.Value, "input_tokens"),
            OutputTokens = Count(usage.Value, "output_tokens"),
            InputAudioTokens = input != null ? Count(input.Value, "audio_tokens") : null,
            InputTextTokens = input != null ? Count(input.Value, "text_tokens") : null,
            OutputTextTokens = Count(usage.Value, "output_tokens"),
        });
    }

    /// <summary>Drops empty fields; returns null when nothing is observable.</summary>
    private static RealtimeModelV4Usage? CompactUsage(RealtimeModelV4Usage usage)
    {
        return usage.InputTokens == null && usage.OutputTokens == null && usage.CachedInputTokens == null
            && usage.InputTextTokens == null && usage.InputAudioTokens == null
            && usage.OutputTextTokens == null && usage.OutputAudioTokens == null
            && usage.CachedInputTextTokens == null && usage.CachedInputAudioTokens == null
            && usage.Seconds == null
            ? null
            : usage;
    }

    private static JsonObject Format(string? type, int? rate)
    {
        var format = new JsonObject();
        if (type != null)
        {
            format["type"] = type;
        }

        if (rate != null)
        {
            format["rate"] = rate;
        }

        return format;
    }

    private static JsonObject Message(string? role, JsonObject content)
    {
        var message = new JsonObject { ["type"] = "message" };
        if (role != null)
        {
            message["role"] = role;
        }

        message["content"] = new JsonArray(content);
        return message;
    }

    private static JsonObject ConversationItemCreate(JsonObject item)
    {
        return new JsonObject { ["type"] = "conversation.item.create", ["item"] = item };
    }

    private static RealtimeModelV4ServerEvent Custom(string? rawType, JsonElement raw)
    {
        return new RealtimeModelV4ServerEvent("custom") { RawType = rawType, Raw = raw };
    }

    private static RealtimeModelV4ServerEvent Response(string type, JsonElement raw, string? delta = null, string? callId = null, string? name = null, string? arguments = null, string? transcript = null, string? text = null)
    {
        return new RealtimeModelV4ServerEvent(type)
        {
            ResponseId = String(raw, "response_id"),
            ItemId = String(raw, "item_id"),
            CallId = callId,
            Name = name,
            Delta = delta,
            Arguments = arguments,
            Transcript = transcript,
            Text = text,
            Raw = raw,
        };
    }

    private static string? ResponseId(JsonElement raw)
    {
        return (Object(raw, "response") is { } response ? String(response, "id") : null) ?? String(raw, "response_id");
    }

    private static string? ItemId(JsonElement raw)
    {
        return (Object(raw, "item") is { } item ? String(item, "id") : null) ?? String(raw, "item_id");
    }

    private static string? String(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static JsonElement? Object(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : null;
    }

    private static int? Count(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? (int)value.GetDouble()
            : null;
    }
}
