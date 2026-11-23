// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.McpHttpTransport;

public sealed class HttpMcpTransportProtocolVersionDowngradeTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-transport.test.ts::HttpMCPTransport > protocol version downgrade::should use LATEST_LEGACY_PROTOCOL_VERSION by default",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_use_LATEST_LEGACY_PROTOCOL_VERSION_by_default()
    {
        var headers = new List<string?>();
        var handler = new RecordingHeaderHandler(headers);
        var transport = new HttpMcpTransport(
            new HttpClient(handler),
            new Uri("http://localhost:4000/mcp")
        );
        using var parameters = JsonDocument.Parse("{}");

        await transport.CallAsync("initialize", parameters.RootElement, CancellationToken.None);

        Assert.Equal([McpClient.LatestLegacyProtocolVersion], headers);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-transport.test.ts::HttpMCPTransport > protocol version downgrade::should use negotiated protocolVersion in headers after it is set",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_use_negotiated_protocolVersion_in_headers_after_it_is_set()
    {
        var headers = new List<string?>();
        var transport = new HttpMcpTransport(
            new HttpClient(new RecordingHeaderHandler(headers)),
            new Uri("http://localhost:4000/mcp")
        )
        {
            ProtocolVersion = "2025-06-18",
        };
        using var parameters = JsonDocument.Parse("{}");

        await transport.CallAsync("tools/list", parameters.RootElement, CancellationToken.None);

        Assert.Equal(["2025-06-18"], headers);
    }

    private sealed class RecordingHeaderHandler(List<string?> headers) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            headers.Add(
                request.Headers.TryGetValues("mcp-protocol-version", out var values)
                    ? values.Single()
                    : null
            );
            return Task.FromResult(Json("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"ok\":true}}"));
        }
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
}
