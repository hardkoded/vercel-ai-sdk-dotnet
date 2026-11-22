// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>
/// The part of the LiveKit client that <see cref="MistralRealtimeTransport"/> uses. Upstream loads the
/// <c>livekit-client</c> module on connection; the port has no LiveKit client, so the host supplies one through this
/// interface. Maps to the <c>livekit-client</c> module (<c>Room</c> and <c>createLocalAudioTrack</c>).
/// </summary>
public interface ILiveKitClient
{
    /// <summary>Creates a room. Maps to <c>new Room()</c>.</summary>
    ILiveKitRoom CreateRoom();

    /// <summary>Acquires the microphone as an owned audio track. Maps to <c>createLocalAudioTrack</c>.</summary>
    Task<ILiveKitLocalAudioTrack> CreateLocalAudioTrackAsync();
}

/// <summary>A media track of a stream or a LiveKit local track. Maps to the browser <c>MediaStreamTrack</c>.</summary>
public interface ILiveKitMediaTrack
{
    /// <summary>Whether <c>readyState</c> is <c>live</c>.</summary>
    bool IsLive { get; }

    /// <summary>Maps to <c>enabled</c>.</summary>
    bool Enabled { get; }

    /// <summary>Maps to <c>muted</c>.</summary>
    bool Muted { get; }

    /// <summary>Raised when the track ends. Maps to the <c>ended</c> event.</summary>
    event EventHandler? Ended;
}

/// <summary>A caller-owned stream. Maps to the browser <c>MediaStream</c>.</summary>
public abstract class LiveKitMediaStream : RealtimeModelV4MediaStream
{
    /// <summary>Maps to <c>getAudioTracks</c>.</summary>
    public abstract IReadOnlyList<ILiveKitMediaTrack> GetAudioTracks();
}

/// <summary>An audio track the transport created and must stop. Maps to <c>LocalAudioTrack</c>.</summary>
public interface ILiveKitLocalAudioTrack
{
    /// <summary>Maps to <c>mediaStreamTrack</c>.</summary>
    ILiveKitMediaTrack MediaStreamTrack { get; }

    /// <summary>Maps to <c>stop</c>.</summary>
    void Stop();
}

/// <summary>Maps to <c>RemoteParticipant</c>.</summary>
public interface ILiveKitRemoteParticipant
{
    /// <summary>Maps to <c>identity</c>.</summary>
    string Identity { get; }

    /// <summary>Maps to <c>isAgent</c>.</summary>
    bool IsAgent { get; }
}

/// <summary>An HTML media element playing a remote track. Maps to <c>HTMLMediaElement</c>.</summary>
public interface ILiveKitMediaElement
{
    /// <summary>Maps to <c>play</c>.</summary>
    Task PlayAsync();

    /// <summary>Maps to <c>pause</c>.</summary>
    void Pause();

    /// <summary>Clears <c>srcObject</c>.</summary>
    void ClearSource();

    /// <summary>Maps to <c>remove</c>.</summary>
    void Remove();
}

/// <summary>Maps to <c>RemoteTrack</c>.</summary>
public interface ILiveKitRemoteTrack
{
    /// <summary>Whether <c>kind</c> is <c>Track.Kind.Audio</c>.</summary>
    bool IsAudio { get; }

    /// <summary>Maps to <c>attach</c>.</summary>
    ILiveKitMediaElement Attach();

    /// <summary>Maps to <c>detach</c>.</summary>
    void Detach(ILiveKitMediaElement element);
}

/// <summary>Maps to <c>RpcInvocationData</c>.</summary>
public sealed class LiveKitRpcInvocationData
{
    /// <summary>Maps to <c>requestId</c>.</summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>Maps to <c>callerIdentity</c>.</summary>
    public string CallerIdentity { get; init; } = string.Empty;

    /// <summary>Maps to <c>payload</c>.</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>Maps to <c>responseTimeout</c>.</summary>
    public TimeSpan ResponseTimeout { get; init; }
}

/// <summary>Maps to <c>LocalParticipant</c>.</summary>
public interface ILiveKitLocalParticipant
{
    /// <summary>Publishes a microphone track. Maps to <c>publishTrack</c> with <c>Track.Source.Microphone</c>.</summary>
    Task PublishMicrophoneTrackAsync(ILiveKitMediaTrack track);

    /// <summary>Maps to <c>unpublishTrack</c>.</summary>
    Task UnpublishTrackAsync(ILiveKitMediaTrack track, bool stopOnUnpublish);

    /// <summary>Maps to <c>registerRpcMethod</c>.</summary>
    void RegisterRpcMethod(string method, Func<LiveKitRpcInvocationData, Task<string>> handler);

    /// <summary>Maps to <c>performRpc</c>.</summary>
    Task<string> PerformRpcAsync(
        string destinationIdentity,
        string method,
        string payload,
        TimeSpan responseTimeout
    );
}

/// <summary>Maps to <c>Room</c>. The events map to <c>RoomEvent</c>.</summary>
public interface ILiveKitRoom
{
    /// <summary>Maps to <c>RoomEvent.ParticipantConnected</c>.</summary>
    event Action<ILiveKitRemoteParticipant>? ParticipantConnected;

    /// <summary>Maps to <c>RoomEvent.ParticipantDisconnected</c>.</summary>
    event Action<ILiveKitRemoteParticipant>? ParticipantDisconnected;

    /// <summary>Maps to <c>RoomEvent.Disconnected</c>.</summary>
    event Action? Disconnected;

    /// <summary>Maps to <c>RoomEvent.Reconnecting</c>.</summary>
    event Action? Reconnecting;

    /// <summary>Maps to <c>RoomEvent.Reconnected</c>.</summary>
    event Action? Reconnected;

    /// <summary>Maps to <c>RoomEvent.TrackSubscribed</c>.</summary>
    event Action<ILiveKitRemoteTrack, ILiveKitRemoteParticipant>? TrackSubscribed;

    /// <summary>Maps to <c>RoomEvent.TrackUnsubscribed</c>.</summary>
    event Action<ILiveKitRemoteTrack>? TrackUnsubscribed;

    /// <summary>Maps to <c>RoomEvent.DataReceived</c>: payload, participant and topic.</summary>
    event Action<byte[], ILiveKitRemoteParticipant?, string?>? DataReceived;

    /// <summary>Maps to <c>remoteParticipants</c>.</summary>
    IReadOnlyCollection<ILiveKitRemoteParticipant> RemoteParticipants { get; }

    /// <summary>Maps to <c>localParticipant</c>.</summary>
    ILiveKitLocalParticipant LocalParticipant { get; }

    /// <summary>Maps to <c>connect</c>.</summary>
    Task ConnectAsync(string serverUrl, string participantToken);

    /// <summary>Maps to <c>disconnect</c>. <paramref name="stopTracks"/> must be <see langword="false"/>.</summary>
    Task DisconnectAsync(bool stopTracks);

    /// <summary>Maps to <c>startAudio</c>.</summary>
    Task StartAudioAsync();
}
