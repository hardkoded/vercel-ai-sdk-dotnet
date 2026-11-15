// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.WebSockets;
using System.Text;

namespace Vercel.AI.ProviderUtils;

/// <summary>Close diagnostics carried by <see cref="IWebSocketLike.OnClose"/>.</summary>
public sealed class WebSocketCloseEvent
{
    /// <summary>Creates a close event.</summary>
    public WebSocketCloseEvent(int? code, string? reason)
    {
        Code = code;
        Reason = reason;
    }

    /// <summary>Close code, when the transport provides one.</summary>
    public int? Code { get; }

    /// <summary>Close reason, when the transport provides one.</summary>
    public string? Reason { get; }
}

/// <summary>The WebSocket surface used by providers. Maps to <c>WebSocketLike</c>.</summary>
public interface IWebSocketLike
{
    /// <summary>Ready state: 0 connecting, 1 open, 2 closing, 3 closed.</summary>
    int ReadyState { get; }

    /// <summary>Bytes queued by a send and not yet transmitted. Null when the implementation does not expose it.</summary>
    long? BufferedAmount { get; }

    /// <summary>Sends a text frame.</summary>
    void Send(string data);

    /// <summary>Sends a binary frame.</summary>
    void Send(byte[] data);

    /// <summary>Closes the socket.</summary>
    void Close(int? code = null, string? reason = null);

    /// <summary>Called when the socket opens.</summary>
    Action<object?>? OnOpen { get; set; }

    /// <summary>Called with one message. The data is a <see cref="string"/> or a <c>byte[]</c>.</summary>
    Action<object?>? OnMessage { get; set; }

    /// <summary>Called on a socket error.</summary>
    Action<object?>? OnError { get; set; }

    /// <summary>Called when the socket closes. The event is a <see cref="WebSocketCloseEvent"/> when the transport provides diagnostics.</summary>
    Action<object?>? OnClose { get; set; }
}

/// <summary>Creates a socket. Maps to <c>WebSocketConstructor</c>.</summary>
/// <param name="url">The WebSocket URL.</param>
/// <param name="protocols">Requested subprotocols.</param>
/// <param name="headers">Request headers, for implementations that can send them.</param>
public delegate IWebSocketLike WebSocketFactory(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string>? headers);

/// <summary>WebSocket helpers. Maps to <c>provider-utils/src/websocket.ts</c>.</summary>
public static class WebSocketUtils
{
    private const int OpenState = 1;

    /// <summary>Returns <paramref name="webSocket"/>, or the <see cref="ClientWebSocket"/> factory.</summary>
    public static WebSocketFactory GetWebSocketConstructor(WebSocketFactory? webSocket)
    {
        return webSocket ?? ClientWebSocketAdapter.Create;
    }

    /// <summary>Converts an http(s) URL to the corresponding ws(s) URL.</summary>
    public static Uri ToWebSocketUrl(string url)
    {
        return ToWebSocketUrl(new Uri(url));
    }

    /// <summary>Converts an http(s) URL to the corresponding ws(s) URL.</summary>
    public static Uri ToWebSocketUrl(Uri url)
    {
        if (url is null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        var builder = new UriBuilder(url);
        if (url.Scheme == Uri.UriSchemeHttp)
        {
            builder.Scheme = "ws";
        }
        else if (url.Scheme == Uri.UriSchemeHttps)
        {
            builder.Scheme = "wss";
        }

        return builder.Uri;
    }

    /// <summary>Reads WebSocket message data as text, handling string and binary payloads.</summary>
    public static Task<string> ReadWebSocketMessageTextAsync(object? data)
    {
        switch (data)
        {
            case string text:
                return Task.FromResult(text);
            case byte[] bytes:
                return Task.FromResult(Encoding.UTF8.GetString(bytes));
            case ArraySegment<byte> segment when segment.Array is not null:
                return Task.FromResult(Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count));
            case ReadOnlyMemory<byte> memory:
                return Task.FromResult(Encoding.UTF8.GetString(memory.ToArray()));
            default:
                return Task.FromResult(data?.ToString() ?? string.Empty);
        }
    }

    /// <summary>
    /// Waits until the socket's send buffer drains below <paramref name="highWaterMark"/> bytes. It does nothing for
    /// implementations that do not expose <see cref="IWebSocketLike.BufferedAmount"/>. It returns as soon as the
    /// socket is no longer open or the token is cancelled.
    /// </summary>
    public static async Task WaitForWebSocketBufferDrainAsync(IWebSocketLike socket, long highWaterMark = 1024 * 1024, int pollIntervalMs = 20, CancellationToken cancellationToken = default)
    {
        if (socket is null)
        {
            throw new ArgumentNullException(nameof(socket));
        }

        while (socket.ReadyState == OpenState && (socket.BufferedAmount ?? 0) > highWaterMark)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await Task.Delay(pollIntervalMs, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}

/// <summary>The default <see cref="IWebSocketLike"/>, backed by <see cref="ClientWebSocket"/>.</summary>
internal sealed class ClientWebSocketAdapter : IWebSocketLike
{
    private readonly ClientWebSocket _socket = new ClientWebSocket();
    private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
    private long _buffered;
    private int _readyState;

    private ClientWebSocketAdapter(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string>? headers)
    {
        if (protocols is not null)
        {
            foreach (var protocol in protocols)
            {
                _socket.Options.AddSubProtocol(protocol);
            }
        }

        if (headers is not null)
        {
            foreach (var pair in headers)
            {
                try
                {
                    _socket.Options.SetRequestHeader(pair.Key, pair.Value);
                }
                catch (ArgumentException)
                {
                    // A restricted header cannot be set on ClientWebSocket.
                }
            }
        }

        _ = RunAsync(url);
    }

    public int ReadyState => Volatile.Read(ref _readyState);

    public long? BufferedAmount => Interlocked.Read(ref _buffered);

    public Action<object?>? OnOpen { get; set; }

    public Action<object?>? OnMessage { get; set; }

    public Action<object?>? OnError { get; set; }

    public Action<object?>? OnClose { get; set; }

    public static IWebSocketLike Create(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string>? headers)
    {
        return new ClientWebSocketAdapter(url, protocols, headers);
    }

    public void Send(string data)
    {
        SendCore(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text);
    }

    public void Send(byte[] data)
    {
        SendCore(data, WebSocketMessageType.Binary);
    }

    public void Close(int? code = null, string? reason = null)
    {
        if (Interlocked.Exchange(ref _readyState, 2) >= 2)
        {
            return;
        }

        _ = CloseAsync(code, reason);
    }

    private async Task RunAsync(Uri url)
    {
        var token = _cancellation.Token;
        try
        {
            await _socket.ConnectAsync(url, token).ConfigureAwait(false);
        }
        catch (Exception)
        {
            Volatile.Write(ref _readyState, 3);
            OnError?.Invoke(null);
            OnClose?.Invoke(new WebSocketCloseEvent(null, null));
            return;
        }

        Volatile.Write(ref _readyState, 1);
        OnOpen?.Invoke(null);
        var buffer = new byte[16 * 1024];
        var message = new MemoryStream();
        try
        {
            while (!token.IsCancellationRequested)
            {
                var received = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token).ConfigureAwait(false);
                if (received.MessageType == WebSocketMessageType.Close)
                {
                    Volatile.Write(ref _readyState, 3);
                    OnClose?.Invoke(new WebSocketCloseEvent((int?)received.CloseStatus, received.CloseStatusDescription));
                    return;
                }

                message.Write(buffer, 0, received.Count);
                if (!received.EndOfMessage)
                {
                    continue;
                }

                var bytes = message.ToArray();
                message.SetLength(0);
                OnMessage?.Invoke(received.MessageType == WebSocketMessageType.Text ? Encoding.UTF8.GetString(bytes) : (object)bytes);
            }
        }
        catch (Exception) when (token.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            Volatile.Write(ref _readyState, 3);
            OnError?.Invoke(null);
            OnClose?.Invoke(new WebSocketCloseEvent(null, null));
            return;
        }

        Volatile.Write(ref _readyState, 3);
        OnClose?.Invoke(new WebSocketCloseEvent(null, null));
    }

    private void SendCore(byte[] data, WebSocketMessageType type)
    {
        if (ReadyState != 1)
        {
            throw new InvalidOperationException("The WebSocket is not open.");
        }

        Interlocked.Add(ref _buffered, data.Length);
        _ = Task.Run(async () =>
        {
            try
            {
                await _sendLock.WaitAsync(_cancellation.Token).ConfigureAwait(false);
                try
                {
                    await _socket.SendAsync(new ArraySegment<byte>(data), type, true, _cancellation.Token).ConfigureAwait(false);
                }
                finally
                {
                    _sendLock.Release();
                }
            }
            catch (Exception)
            {
                if (!_cancellation.IsCancellationRequested)
                {
                    OnError?.Invoke(null);
                }
            }
            finally
            {
                Interlocked.Add(ref _buffered, -data.Length);
            }
        });
    }

    private async Task CloseAsync(int? code, string? reason)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)
            {
                await _socket.CloseAsync((WebSocketCloseStatus)(code ?? 1000), reason, timeout.Token).ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // The socket may already be closed.
        }
        finally
        {
            _cancellation.Cancel();
            _socket.Dispose();
            Volatile.Write(ref _readyState, 3);
        }
    }
}
