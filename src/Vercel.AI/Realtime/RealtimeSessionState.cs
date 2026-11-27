// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>How a realtime session ended. Maps to the <c>finalization</c> union of <c>RealtimeSessionState</c>.</summary>
public enum RealtimeFinalization
{
    /// <summary>Final usage has not arrived. Maps to <c>'pending'</c>.</summary>
    Pending,

    /// <summary>The provider confirmed the end of the session. Maps to <c>'confirmed'</c>.</summary>
    Confirmed,

    /// <summary>The session ended without a confirmation. Maps to <c>'unconfirmed'</c>.</summary>
    Unconfirmed,
}

/// <summary>Session-level state of a realtime conversation. Maps to <c>RealtimeSessionState</c>.</summary>
public sealed record RealtimeSessionState
{
    /// <summary>The delegation mode: <c>client</c> or <c>provider</c>.</summary>
    public string? DelegationMode { get; init; }

    /// <summary>The session id.</summary>
    public string? SessionId { get; init; }

    /// <summary>The transcript fragments, oldest first.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> Transcripts { get; init; } = [];

    /// <summary>The delegations the provider created.</summary>
    public IReadOnlyList<RealtimeModelV4ServerEvent> Delegations { get; init; } = [];

    /// <summary>The last reported usage.</summary>
    public RealtimeModelV4Usage? Usage { get; init; }

    /// <summary>Whether the end of the session is confirmed.</summary>
    public RealtimeFinalization Finalization { get; init; } = RealtimeFinalization.Pending;

    /// <summary>Why the session ended.</summary>
    public string? TerminationReason { get; init; }

    /// <summary>Whether the input is muted.</summary>
    public bool IsInputMuted { get; init; }

    /// <summary>Creates the initial state. Maps to <c>createSessionState</c>.</summary>
    public static RealtimeSessionState CreateSessionState() => new();

    /// <summary>Applies a server event to a session state. Maps to <c>reduceSessionState</c>.</summary>
    public static RealtimeSessionState ReduceSessionState(
        RealtimeSessionState state,
        RealtimeModelV4ServerEvent serverEvent,
        int limit,
        bool? muted = null
    )
    {
        switch (serverEvent.Type)
        {
            case "session-started":
                return state with
                {
                    SessionId = serverEvent.SessionId,
                    DelegationMode = serverEvent.DelegationMode,
                };
            case "session-usage":
                return state with { Usage = serverEvent.Usage };
            case "session-closed":
                return state with
                {
                    SessionId = serverEvent.SessionId ?? state.SessionId,
                    Usage = serverEvent.Usage,
                    TerminationReason = serverEvent.Reason,
                    Finalization = RealtimeFinalization.Confirmed,
                };
            case "transcript-fragment":
                return state with { Transcripts = Append(state.Transcripts, serverEvent, limit) };
            case "delegation-created":
                return state.Delegations.Any(item => item.DelegationId == serverEvent.DelegationId)
                    ? state
                    : state with
                    {
                        Delegations = Append(state.Delegations, serverEvent, limit),
                    };
            case "command-acknowledged":
                return muted == null ? state : state with { IsInputMuted = muted.Value };
            default:
                return state;
        }
    }

    private static IReadOnlyList<RealtimeModelV4ServerEvent> Append(
        IReadOnlyList<RealtimeModelV4ServerEvent> items,
        RealtimeModelV4ServerEvent item,
        int limit
    )
    {
        var next = items.Append(item);
        var count = items.Count + 1;
        return (count > limit ? next.Skip(count - limit) : next).ToList();
    }
}
