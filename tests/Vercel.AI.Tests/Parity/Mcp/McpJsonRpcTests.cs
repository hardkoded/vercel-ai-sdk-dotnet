// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using System.Text.Json.Nodes;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpJsonRpcTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::validates JSON-RPC requests",
        Coverage = UpstreamCoverage.Covered)]
    public void Validates_json_rpc_requests()
    {
        var message = McpJsonRpc.Validate(JsonNode.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\"}"));
        ParityAssert.JsonEqual(message, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::validates JSON-RPC responses",
        Coverage = UpstreamCoverage.Covered)]
    public void Validates_json_rpc_responses()
    {
        var message = McpJsonRpc.Validate(JsonNode.Parse("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"tools\":[]}}"));
        ParityAssert.JsonEqual(message, "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"tools\":[]}}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::rejects invalid JSON-RPC messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_json_rpc_messages()
    {
        Assert.Throws<ArgumentException>(() =>
            McpJsonRpc.Validate(JsonNode.Parse("{\"jsonrpc\":\"1.0\",\"id\":1,\"result\":{\"tools\":[]}}")));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/json-rpc-message.test.ts::validateJSONRPCMessage::does not export JSON-RPC schemas",
        Coverage = UpstreamCoverage.Covered)]
    public void Does_not_export_json_rpc_schemas()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in typeof(McpJsonRpc).Assembly.GetExportedTypes())
        {
            names.Add(type.Name);
        }

        Assert.DoesNotContain("JSONRPCMessageSchema", names);
        Assert.DoesNotContain("JSONRPCRequestSchema", names);
        Assert.DoesNotContain("JSONRPCResponseSchema", names);
    }
}
