// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Sdk.Mcp;

namespace Vercel.AI.Sdk.Tests;

public sealed class McpTests
{
    [Fact]
    public async Task Framing_round_trips_a_json_rpc_message()
    {
        var encoded = McpFraming.Encode("{\"jsonrpc\":\"2.0\"}");
        using var stream = new MemoryStream(encoded);
        var decoded = await McpFraming.ReadAsync(stream, CancellationToken.None);
        Assert.Equal("{\"jsonrpc\":\"2.0\"}", decoded);
    }

    [Fact]
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
    public async Task Http_transport_reads_an_sse_result()
    {
        var handler = new SseMcpHandler();
        var transport = new HttpMcpTransport(new HttpClient(handler), new Uri("https://example.test/mcp"));
        using var parameters = JsonDocument.Parse("{}");
        var result = await transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None);
        Assert.True(result.TryGetProperty("tools", out var tools));
        Assert.Equal(0, tools.GetArrayLength());
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
}
