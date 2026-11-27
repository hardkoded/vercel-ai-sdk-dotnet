// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI;

/// <summary>The command a <see cref="RealtimeCommandTracker"/> completed.</summary>
public sealed class RealtimeCommand
{
    /// <summary>Creates a completed command.</summary>
    public RealtimeCommand(bool? muted)
    {
        Muted = muted;
    }

    /// <summary>The mute state the command asked for, or null when it is not a mute command.</summary>
    public bool? Muted { get; }
}

/// <summary>Attempt-local ACK correlation; unresolved commands are never evicted. Maps to <c>RealtimeCommandTracker</c>.</summary>
public sealed class RealtimeCommandTracker
{
    private readonly object _gate = new();
    private readonly Dictionary<string, RealtimeCommand> _pending = [];
    private readonly HashSet<string> _recent = [];
    private readonly Queue<string> _recentOrder = new();
    private readonly Func<RealtimeModelV4ClientEvent, Task> _sendCommand;

    /// <summary>Creates a tracker that sends commands with <paramref name="sendCommand"/>.</summary>
    public RealtimeCommandTracker(Func<RealtimeModelV4ClientEvent, Task> sendCommand)
    {
        _sendCommand = sendCommand ?? throw new ArgumentNullException(nameof(sendCommand));
    }

    /// <summary>Throws when <paramref name="id"/> was used or too many commands are pending. Maps to <c>validateId</c>.</summary>
    public void ValidateId(string id)
    {
        lock (_gate)
        {
            ValidateIdLocked(id);
        }
    }

    /// <summary>Sends a command and tracks its identifier. Maps to <c>send</c>.</summary>
    public Task Send(RealtimeModelV4ClientEvent clientEvent)
    {
        bool? muted =
            clientEvent.Type == "input-audio-mute"
                ? true
                : clientEvent.Type == "input-audio-unmute"
                    ? false
                    : null;
        if (muted != null && clientEvent.EventId == null)
        {
            clientEvent = clientEvent.Clone();
            clientEvent.EventId = JsonValues.GenerateId(string.Empty);
        }

        var id = clientEvent.EventId;
        if (id != null)
        {
            lock (_gate)
            {
                ValidateIdLocked(id);
                _pending[id] = new RealtimeCommand(muted);
            }
        }

        var awaitsAcknowledgement =
            clientEvent.Type == "context-append"
            || clientEvent.Type == "session-update"
            || muted != null;
        try
        {
            var sent = _sendCommand(clientEvent);
            return id == null ? sent : CompleteAfterAsync(sent, id, awaitsAcknowledgement);
        }
        catch
        {
            if (id != null)
            {
                Complete(id);
            }

            throw;
        }
    }

    /// <summary>Completes the command a server event acknowledges or rejects. Maps to <c>receive</c>.</summary>
    public RealtimeCommand? Receive(RealtimeModelV4ServerEvent serverEvent)
    {
        if (serverEvent.Type == "command-acknowledged" && serverEvent.ClientEventId != null)
        {
            return Complete(serverEvent.ClientEventId);
        }

        if (serverEvent.Type == "error" && serverEvent.ClientEventId != null)
        {
            Complete(serverEvent.ClientEventId);
        }

        return null;
    }

    private async Task CompleteAfterAsync(Task sent, string id, bool awaitsAcknowledgement)
    {
        try
        {
            await sent.ConfigureAwait(false);
        }
        catch
        {
            Complete(id);
            throw;
        }

        if (!awaitsAcknowledgement)
        {
            Complete(id);
        }
    }

    private void ValidateIdLocked(string id)
    {
        if (_pending.ContainsKey(id) || _recent.Contains(id))
        {
            throw new InvalidOperationException(
                "Realtime command ID was already used; retry with a fresh eventId"
            );
        }

        if (_pending.Count >= 512)
        {
            throw new InvalidOperationException("Too many pending realtime commands");
        }
    }

    private RealtimeCommand? Complete(string id)
    {
        lock (_gate)
        {
            if (!_pending.TryGetValue(id, out var command))
            {
                return null;
            }

            _pending.Remove(id);
            if (_recent.Add(id))
            {
                _recentOrder.Enqueue(id);
            }

            if (_recent.Count > 4096)
            {
                _recent.Remove(_recentOrder.Dequeue());
            }

            return command;
        }
    }
}
