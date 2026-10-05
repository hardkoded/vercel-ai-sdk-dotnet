// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mcp;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Mcp;

public sealed class MCPClientErrorTests
{
    [Fact]
    public void ShouldExposeHttpErrorMetadataThroughThePackageEntryPoint()
    {
        var error = new MCPClientError(
            "MCP request failed",
            code: -32000,
            statusCode: 503,
            url: "https://example.com/mcp",
            responseBody: "Service Unavailable");

        Assert.True(MCPClientError.IsInstance(error));
        Assert.Equal("MCP request failed", error.Message);
        Assert.Equal(-32000, error.Code);
        Assert.Equal(503, error.StatusCode);
        Assert.Equal("https://example.com/mcp", error.Url);
        Assert.Equal("Service Unavailable", error.ResponseBody);
    }

    [Fact]
    public void NarrowsUnknownErrorsAndExposesOptionalMcpErrorMetadata()
    {
        Exception error = new MCPClientError("MCP request failed");

        Assert.True(MCPClientError.IsInstance(error));
        var mcpError = (MCPClientError)error;
        Assert.Null(mcpError.Code);
        Assert.Null(mcpError.StatusCode);
        Assert.Null(mcpError.Url);
        Assert.Null(mcpError.ResponseBody);
        Assert.False(MCPClientError.IsInstance(new AiSdkException("MCP request failed")));
    }
}
