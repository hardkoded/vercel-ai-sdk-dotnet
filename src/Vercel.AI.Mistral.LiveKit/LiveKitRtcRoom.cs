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
            () => AudioStream.FromTrack(track, (uint)options.SampleRate, (uint)options.NumChannels),
            options.OnRemoteAudioFrame,
            options.OnRemoteAudioError
        );

    public void Detach(ILiveKitMediaElement element)
    {
        (element as LiveKitRtcAudioElement)?.Detach();
    }
}

/// <summary>
/// Reads the agent's audio and hands its frames to the host while playing. Each play opens a fresh stream and each
/// pause disposes it at once, so no audio queues while paused or while a slow callback finishes. A new pump starts only after the previous one finished its
/// in-flight callback, so <c>onFrame</c> is never called concurrently.
/// </summary>
internal sealed class LiveKitRtcAudioElement(
    Func<AudioStream> createStream,
    Func<AudioFrame, ValueTask>? onFrame,
    Action<Exception>? onError
) : ILiveKitMediaElement
{
    private readonly object _gate = new();
    private Pump? _current;
    private Task _previous = Task.CompletedTask;
    private bool _disposed;

    public Task PlayAsync()
    {
        lock (_gate)
        {
            if (_disposed || _current is not null)
            {
                return Task.CompletedTask;
            }

            var pump = new Pump();
            _current = pump;
            var previous = _previous;
            pump.Task = Task.Run(() => RunAsync(pump, previous));
            _previous = pump.Task;
        }

        return Task.CompletedTask;
    }

    public void Pause()
    {
        Pump? pump;
        lock (_gate)
        {
            pump = _current;
            _current = null;
        }

        pump?.Stop();
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
    }

    private async Task RunAsync(Pump pump, Task previous)
    {
        try
        {
            await previous.ConfigureAwait(false);

            // The stream opens only now, so no audio queues while the previous callback finishes.
            var stream = pump.Open(createStream);
            if (stream is null)
            {
                return;
            }

            await foreach (var frame in stream.WithCancellation(pump.Cancellation.Token))
            {
                if (onFrame is not null)
                {
                    await onFrame(frame.Frame).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (pump.Cancellation.IsCancellationRequested) { }
        catch (ObjectDisposedException) when (pump.Cancellation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            lock (_gate)
            {
                // Only the pump that failed leaves the playing state, never a newer one.
                if (ReferenceEquals(_current, pump))
                {
                    _current = null;
                }
            }

            onError?.Invoke(exception);
        }
        finally
        {
            lock (_gate)
            {
                // A pump that ended on its own must not stay current, or play would never restart.
                if (ReferenceEquals(_current, pump))
                {
                    _current = null;
                }
            }

            pump.Stop();
        }
    }

    private sealed class Pump
    {
        private readonly object _gate = new();
        private AudioStream? _stream;
        private bool _stopped;

        public CancellationTokenSource Cancellation { get; } = new();

        public Task Task { get; set; } = Task.CompletedTask;

        /// <summary>Opens the stream, or returns <see langword="null"/> when the pump was stopped first.</summary>
        public AudioStream? Open(Func<AudioStream> createStream)
        {
            lock (_gate)
            {
                return _stopped ? null : _stream = createStream();
            }
        }

        /// <summary>
        /// Cancels the pump and disposes its stream at once, without waiting for the callback. Safe to call more than
        /// once and from any thread. The token source is never disposed: it has no timer or linked token, so there is
        /// nothing to release, and disposing it would race with a concurrent <c>Cancel</c>.
        /// </summary>
        public void Stop()
        {
            AudioStream? stream;
            lock (_gate)
            {
                if (_stopped)
                {
                    return;
                }

                _stopped = true;
                stream = _stream;
                _stream = null;
            }

            Cancellation.Cancel();
            stream?.Dispose();
        }
    }
}

internal sealed class LiveKitRtcLocalParticipant(
    LocalParticipant participant,
    ConcurrentDictionary<ILiveKitMediaTrack, LocalTrackPublication> published
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
        published[track] = publication;
    }

    public async Task UnpublishTrackAsync(ILiveKitMediaTrack track, bool stopOnUnpublish)
    {
        if (published.TryRemove(track, out var publication))
        {
            // The SID changes when the SDK republishes after a full reconnect, so read it now.
            await participant.UnpublishTrackAsync(publication.Sid).ConfigureAwait(false);
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
    private readonly ConcurrentDictionary<ILiveKitMediaTrack, LocalTrackPublication> _published =
        new();
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
