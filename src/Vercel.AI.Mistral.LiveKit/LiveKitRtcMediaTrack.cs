// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using LiveKit.Rtc;

namespace Vercel.AI.Mistral.LiveKit;

/// <summary>An <see cref="ILiveKitMediaTrack"/> over a <see cref="LocalAudioTrack"/>.</summary>
public sealed class LiveKitRtcMediaTrack : ILiveKitMediaTrack
{
    private int _ended;

    /// <summary>Wraps a local audio track.</summary>
    public LiveKitRtcMediaTrack(LocalAudioTrack track)
    {
        Track = track ?? throw new ArgumentNullException(nameof(track));
    }

    /// <summary>The wrapped LiveKit track.</summary>
    public LocalAudioTrack Track { get; }

    /// <inheritdoc />
    public bool IsLive => Volatile.Read(ref _ended) == 0;

    /// <inheritdoc />
    public bool Enabled => !Track.IsMuted;

    /// <inheritdoc />
    public bool Muted => false;

    /// <inheritdoc />
    public event EventHandler? Ended;

    /// <summary>Ends the track and releases it. Raises <see cref="Ended"/> once.</summary>
    internal void Stop()
    {
        if (Interlocked.Exchange(ref _ended, 1) != 0)
        {
            return;
        }

        try
        {
            Track.Dispose();
        }
        finally
        {
            Ended?.Invoke(this, EventArgs.Empty);
        }
    }
}

/// <summary>A caller-owned stream of <see cref="LiveKitRtcMediaTrack"/>s.</summary>
public sealed class LiveKitRtcMediaStream : LiveKitMediaStream
{
    private readonly IReadOnlyList<ILiveKitMediaTrack> _tracks;

    /// <summary>Creates a stream of the given tracks.</summary>
    public LiveKitRtcMediaStream(params LiveKitRtcMediaTrack[] tracks)
    {
        _tracks = [.. tracks];
    }

    /// <inheritdoc />
    public override IReadOnlyList<ILiveKitMediaTrack> GetAudioTracks() => _tracks;
}
