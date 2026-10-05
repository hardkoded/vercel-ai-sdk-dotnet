// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > message handling::should handle incoming messages correctly",
        Coverage = UpstreamCoverage.Partial,
        Note = "Round-trips a Content-Length frame. Upstream delivers newline-delimited JSON.")]
    public async Task Framing_round_trips_a_json_rpc_message()
    {
        var encoded = McpFraming.Encode("{\"jsonrpc\":\"2.0\"}");
        using var stream = new MemoryStream(encoded);
        var decoded = await McpFraming.ReadAsync(stream, CancellationToken.None);
        Assert.Equal("{\"jsonrpc\":\"2.0\"}", decoded);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should return serializable tool definitions via listTools()",
        Coverage = UpstreamCoverage.Partial,
        Note = "Lists one tool and calls it. Does not cover pagination.")]
    public async Task List_tools_adapts_tools_call()
    {
        var transport = new FakeTransport();
        var tools = await McpClient.ListToolsAsync(transport);
        Assert.Single(tools);
        using var arguments = JsonDocument.Parse("{\"city\":\"Paris\"}");
        var output = await tools[0].Execute!(arguments.RootElement, CancellationToken.None);
        Assert.Equal("sunny", output);
        Assert.Equal("tools/call", transport.Methods[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-transport.test.ts::HttpMCPTransport::should handle text/event-stream responses",
        Coverage = UpstreamCoverage.Partial,
        Note = "Reads one SSE JSON-RPC result.")]
    public async Task Http_transport_reads_an_sse_result()
    {
        var handler = new SseMcpHandler();
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");
        var result = await transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None);
        Assert.True(result.TryGetProperty("tools", out var tools));
        Assert.Equal(0, tools.GetArrayLength());
    }

    [Fact]
    public async Task Http_transport_throws_mcp_client_error_with_http_metadata()
    {
        var handler = new ScriptedMcpHandler(HttpStatusCode.ServiceUnavailable, "text/plain", "Service Unavailable");
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");

        var error = await Assert.ThrowsAsync<MCPClientError>(() => transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None));

        Assert.Equal(503, error.StatusCode);
        Assert.Equal("https://example.test/mcp", error.Url);
        Assert.Equal("Service Unavailable", error.ResponseBody);
        Assert.Null(error.Code);
    }

    [Fact]
    public async Task Http_transport_throws_mcp_client_error_with_json_rpc_code()
    {
        var body = "{\"jsonrpc\":\"2.0\",\"id\":1,\"error\":{\"code\":-32601,\"message\":\"Method not found\",\"data\":{\"method\":\"tools/list\"}}}";
        var handler = new ScriptedMcpHandler(HttpStatusCode.OK, "application/json", body);
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");

        var error = await Assert.ThrowsAsync<MCPClientError>(() => transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None));

        Assert.Equal("Method not found", error.Message);
        Assert.Equal(-32601, error.Code);
        Assert.Equal("{\"method\":\"tools/list\"}", error.Data!.Value.GetRawText());
        Assert.Null(error.StatusCode);
    }

    private sealed class FakeTransport : IMcpTransport
    {
        public List<string> Methods { get; } = new();

        public Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
        {
            Methods.Add(method);
            var json = method == "tools/call"
                ? "{\"content\":[{\"type\":\"text\",\"text\":\"sunny\"}]}"
                : "{\"tools\":[{\"name\":\"weather\",\"description\":\"Forecast\",\"inputSchema\":{\"type\":\"object\"}}]}";
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }
    }

    private sealed class SseMcpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = "event: message\ndata: {\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"tools\":[]}}\n\n";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/event-stream"),
            };
            return Task.FromResult(response);
        }
    }

    private sealed class ScriptedMcpHandler(HttpStatusCode status, string mediaType, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType),
            };
            return Task.FromResult(response);
        }
    }
}
