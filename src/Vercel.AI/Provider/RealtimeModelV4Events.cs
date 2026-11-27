// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Provider;

/// <summary>
/// A normalized event emitted by a realtime model (model to client). Maps to the <c>RealtimeModelV4ServerEvent</c>
/// union: <see cref="Type"/> is the union tag (for example <c>response-done</c>) and the other members are the
/// fields that tag carries; fields a tag does not define stay null. <see cref="Raw"/> holds the original provider event.
/// </summary>
public sealed class RealtimeModelV4ServerEvent
{
    /// <summary>Creates an event with a union tag.</summary>
    public RealtimeModelV4ServerEvent(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>The union tag.</summary>
    public string Type { get; }

    /// <summary>Session id for <c>session-started</c>, <c>session-closed</c> and <c>session-created</c>.</summary>
    public string? SessionId { get; set; }

    /// <summary>Response id.</summary>
    public string? ResponseId { get; set; }

    /// <summary>Item id.</summary>
    public string? ItemId { get; set; }

    /// <summary>Previous item id for <c>audio-committed</c>.</summary>
    public string? PreviousItemId { get; set; }

    /// <summary>Function call id.</summary>
    public string? CallId { get; set; }

    /// <summary>Function name for <c>function-call-arguments-done</c>.</summary>
    public string? Name { get; set; }

    /// <summary>Delta text or base64 audio.</summary>
    public string? Delta { get; set; }

    /// <summary>Complete JSON arguments for <c>function-call-arguments-done</c>.</summary>
    public string? Arguments { get; set; }

    /// <summary>Transcript for transcription and <c>audio-transcript-done</c> events.</summary>
    public string? Transcript { get; set; }

    /// <summary>Text for <c>text-done</c>.</summary>
    public string? Text { get; set; }

    /// <summary>Response status for <c>response-done</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Error message for <c>error</c>.</summary>
    public string? Message { get; set; }

    /// <summary>Error code for <c>error</c>.</summary>
    public string? Code { get; set; }

    /// <summary>Client event id for <c>error</c> and <c>command-acknowledged</c>.</summary>
    public string? ClientEventId { get; set; }

    /// <summary>The original provider event type for <c>custom</c> events.</summary>
    public string? RawType { get; set; }

    /// <summary>Close reason for <c>session-closed</c>.</summary>
    public string? Reason { get; set; }

    /// <summary>Normalized usage on completion events or provider snapshots.</summary>
    public RealtimeModelV4Usage? Usage { get; set; }

    /// <summary>Delegation mode for <c>session-started</c>: <c>client</c> or <c>provider</c>.</summary>
    public string? DelegationMode { get; set; }

    /// <summary>Context window usage ratio (0 to 1) for <c>session-usage</c>.</summary>
    public double? ContextWindowUsageRatio { get; set; }

    /// <summary>Speaker for <c>transcript-fragment</c>: <c>user</c> or <c>assistant</c>.</summary>
    public string? Speaker { get; set; }

    /// <summary>Fragment start in milliseconds for <c>transcript-fragment</c>.</summary>
    public double? StartMs { get; set; }

    /// <summary>Fragment end in milliseconds for <c>transcript-fragment</c>.</summary>
    public double? EndMs { get; set; }

    /// <summary>Delegation id for <c>delegation-created</c>.</summary>
    public string? DelegationId { get; set; }

    /// <summary>Delegation target for <c>delegation-created</c>: <c>client</c> or <c>provider</c>.</summary>
    public string? Target { get; set; }

    /// <summary>Offset in milliseconds for <c>delegation-created</c>.</summary>
    public double? OffsetMs { get; set; }

    /// <summary>Acknowledged command name for <c>command-acknowledged</c>.</summary>
    public string? Command { get; set; }

    /// <summary>The conversation item for <c>conversation-item-added</c>.</summary>
    public JsonElement? Item { get; set; }

    /// <summary>The original provider event.</summary>
    public JsonElement? Raw { get; set; }
}

/// <summary>A tool definition in a realtime session config. Maps to <c>RealtimeModelV4ToolDefinition</c>.</summary>
public sealed class RealtimeModelV4ToolDefinition
{
    /// <summary>Creates a function tool definition.</summary>
    public RealtimeModelV4ToolDefinition(string name, string? description, JsonElement parameters)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Parameters = parameters;
    }

    /// <summary>Function name.</summary>
    public string Name { get; }

    /// <summary>Function description.</summary>
    public string? Description { get; }

    /// <summary>JSON Schema of the parameters.</summary>
    public JsonElement Parameters { get; }
}

/// <summary>Audio transcription options of a realtime session config.</summary>
public sealed class RealtimeModelV4AudioTranscription
{
    /// <summary>Transcription model.</summary>
    public string? Model { get; set; }

    /// <summary>Language hint.</summary>
    public string? Language { get; set; }

    /// <summary>Prompt.</summary>
    public string? Prompt { get; set; }
}

/// <summary>Turn detection options. Maps to <c>RealtimeModelV4SessionConfig.turnDetection</c>.</summary>
public sealed class RealtimeModelV4TurnDetection
{
    /// <summary><c>server-vad</c>, <c>semantic-vad</c> or <c>disabled</c>.</summary>
    public string Type { get; set; } = "server-vad";

    /// <summary>Activation threshold.</summary>
    public double? Threshold { get; set; }

    /// <summary>Silence duration in milliseconds.</summary>
    public int? SilenceDurationMs { get; set; }

    /// <summary>Prefix padding in milliseconds.</summary>
    public int? PrefixPaddingMs { get; set; }
}

/// <summary>Maps to <c>RealtimeModelV4SessionConfig</c>.</summary>
public sealed class RealtimeModelV4SessionConfig
{
    /// <summary>System instructions.</summary>
    public string? Instructions { get; set; }

    /// <summary>Voice name.</summary>
    public string? Voice { get; set; }

    /// <summary><c>text</c> and/or <c>audio</c>.</summary>
    public IReadOnlyList<string>? OutputModalities { get; set; }

    /// <summary>Input audio MIME type, for example <c>audio/pcm</c>.</summary>
    public string? InputAudioFormatType { get; set; }

    /// <summary>Input audio sample rate in Hz.</summary>
    public int? InputAudioFormatRate { get; set; }

    /// <summary>Output audio MIME type.</summary>
    public string? OutputAudioFormatType { get; set; }

    /// <summary>Output audio sample rate in Hz.</summary>
    public int? OutputAudioFormatRate { get; set; }

    /// <summary>Input audio transcription.</summary>
    public RealtimeModelV4AudioTranscription? InputAudioTranscription { get; set; }

    /// <summary>Output audio transcription.</summary>
    public RealtimeModelV4AudioTranscription? OutputAudioTranscription { get; set; }

    /// <summary>Turn detection. <see cref="TurnDetectionDisabled"/> maps the upstream <c>null</c> value.</summary>
    public RealtimeModelV4TurnDetection? TurnDetection { get; set; }

    /// <summary>True when upstream passes <c>turnDetection: null</c> (no turn detection).</summary>
    public bool TurnDetectionDisabled { get; set; }

    /// <summary>Function tools.</summary>
    public IReadOnlyList<RealtimeModelV4ToolDefinition>? Tools { get; set; }

    /// <summary>Provider-specific options.</summary>
    public JsonObject? ProviderOptions { get; set; }
}

/// <summary>
/// A conversation item created by the client. Maps to <c>RealtimeModelV4ConversationItem</c>; <see cref="Type"/> is
/// <c>text-message</c>, <c>audio-message</c> or <c>function-call-output</c>.
/// </summary>
public sealed class RealtimeModelV4ConversationItem
{
    /// <summary>Creates an item.</summary>
    public RealtimeModelV4ConversationItem(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>Union tag.</summary>
    public string Type { get; }

    /// <summary>Role (<c>user</c>) for message items.</summary>
    public string? Role { get; set; }

    /// <summary>Text of a <c>text-message</c>.</summary>
    public string? Text { get; set; }

    /// <summary>Base64 audio of an <c>audio-message</c>.</summary>
    public string? Audio { get; set; }

    /// <summary>Call id of a <c>function-call-output</c>.</summary>
    public string? CallId { get; set; }

    /// <summary>Function name of a <c>function-call-output</c>.</summary>
    public string? Name { get; set; }

    /// <summary>Output of a <c>function-call-output</c>.</summary>
    public string? Output { get; set; }
}

/// <summary>Options of a <c>response-create</c> client event.</summary>
public sealed class RealtimeModelV4ResponseOptions
{
    /// <summary>Output modalities.</summary>
    public IReadOnlyList<string>? Modalities { get; set; }

    /// <summary>Instructions for this response.</summary>
    public string? Instructions { get; set; }

    /// <summary>Metadata.</summary>
    public JsonObject? Metadata { get; set; }
}

/// <summary>
/// A normalized event sent by the client. Maps to the <c>RealtimeModelV4ClientEvent</c> union: <see cref="Type"/> is
/// the tag (<c>session-update</c>, <c>session-start</c>, <c>session-close</c>, <c>input-audio-mute</c>,
/// <c>input-audio-unmute</c>, <c>context-append</c>, <c>input-audio-append</c>, <c>input-audio-commit</c>,
/// <c>input-audio-clear</c>, <c>conversation-item-create</c>, <c>conversation-item-truncate</c>,
/// <c>response-create</c>, <c>response-cancel</c>).
/// </summary>
public sealed class RealtimeModelV4ClientEvent
{
    /// <summary>Creates an event with a union tag.</summary>
    public RealtimeModelV4ClientEvent(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>Returns a shallow copy, as the object spread of the upstream union does.</summary>
    internal RealtimeModelV4ClientEvent Clone() => (RealtimeModelV4ClientEvent)MemberwiseClone();

    /// <summary>The union tag.</summary>
    public string Type { get; }

    /// <summary>Event id.</summary>
    public string? EventId { get; set; }

    /// <summary>Config for <c>session-update</c> and <c>session-start</c>.</summary>
    public RealtimeModelV4SessionConfig? Config { get; set; }

    /// <summary>Content for <c>context-append</c>.</summary>
    public string? Content { get; set; }

    /// <summary>Delegation id for <c>context-append</c>.</summary>
    public string? DelegationId { get; set; }

    /// <summary>Base64 audio for <c>input-audio-append</c>.</summary>
    public string? Audio { get; set; }

    /// <summary>Item for <c>conversation-item-create</c>.</summary>
    public RealtimeModelV4ConversationItem? Item { get; set; }

    /// <summary>Item id for <c>conversation-item-truncate</c>.</summary>
    public string? ItemId { get; set; }

    /// <summary>Content index for <c>conversation-item-truncate</c>.</summary>
    public int? ContentIndex { get; set; }

    /// <summary>Audio end in milliseconds for <c>conversation-item-truncate</c>.</summary>
    public int? AudioEndMs { get; set; }

    /// <summary>Options for <c>response-create</c>.</summary>
    public RealtimeModelV4ResponseOptions? Options { get; set; }

    /// <summary>Provider options for <c>context-append</c>.</summary>
    public JsonObject? ProviderOptions { get; set; }
}
