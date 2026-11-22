// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using LiveKit.Rtc;

namespace Vercel.AI.Mistral.LiveKit;

/// <summary>
/// Options of <see cref="LiveKitRtcClient"/>. A .NET host has no browser microphone or speaker, so it feeds the
/// microphone and plays the agent's voice itself.
/// </summary>
public sealed class LiveKitRtcClientOptions
{
    /// <summary>
    /// The source the host captures microphone frames into. When <see langword="null"/> the client creates one with
    /// <see cref="SampleRate"/> and <see cref="NumChannels"/> and exposes it as
    /// <see cref="LiveKitRtcClient.MicrophoneSource"/>. A source passed here belongs to the host.
    /// </summary>
    public AudioSource? MicrophoneSource { get; init; }

    /// <summary>Sample rate in Hz of the created microphone source and of the remote audio. Defaults to 48000.</summary>
    public int SampleRate { get; init; } = 48000;

    /// <summary>Channel count of the created microphone source and of the remote audio. Defaults to 1.</summary>
    public int NumChannels { get; init; } = 1;

    /// <summary>
    /// Receives each frame of the agent's voice while the transport is playing it (the .NET counterpart of the
    /// browser audio element). When <see langword="null"/> the frames are read and dropped.
    /// </summary>
    public Func<AudioFrame, ValueTask>? OnRemoteAudioFrame { get; init; }

    /// <summary>
    /// Called when <see cref="OnRemoteAudioFrame"/> or the audio stream throws. Playback then stops; the transport
    /// starts it again on the next play. Frames are delivered one at a time, never concurrently.
    /// </summary>
    public Action<Exception>? OnRemoteAudioError { get; init; }

    /// <summary>Options passed to <c>Room.ConnectAsync</c>.</summary>
    public RoomOptions? RoomOptions { get; init; }
}
