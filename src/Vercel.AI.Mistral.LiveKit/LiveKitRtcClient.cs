// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using LiveKit.Rtc;

namespace Vercel.AI.Mistral.LiveKit;

/// <summary>
/// An <see cref="ILiveKitClient"/> over the <c>Livekit.Rtc.Dotnet</c> client, so a .NET host can join the Mistral voice
/// room. Pass it to <c>new MistralRealtimeModel(id, client)</c> or <c>MistralProvider.ExperimentalRealtime.Model(id, client)</c>.
/// The host captures microphone audio into <see cref="MicrophoneSource"/> and receives the agent's voice through
/// <see cref="LiveKitRtcClientOptions.OnRemoteAudioFrame"/>.
/// </summary>
public sealed class LiveKitRtcClient : ILiveKitClient, IDisposable
{
    private readonly LiveKitRtcClientOptions _options;
    private readonly bool _ownsSource;

    /// <summary>Creates a client.</summary>
    public LiveKitRtcClient(LiveKitRtcClientOptions? options = null)
    {
        _options = options ?? new LiveKitRtcClientOptions();
        _ownsSource = _options.MicrophoneSource is null;
        MicrophoneSource = _options.MicrophoneSource
            ?? new AudioSource(_options.SampleRate, _options.NumChannels);
    }

    /// <summary>The source the host captures microphone frames into (<c>AudioSource.CaptureFrameAsync</c>).</summary>
    public AudioSource MicrophoneSource { get; }

    /// <inheritdoc />
    public ILiveKitRoom CreateRoom() => new LiveKitRtcRoom(_options);

    /// <inheritdoc />
    public Task<ILiveKitLocalAudioTrack> CreateLocalAudioTrackAsync()
    {
        var track = new LiveKitRtcMediaTrack(
            LocalAudioTrack.Create("microphone", MicrophoneSource)
        );
        return Task.FromResult<ILiveKitLocalAudioTrack>(new LiveKitRtcLocalAudioTrack(track));
    }

    /// <summary>Releases the microphone source when the client created it.</summary>
    public void Dispose()
    {
        if (_ownsSource)
        {
            MicrophoneSource.Dispose();
        }
    }
}

internal sealed class LiveKitRtcLocalAudioTrack(LiveKitRtcMediaTrack track)
    : ILiveKitLocalAudioTrack
{
    public ILiveKitMediaTrack MediaStreamTrack => track;

    public void Stop() => track.Stop();
}
