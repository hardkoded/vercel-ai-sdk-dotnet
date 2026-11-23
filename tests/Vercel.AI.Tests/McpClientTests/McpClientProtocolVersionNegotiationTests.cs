// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.McpClientTests;

public sealed class McpClientProtocolVersionNegotiationTests
{
    private const string Prefix =
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > protocol version negotiation::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set protocolVersion on transport after successful init",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_set_protocolVersion_on_transport_after_successful_init()
    {
        var transport = new NegotiationTransport(McpClient.LatestLegacyProtocolVersion);

        await McpClient.ConnectAsync(transport);

        Assert.Equal(McpClient.LatestLegacyProtocolVersion, transport.ProtocolVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set negotiated protocolVersion on transport when server downgrades",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8"
    )]
    public async Task Should_set_negotiated_protocolVersion_on_transport_when_server_downgrades()
    {
        var transport = new NegotiationTransport("2025-06-18");

        await McpClient.ConnectAsync(transport);

        Assert.Equal("2025-06-18", transport.ProtocolVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to legacy initialization when discovery gets no response",
        Coverage = UpstreamCoverage.Partial,
        Commit = "fac1d6a5882903594278edef0d58859d25c80ae8",
        Note = "The port does not send notifications/initialized after initialize, so that message is not in the expected method list."
    )]
    public async Task Partial_Should_fall_back_to_legacy_initialization_when_discovery_gets_no_response()
    {
        var transport = new NegotiationTransport(
            McpClient.LatestLegacyProtocolVersion,
            silentDiscovery: true
        );

        await McpClient.ConnectAsync(transport);

        Assert.Equal(McpClient.LatestLegacyProtocolVersion, transport.ProtocolVersion);
        Assert.Equal(["server/discover", "initialize", "tools/list"], transport.Methods);
    }

    private sealed class NegotiationTransport(
        string initializeVersion,
        bool silentDiscovery = false
    ) : IMcpProtocolVersionDiscoveryTransport
    {
        public List<string> Methods { get; } = [];

        public bool SupportsProtocolVersionDiscovery => silentDiscovery;

        public bool WaitsForProtocolVersionDiscovery => false;

        public string? ProtocolVersion { get; set; }

        public async Task<JsonElement> CallAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            Methods.Add(method);
            switch (method)
            {
                case "server/discover":
                    // A server that never answers. It ignores the token to prove the client bounds the wait itself.
                    await new TaskCompletionSource<bool>().Task.ConfigureAwait(false);
                    throw new InvalidOperationException();
                case "initialize":
                    return JsonDocument
                        .Parse(
                            "{\"protocolVersion\":\""
                                + initializeVersion
                                + "\",\"capabilities\":{\"tools\":{}},\"serverInfo\":{\"name\":\"test-server\",\"version\":\"1.0.0\"}}"
                        )
                        .RootElement;
                default:
                    return JsonDocument.Parse("{\"tools\":[]}").RootElement;
            }
        }
    }
}
