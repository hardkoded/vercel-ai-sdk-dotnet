// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Mcp;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

public sealed class SseMcpTransportTests : IDisposable
{
    private const string EndpointEvent = "event: endpoint\ndata: http://localhost:3000/messages\n\n";

    private readonly SseTestServer _server = new();
    private readonly HttpClient _client;
    private SseMcpTransport _transport;

    public SseMcpTransportTests()
    {
        _client = new HttpClient(_server);
        _transport = new SseMcpTransport(_client, new Uri(SseTestServer.SseUrl));
    }

    public void Dispose()
    {
        _transport.Dispose();
        _client.Dispose();
    }

    private static readonly string TestMessage = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{\"foo\":\"bar\"},\"id\":\"1\"}";

    private static JsonElement Message(string json = "{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{\"foo\":\"bar\"},\"id\":\"1\"}")
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private TestResponseController Connect()
    {
        var controller = new TestResponseController();
        _server.Controlled(controller);
        return controller;
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should establish connection and receive endpoint",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldEstablishConnectionAndReceiveEndpoint()
    {
        var controller = Connect();

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);

        await connectTask;
        await _transport.CloseAsync();

        Assert.Single(_server.Calls);
        Assert.Equal("GET", _server.Calls[0].Method);
        Assert.Equal("http://localhost:3000/sse", _server.Calls[0].Url);
        Assert.Equal(SseMcpTransport.LatestLegacyProtocolVersion, _server.Calls[0].Headers["mcp-protocol-version"]);
        Assert.Equal("text/event-stream", _server.Calls[0].Headers["accept"]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should throw if server returns non-200 status",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowIfServerReturnsNon200Status()
    {
        _server.SseResponse = (_, _) => Task.FromResult(SseTestServer.Error(HttpStatusCode.InternalServerError, "Internal Server Error", "Internal Server Error"));

        MCPClientError? caughtError = null;
        try
        {
            await _transport.StartAsync();
        }
        catch (MCPClientError error)
        {
            caughtError = error;
        }

        Assert.NotNull(caughtError);
        Assert.Equal(500, caughtError!.StatusCode);
        Assert.Equal("http://localhost:3000/sse", caughtError.Url);
        Assert.Equal("Internal Server Error", caughtError.ResponseBody);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should reject when the connection closes before receiving an endpoint",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectWhenTheConnectionClosesBeforeReceivingAnEndpoint()
    {
        var controller = Connect();

        Exception? reportedError = null;
        _transport.OnError = error => reportedError = error;

        var connectTask = _transport.StartAsync();
        controller.Close();

        var thrown = await Assert.ThrowsAsync<MCPClientError>(() => connectTask);
        Assert.Contains("MCP SSE Transport Error: Connection closed before endpoint event was received", thrown.Message);
        Assert.IsType<MCPClientError>(reportedError);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should handle valid JSON-RPC messages",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldHandleValidJsonRpcMessages()
    {
        var controller = Connect();

        var messageTask = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnMessage = message => messageTask.TrySetResult(message.Clone());

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);

        await connectTask;

        controller.Write("event: message\ndata: " + TestMessage + "\n\n");

        var received = await messageTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(TestMessage), JsonNode.Parse(received.GetRawText())));

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should handle JSON-RPC messages without explicit event field",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldHandleJsonRpcMessagesWithoutExplicitEventField()
    {
        var controller = Connect();

        var messageTask = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnMessage = message => messageTask.TrySetResult(message.Clone());

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);

        await connectTask;

        controller.Write("data: " + TestMessage + "\n\n");

        var received = await messageTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(TestMessage), JsonNode.Parse(received.GetRawText())));

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should handle invalid JSON-RPC messages",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldHandleInvalidJsonRpcMessages()
    {
        var controller = Connect();

        var errorTask = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnError = error => errorTask.TrySetResult(error);

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);
        await connectTask;

        controller.Write("event: message\ndata: {\"foo\":\"bar\"}\n\n");

        var error = await errorTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsType<MCPClientError>(error);
        Assert.Contains("Failed to parse message", error.Message);

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should reject SSE messages containing __proto__ (prototype pollution)",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectSseMessagesContainingProtoPrototypePollution()
    {
        var controller = Connect();

        var errorTask = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnError = error => errorTask.TrySetResult(error);

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);
        await connectTask;

        var malicious = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"__proto__\":{\"polluted\":true}}}";
        controller.Write("event: message\ndata: " + malicious + "\n\n");

        var error = await errorTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsType<MCPClientError>(error);
        Assert.Equal("MCP SSE Transport Error: Failed to parse message", error.Message);
        Assert.IsType<JsonParseException>(error.InnerException);
        Assert.Equal(
            "JSON parsing failed: Text: " + malicious + ". Error message: Object contains forbidden prototype property",
            error.InnerException!.Message);

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should send messages as POST requests",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendMessagesAsPostRequests()
    {
        var controller = Connect();

        var connectTask = _transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        var message = Message();

        await _transport.SendAsync(message);

        Assert.Equal(2, _server.Calls.Count);
        Assert.Equal("POST", _server.Calls[1].Method);
        Assert.Equal("http://localhost:3000/messages", _server.Calls[1].Url);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(message.GetRawText()), JsonNode.Parse(_server.Calls[1].Body)));

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should abort a hanging POST with the request signal",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldAbortAHangingPostWithTheRequestSignal()
    {
        var controller = Connect();
        var postStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var postAborted = false;
        _server.MessagesResponse = async (_, cancellationToken) =>
        {
            postStarted.TrySetResult(true);
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                postAborted = true;
                throw;
            }

            return SseTestServer.Json("{}");
        };

        var connectTask = _transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        using var abort = new CancellationTokenSource();
        var sendTask = _transport.SendAsync(Message("{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}"), abort.Token);

        await postStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        abort.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sendTask);
        Assert.True(postAborted);
        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should reject cross-origin endpoints before connecting",
        Coverage = UpstreamCoverage.Covered,
        Note = "The private connected and endpoint fields are checked through SendAsync, which rejects with Not connected.")]
    public async Task ShouldRejectCrossOriginEndpointsBeforeConnecting()
    {
        var controller = Connect();

        var errorTask = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnError = error => errorTask.TrySetResult(error);

        var connectTask = _transport.StartAsync();
        controller.Write("event: endpoint\ndata: http://localhost:3333/messages\n\n");

        var thrown = await Assert.ThrowsAsync<MCPClientError>(() => connectTask);
        Assert.Contains("Endpoint origin does not match connection origin: http://localhost:3333", thrown.Message);

        var error = await errorTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsType<MCPClientError>(error);

        var notConnected = await Assert.ThrowsAsync<MCPClientError>(
            () => _transport.SendAsync(Message("{\"jsonrpc\":\"2.0\",\"method\":\"test\",\"params\":{},\"id\":\"1\"}")));
        Assert.Contains("Not connected", notConnected.Message);

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should ignore endpoint events after connecting",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIgnoreEndpointEventsAfterConnecting()
    {
        var controller = Connect();

        var connectTask = _transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        controller.Write("event: endpoint\ndata: http://localhost:3333/messages\n\n");
        await Task.Delay(100);

        await _transport.SendAsync(Message());

        var postCalls = _server.Calls.Where(call => call.Method == "POST").ToList();
        Assert.Single(postCalls);
        Assert.Equal("http://localhost:3000/messages", postCalls[0].Url);

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should reject non-2xx POST responses with HTTP details",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRejectNon2xxPostResponsesWithHttpDetails()
    {
        var controller = Connect();

        _server.MessagesResponse = (_, _) => Task.FromResult(SseTestServer.Error(HttpStatusCode.InternalServerError, "Internal Server Error", "Internal Server Error"));

        Exception? reportedError = null;
        _transport.OnError = error => reportedError = error;

        var connectTask = _transport.StartAsync();
        controller.Write(EndpointEvent);
        await connectTask;

        var thrown = await Assert.ThrowsAsync<MCPClientError>(() => _transport.SendAsync(Message()));
        Assert.Equal("MCP SSE Transport Error: POSTing to endpoint (HTTP 500): Internal Server Error", thrown.Message);
        Assert.Equal(500, thrown.StatusCode);
        Assert.Equal("http://localhost:3000/messages", thrown.Url);
        Assert.Equal("Internal Server Error", thrown.ResponseBody);
        var reported = Assert.IsType<MCPClientError>(reportedError);
        Assert.Equal(500, reported.StatusCode);
        Assert.Equal("http://localhost:3000/messages", reported.Url);
        Assert.Equal("Internal Server Error", reported.ResponseBody);

        // The connection stays usable.
        _server.MessagesResponse = (_, _) => Task.FromResult(SseTestServer.Json("{}"));
        await _transport.SendAsync(Message());

        await _transport.CloseAsync();
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should handle invalid endpoint URLs",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldHandleInvalidEndpointUrls()
    {
        _server.SseResponse = (_, _) => Task.FromResult(SseTestServer.Error(HttpStatusCode.InternalServerError, "Internal Server Error", "Internal Server Error"));

        _transport = new SseMcpTransport(_client, new Uri("http://localhost:3333/sse"));

        var errorTask = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        _transport.OnError = error => errorTask.TrySetResult(error);

        await Assert.ThrowsAnyAsync<Exception>(() => _transport.StartAsync());

        var error = await errorTask.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Contains("MCP SSE Transport Error: 500 Internal Server Error", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::SseMCPTransport::should send custom headers with all requests",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSendCustomHeadersWithAllRequests()
    {
        var controller = Connect();

        var customHeaders = new Dictionary<string, string>
        {
            ["authorization"] = "Bearer test-token",
            ["x-custom-header"] = "test-value",
        };

        _transport = new SseMcpTransport(_client, new Uri(SseTestServer.SseUrl), customHeaders);

        var connectTask = _transport.StartAsync();

        controller.Write(EndpointEvent);

        await connectTask;

        await _transport.SendAsync(Message());

        // Verify SSE connection headers
        var sse = _server.Calls[0].Headers;
        Assert.Equal(SseMcpTransport.LatestLegacyProtocolVersion, sse["mcp-protocol-version"]);
        Assert.Equal("text/event-stream", sse["accept"]);
        Assert.Equal("Bearer test-token", sse["authorization"]);
        Assert.Equal("test-value", sse["x-custom-header"]);
        Assert.Contains("ai-sdk/", sse["user-agent"]);

        // Verify POST request headers
        var post = _server.Calls[1].Headers;
        Assert.StartsWith("application/json", post["content-type"]);
        Assert.Equal(SseMcpTransport.LatestLegacyProtocolVersion, post["mcp-protocol-version"]);
        Assert.Equal("Bearer test-token", post["authorization"]);
        Assert.Equal("test-value", post["x-custom-header"]);
        Assert.Contains("ai-sdk/", post["user-agent"]);

        await _transport.CloseAsync();
    }
}
