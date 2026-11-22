// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using LiveKit.Rtc;
using ParticipantKind = LiveKit.Proto.ParticipantKind;
using TrackKind = LiveKit.Proto.TrackKind;
using TrackSource = LiveKit.Proto.TrackSource;

namespace Vercel.AI.Mistral.LiveKit;

internal sealed class LiveKitRtcParticipant(Participant participant) : ILiveKitRemoteParticipant
{
    public string Identity => participant.Identity;

    public bool IsAgent => participant.Kind == ParticipantKind.Agent;
}

internal sealed class LiveKitRtcRemoteTrack(RemoteTrack track, LiveKitRtcClientOptions options)
    : ILiveKitRemoteTrack
{
    public bool IsAudio => track.Kind == TrackKind.KindAudio;

    public ILiveKitMediaElement Attach() =>
        new LiveKitRtcAudioElement(
            AudioStream.FromTrack(track, (uint)options.SampleRate, (uint)options.NumChannels),
            options.OnRemoteAudioFrame
        );

    public void Detach(ILiveKitMediaElement element)
    {
        (element as LiveKitRtcAudioElement)?.Detach();
    }
}

/// <summary>Reads the agent's audio stream and hands its frames to the host while playing.</summary>
internal sealed class LiveKitRtcAudioElement(
    AudioStream stream,
    Func<AudioFrame, ValueTask>? onFrame
) : ILiveKitMediaElement
{
    private readonly object _gate = new();
    private CancellationTokenSource? _pump;
    private Task _pumping = Task.CompletedTask;
    private bool _disposed;

    public Task PlayAsync()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return Task.CompletedTask;
            }

            if (_pump is null)
            {
                var cts = new CancellationTokenSource();
                _pump = cts;
                _pumping = Task.Run(() => PumpAsync(cts.Token));
            }
        }

        return Task.CompletedTask;
    }

    public void Pause()
    {
        Task stopped;
        lock (_gate)
        {
            if (_pump is null)
            {
                return;
            }

            _pump.Cancel();
            _pump.Dispose();
            _pump = null;
            stopped = _pumping;
        }

        _ = stopped.ContinueWith(_ => { }, TaskScheduler.Default);
    }

    public void ClearSource() => Release();

    public void Remove() => Release();

    internal void Detach() => Release();

    private void Release()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        Pause();
        stream.Dispose();
    }

    private async Task PumpAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var frame in stream.WithCancellation(cancellationToken))
            {
                if (onFrame is not null)
                {
                    await onFrame(frame.Frame).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (ObjectDisposedException) { }
    }
}

internal sealed class LiveKitRtcLocalParticipant(
    LocalParticipant participant,
    ConcurrentDictionary<ILiveKitMediaTrack, string> published
) : ILiveKitLocalParticipant
{
    public async Task PublishMicrophoneTrackAsync(ILiveKitMediaTrack track)
    {
        if (track is not LiveKitRtcMediaTrack rtcTrack)
        {
            throw new ArgumentException(
                "The track must be a LiveKitRtcMediaTrack created for this client.",
                nameof(track)
            );
        }

        var publication = await participant
            .PublishTrackAsync(
                rtcTrack.Track,
                new TrackPublishOptions { Source = TrackSource.SourceMicrophone }
            )
            .ConfigureAwait(false);
        published[track] = publication.Sid;
    }

    public async Task UnpublishTrackAsync(ILiveKitMediaTrack track, bool stopOnUnpublish)
    {
        if (published.TryRemove(track, out var sid))
        {
            await participant.UnpublishTrackAsync(sid).ConfigureAwait(false);
        }

        if (stopOnUnpublish && track is LiveKitRtcMediaTrack rtcTrack)
        {
            rtcTrack.Stop();
        }
    }

    public void RegisterRpcMethod(
        string method,
        Func<LiveKitRpcInvocationData, Task<string>> handler
    )
    {
        participant.RegisterRpcMethod(method, data => handler(
                new LiveKitRpcInvocationData
                {
                    RequestId = data.RequestId,
                    CallerIdentity = data.CallerIdentity,
                    Payload = data.Payload,
                    ResponseTimeout = TimeSpan.FromSeconds(data.ResponseTimeout),
                }
            ));
    }

    public Task<string> PerformRpcAsync(
        string destinationIdentity,
        string method,
        string payload,
        TimeSpan responseTimeout
    ) =>
        participant.PerformRpcAsync(
            destinationIdentity,
            method,
            payload,
            responseTimeout.TotalSeconds
        );
}

internal sealed class LiveKitRtcRoom : ILiveKitRoom
{
    private readonly LiveKitRtcClientOptions _options;
    private readonly Room _room = new();
    private readonly ConcurrentDictionary<Participant, LiveKitRtcParticipant> _participants = new();
    private readonly ConcurrentDictionary<RemoteTrack, LiveKitRtcRemoteTrack> _tracks = new();
    private readonly ConcurrentDictionary<ILiveKitMediaTrack, string> _published = new();
    private LiveKitRtcLocalParticipant? _local;

    public LiveKitRtcRoom(LiveKitRtcClientOptions options)
    {
        _options = options;
        _room.ParticipantConnected += (_, p) => ParticipantConnected?.Invoke(Wrap(p));
        _room.ParticipantDisconnected += (_, p) =>
        {
            ParticipantDisconnected?.Invoke(Wrap(p));
            _participants.TryRemove(p, out LiveKitRtcParticipant? _);
        };
        _room.Disconnected += (_, _) => Disconnected?.Invoke();
        _room.Reconnecting += (_, _) => Reconnecting?.Invoke();
        _room.Reconnected += (_, _) => Reconnected?.Invoke();
        _room.TrackSubscribed += (_, e) =>
            TrackSubscribed?.Invoke(WrapTrack(e.Track), Wrap(e.Participant));
        _room.TrackUnsubscribed += (_, e) =>
        {
            if (_tracks.TryRemove(e.Track, out var track))
            {
                TrackUnsubscribed?.Invoke(track);
            }
        };
        _room.DataReceived += (_, e) =>
            DataReceived?.Invoke(
                e.Data,
                e.Participant is null ? null : Wrap(e.Participant),
                e.Topic
            );
    }

    public event Action<ILiveKitRemoteParticipant>? ParticipantConnected;

    public event Action<ILiveKitRemoteParticipant>? ParticipantDisconnected;

    public event Action? Disconnected;

    public event Action? Reconnecting;

    public event Action? Reconnected;

    public event Action<ILiveKitRemoteTrack, ILiveKitRemoteParticipant>? TrackSubscribed;

    public event Action<ILiveKitRemoteTrack>? TrackUnsubscribed;

    public event Action<byte[], ILiveKitRemoteParticipant?, string?>? DataReceived;

    public IReadOnlyCollection<ILiveKitRemoteParticipant> RemoteParticipants =>
        [.. _room.RemoteParticipants.Values.Select(Wrap)];

    public ILiveKitLocalParticipant LocalParticipant =>
        _local ??= new LiveKitRtcLocalParticipant(
            _room.LocalParticipant
                ?? throw new InvalidOperationException("The LiveKit room is not connected."),
            _published
        );

    public Task ConnectAsync(string serverUrl, string participantToken) =>
        _room.ConnectAsync(serverUrl, participantToken, _options.RoomOptions ?? new RoomOptions());

    public async Task DisconnectAsync(bool stopTracks)
    {
        try
        {
            await _room.DisconnectAsync().ConfigureAwait(false);
        }
        finally
        {
            _room.Dispose();
        }
    }

    /// <summary>Audio playback needs no user gesture outside a browser.</summary>
    public Task StartAudioAsync() => Task.CompletedTask;

    private LiveKitRtcParticipant Wrap(Participant participant) =>
        _participants.GetOrAdd(participant, p => new LiveKitRtcParticipant(p));

    private LiveKitRtcRemoteTrack WrapTrack(RemoteTrack track) =>
        _tracks.GetOrAdd(track, t => new LiveKitRtcRemoteTrack(t, _options));
}
