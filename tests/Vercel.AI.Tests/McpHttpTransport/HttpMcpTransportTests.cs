// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.McpHttpTransport;

public sealed class HttpMcpTransportTests
{
    private const string Prefix =
        "packages/mcp/src/tool/mcp-http-transport.test.ts::HttpMCPTransport::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should expose HTTP status and JSON-RPC error details on a JSON-RPC error from POST",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port has no onmessage/onerror callbacks on HttpMcpTransport and the exception has no object cause, so those assertions and the cause are not checked."
    )]
    public async Task Should_expose_HTTP_status_and_JSON_RPC_error_details_on_a_JSON_RPC_error_from_POST()
    {
        var body =
            "{\"jsonrpc\":\"2.0\",\"id\":1,\"error\":{\"code\":-32000,\"message\":\"Unauthorized: token expired\",\"data\":{\"reason\":\"expired\"}}}";
        var transport = new HttpMcpTransport(
            new HttpClient(new ScriptedHandler(HttpStatusCode.Unauthorized, body)),
            new Uri("http://localhost:4000/mcp")
        );
        using var parameters = JsonDocument.Parse("{}");

        var error = await Assert.ThrowsAsync<MCPClientError>(() =>
            transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None)
        );

        Assert.Equal("Unauthorized: token expired", error.Message);
        Assert.Equal(-32000, error.Code);
        Assert.Equal("{\"reason\":\"expired\"}", error.Data!.Value.GetRawText());
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("http://localhost:4000/mcp", error.Url);
        Assert.Equal(body, error.ResponseBody);
    }

    private sealed class ScriptedHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
    }
}
