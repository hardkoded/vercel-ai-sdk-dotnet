// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

public sealed class SseMcpTransportProtocolVersionDowngradeTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport > protocol version downgrade::should use negotiated protocolVersion in POST headers after it is set",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseNegotiatedProtocolVersionInPostHeadersAfterItIsSet()
    {
        var server = new SseTestServer();
        var controller = new TestResponseController();
        server.Controlled(controller);
        using var client = new HttpClient(server);
        using var transport = new SseMcpTransport(client, new Uri(SseTestServer.SseUrl));

        var connectTask = transport.StartAsync();
        controller.Write("event: endpoint\ndata: http://localhost:3000/messages\n\n");
        await connectTask;

        // Simulate protocol version negotiation
        transport.ProtocolVersion = "2025-06-18";

        using var document = JsonDocument.Parse("{\"jsonrpc\":\"2.0\",\"method\":\"tools/list\",\"params\":{},\"id\":\"1\"}");
        await transport.SendAsync(document.RootElement);

        var postCall = server.Calls.First(call => call.Method == "POST");
        Assert.Equal("2025-06-18", postCall.Headers["mcp-protocol-version"]);

        await transport.CloseAsync();
    }
}
