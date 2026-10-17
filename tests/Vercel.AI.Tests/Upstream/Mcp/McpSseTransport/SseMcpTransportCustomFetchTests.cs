// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

/// <summary>Upstream's <c>fetch</c> option is the <see cref="HttpClient"/> the transport is built with.</summary>
public sealed class SseMcpTransportCustomFetchTests
{
    private const string EndpointEvent = "event: endpoint\ndata: http://localhost:3000/messages\n\n";

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport > custom fetch::should use provided fetch function for SSE connection",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseProvidedFetchFunctionForSseConnection()
    {
        var server = new SseTestServer();
        var controller = new TestResponseController();
        server.Controlled(controller);
        var seen = new List<HttpRequestMessage>();
        using var customFetch = new HttpClient(new RecordingHandler(server, seen));

        using var transport = new SseMcpTransport(customFetch, new Uri("http://localhost:3000/sse"));

        var connectTask = transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        Assert.Contains(seen, request => request.Method == HttpMethod.Get && request.RequestUri!.AbsoluteUri == "http://localhost:3000/sse");
        Assert.NotEmpty(seen[0].Headers);

        await transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport > custom fetch::should use provided fetch function for POST send()",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldUseProvidedFetchFunctionForPostSend()
    {
        var server = new SseTestServer();
        var controller = new TestResponseController();
        server.Controlled(controller);
        server.MessagesResponse = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        var seen = new List<HttpRequestMessage>();
        using var customFetch = new HttpClient(new RecordingHandler(server, seen));

        using var transport = new SseMcpTransport(customFetch, new Uri("http://localhost:3000/sse"));

        var connectTask = transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        seen.Clear();

        using var document = JsonDocument.Parse("{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}");
        await transport.SendAsync(document.RootElement);

        Assert.Contains(seen, request => request.Method == HttpMethod.Post && request.RequestUri!.AbsoluteUri == "http://localhost:3000/messages");

        await transport.CloseAsync();
    }

    private sealed class RecordingHandler : DelegatingHandler
    {
        private readonly List<HttpRequestMessage> _seen;

        public RecordingHandler(HttpMessageHandler inner, List<HttpRequestMessage> seen)
            : base(inner)
        {
            _seen = seen;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _seen.Add(request);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
