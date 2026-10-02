// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpHttpHeaderTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::extracts statically reachable header bindings",
        Coverage = UpstreamCoverage.Covered)]
    public void Extracts_statically_reachable_header_bindings()
    {
        var result = McpHttpHeaders.GetBindings(JsonNode.Parse(
            "{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"options\":{\"type\":\"object\",\"properties\":{\"dryRun\":{\"type\":\"boolean\",\"x-mcp-header\":\"Dry-Run\"}}}}}"));

        Assert.True(result.Success);
        Assert.Equal(2, result.Bindings.Count);
        Assert.Equal("Region", result.Bindings[0].HeaderName);
        Assert.Equal(new[] { "region" }, result.Bindings[0].Path);
        Assert.Equal("string", result.Bindings[0].ValueType);
        Assert.Equal("Dry-Run", result.Bindings[1].HeaderName);
        Assert.Equal(new[] { "options", "dryRun" }, result.Bindings[1].Path);
        Assert.Equal("boolean", result.Bindings[1].ValueType);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::rejects invalid x-mcp-header schemas",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_header_schemas()
    {
        var cases = new[]
        {
            ("{\"type\":\"object\",\"items\":{\"type\":\"string\",\"x-mcp-header\":\"Invalid\"}}", "not on a statically reachable property"),
            ("{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"}}}}", "not on a statically reachable property"),
            ("{\"type\":\"object\",\"additionalProperties\":{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"}}}}", "not on a statically reachable property"),
            ("{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"number\",\"x-mcp-header\":\"Count\"}}}", "can only annotate boolean, integer, or string"),
            ("{\"type\":\"object\",\"properties\":{\"first\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"second\":{\"type\":\"string\",\"x-mcp-header\":\"region\"}}}", "is not unique"),
        };

        for (var index = 0; index < cases.Length; index++)
        {
            var result = McpHttpHeaders.GetBindings(JsonNode.Parse(cases[index].Item1));
            Assert.False(result.Success);
            Assert.Contains(cases[index].Item2, result.Error ?? string.Empty);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::creates encoded headers from tool arguments",
        Coverage = UpstreamCoverage.Covered)]
    public void Creates_encoded_headers_from_tool_arguments()
    {
        var result = McpHttpHeaders.GetBindings(JsonNode.Parse(
            "{\"type\":\"object\",\"properties\":{\"region\":{\"type\":\"string\",\"x-mcp-header\":\"Region\"},\"count\":{\"type\":\"integer\",\"x-mcp-header\":\"Count\"},\"options\":{\"type\":\"object\",\"properties\":{\"enabled\":{\"type\":\"boolean\",\"x-mcp-header\":\"Enabled\"}}}}}"));
        Assert.True(result.Success);

        var headers = McpHttpHeaders.CreateHeaders(
            result.Bindings,
            (JsonObject)JsonNode.Parse("{\"region\":\"Hello, 世界\",\"count\":42,\"options\":{\"enabled\":false}}")!);

        Assert.Equal("=?base64?SGVsbG8sIOS4lueVjA==?=", headers["Mcp-Param-Region"]);
        Assert.Equal("42", headers["Mcp-Param-Count"]);
        Assert.Equal("false", headers["Mcp-Param-Enabled"]);
        Assert.Equal(3, headers.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-http-headers.test.ts::MCP HTTP headers::encodes MCP header values safely",
        Coverage = UpstreamCoverage.Covered)]
    public void Encodes_header_values_safely()
    {
        Assert.Equal("plain-ascii", McpHttpHeaders.EncodeHeaderValue("plain-ascii"));
        Assert.Equal("=?base64?IHBhZGRlZCA=?=", McpHttpHeaders.EncodeHeaderValue(" padded "));
        Assert.Equal("=?base64?PT9iYXNlNjQ/bGl0ZXJhbD89?=", McpHttpHeaders.EncodeHeaderValue("=?base64?literal?="));
    }
}
