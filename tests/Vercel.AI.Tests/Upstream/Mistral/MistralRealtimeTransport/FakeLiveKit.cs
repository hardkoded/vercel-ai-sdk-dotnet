// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralRealtimeTransport;

/// <summary>The test double of the <c>livekit-client</c> module that upstream mocks with <c>vi.mock</c>.</summary>
internal sealed class FakeLiveKit : ILiveKitClient
{
    public static readonly FakeAgent Agent = new("mistral-agent", true);

    public List<FakeRoom> Rooms { get; } = [];

    public bool WithAgent { get; set; } = true;

    public int CreateLocalAudioTrackCalls { get; private set; }

    public Func<Task<ILiveKitLocalAudioTrack>> CreateLocalAudioTrack { get; set; } =
        () => throw new InvalidOperationException("No microphone configured.");

    public ILiveKitRoom CreateRoom()
    {
        var room = new FakeRoom(WithAgent);
        Rooms.Add(room);
        return room;
    }

    public Task<ILiveKitLocalAudioTrack> CreateLocalAudioTrackAsync()
    {
        CreateLocalAudioTrackCalls++;
        return CreateLocalAudioTrack();
    }
}

internal sealed class FakeAgent(string identity, bool isAgent) : ILiveKitRemoteParticipant
{
    public string Identity { get; } = identity;

    public bool IsAgent { get; } = isAgent;
}

internal sealed class FakeMediaTrack : ILiveKitMediaTrack
{
    public bool IsLive => true;

    public bool Enabled => true;

    public bool Muted => false;

    public int StopCalls { get; set; }

    public event EventHandler? Ended;

    public void End() => Ended?.Invoke(this, EventArgs.Empty);
}

internal sealed class FakeStream(FakeMediaTrack track) : LiveKitMediaStream
{
    public override IReadOnlyList<ILiveKitMediaTrack> GetAudioTracks() => [track];
}

internal sealed class FakeLocalAudioTrack(FakeMediaTrack track) : ILiveKitLocalAudioTrack
{
    public int StopCalls { get; private set; }

    public ILiveKitMediaTrack MediaStreamTrack => track;

    public void Stop()
    {
        StopCalls++;
        track.StopCalls++;
    }
}

internal sealed class FakeMicrophone
{
    public FakeMicrophone()
    {
        Track = new FakeMediaTrack();
        Stream = new FakeStream(Track);
        Local = new FakeLocalAudioTrack(Track);
    }

    public FakeMediaTrack Track { get; }

    public FakeStream Stream { get; }

    public FakeLocalAudioTrack Local { get; }
}

internal sealed class FakeElement : ILiveKitMediaElement
{
    public int PlayCalls { get; private set; }

    public int PauseCalls { get; private set; }

    public int RemoveCalls { get; private set; }

    public bool SourceCleared { get; private set; }

    public Task PlayAsync()
    {
        PlayCalls++;
        return Task.CompletedTask;
    }

    public void Pause() => PauseCalls++;

    public void ClearSource() => SourceCleared = true;

    public void Remove() => RemoveCalls++;
}

internal sealed class FakeRemoteTrack(FakeElement element) : ILiveKitRemoteTrack
{
    public List<ILiveKitMediaElement> Detached { get; } = [];

    public bool IsAudio => true;

    public ILiveKitMediaElement Attach() => element;

    public void Detach(ILiveKitMediaElement detached) => Detached.Add(detached);
}

internal sealed class FakeLocalParticipant : ILiveKitLocalParticipant
{
    public List<ILiveKitMediaTrack> Published { get; } = [];

    public List<(ILiveKitMediaTrack Track, bool Stop)> Unpublished { get; } = [];

    public Dictionary<string, Func<LiveKitRpcInvocationData, Task<string>>> Rpc { get; } = [];

    public List<(
        string Destination,
        string Method,
        string Payload,
        TimeSpan Timeout
    )> RpcCalls { get; } = [];

    public Task PublishMicrophoneTrackAsync(ILiveKitMediaTrack track)
    {
        Published.Add(track);
        return Task.CompletedTask;
    }

    public Task UnpublishTrackAsync(ILiveKitMediaTrack track, bool stopOnUnpublish)
    {
        Unpublished.Add((track, stopOnUnpublish));
        return Task.CompletedTask;
    }

    public void RegisterRpcMethod(
        string method,
        Func<LiveKitRpcInvocationData, Task<string>> handler
    )
    {
        Rpc[method] = handler;
    }

    public Task<string> PerformRpcAsync(
        string destinationIdentity,
        string method,
        string payload,
        TimeSpan responseTimeout
    )
    {
        RpcCalls.Add((destinationIdentity, method, payload, responseTimeout));
        return Task.FromResult("{\"type\":\"session.updated\"}");
    }
}

internal sealed class FakeRoom : ILiveKitRoom
{
    public FakeRoom(bool withAgent)
    {
        if (withAgent)
        {
            Participants.Add(FakeLiveKit.Agent);
        }
    }

    public List<ILiveKitRemoteParticipant> Participants { get; } = [];

    public FakeLocalParticipant Local { get; } = new();

    public List<(string Url, string Token)> ConnectCalls { get; } = [];

    public List<bool> DisconnectCalls { get; } = [];

    public event Action<ILiveKitRemoteParticipant>? ParticipantConnected;

    public event Action<ILiveKitRemoteParticipant>? ParticipantDisconnected;

    public event Action? Disconnected;

    public event Action? Reconnecting;

    public event Action? Reconnected;

    public event Action<ILiveKitRemoteTrack, ILiveKitRemoteParticipant>? TrackSubscribed;

    public event Action<ILiveKitRemoteTrack>? TrackUnsubscribed;

    public event Action<byte[], ILiveKitRemoteParticipant?, string?>? DataReceived;

    public IReadOnlyCollection<ILiveKitRemoteParticipant> RemoteParticipants => Participants;

    public ILiveKitLocalParticipant LocalParticipant => Local;

    public Task ConnectAsync(string serverUrl, string participantToken)
    {
        ConnectCalls.Add((serverUrl, participantToken));
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(bool stopTracks)
    {
        DisconnectCalls.Add(stopTracks);
        return Task.CompletedTask;
    }

    public Task StartAudioAsync() => Task.CompletedTask;

    public void EmitParticipantConnected(ILiveKitRemoteParticipant p) =>
        ParticipantConnected?.Invoke(p);

    public void EmitParticipantDisconnected(ILiveKitRemoteParticipant p) =>
        ParticipantDisconnected?.Invoke(p);

    public void EmitDisconnected() => Disconnected?.Invoke();

    public void EmitReconnecting() => Reconnecting?.Invoke();

    public void EmitReconnected() => Reconnected?.Invoke();

    public void EmitTrackSubscribed(ILiveKitRemoteTrack t, ILiveKitRemoteParticipant p) =>
        TrackSubscribed?.Invoke(t, p);

    public void EmitTrackUnsubscribed(ILiveKitRemoteTrack t) => TrackUnsubscribed?.Invoke(t);

    public void EmitDataReceived(byte[] payload, ILiveKitRemoteParticipant? p, string? topic) =>
        DataReceived?.Invoke(payload, p, topic);
}
