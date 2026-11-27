// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Ui;

namespace Vercel.AI;

/// <summary>The status of a realtime connection. Maps to <c>RealtimeStatus</c>.</summary>
public enum RealtimeStatus
{
    /// <summary>Maps to <c>'disconnected'</c>.</summary>
    Disconnected,

    /// <summary>Maps to <c>'connecting'</c>.</summary>
    Connecting,

    /// <summary>Maps to <c>'connected'</c>.</summary>
    Connected,

    /// <summary>Maps to <c>'closing'</c>.</summary>
    Closing,

    /// <summary>Maps to <c>'error'</c>.</summary>
    Error,
}

/// <summary>The state a <see cref="RealtimeEventReducer"/> folds events into. Maps to <c>RealtimeState</c>.</summary>
public sealed record RealtimeState
{
    /// <summary>The connection status.</summary>
    public RealtimeStatus Status { get; init; } = RealtimeStatus.Disconnected;

    /// <summary>The conversation as UI messages.</summary>
    public IReadOnlyList<UIMessage> Messages { get; init; } = [];

    /// <summary>The most recent server events.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> Events { get; init; } = [];

    /// <summary>Whether the microphone is capturing.</summary>
    public bool IsCapturing { get; init; }

    /// <summary>Whether audio is playing.</summary>
    public bool IsPlaying { get; init; }

    /// <summary>The session-level state.</summary>
    public RealtimeSessionState? Session { get; init; }
}

/// <summary>An effect the caller must run after a reduction. Maps to <c>RealtimeReducerEffect</c>.</summary>
public sealed class RealtimeReducerEffect
{
    /// <summary>Creates an effect with a union tag: <c>play-audio</c>, <c>speech-started</c>, <c>tool-call</c> or <c>error</c>.</summary>
    public RealtimeReducerEffect(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>The union tag.</summary>
    public string Type { get; }

    /// <summary>Item id for <c>play-audio</c>.</summary>
    public string? ItemId { get; init; }

    /// <summary>Base64 audio for <c>play-audio</c>.</summary>
    public string? Delta { get; init; }

    /// <summary>Call id for <c>tool-call</c>.</summary>
    public string? CallId { get; init; }

    /// <summary>Tool name for <c>tool-call</c>.</summary>
    public string? Name { get; init; }

    /// <summary>Parsed arguments for <c>tool-call</c>.</summary>
    public JsonElement? Args { get; init; }

    /// <summary>Unparsed arguments for <c>tool-call</c>.</summary>
    public string? RawArguments { get; init; }

    /// <summary>The error for <c>error</c>.</summary>
    public Exception? Error { get; init; }
}

/// <summary>The output of a tool call. Maps to <c>RealtimeToolOutput</c>.</summary>
public sealed class RealtimeToolOutput
{
    /// <summary>Creates a tool output.</summary>
    public RealtimeToolOutput(string callId, string? name, string output)
    {
        CallId = callId;
        Name = name;
        Output = output;
    }

    /// <summary>The call id.</summary>
    public string CallId { get; }

    /// <summary>The tool name.</summary>
    public string? Name { get; }

    /// <summary>The JSON text of the output.</summary>
    public string Output { get; }
}

/// <summary>The result of <see cref="RealtimeEventReducer.ReduceServerEventAsync"/>.</summary>
public sealed class RealtimeReduction
{
    /// <summary>Creates a reduction.</summary>
    public RealtimeReduction(RealtimeState state, IReadOnlyList<RealtimeReducerEffect> effects)
    {
        State = state;
        Effects = effects;
    }

    /// <summary>The next state.</summary>
    public RealtimeState State { get; }

    /// <summary>The effects to run.</summary>
    public IReadOnlyList<RealtimeReducerEffect> Effects { get; }
}

/// <summary>The result of <see cref="RealtimeEventReducer.AddToolOutput"/>.</summary>
public sealed class RealtimeToolOutputResult
{
    /// <summary>Creates a result.</summary>
    public RealtimeToolOutputResult(RealtimeState state, RealtimeToolOutput output)
    {
        State = state;
        Output = output;
    }

    /// <summary>The next state.</summary>
    public RealtimeState State { get; }

    /// <summary>The output to send to the model.</summary>
    public RealtimeToolOutput Output { get; }
}

/// <summary>Folds normalized server events into UI messages. Maps to <c>RealtimeEventReducer</c>.</summary>
public sealed class RealtimeEventReducer
{
    private readonly object _gate = new();
    private readonly int _maxEvents;
    private string? _currentAssistantMessageId;
    private readonly Dictionary<string, string> _textAccumulators = [];
    private readonly Dictionary<string, string> _toolArgAccumulators = [];
    private readonly Dictionary<string, string> _toolCallIdToMessageId = [];
    private readonly Dictionary<string, string> _toolCallIdToName = [];
    private readonly Dictionary<string, int> _inputAudioMessageInsertIndex = [];
    private readonly Dictionary<string, (string MessageId, int PartIndex)> _itemIdToPartLocation =
        [];

    /// <summary>Creates a reducer that keeps <paramref name="maxEvents"/> events.</summary>
    public RealtimeEventReducer(int maxEvents = 500)
    {
        _maxEvents = maxEvents;
    }

    /// <summary>Creates the initial state. Maps to <c>createInitialRealtimeState</c>.</summary>
    public static RealtimeState CreateInitialRealtimeState() => new();

    /// <summary>Sets the status. Maps to <c>setStatus</c>.</summary>
    public RealtimeState SetStatus(RealtimeState state, RealtimeStatus status) =>
        state with
        {
            Status = status,
        };

    /// <summary>Sets whether the microphone captures. Maps to <c>setCapturing</c>.</summary>
    public RealtimeState SetCapturing(RealtimeState state, bool isCapturing) =>
        state with
        {
            IsCapturing = isCapturing,
        };

    /// <summary>Sets whether audio plays. Maps to <c>setPlaying</c>.</summary>
    public RealtimeState SetPlaying(RealtimeState state, bool isPlaying) =>
        state with
        {
            IsPlaying = isPlaying,
        };

    /// <summary>Adds a user text message. Maps to <c>addUserTextMessage</c>.</summary>
    public RealtimeState AddUserTextMessage(RealtimeState state, string text)
    {
        var message = new UIMessage(UIMessageRole.User, [new TextUIPart(text) { State = "done" }])
        {
            Id = "user-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
        return state with { Messages = [.. state.Messages, message] };
    }

    /// <summary>Marks a tool call as answered. Maps to <c>addToolOutput</c>.</summary>
    public RealtimeToolOutputResult AddToolOutput(
        RealtimeState state,
        string callId,
        JsonElement result
    )
    {
        lock (_gate)
        {
            _toolCallIdToName.TryGetValue(callId, out var name);
            return new RealtimeToolOutputResult(
                UpdateToolPartState(state, callId, result),
                new RealtimeToolOutput(callId, name, result.GetRawText())
            );
        }
    }

    /// <summary>Folds a server event into the state. Maps to <c>reduceServerEvent</c>.</summary>
    public Task<RealtimeReduction> ReduceServerEventAsync(
        RealtimeState state,
        RealtimeModelV4ServerEvent serverEvent
    )
    {
        lock (_gate)
        {
            return Task.FromResult(ReduceServerEvent(state, serverEvent));
        }
    }

    private RealtimeReduction ReduceServerEvent(
        RealtimeState state,
        RealtimeModelV4ServerEvent serverEvent
    )
    {
        var nextState = PushEvent(state, serverEvent);
        var effects = new List<RealtimeReducerEffect>();

        switch (serverEvent.Type)
        {
            case "session-created":
            case "session-updated":
                if (nextState.Status == RealtimeStatus.Connecting)
                {
                    nextState = SetStatus(nextState, RealtimeStatus.Connected);
                }

                break;

            case "audio-delta":
                effects.Add(
                    new RealtimeReducerEffect("play-audio")
                    {
                        ItemId = serverEvent.ItemId,
                        Delta = serverEvent.Delta,
                    }
                );
                break;

            case "audio-committed":
                if (serverEvent.ItemId != null)
                {
                    _inputAudioMessageInsertIndex[serverEvent.ItemId] = nextState.Messages.Count;
                }

                break;

            case "audio-transcript-delta":
            case "text-delta":
                nextState = AppendTextDelta(
                    nextState,
                    serverEvent.ItemId ?? string.Empty,
                    serverEvent.Delta ?? string.Empty
                );
                break;

            case "audio-transcript-done":
                nextState = FinalizeText(
                    nextState,
                    serverEvent.ItemId ?? string.Empty,
                    serverEvent.Transcript
                );
                break;

            case "text-done":
                nextState = FinalizeText(
                    nextState,
                    serverEvent.ItemId ?? string.Empty,
                    serverEvent.Text
                );
                break;

            case "input-transcription-completed":
                nextState = AddInputTranscriptionMessage(
                    nextState,
                    serverEvent.ItemId ?? string.Empty,
                    serverEvent.Transcript ?? string.Empty
                );
                break;

            case "response-created":
            case "response-done":
                _currentAssistantMessageId = null;
                break;

            case "speech-started":
                _currentAssistantMessageId = null;
                effects.Add(new RealtimeReducerEffect("speech-started"));
                break;

            case "function-call-arguments-delta":
            {
                var callId = serverEvent.CallId ?? string.Empty;
                (nextState, var messageId) = GetOrCreateAssistantMessage(nextState);
                _toolCallIdToMessageId[callId] = messageId;
                _toolArgAccumulators.TryGetValue(callId, out var accumulated);
                _toolArgAccumulators[callId] = (accumulated ?? string.Empty) + serverEvent.Delta;
                nextState = EnsureToolPart(nextState, messageId, callId);
                break;
            }

            case "function-call-arguments-done":
                nextState = ReduceFunctionCallDone(nextState, serverEvent, effects);
                break;

            case "error":
                effects.Add(
                    new RealtimeReducerEffect("error")
                    {
                        Error = new AiSdkException(serverEvent.Message ?? string.Empty),
                    }
                );
                break;
        }

        return new RealtimeReduction(nextState, effects);
    }

    private static UIMessage Replace(UIMessage message, IReadOnlyList<UIMessagePart> parts) =>
        new(message.Role, parts) { Id = message.Id, Metadata = message.Metadata };

    private RealtimeState ReduceFunctionCallDone(
        RealtimeState state,
        RealtimeModelV4ServerEvent serverEvent,
        List<RealtimeReducerEffect> effects
    )
    {
        var callId = serverEvent.CallId ?? string.Empty;
        var name = serverEvent.Name ?? string.Empty;
        var arguments = serverEvent.Arguments ?? string.Empty;
        _toolArgAccumulators.Remove(callId);
        _toolCallIdToName[callId] = name;

        JsonElement? parsed = null;
        try
        {
            using var document = JsonDocument.Parse(arguments);
            parsed = document.RootElement.Clone();
        }
        catch (JsonException) { }

        var parsedInput = parsed ?? JsonSerializer.SerializeToElement(new JsonObject());
        if (_toolCallIdToMessageId.TryGetValue(callId, out var messageId))
        {
            state = MarkToolInputAvailable(state, messageId, callId, name, parsedInput);
        }

        effects.Add(
            parsed == null
                ? new RealtimeReducerEffect("error")
                {
                    Error = new AiSdkException("Failed to parse tool arguments: " + arguments),
                }
                : new RealtimeReducerEffect("tool-call")
                {
                    CallId = callId,
                    Name = name,
                    Args = parsedInput,
                    RawArguments = arguments,
                }
        );
        return state;
    }

    private RealtimeState PushEvent(RealtimeState state, RealtimeModelV4ServerEvent serverEvent)
    {
        var events = state.Events.Append(serverEvent).ToList();
        return state with
        {
            Events = events.Count > _maxEvents ? events.Skip(events.Count - _maxEvents).ToList() : events,
        };
    }

    private (RealtimeState State, string MessageId) GetOrCreateAssistantMessage(
        RealtimeState state
    )
    {
        if (_currentAssistantMessageId != null)
        {
            return (state, _currentAssistantMessageId);
        }

        var messageId =
            "assistant-"
            + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            + "-"
            + Guid.NewGuid().ToString("N").Substring(0, 4);
        _currentAssistantMessageId = messageId;
        var message = new UIMessage(UIMessageRole.Assistant, []) { Id = messageId };
        return (state with { Messages = [.. state.Messages, message] }, messageId);
    }

    private RealtimeState AddInputTranscriptionMessage(
        RealtimeState state,
        string itemId,
        string transcript
    )
    {
        var messageId = "user-" + itemId;
        UIMessagePart[] parts = [new TextUIPart(transcript) { State = "done" }];
        if (state.Messages.Any(message => message.Id == messageId))
        {
            return state with
            {
                Messages = state
                    .Messages.Select(message =>
                        message.Id == messageId ? Replace(message, parts) : message
                    )
                    .ToList(),
            };
        }

        var insertIndex = Math.Min(
            _inputAudioMessageInsertIndex.TryGetValue(itemId, out var index)
                ? index
                : state.Messages.Count,
            state.Messages.Count
        );
        var messages = state.Messages.ToList();
        messages.Insert(
            insertIndex,
            new UIMessage(UIMessageRole.User, parts) { Id = messageId }
        );
        return state with { Messages = messages };
    }

    private RealtimeState AppendTextDelta(RealtimeState state, string itemId, string delta)
    {
        var (stateWithMessage, messageId) = GetOrCreateAssistantMessage(state);
        _textAccumulators.TryGetValue(itemId, out var accumulated);
        var text = (accumulated ?? string.Empty) + delta;
        _textAccumulators[itemId] = text;

        if (_itemIdToPartLocation.TryGetValue(itemId, out var location))
        {
            return UpdateMessagePart(
                stateWithMessage,
                location.MessageId,
                location.PartIndex,
                new TextUIPart(text) { State = "streaming" }
            );
        }

        return stateWithMessage with
        {
            Messages = stateWithMessage
                .Messages.Select(message =>
                {
                    if (message.Id != messageId)
                    {
                        return message;
                    }

                    _itemIdToPartLocation[itemId] = (messageId, message.Parts.Count);
                    return Replace(
                        message,
                        [.. message.Parts, new TextUIPart(text) { State = "streaming" }]
                    );
                })
                .ToList(),
        };
    }

    private RealtimeState FinalizeText(RealtimeState state, string itemId, string? finalText)
    {
        _textAccumulators.TryGetValue(itemId, out var accumulated);
        var text = finalText ?? accumulated ?? string.Empty;
        _textAccumulators.Remove(itemId);
        if (!_itemIdToPartLocation.TryGetValue(itemId, out var location))
        {
            return state;
        }

        _itemIdToPartLocation.Remove(itemId);
        return UpdateMessagePart(
            state,
            location.MessageId,
            location.PartIndex,
            new TextUIPart(text) { State = "done" }
        );
    }

    private static RealtimeState EnsureToolPart(
        RealtimeState state,
        string messageId,
        string callId
    ) =>
        state with
        {
            Messages = state
                .Messages.Select(message =>
                {
                    if (
                        message.Id != messageId
                        || message.Parts.Any(part =>
                            part is ToolUIPart { IsDynamic: true } tool && tool.ToolCallId == callId
                        )
                    )
                    {
                        return message;
                    }

                    return Replace(
                        message,
                        [
                            .. message.Parts,
                            new ToolUIPart(
                                string.Empty,
                                callId,
                                ToolUIPartState.InputStreaming,
                                isDynamic: true
                            ),
                        ]
                    );
                })
                .ToList(),
        };

    private static RealtimeState MarkToolInputAvailable(
        RealtimeState state,
        string messageId,
        string callId,
        string name,
        JsonElement input
    ) =>
        MapToolPart(
            state,
            messageId,
            callId,
            _ => new ToolUIPart(name, callId, ToolUIPartState.InputAvailable, isDynamic: true)
            {
                Input = input,
            }
        );

    private RealtimeState UpdateToolPartState(
        RealtimeState state,
        string callId,
        JsonElement result
    )
    {
        if (!_toolCallIdToMessageId.TryGetValue(callId, out var messageId))
        {
            return state;
        }

        return MapToolPart(
            state,
            messageId,
            callId,
            tool => new ToolUIPart(
                tool.ToolName,
                callId,
                ToolUIPartState.OutputAvailable,
                isDynamic: true
            )
            {
                Input = tool.Input,
                Output = result,
            }
        );
    }

    private static RealtimeState MapToolPart(
        RealtimeState state,
        string messageId,
        string callId,
        Func<ToolUIPart, ToolUIPart> update
    ) =>
        state with
        {
            Messages = state
                .Messages.Select(message =>
                    message.Id != messageId
                        ? message
                        : Replace(
                            message,
                            message
                                .Parts.Select(part =>
                                    part is ToolUIPart { IsDynamic: true } tool
                                    && tool.ToolCallId == callId
                                        ? update(tool)
                                        : part
                                )
                                .ToList()
                        )
                )
                .ToList(),
        };

    private static RealtimeState UpdateMessagePart(
        RealtimeState state,
        string messageId,
        int partIndex,
        TextUIPart part
    ) =>
        state with
        {
            Messages = state
                .Messages.Select(message =>
                {
                    if (message.Id != messageId)
                    {
                        return message;
                    }

                    var parts = message.Parts.ToList();
                    parts[partIndex] = part;
                    return Replace(message, parts);
                })
                .ToList(),
        };
}
