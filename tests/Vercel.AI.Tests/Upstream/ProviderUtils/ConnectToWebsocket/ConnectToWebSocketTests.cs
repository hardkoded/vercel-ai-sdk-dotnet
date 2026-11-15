// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ConnectToWebsocket;

/// <summary>Port of <c>connect-to-websocket.test.ts</c> &gt; <c>connectToWebSocket</c>.</summary>
[Collection("MockWebSocket")]
public sealed class ConnectToWebSocketTests
{
    private const string Prefix = "packages/provider-utils/src/connect-to-websocket.test.ts::connectToWebSocket::";

    [Fact]
    [UpstreamTest(Prefix + "should construct the socket with protocols and stripped headers", Coverage = UpstreamCoverage.Covered)]
    public void Should_construct_the_socket_with_protocols_and_stripped_headers()
    {
        MockWebSocket.Reset();
        var connection = ConnectToWebSocket.Connect(Options(o =>
        {
            o.Protocols = new[] { "proto-a" };
            o.Headers = new Dictionary<string, string?> { ["Authorization"] = "Bearer token", ["X-Unset"] = null };
        }));

        var ws = MockWebSocket.Instances[0];
        Assert.Same(ws, connection.Socket);
        Assert.Equal("wss://example.com/socket", ws.Url.ToString());
        Assert.Equal(new[] { "proto-a" }, ws.Protocols);
        Assert.Equal(new Dictionary<string, string> { ["Authorization"] = "Bearer token" }, ws.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should route the constructor throw to onProcessingError and return no socket", Coverage = UpstreamCoverage.Covered)]
    public void Should_route_the_constructor_throw_to_onProcessingError_and_return_no_socket()
    {
        Exception? error = null;
        var connection = ConnectToWebSocket.Connect(Options(o =>
        {
            o.WebSocket = (_, _, _) => throw new InvalidOperationException("bad url");
            o.OnProcessingError = e => error = e;
        }));

        Assert.Equal("bad url", error!.Message);
        Assert.Null(connection.Socket);
        connection.Close();
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onAbort without opening a socket when already aborted", Coverage = UpstreamCoverage.Covered)]
    public void Should_call_onAbort_without_opening_a_socket_when_already_aborted()
    {
        MockWebSocket.Reset();
        Exception? aborted = null;
        using var controller = new CancellationTokenSource();
        controller.Cancel();

        var connection = ConnectToWebSocket.Connect(Options(o =>
        {
            o.AbortSignal = controller.Token;
            o.OnAbort = e => aborted = e;
        }));

        Assert.IsAssignableFrom<OperationCanceledException>(aborted);
        Assert.Null(connection.Socket);
        Assert.Empty(MockWebSocket.Instances);
    }

    [Fact]
    [UpstreamTest(Prefix + "should call onAbort when the signal fires and not after close", Coverage = UpstreamCoverage.Covered)]
    public void Should_call_onAbort_when_the_signal_fires_and_not_after_close()
    {
        MockWebSocket.Reset();
        var calls = 0;
        using var controller = new CancellationTokenSource();

        var connection = ConnectToWebSocket.Connect(Options(o =>
        {
            o.AbortSignal = controller.Token;
            o.OnAbort = _ => calls++;
        }));

        connection.Close();
        controller.Cancel();
        Assert.Equal(0, calls);

        using var second = new CancellationTokenSource();
        ConnectToWebSocket.Connect(Options(o =>
        {
            o.AbortSignal = second.Token;
            o.OnAbort = _ => calls++;
        }));
        second.Cancel();
        Assert.Equal(1, calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should decode messages and route processing failures to onProcessingError", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_decode_messages_and_route_processing_failures_to_onProcessingError()
    {
        MockWebSocket.Reset();
        var texts = new List<string>();
        var errors = new List<Exception>();
        ConnectToWebSocket.Connect(Options(o =>
        {
            o.OnMessageText = text =>
            {
                texts.Add(text);
                return texts.Count == 1 ? Task.FromException(new InvalidOperationException("boom")) : Task.CompletedTask;
            };
            o.OnProcessingError = errors.Add;
        }));

        var ws = MockWebSocket.Instances[0];
        ws.OnMessage!("frame-1");
        await Wait.UntilAsync(() => errors.Count == 1);
        Assert.Equal("frame-1", texts[0]);
        Assert.Equal("boom", errors[0].Message);

        ws.OnMessage!(System.Text.Encoding.UTF8.GetBytes("frame-2"));
        await Wait.UntilAsync(() => texts.Count == 2);
        Assert.Equal("frame-2", texts[1]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should route open, socket error, and close events", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_route_open_socket_error_and_close_events()
    {
        MockWebSocket.Reset();
        IWebSocketLike? opened = null;
        var socketErrors = 0;
        var closes = new List<(int?, string?)>();
        ConnectToWebSocket.Connect(Options(o =>
        {
            o.OnOpen = socket => opened = socket;
            o.OnSocketError = () => socketErrors++;
            o.OnClose = (code, reason) => closes.Add((code, reason));
        }));

        var ws = MockWebSocket.Instances[0];
        ws.OnOpen!(null);
        ws.OnError!(null);
        ws.OnClose!(new object());
        Assert.Same(ws, opened);
        await Wait.UntilAsync(() => closes.Count == 1);
        Assert.Equal(1, socketErrors);
        Assert.Equal((null, null), closes.Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass the close code and reason to onClose", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_close_code_and_reason_to_onClose()
    {
        MockWebSocket.Reset();
        var closes = new List<(int?, string?)>();
        ConnectToWebSocket.Connect(Options(o => o.OnClose = (code, reason) => closes.Add((code, reason))));

        MockWebSocket.Instances[0].OnClose!(new WebSocketCloseEvent(1011, "server error"));
        await Wait.UntilAsync(() => closes.Count == 1);
        Assert.Equal((1011, "server error"), closes.Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "should ignore non-standard close event shapes", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_ignore_non_standard_close_event_shapes()
    {
        MockWebSocket.Reset();
        var closes = new List<(int?, string?)>();
        ConnectToWebSocket.Connect(Options(o => o.OnClose = (code, reason) => closes.Add((code, reason))));

        MockWebSocket.Instances[0].OnClose!(new { code = "not-a-number", reason = 42 });
        await Wait.UntilAsync(() => closes.Count == 1);
        Assert.Equal((null, null), closes.Single());
    }

    [Fact]
    [UpstreamTest(Prefix + "should route onOpen throws to onProcessingError", Coverage = UpstreamCoverage.Covered)]
    public void Should_route_onOpen_throws_to_onProcessingError()
    {
        MockWebSocket.Reset();
        Exception? error = null;
        ConnectToWebSocket.Connect(Options(o =>
        {
            o.OnProcessingError = e => error = e;
            o.OnOpen = _ => throw new InvalidOperationException("send failed");
        }));

        MockWebSocket.Instances[0].OnOpen!(null);
        Assert.Equal("send failed", error!.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should process messages in order and run close after pending messages", Coverage = UpstreamCoverage.Partial, Note = "Blob frames do not exist in .NET. A slow handler stands in for the async Blob decode.")]
    public async Task Should_process_messages_in_order_and_run_close_after_pending_messages()
    {
        MockWebSocket.Reset();
        var events = new List<string>();
        var closed = false;
        ConnectToWebSocket.Connect(Options(o =>
        {
            o.OnMessageText = async text =>
            {
                if (text == "slow-frame")
                {
                    await Task.Delay(50);
                }

                lock (events)
                {
                    events.Add(text);
                }
            };
            o.OnClose = (_, _) =>
            {
                lock (events)
                {
                    events.Add("close");
                }

                closed = true;
            };
        }));

        var ws = MockWebSocket.Instances[0];
        ws.OnMessage!("slow-frame");
        ws.OnMessage!("fast-frame");
        ws.OnClose!(new object());
        await Wait.UntilAsync(() => closed);
        Assert.Equal(new[] { "slow-frame", "fast-frame", "close" }, events);
    }

    [Fact]
    [UpstreamTest(Prefix + "should close the socket with the code and swallow close throws", Coverage = UpstreamCoverage.Covered)]
    public void Should_close_the_socket_with_the_code_and_swallow_close_throws()
    {
        MockWebSocket.Reset();
        var connection = ConnectToWebSocket.Connect(Options(_ => { }));

        var ws = MockWebSocket.Instances[0];
        connection.Close(1000);
        Assert.Equal(new int?[] { 1000 }, ws.Closes);

        ws.CloseHook = () => throw new InvalidOperationException("already closed");
        connection.Close();
    }

    private static ConnectToWebSocketOptions Options(Action<ConnectToWebSocketOptions> configure)
    {
        var options = new ConnectToWebSocketOptions { Url = new Uri("wss://example.com/socket"), WebSocket = MockWebSocket.Factory };
        configure(options);
        return options;
    }
}
