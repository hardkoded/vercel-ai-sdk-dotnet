// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;
using Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

namespace Vercel.AI.Tests;

public sealed class SseMcpTransportRestartTests
{
    private const string EndpointEvent = "event: endpoint\ndata: http://localhost:3000/messages\n\n";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task A_stream_that_ends_after_connecting_lets_the_next_start_accept_a_new_endpoint_event()
    {
        var server = new SseTestServer();
        using var client = new HttpClient(server);
        using var transport = new SseMcpTransport(client, new Uri(SseTestServer.SseUrl));
        var first = new TestResponseController();
        server.Controlled(first);
        var closed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnError = error => closed.TrySetResult(error);

        var start = transport.StartAsync();
        first.Write(EndpointEvent);
        await start.WaitAsync(Limit);
        first.Close();
        var error = await closed.Task.WaitAsync(Limit);
        Assert.Equal("MCP SSE Transport Error: Connection closed unexpectedly", error.Message);

        var second = new TestResponseController();
        server.Controlled(second);
        var restart = transport.StartAsync();
        second.Write(EndpointEvent);
        await restart.WaitAsync(Limit);

        using var message = System.Text.Json.JsonDocument.Parse("{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"id\":\"1\"}");
        await transport.SendAsync(message.RootElement).WaitAsync(Limit);
        await transport.CloseAsync();
    }
}
