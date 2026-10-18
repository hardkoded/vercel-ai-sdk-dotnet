// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Mcp.McpSseTransport;

public sealed class DeserializeMessageTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::deserializeMessage::should reject payloads containing __proto__ (prototype pollution)",
        Coverage = UpstreamCoverage.Covered)]
    public void ShouldRejectPayloadsContainingProtoPrototypePollution()
    {
        var malicious = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"__proto__\":{\"polluted\":true}}}";

        var error = Assert.Throws<JsonParseException>(() => SseMcpTransport.DeserializeMessage(malicious));
        Assert.Equal(
            "JSON parsing failed: Text: " + malicious + ". Error message: Object contains forbidden prototype property",
            error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-sse-transport.test.ts::deserializeMessage::should reject payloads containing constructor.prototype",
        Coverage = UpstreamCoverage.Covered)]
    public void ShouldRejectPayloadsContainingConstructorPrototype()
    {
        var malicious = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"constructor\":{\"prototype\":{\"polluted\":true}}}}";

        var error = Assert.Throws<JsonParseException>(() => SseMcpTransport.DeserializeMessage(malicious));
        Assert.Equal(
            "JSON parsing failed: Text: " + malicious + ". Error message: Object contains forbidden prototype property",
            error.Message);
    }
}
