// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mcp;
using Client = Vercel.AI.Mcp.McpClient;

namespace Vercel.AI.Tests.Upstream.Mcp.McpClient;

/// <summary>Port of <c>mcp-client.test.ts</c> &gt; <c>MCPClient</c> &gt; <c>protocol version negotiation</c>.</summary>
public sealed class ProtocolVersionNegotiationTests
{
    private const string Prefix =
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > protocol version negotiation::";

    private const string Commit = "62bcb3ef276d13c09621c2b0b96354f9bf2f2900";

    [Fact]
    [UpstreamTest(
        Prefix + "should set protocolVersion on transport after successful init",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldSetProtocolVersionOnTransportAfterSuccessfulInit()
    {
        var transport = new ProtocolDiscoveryTransport(supportsDiscovery: false);

        await Client.CreateAsync(transport);

        Assert.Equal(Client.LatestLegacyProtocolVersion, transport.ProtocolVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set negotiated protocolVersion on transport when server downgrades",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldSetNegotiatedProtocolVersionOnTransportWhenServerDowngrades()
    {
        var transport = new ProtocolDiscoveryTransport(
            supportsDiscovery: false,
            initializeProtocolVersion: "2025-06-18"
        );

        await Client.CreateAsync(transport);

        Assert.Equal("2025-06-18", transport.ProtocolVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should support transports with getter-only protocolVersion and setProtocolVersion",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldSupportTransportsWithGetterOnlyProtocolVersionAndSetProtocolVersion()
    {
        var transport = new ProtocolDiscoveryTransport(
            supportsDiscovery: false,
            initializeProtocolVersion: "2025-06-18"
        );

        await Client.CreateAsync(transport);

        Assert.Equal("2025-06-18", transport.ProtocolVersion);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should discover a modern server and add metadata to requests",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldDiscoverAModernServerAndAddMetadataToRequests()
    {
        var transport = new ProtocolDiscoveryTransport();

        var client = await Client.CreateAsync(transport);
        await client.ListToolsAsync();

        Assert.Equal(Client.LatestProtocolVersion, transport.ProtocolVersion);
        Assert.Equal("modern-test-server", client.ServerInfo.Name);
        Assert.Equal("1.0.0", client.ServerInfo.Version);
        Assert.Equal(["server/discover", "tools/list"], transport.Methods);

        foreach (var message in transport.Messages.Where(message => !message.IsNotification))
        {
            var meta = message.Parameters.GetProperty("_meta");
            Assert.Equal(
                Client.LatestProtocolVersion,
                meta.GetProperty("io.modelcontextprotocol/protocolVersion").GetString()
            );
            Assert.Equal(
                JsonValueKind.Object,
                meta.GetProperty("io.modelcontextprotocol/clientCapabilities").ValueKind
            );
            var clientInfo = meta.GetProperty("io.modelcontextprotocol/clientInfo");
            Assert.Equal("ai-sdk-mcp-client", clientInfo.GetProperty("name").GetString());
            Assert.Equal("1.0.0", clientInfo.GetProperty("version").GetString());
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to legacy initialization for a legacy server",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldFallBackToLegacyInitializationForALegacyServer()
    {
        var transport = new ProtocolDiscoveryTransport(DiscoveryBehavior.Legacy);

        var client = await Client.CreateAsync(transport);
        await client.ListToolsAsync();

        Assert.Equal(Client.LatestLegacyProtocolVersion, transport.ProtocolVersion);
        Assert.Equal(
            ["server/discover", "initialize", "notifications/initialized", "tools/list"],
            transport.Methods
        );
        var toolList = transport.Messages.Single(message => message.Method == "tools/list");
        Assert.False(toolList.Parameters.TryGetProperty("_meta", out _));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fall back to legacy initialization when discovery gets no response",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit,
        Note = "Waits for the real one-second discovery timeout instead of fake timers."
    )]
    public async Task ShouldFallBackToLegacyInitializationWhenDiscoveryGetsNoResponse()
    {
        var transport = new ProtocolDiscoveryTransport(DiscoveryBehavior.Silent);

        await Client.CreateAsync(transport);

        Assert.Equal(Client.LatestLegacyProtocolVersion, transport.ProtocolVersion);
        Assert.Equal(
            ["server/discover", "initialize", "notifications/initialized"],
            transport.Methods
        );
    }

    [Theory]
    [InlineData("2025-11-25,2025-06-18")]
    [InlineData("2025-06-18")]
    [UpstreamTest(
        Prefix
            + "should explain how to opt into legacy initialization for advertised versions $supported",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldExplainHowToOptIntoLegacyInitializationForAdvertisedVersions(
        string supportedVersions
    )
    {
        var supported = supportedVersions.Split(',');
        var data = JsonDocument
            .Parse(
                JsonSerializer.Serialize(
                    new { requested = Client.LatestProtocolVersion, supported }
                )
            )
            .RootElement.Clone();
        var discoveryError = DiscoveryError(-32022, "Unsupported protocol version", data);
        var transport = new ProtocolDiscoveryTransport(
            DiscoveryBehavior.Unsupported,
            discoveryError: discoveryError
        );

        var error = await Assert.ThrowsAsync<MCPClientError>(() => Client.CreateAsync(transport));

        Assert.Contains($"rejected protocol version {Client.LatestProtocolVersion}", error.Message);
        foreach (var version in supported)
        {
            Assert.Contains(version, error.Message);
        }

        Assert.Contains("ProtocolVersionDiscovery to false", error.Message);
        Assert.Contains("legacy", error.Message);
        Assert.Equal(discoveryError.Code, error.Code);
        Assert.Equal(data.GetRawText(), error.Data!.Value.GetRawText());
        var cause = Assert.IsType<MCPClientError>(error.InnerException);
        Assert.Same(discoveryError, cause);
        Assert.Equal(discoveryError.Message, cause.Message);
        Assert.Equal(discoveryError.Code, cause.Code);
        Assert.Equal(data.GetRawText(), cause.Data!.Value.GetRawText());
        Assert.IsType<JsonRpcErrorCause>(cause.InnerException);
        Assert.Equal(["server/discover"], transport.Methods);
    }

    [Theory]
    [InlineData("missing data", null)]
    [InlineData("null data", "null")]
    [InlineData("missing supported versions", "{}")]
    [InlineData("non-array supported versions", "{\"supported\":\"2025-06-18\"}")]
    [InlineData("non-string supported versions", "{\"supported\":[\"2025-06-18\",null]}")]
    [InlineData("empty supported versions", "{\"supported\":[]}")]
    [InlineData("no mutually supported versions", "{\"supported\":[\"2099-01-01\"]}")]
    [InlineData(
        "the rejected modern version is supported",
        "{\"supported\":[\"2026-07-28\",\"2025-06-18\"]}"
    )]
    [UpstreamTest(
        Prefix + "should preserve a version error with $name",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldPreserveAVersionErrorWith(string name, string? data)
    {
        Assert.NotNull(name);
        var discoveryError = DiscoveryError(
            -32022,
            "Unsupported protocol version",
            data is null ? null : JsonDocument.Parse(data).RootElement.Clone()
        );
        var transport = new ProtocolDiscoveryTransport(
            DiscoveryBehavior.Unsupported,
            discoveryError: discoveryError
        );

        var error = await Assert.ThrowsAsync<MCPClientError>(() => Client.CreateAsync(transport));

        AssertPreserved(discoveryError, error);
        Assert.Equal(["server/discover"], transport.Methods);
    }

    [Theory]
    [InlineData(-32020)]
    [InlineData(-32021)]
    [UpstreamTest(
        Prefix
            + "should preserve modern protocol error %s even when legacy versions are advertised",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldPreserveModernProtocolErrorEvenWhenLegacyVersionsAreAdvertised(int code)
    {
        var discoveryError = DiscoveryError(
            code,
            "Modern protocol error",
            JsonDocument.Parse("{\"supported\":[\"2025-06-18\"]}").RootElement.Clone()
        );
        var transport = new ProtocolDiscoveryTransport(
            DiscoveryBehavior.Unsupported,
            discoveryError: discoveryError
        );

        var error = await Assert.ThrowsAsync<MCPClientError>(() => Client.CreateAsync(transport));

        AssertPreserved(discoveryError, error);
        Assert.Equal(["server/discover"], transport.Methods);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should skip discovery and initialize when protocolVersionDiscovery is false",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldSkipDiscoveryAndInitializeWhenProtocolVersionDiscoveryIsFalse()
    {
        var transport = new ProtocolDiscoveryTransport(DiscoveryBehavior.Unsupported, false);

        var client = await Client.CreateAsync(
            transport,
            new McpClientOptions { ProtocolVersionDiscovery = false }
        );
        var tools = await client.ListToolsAsync();

        Assert.Empty(tools);
        Assert.Equal(Client.LatestLegacyProtocolVersion, transport.ProtocolVersion);
        Assert.Equal(["initialize", "notifications/initialized", "tools/list"], transport.Methods);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should require resultType from modern servers",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task ShouldRequireResultTypeFromModernServers()
    {
        var transport = new ProtocolDiscoveryTransport(includeToolListResultType: false);

        var client = await Client.CreateAsync(transport);

        var error = await Assert.ThrowsAsync<MCPClientError>(() => client.ListToolsAsync());
        Assert.Contains("Modern MCP result is missing resultType", error.Message);
    }

    private enum DiscoveryBehavior
    {
        Modern,
        Legacy,
        Silent,
        Unsupported,
    }

    /// <summary>Builds the error a transport throws for a JSON-RPC error response, with the error object as its cause.</summary>
    private static MCPClientError DiscoveryError(int code, string message, JsonElement? data)
    {
        var error = new Dictionary<string, object?> { ["code"] = code, ["message"] = message };
        if (data is { } value)
        {
            error["data"] = value;
        }

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(new Dictionary<string, object?> { ["error"] = error })
        );
        return StdioMcpTransport.CreateError(document.RootElement);
    }

    private static void AssertPreserved(MCPClientError expected, MCPClientError actual)
    {
        Assert.Same(expected, actual);
        var cause = Assert.IsType<JsonRpcErrorCause>(actual.InnerException);
        Assert.Equal(expected.Code, cause.Error.GetProperty("code").GetInt32());
        Assert.Equal(expected.Message, cause.Error.GetProperty("message").GetString());
    }

    private sealed record SentMessage(string Method, JsonElement Parameters, bool IsNotification);

    /// <summary>Port of <c>ProtocolDiscoveryTransport</c>, also used for the plain initialize cases.</summary>
    private sealed class ProtocolDiscoveryTransport(
        DiscoveryBehavior behavior = DiscoveryBehavior.Modern,
        bool includeToolListResultType = true,
        MCPClientError? discoveryError = null,
        bool supportsDiscovery = true,
        string initializeProtocolVersion = Client.LatestLegacyProtocolVersion
    ) : IMcpProtocolVersionDiscoveryTransport, IMcpNotificationTransport
    {
        public List<SentMessage> Messages { get; } = [];

        public IEnumerable<string> Methods => Messages.Select(message => message.Method);

        public bool SupportsProtocolVersionDiscovery => supportsDiscovery;

        public bool WaitsForProtocolVersionDiscovery => false;

        public string? ProtocolVersion => _protocolVersion;

        private string? _protocolVersion;

        public void SetProtocolVersion(string version) => _protocolVersion = version;

        public Task NotifyAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            Messages.Add(new SentMessage(method, parameters.Clone(), true));
            return Task.CompletedTask;
        }

        public async Task<JsonElement> CallAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            Messages.Add(new SentMessage(method, parameters.Clone(), false));
            switch (method)
            {
                case "server/discover":
                    switch (behavior)
                    {
                        case DiscoveryBehavior.Silent:
                            await Task.Delay(Timeout.Infinite, cancellationToken);
                            break;
                        case DiscoveryBehavior.Legacy:
                            throw new MCPClientError("Method not found", code: -32601);
                        case DiscoveryBehavior.Unsupported:
                            throw discoveryError!;
                    }

                    return Parse(
                        "{\"resultType\":\"complete\",\"supportedVersions\":[\""
                            + Client.LatestProtocolVersion
                            + "\"],\"capabilities\":{\"tools\":{}},\"_meta\":{\"io.modelcontextprotocol/serverInfo\":{\"name\":\"modern-test-server\",\"version\":\"1.0.0\"}}}"
                    );
                case "initialize":
                    return Parse(
                        "{\"protocolVersion\":\""
                            + initializeProtocolVersion
                            + "\",\"serverInfo\":{\"name\":\"legacy-test-server\",\"version\":\"1.0.0\"},\"capabilities\":{\"tools\":{}}}"
                    );
                case "tools/list":
                    return Parse(
                        includeToolListResultType
                            ? "{\"resultType\":\"complete\",\"tools\":[]}"
                            : "{\"tools\":[]}"
                    );
                default:
                    return Parse("{}");
            }
        }

        private static JsonElement Parse(string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
    }
}
