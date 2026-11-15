// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>A WebSocket opened by <see cref="ConnectToWebSocket"/>.</summary>
public sealed class WebSocketConnection
{
    private readonly Action<int?> _close;

    internal WebSocketConnection(IWebSocketLike? socket, Action<int?> close)
    {
        Socket = socket;
        _close = close;
    }

    /// <summary>Null when the factory threw or the token was already cancelled.</summary>
    public IWebSocketLike? Socket { get; }

    /// <summary>Unregisters the abort listener and closes the socket. It never throws.</summary>
    public void Close(int? code = null)
    {
        _close(code);
    }
}

/// <summary>Options for <see cref="ConnectToWebSocket.Connect"/>.</summary>
public sealed class ConnectToWebSocketOptions
{
    /// <summary>The URL to open.</summary>
    public Uri Url { get; set; } = null!;

    /// <summary>Requested subprotocols.</summary>
    public IReadOnlyList<string>? Protocols { get; set; }

    /// <summary>Request headers. Null values are removed.</summary>
    public IReadOnlyDictionary<string, string?>? Headers { get; set; }

    /// <summary>Custom WebSocket implementation.</summary>
    public WebSocketFactory? WebSocket { get; set; }

    /// <summary>Aborts the connection.</summary>
    public CancellationToken AbortSignal { get; set; }

    /// <summary>Called when the socket opens.</summary>
    public Action<IWebSocketLike>? OnOpen { get; set; }

    /// <summary>One decoded message. A throw goes to <see cref="OnProcessingError"/>.</summary>
    public Func<string, Task> OnMessageText { get; set; } = _ => Task.CompletedTask;

    /// <summary>Factory throws and message decoding or processing failures.</summary>
    public Action<Exception> OnProcessingError { get; set; } = _ => { };

    /// <summary>Called on a socket error.</summary>
    public Action? OnSocketError { get; set; }

    /// <summary>Receives the close code and reason when the transport provides them.</summary>
    public Action<int?, string?>? OnClose { get; set; }

    /// <summary>Also called, without opening a socket, when the token is already cancelled.</summary>
    public Action<Exception>? OnAbort { get; set; }
}

/// <summary>
/// Opens a WebSocket for a provider model, owning the transport-generic layer: factory resolution, header hygiene,
/// abort wiring, and message decoding. Maps to <c>connectToWebSocket</c>.
/// </summary>
public static class ConnectToWebSocket
{
    /// <summary>Opens the socket.</summary>
    public static WebSocketConnection Connect(ConnectToWebSocketOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        IWebSocketLike? socket = null;
        CancellationTokenRegistration? registration = null;
        var gate = new object();

        void Close(int? code)
        {
            lock (gate)
            {
                registration?.Dispose();
                registration = null;
            }

            try
            {
                socket?.Close(code);
            }
            catch (Exception)
            {
                // The socket may already be closed.
            }
        }

        var signal = options.AbortSignal;
        if (signal.IsCancellationRequested)
        {
            options.OnAbort?.Invoke(new OperationCanceledException(signal));
            return new WebSocketConnection(null, Close);
        }

        try
        {
            var factory = WebSocketUtils.GetWebSocketConstructor(options.WebSocket);
            var headers = options.Headers is null
                ? new Dictionary<string, string>()
                : options.Headers.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value!);
            socket = factory(options.Url, options.Protocols, headers);
        }
        catch (Exception error)
        {
            options.OnProcessingError(error);
            return new WebSocketConnection(null, Close);
        }

        if (signal.CanBeCanceled && options.OnAbort is not null)
        {
            lock (gate)
            {
                registration = signal.Register(() => options.OnAbort(new OperationCanceledException(signal)));
            }
        }

        var openedSocket = socket;

        // Messages are processed through a task tail so async decoding cannot reorder them, and error and close
        // handling cannot overtake a still-decoding terminal frame.
        var tail = Task.CompletedTask;
        var tailGate = new object();

        void Chain(Func<Task> work)
        {
            lock (tailGate)
            {
                tail = RunAfterAsync(tail, work, options.OnProcessingError);
            }
        }

        socket.OnOpen = _ =>
        {
            try
            {
                options.OnOpen?.Invoke(openedSocket);
            }
            catch (Exception error)
            {
                options.OnProcessingError(error);
            }
        };
        socket.OnMessage = data => Chain(async () => await options.OnMessageText(await WebSocketUtils.ReadWebSocketMessageTextAsync(data).ConfigureAwait(false)).ConfigureAwait(false));
        socket.OnError = _ => Chain(() =>
        {
            options.OnSocketError?.Invoke();
            return Task.CompletedTask;
        });
        socket.OnClose = closeEvent => Chain(() =>
        {
            var info = closeEvent as WebSocketCloseEvent;
            options.OnClose?.Invoke(info?.Code, info?.Reason);
            return Task.CompletedTask;
        });

        return new WebSocketConnection(socket, Close);
    }

    private static async Task RunAfterAsync(Task previous, Func<Task> work, Action<Exception> onProcessingError)
    {
        await previous.ConfigureAwait(false);
        try
        {
            await work().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            onProcessingError(error);
        }
    }
}
