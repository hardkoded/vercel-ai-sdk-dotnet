// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.Websocket;

/// <summary>Port of <c>websocket.test.ts</c> &gt; <c>waitForWebSocketBufferDrain</c>.</summary>
public sealed class WaitForWebSocketBufferDrainTests
{
    private const string Prefix = "packages/provider-utils/src/websocket.test.ts::waitForWebSocketBufferDrain::";

    [Fact]
    [UpstreamTest(Prefix + "should resolve immediately when the buffer is below the high-water mark", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_immediately_when_the_buffer_is_below_the_high_water_mark()
    {
        await WebSocketUtils.WaitForWebSocketBufferDrainAsync(SocketWithBuffer(0));
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve immediately when bufferedAmount is not exposed", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_immediately_when_bufferedAmount_is_not_exposed()
    {
        var socket = SocketWithBuffer(0);
        socket.BufferedAmount = null;
        await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket);
    }

    [Fact]
    [UpstreamTest(Prefix + "should wait until the buffer drains below the high-water mark", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_wait_until_the_buffer_drains_below_the_high_water_mark()
    {
        var socket = SocketWithBuffer(100);
        var drained = false;
        var wait = WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, highWaterMark: 10, pollIntervalMs: 1).ContinueWith(_ => drained = true);

        await Task.Delay(50);
        Assert.False(drained);

        socket.BufferedAmount = 0;
        await wait;
        Assert.True(drained);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve when the socket is no longer open even with a full buffer", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_when_the_socket_is_no_longer_open_even_with_a_full_buffer()
    {
        // bufferedAmount never drains on a closed socket
        var socket = SocketWithBuffer(100);
        socket.ReadyState = 3;
        await WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, highWaterMark: 10);
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve when the socket closes mid-wait", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_when_the_socket_closes_mid_wait()
    {
        var socket = SocketWithBuffer(100);
        var wait = WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, highWaterMark: 10, pollIntervalMs: 1);
        socket.ReadyState = 3;
        await wait;
    }

    [Fact]
    [UpstreamTest(Prefix + "should resolve when the abort signal fires mid-wait", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_when_the_abort_signal_fires_mid_wait()
    {
        var socket = SocketWithBuffer(100);
        using var controller = new CancellationTokenSource();
        var wait = WebSocketUtils.WaitForWebSocketBufferDrainAsync(socket, highWaterMark: 10, pollIntervalMs: 1, cancellationToken: controller.Token);
        controller.Cancel();
        await wait;
    }

    private static MockWebSocket SocketWithBuffer(long initial)
    {
        return new MockWebSocket(new Uri("wss://example.com"), null, null) { ReadyState = 1, BufferedAmount = initial };
    }
}
