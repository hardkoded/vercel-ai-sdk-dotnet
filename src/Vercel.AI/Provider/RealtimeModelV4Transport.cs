// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Provider;

/// <summary>
/// Media owned by the host application and handed to a provider transport. Maps to <c>RealtimeModelV4MediaStream</c>
/// (the browser <c>MediaStream</c>). The host defines the concrete type; a transport must never stop the tracks of a
/// caller-owned stream.
/// </summary>
public abstract class RealtimeModelV4MediaStream { }

/// <summary>Callbacks for a provider-owned transport. Maps to <c>RealtimeModelV4TransportOptions</c>.</summary>
public sealed class RealtimeModelV4TransportOptions
{
    /// <summary>Creates transport options.</summary>
    public RealtimeModelV4TransportOptions(
        Func<RealtimeModelV4ServerEvent, Task> onEvent,
        Action<Exception> onError,
        Action<Exception, Task?> onFatalError,
        Action<Exception?> onClose,
        Action<bool> onCapturing,
        Action<bool> onPlaying
    )
    {
        OnEvent = onEvent ?? throw new ArgumentNullException(nameof(onEvent));
        OnError = onError ?? throw new ArgumentNullException(nameof(onError));
        OnFatalError = onFatalError ?? throw new ArgumentNullException(nameof(onFatalError));
        OnClose = onClose ?? throw new ArgumentNullException(nameof(onClose));
        OnCapturing = onCapturing ?? throw new ArgumentNullException(nameof(onCapturing));
        OnPlaying = onPlaying ?? throw new ArgumentNullException(nameof(onPlaying));
    }

    /// <summary>Called with each normalized server event. Maps to <c>onEvent</c>.</summary>
    public Func<RealtimeModelV4ServerEvent, Task> OnEvent { get; }

    /// <summary>Called with a recoverable error. Maps to <c>onError</c>.</summary>
    public Action<Exception> OnError { get; }

    /// <summary>Called with an unrecoverable error and an optional drain task. Maps to <c>onFatalError</c>.</summary>
    public Action<Exception, Task?> OnFatalError { get; }

    /// <summary>Called when the transport closes, with the error that closed it. Maps to <c>onClose</c>.</summary>
    public Action<Exception?> OnClose { get; }

    /// <summary>Called when local capture starts or stops. Maps to <c>onCapturing</c>.</summary>
    public Action<bool> OnCapturing { get; }

    /// <summary>Called when remote playback starts or stops. Maps to <c>onPlaying</c>.</summary>
    public Action<bool> OnPlaying { get; }
}

/// <summary>Arguments of <see cref="IRealtimeModelV4Transport.ConnectAsync"/>. Maps to the options of <c>connect</c>.</summary>
public sealed class RealtimeModelV4TransportConnectOptions
{
    /// <summary>Application endpoint for creating and closing provider sessions. Maps to <c>api</c>.</summary>
    public string Api { get; init; } = string.Empty;

    /// <summary>Session configuration. Maps to <c>sessionConfig</c>.</summary>
    public RealtimeModelV4SessionConfig? SessionConfig { get; init; }

    /// <summary>Caller-owned media. The transport must never stop its tracks. Maps to <c>stream</c>.</summary>
    public RealtimeModelV4MediaStream? Stream { get; init; }

    /// <summary>Whether to capture local media on connect. Maps to <c>capture</c>.</summary>
    public bool? Capture { get; init; }

    /// <summary>Cancels the connection. Maps to <c>abortSignal</c>.</summary>
    public CancellationToken AbortSignal { get; init; }
}

/// <summary>
/// Transport for providers whose media protocol requires its own client. Maps to <c>RealtimeModelV4Transport</c>.
/// Construction must not acquire media or open a connection. Provider-specific dependencies and session
/// setup and cleanup belong to the implementation.
/// </summary>
public interface IRealtimeModelV4Transport : IDisposable
{
    /// <summary>Opens the connection. Maps to <c>connect</c>.</summary>
    Task ConnectAsync(RealtimeModelV4TransportConnectOptions options);

    /// <summary>Sends an event. <paramref name="shouldSend"/> is checked right before sending. Maps to <c>sendEvent</c>.</summary>
    Task SendEventAsync(RealtimeModelV4ClientEvent clientEvent, Func<bool>? shouldSend = null);

    /// <summary>Starts capturing local media. Maps to <c>startCapture</c>.</summary>
    Task StartCaptureAsync(RealtimeModelV4MediaStream? stream = null);

    /// <summary>Stops capturing local media. Maps to <c>stopCapture</c>.</summary>
    Task StopCaptureAsync();

    /// <summary>Stops remote playback. Maps to <c>stopPlayback</c>.</summary>
    void StopPlayback();

    /// <summary>Resumes remote playback. Maps to <c>resumePlayback</c>.</summary>
    Task ResumePlaybackAsync();

    /// <summary>Drains already accepted incoming events. Maps to <c>finish</c>.</summary>
    Task FinishAsync();

    /// <summary>Releases local media and awaits remote session cleanup. Must be idempotent. Maps to <c>close</c>.</summary>
    Task CloseAsync();
}

/// <summary>
/// A realtime model that creates a provider-owned transport. Maps to the optional <c>RealtimeModelV4.createTransport</c>.
/// It is a separate interface because <c>netstandard2.0</c> has no default interface members.
/// </summary>
public interface IProviderTransportRealtimeModel : IRealtimeModel
{
    /// <summary>Creates a provider-owned transport without opening a connection. Maps to <c>createTransport</c>.</summary>
    IRealtimeModelV4Transport CreateTransport(RealtimeModelV4TransportOptions options);
}
