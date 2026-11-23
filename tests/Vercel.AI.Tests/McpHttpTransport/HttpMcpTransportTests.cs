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
        Commit = "d55f3cfc456d1b7bc7db3a4b70fbf602ddc8647d",
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

    [Fact]
    [UpstreamTest(
        Prefix
            + "should explain legacy discovery incompatibility from an HTTP 400 without falling back",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d55f3cfc456d1b7bc7db3a4b70fbf602ddc8647d"
    )]
    public async Task Should_explain_legacy_discovery_incompatibility_from_an_HTTP_400_without_falling_back()
    {
        var methods = new List<string>();
        var data =
            "{\"requested\":\""
            + McpClient.LatestProtocolVersion
            + "\",\"supported\":[\"2025-06-18\"]}";
        var handler = new RecordingHandler(methods, HttpStatusCode.BadRequest, data);

        var error = await Assert.ThrowsAsync<MCPClientError>(() =>
            McpClient.ConnectAsync(
                new HttpMcpTransport(new HttpClient(handler), new Uri("http://localhost:4000/mcp"))
            )
        );

        Assert.Contains("ProtocolVersionDiscovery to false", error.Message);
        Assert.Equal(-32022, error.Code);
        Assert.Equal(data, error.Data!.Value.GetRawText());
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("http://localhost:4000/mcp", error.Url);
        Assert.IsType<MCPClientError>(error.InnerException);
        Assert.Equal(["server/discover"], methods);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should wait for a slow protocol discovery response instead of falling back to initialize",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_wait_for_a_slow_protocol_discovery_response_instead_of_falling_back_to_initialize()
    {
        var methods = new List<string>();
        var handler = new DelegateHandler(
            async (message, _) =>
            {
                var method = message.RootElement.GetProperty("method").GetString()!;
                methods.Add(method);
                var id = message.RootElement.GetProperty("id").GetRawText();
                if (method == "tools/list")
                {
                    return Json(
                        "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{\"tools\":[]}}"
                    );
                }

                if (method != "server/discover")
                {
                    return Json(
                        "{\"jsonrpc\":\"2.0\",\"id\":"
                            + id
                            + ",\"error\":{\"code\":-32022,\"message\":\"Unsupported protocol version: "
                            + McpClient.LatestLegacyProtocolVersion
                            + "\"}}"
                    );
                }

                await Task.Delay(2000).ConfigureAwait(false);
                return Json(
                    "{\"jsonrpc\":\"2.0\",\"id\":"
                        + id
                        + ",\"result\":{\"resultType\":\"complete\",\"supportedVersions\":[\""
                        + McpClient.LatestProtocolVersion
                        + "\"],\"capabilities\":{\"tools\":{}}}}"
                );
            }
        );

        await McpClient.ConnectAsync(
            new HttpMcpTransport(new HttpClient(handler), new Uri("http://localhost:4000/mcp"))
        );

        Assert.Equal(["server/discover", "tools/list"], methods);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should reject with a network error from protocol discovery without falling back to initialize",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_reject_with_a_network_error_from_protocol_discovery_without_falling_back_to_initialize()
    {
        var methods = new List<string>();
        var networkError = new HttpRequestException("fetch failed");
        var handler = new DelegateHandler(
            async (message, _) =>
            {
                methods.Add(message.RootElement.GetProperty("method").GetString()!);
                await Task.Yield();
                throw networkError;
            }
        );

        var error = await Assert.ThrowsAsync<HttpRequestException>(() =>
            McpClient.ConnectAsync(
                new HttpMcpTransport(new HttpClient(handler), new Uri("http://localhost:4000/mcp"))
            )
        );

        Assert.Same(networkError, error);
        Assert.Equal(["server/discover"], methods);
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

    private sealed class DelegateHandler(
        Func<JsonDocument, CancellationToken, Task<HttpResponseMessage>> respond
    ) : HttpMessageHandler
    {
        public DelegateHandler(Func<JsonDocument, CancellationToken, HttpResponseMessage> respond)
            : this((m, c) => Task.FromResult(respond(m, c))) { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            using var message = JsonDocument.Parse(
                await request.Content!.ReadAsStringAsync().ConfigureAwait(false)
            );
            return await respond(message, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class RecordingHandler(List<string> methods, HttpStatusCode status, string data)
        : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            using var message = JsonDocument.Parse(
                await request.Content!.ReadAsStringAsync().ConfigureAwait(false)
            );
            methods.Add(message.RootElement.GetProperty("method").GetString()!);
            var body =
                "{\"jsonrpc\":\"2.0\",\"id\":"
                + message.RootElement.GetProperty("id").GetRawText()
                + ",\"error\":{\"code\":-32022,\"message\":\"Unsupported protocol version\",\"data\":"
                + data
                + "}}";
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }
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
