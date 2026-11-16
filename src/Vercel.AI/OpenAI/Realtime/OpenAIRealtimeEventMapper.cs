// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Session options of the OpenAI realtime event mapper. Maps to <c>OpenAIRealtimeSessionOptions</c>.</summary>
internal sealed class OpenAIRealtimeSessionOptions
{
    /// <summary>Maps to <c>intent</c>.</summary>
    public RealtimeModelV4SessionIntent? Intent { get; init; }
}

/// <summary>
/// Maps between normalized realtime events and the OpenAI Realtime API wire format. Maps to
/// <c>openai-realtime-event-mapper.ts</c>.
/// </summary>
internal static class OpenAIRealtimeEventMapper
{
    /// <summary>Parses a raw OpenAI Realtime API server event into a normalized event. Maps to <c>parseOpenAIRealtimeServerEvent</c>.</summary>
    public static RealtimeModelV4ServerEvent ParseOpenAIRealtimeServerEvent(JsonElement raw)
    {
        var type = GetString(raw, "type");
        var item = GetObject(raw, "item");
        var response = GetObject(raw, "response");
        var itemId = GetString(raw, "item_id");
        var responseId = GetString(raw, "response_id");

        RealtimeModelV4ServerEvent Event(string tag)
        {
            return new RealtimeModelV4ServerEvent(tag) { Raw = raw };
        }

        switch (type)
        {
            // Session lifecycle
            case "session.created":
                var created = Event("session-created");
                created.SessionId = GetString(GetObject(raw, "session"), "id");
                return created;

            case "session.updated":
                return Event("session-updated");

            // Input audio buffer
            case "input_audio_buffer.speech_started":
                return WithIds(Event("speech-started"), null, itemId);

            case "input_audio_buffer.speech_stopped":
                return WithIds(Event("speech-stopped"), null, itemId);

            case "input_audio_buffer.committed":
                var committed = WithIds(Event("audio-committed"), null, itemId);
                committed.PreviousItemId = GetString(raw, "previous_item_id");
                return committed;

            // Conversation items
            case "conversation.item.added":
                var added = WithIds(Event("conversation-item-added"), null, GetString(item, "id") ?? itemId);
                added.Item = item;
                return added;

            case "conversation.item.input_audio_transcription.completed":
                var transcribed = WithIds(Event("input-transcription-completed"), null, itemId);
                transcribed.Transcript = GetString(raw, "transcript") ?? string.Empty;
                transcribed.Usage = MapTranscriptionUsage(GetObject(raw, "usage"));
                return transcribed;

            // Response lifecycle
            case "response.created":
                return WithIds(Event("response-created"), GetString(response, "id") ?? responseId, null);

            case "response.done":
                var done = WithIds(Event("response-done"), GetString(response, "id") ?? responseId, null);
                done.Status = GetString(response, "status") ?? "completed";
                done.Usage = MapResponseUsage(GetObject(response, "usage"));
                return done;

            // Output item lifecycle
            case "response.output_item.added":
                return WithIds(Event("output-item-added"), responseId, GetString(item, "id") ?? itemId);

            case "response.output_item.done":
                return WithIds(Event("output-item-done"), responseId, GetString(item, "id") ?? itemId);

            case "response.content_part.added":
                return WithIds(Event("content-part-added"), responseId, itemId);

            case "response.content_part.done":
                return WithIds(Event("content-part-done"), responseId, itemId);

            // Audio output
            case "response.output_audio.delta":
                var audioDelta = WithIds(Event("audio-delta"), responseId, itemId);
                audioDelta.Delta = GetString(raw, "delta");
                return audioDelta;

            case "response.output_audio.done":
                return WithIds(Event("audio-done"), responseId, itemId);

            // Audio transcript output
            case "response.output_audio_transcript.delta":
                var transcriptDelta = WithIds(Event("audio-transcript-delta"), responseId, itemId);
                transcriptDelta.Delta = GetString(raw, "delta");
                return transcriptDelta;

            case "response.output_audio_transcript.done":
                var transcriptDone = WithIds(Event("audio-transcript-done"), responseId, itemId);
                transcriptDone.Transcript = GetString(raw, "transcript");
                return transcriptDone;

            // Text output
            case "response.output_text.delta":
                var textDelta = WithIds(Event("text-delta"), responseId, itemId);
                textDelta.Delta = GetString(raw, "delta");
                return textDelta;

            case "response.output_text.done":
                var textDone = WithIds(Event("text-done"), responseId, itemId);
                textDone.Text = GetString(raw, "text");
                return textDone;

            // Function calling
            case "response.function_call_arguments.delta":
                var argumentsDelta = WithIds(Event("function-call-arguments-delta"), responseId, itemId);
                argumentsDelta.CallId = GetString(raw, "call_id");
                argumentsDelta.Delta = GetString(raw, "delta");
                return argumentsDelta;

            case "response.function_call_arguments.done":
                var argumentsDone = WithIds(Event("function-call-arguments-done"), responseId, itemId);
                argumentsDone.CallId = GetString(raw, "call_id");
                argumentsDone.Name = GetString(raw, "name");
                argumentsDone.Arguments = GetString(raw, "arguments");
                return argumentsDone;

            // Error
            case "error":
                var error = GetObject(raw, "error");
                var errorEvent = Event("error");
                errorEvent.Message = GetString(error, "message") ?? GetString(raw, "message") ?? "Unknown error";
                errorEvent.Code = GetString(error, "code") ?? GetString(raw, "code");
                errorEvent.ClientEventId = GetString(error, "event_id");
                return errorEvent;

            // Pass-through
            default:
                var custom = Event("custom");
                custom.RawType = type;
                return custom;
        }
    }

    /// <summary>Serializes a normalized client event into OpenAI's Realtime API format. Maps to <c>serializeOpenAIRealtimeClientEvent</c>.</summary>
    /// <returns>The wire event, or <c>null</c> for an event the realtime API has no counterpart for.</returns>
    public static JsonNode? SerializeOpenAIRealtimeClientEvent(
        RealtimeModelV4ClientEvent @event,
        string modelId,
        OpenAIRealtimeSessionOptions? options = null)
    {
        AssertSupportedOpenAIIntent(options?.Intent);

        switch (@event.Type)
        {
            case "session-update":
                var update = new JsonObject
                {
                    ["type"] = "session.update",
                    ["session"] = BuildOpenAISessionConfig(@event.Config ?? new RealtimeModelV4SessionConfig(), modelId, options),
                };
                AddEventId(update, @event);
                return update;

            case "input-audio-append":
                var append = new JsonObject
                {
                    ["type"] = "input_audio_buffer.append",
                    ["audio"] = @event.Audio,
                };
                AddEventId(append, @event);
                return append;

            case "input-audio-commit":
                return new JsonObject { ["type"] = "input_audio_buffer.commit" };

            case "input-audio-clear":
                return new JsonObject { ["type"] = "input_audio_buffer.clear" };

            case "conversation-item-create":
                return SerializeConversationItem(@event.Item);

            case "conversation-item-truncate":
                return new JsonObject
                {
                    ["type"] = "conversation.item.truncate",
                    ["item_id"] = @event.ItemId,
                    ["content_index"] = @event.ContentIndex,
                    ["audio_end_ms"] = @event.AudioEndMs,
                };

            case "response-create":
                var create = new JsonObject { ["type"] = "response.create" };
                if (@event.Options != null)
                {
                    var response = new JsonObject();
                    if (@event.Options.Modalities != null)
                    {
                        response["output_modalities"] = ToArray(@event.Options.Modalities);
                    }

                    if (@event.Options.Instructions != null)
                    {
                        response["instructions"] = @event.Options.Instructions;
                    }

                    if (@event.Options.Metadata != null)
                    {
                        response["metadata"] = @event.Options.Metadata.DeepClone();
                    }

                    create["response"] = response;
                }

                return create;

            case "response-cancel":
                return new JsonObject { ["type"] = "response.cancel" };

            default:
                return null;
        }
    }

    /// <summary>Builds an OpenAI-specific session configuration from a normalized config. Maps to <c>buildOpenAISessionConfig</c>.</summary>
    public static JsonObject BuildOpenAISessionConfig(
        RealtimeModelV4SessionConfig config,
        string modelId,
        OpenAIRealtimeSessionOptions? options = null)
    {
        var intent = options?.Intent ?? RealtimeModelV4SessionIntent.Conversation;
        AssertSupportedOpenAIIntent(intent);
        var transcription = intent == RealtimeModelV4SessionIntent.Transcription;
        var conversation = intent == RealtimeModelV4SessionIntent.Conversation;
        if (transcription)
        {
            foreach (var (key, present) in new (string, bool)[]
            {
                ("instructions", config.Instructions != null),
                ("voice", config.Voice != null),
                ("outputModalities", config.OutputModalities != null),
                ("outputAudioFormat", config.OutputAudioFormatType != null || config.OutputAudioFormatRate != null),
                ("outputAudioTranscription", config.OutputAudioTranscription != null),
                ("tools", config.Tools != null),
            })
            {
                if (present)
                {
                    throw new InvalidOperationException("OpenAI realtime transcription sessions do not support '" + key + "'.");
                }
            }
        }

        var session = new JsonObject { ["type"] = transcription ? "transcription" : "realtime" };
        if (conversation)
        {
            session["model"] = modelId;
        }

        if (conversation && config.Instructions != null)
        {
            session["instructions"] = config.Instructions;
        }

        if (conversation && config.OutputModalities != null)
        {
            session["output_modalities"] = ToArray(config.OutputModalities);
        }

        var audio = new JsonObject();
        var inputFormat = config.InputAudioFormatType != null || config.InputAudioFormatRate != null;
        if (transcription || inputFormat || config.InputAudioTranscription != null || config.TurnDetection != null)
        {
            var input = new JsonObject();
            if (inputFormat)
            {
                input["format"] = AudioFormat(config.InputAudioFormatType, config.InputAudioFormatRate);
            }

            if (config.TurnDetection != null)
            {
                if (config.TurnDetection.Type == "disabled")
                {
                    input["turn_detection"] = null;
                }
                else
                {
                    var detection = new JsonObject
                    {
                        ["type"] = config.TurnDetection.Type == "server-vad" ? "server_vad" : "semantic_vad",
                    };
                    if (config.TurnDetection.Threshold != null)
                    {
                        detection["threshold"] = config.TurnDetection.Threshold;
                    }

                    if (config.TurnDetection.SilenceDurationMs != null)
                    {
                        detection["silence_duration_ms"] = config.TurnDetection.SilenceDurationMs;
                    }

                    if (config.TurnDetection.PrefixPaddingMs != null)
                    {
                        detection["prefix_padding_ms"] = config.TurnDetection.PrefixPaddingMs;
                    }

                    input["turn_detection"] = detection;
                }
            }

            if (transcription)
            {
                input["transcription"] = Transcription(config.InputAudioTranscription, config.InputAudioTranscription?.Model ?? modelId);
            }
            else if (config.InputAudioTranscription != null)
            {
                input["transcription"] = Transcription(config.InputAudioTranscription, config.InputAudioTranscription.Model ?? "gpt-realtime-whisper");
            }

            audio["input"] = input;
        }

        var outputFormat = config.OutputAudioFormatType != null || config.OutputAudioFormatRate != null;
        if (conversation && (outputFormat || config.Voice != null))
        {
            var output = new JsonObject();
            if (outputFormat)
            {
                output["format"] = AudioFormat(config.OutputAudioFormatType, config.OutputAudioFormatRate);
            }

            if (config.Voice != null)
            {
                output["voice"] = config.Voice;
            }

            audio["output"] = output;
        }

        if (audio.Count > 0)
        {
            session["audio"] = audio;
        }

        if (conversation && config.Tools != null && config.Tools.Count > 0)
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
            session["tool_choice"] = "auto";
        }

        if (config.ProviderOptions != null)
        {
            foreach (var pair in config.ProviderOptions)
            {
                session[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return session;
    }

    internal static string IntentName(RealtimeModelV4SessionIntent intent)
    {
        return intent switch
        {
            RealtimeModelV4SessionIntent.Conversation => "conversation",
            RealtimeModelV4SessionIntent.Transcription => "transcription",
            _ => ((int)intent).ToString(CultureInfo.InvariantCulture),
        };
    }

    private static void AssertSupportedOpenAIIntent(RealtimeModelV4SessionIntent? intent)
    {
        if (intent != null && intent != RealtimeModelV4SessionIntent.Conversation && intent != RealtimeModelV4SessionIntent.Transcription)
        {
            throw new InvalidOperationException("OpenAI realtime session intent '" + IntentName(intent.Value) + "' is not supported.");
        }
    }

    private static JsonNode? SerializeConversationItem(RealtimeModelV4ConversationItem? item)
    {
        if (item == null)
        {
            return null;
        }

        switch (item.Type)
        {
            case "text-message":
                return ItemCreate(new JsonObject
                {
                    ["type"] = "message",
                    ["role"] = item.Role,
                    ["content"] = new JsonArray(new JsonObject { ["type"] = "input_text", ["text"] = item.Text }),
                });
            case "audio-message":
                return ItemCreate(new JsonObject
                {
                    ["type"] = "message",
                    ["role"] = item.Role,
                    ["content"] = new JsonArray(new JsonObject { ["type"] = "input_audio", ["audio"] = item.Audio }),
                });
            case "function-call-output":
                return ItemCreate(new JsonObject
                {
                    ["type"] = "function_call_output",
                    ["call_id"] = item.CallId,
                    ["output"] = item.Output,
                });
            default:
                return null;
        }
    }

    private static JsonObject ItemCreate(JsonObject item)
    {
        return new JsonObject { ["type"] = "conversation.item.create", ["item"] = item };
    }

    private static void AddEventId(JsonObject target, RealtimeModelV4ClientEvent @event)
    {
        if (@event.EventId != null)
        {
            target["event_id"] = @event.EventId;
        }
    }

    private static JsonObject AudioFormat(string? type, int? rate)
    {
        var format = new JsonObject { ["type"] = type };
        if (rate != null)
        {
            format["rate"] = rate;
        }

        return format;
    }

    private static JsonObject Transcription(RealtimeModelV4AudioTranscription? source, string model)
    {
        var transcription = new JsonObject { ["model"] = model };
        if (source?.Language != null)
        {
            transcription["language"] = source.Language;
        }

        if (source?.Prompt != null)
        {
            transcription["prompt"] = source.Prompt;
        }

        return transcription;
    }

    private static JsonArray ToArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static RealtimeModelV4ServerEvent WithIds(RealtimeModelV4ServerEvent @event, string? responseId, string? itemId)
    {
        @event.ResponseId = responseId;
        @event.ItemId = itemId;
        return @event;
    }

    /// <summary>
    /// Maps OpenAI realtime <c>response.done</c> usage to normalized usage. Input buckets are reported gross
    /// (cache-inclusive) with the cached portion surfaced separately. Maps to <c>mapResponseUsage</c>.
    /// </summary>
    private static RealtimeModelV4Usage? MapResponseUsage(JsonElement? usage)
    {
        if (usage is not { } value)
        {
            return null;
        }

        var input = GetObject(value, "input_token_details");
        var output = GetObject(value, "output_token_details");
        var cached = GetObject(input, "cached_tokens_details");
        return CompactUsage(new RealtimeModelV4Usage
        {
            InputTokens = GetInt(value, "input_tokens"),
            OutputTokens = GetInt(value, "output_tokens"),
            CachedInputTokens = GetInt(input, "cached_tokens"),
            InputAudioTokens = GetInt(input, "audio_tokens"),
            InputTextTokens = GetInt(input, "text_tokens"),
            OutputAudioTokens = GetInt(output, "audio_tokens"),
            OutputTextTokens = GetInt(output, "text_tokens"),
            CachedInputAudioTokens = GetInt(cached, "audio_tokens"),
            CachedInputTextTokens = GetInt(cached, "text_tokens"),
        });
    }

    /// <summary>
    /// Maps OpenAI realtime transcription-completed usage to normalized usage. Duration-billed transcription
    /// reports <c>seconds</c>; token-billed reports tokens. Maps to <c>mapTranscriptionUsage</c>.
    /// </summary>
    private static RealtimeModelV4Usage? MapTranscriptionUsage(JsonElement? usage)
    {
        if (usage is not { } value)
        {
            return null;
        }

        if (GetString(value, "type") == "duration")
        {
            return GetDouble(value, "seconds") is { } seconds ? new RealtimeModelV4Usage { Seconds = seconds } : null;
        }

        var input = GetObject(value, "input_token_details");
        return CompactUsage(new RealtimeModelV4Usage
        {
            InputTokens = GetInt(value, "input_tokens"),
            OutputTokens = GetInt(value, "output_tokens"),
            InputAudioTokens = GetInt(input, "audio_tokens"),
            InputTextTokens = GetInt(input, "text_tokens"),
            OutputTextTokens = GetInt(value, "output_tokens"),
        });
    }

    /// <summary>Returns <c>null</c> when nothing is observable. Maps to <c>compactUsage</c>.</summary>
    private static RealtimeModelV4Usage? CompactUsage(RealtimeModelV4Usage usage)
    {
        var observable = usage.InputTokens != null
            || usage.OutputTokens != null
            || usage.CachedInputTokens != null
            || usage.InputTextTokens != null
            || usage.InputAudioTokens != null
            || usage.OutputTextTokens != null
            || usage.OutputAudioTokens != null
            || usage.CachedInputTextTokens != null
            || usage.CachedInputAudioTokens != null
            || usage.Seconds != null;
        return observable ? usage : null;
    }

    private static JsonElement? GetObject(JsonElement? parent, string name)
    {
        if (parent is { ValueKind: JsonValueKind.Object } value
            && value.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.Object)
        {
            return property;
        }

        return null;
    }

    private static string? GetString(JsonElement? parent, string name)
    {
        if (parent is { ValueKind: JsonValueKind.Object } value
            && value.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static double? GetDouble(JsonElement? parent, string name)
    {
        if (parent is { ValueKind: JsonValueKind.Object } value
            && value.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.Number)
        {
            return property.GetDouble();
        }

        return null;
    }

    private static int? GetInt(JsonElement? parent, string name)
    {
        return GetDouble(parent, name) is { } number ? (int)number : null;
    }
}
