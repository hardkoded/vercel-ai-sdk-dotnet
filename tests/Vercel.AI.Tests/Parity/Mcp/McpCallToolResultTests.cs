// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using System.Text.Json.Nodes;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests;

public sealed class McpCallToolResultTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::normalizes structured-only results with %s content",
        Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_structured_only_results()
    {
        var cases = new[]
        {
            ("{\"structuredContent\":{\"value\":42}}", "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"value\\\":42}\"}],\"structuredContent\":{\"value\":42},\"isError\":false}"),
            ("{\"structuredContent\":[1,\"two\",false]}", "{\"content\":[{\"type\":\"text\",\"text\":\"[1,\\\"two\\\",false]\"}],\"structuredContent\":[1,\"two\",false],\"isError\":false}"),
            ("{\"structuredContent\":\"result\"}", "{\"content\":[{\"type\":\"text\",\"text\":\"\\\"result\\\"\"}],\"structuredContent\":\"result\",\"isError\":false}"),
            ("{\"structuredContent\":42}", "{\"content\":[{\"type\":\"text\",\"text\":\"42\"}],\"structuredContent\":42,\"isError\":false}"),
            ("{\"structuredContent\":true}", "{\"content\":[{\"type\":\"text\",\"text\":\"true\"}],\"structuredContent\":true,\"isError\":false}"),
            ("{\"structuredContent\":null}", "{\"content\":[{\"type\":\"text\",\"text\":\"null\"}],\"structuredContent\":null,\"isError\":false}"),
        };

        for (var index = 0; index < cases.Length; index++)
        {
            ParityAssert.JsonEqual(McpCallToolResult.Parse(JsonNode.Parse(cases[index].Item1)), cases[index].Item2);
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::preserves structured-only error results",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_structured_only_error_results()
    {
        ParityAssert.JsonEqual(
            McpCallToolResult.Parse(JsonNode.Parse("{\"structuredContent\":{\"code\":\"NOT_FOUND\"},\"isError\":true}")),
            "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"code\\\":\\\"NOT_FOUND\\\"}\"}],\"structuredContent\":{\"code\":\"NOT_FOUND\"},\"isError\":true}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::preserves results that already contain content",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_results_that_already_contain_content()
    {
        ParityAssert.JsonEqual(
            McpCallToolResult.Parse(JsonNode.Parse("{\"content\":[{\"type\":\"text\",\"text\":\"Existing content\"}],\"structuredContent\":{\"value\":42}}")),
            "{\"content\":[{\"type\":\"text\",\"text\":\"Existing content\"}],\"structuredContent\":{\"value\":42},\"isError\":false}");
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects results without content, structuredContent, or toolResult",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_results_without_content_structured_content_or_tool_result()
    {
        Assert.False(McpCallToolResult.TryParse(JsonNode.Parse("{}"), out _));
        Assert.False(McpCallToolResult.TryParse(JsonNode.Parse("{\"_meta\":{}}"), out _));
        Assert.False(McpCallToolResult.TryParse(JsonNode.Parse("{\"value\":42}"), out _));
        Assert.False(McpCallToolResult.TryParse(
            new Dictionary<string, object?> { ["toolResult"] = McpUndefined.Value },
            out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::accepts legacy toolResult responses",
        Coverage = UpstreamCoverage.Covered)]
    public void Accepts_legacy_tool_result_responses()
    {
        var parsed = McpCallToolResult.Parse(JsonNode.Parse("{\"toolResult\":{\"value\":42}}"));
        ParityAssert.JsonEqual(parsed, "{\"toolResult\":{\"value\":42}}");
        Assert.False(parsed.ContainsKey("isError"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects malformed known content types",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_malformed_known_content_types()
    {
        Assert.False(McpCallToolResult.TryParse(JsonNode.Parse("{\"content\":[{\"type\":\"text\"}]}"), out _));
        Assert.False(McpCallToolResult.TryParse(
            JsonNode.Parse("{\"content\":[{\"type\":\"image\",\"data\":\"not-base64\",\"mimeType\":\"image/png\"}]}"),
            out _));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects non-JSON structured content",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_non_json_structured_content()
    {
        object?[] values =
        {
            McpUndefined.Value,
            new BigInteger(1),
            double.NaN,
            double.PositiveInfinity,
        };

        for (var index = 0; index < values.Length; index++)
        {
            var value = new Dictionary<string, object?>
            {
                ["structuredContent"] = values[index],
            };
            Assert.False(McpCallToolResult.TryParse(value, out _));
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/types.test.ts::CallToolResultSchema::rejects cyclic structured content",
        Coverage = UpstreamCoverage.Covered)]
    public void Rejects_cyclic_structured_content()
    {
        var structured = new Dictionary<string, object?>();
        structured["self"] = structured;

        Assert.Throws<InvalidOperationException>(() => McpCallToolResult.Parse(
            new Dictionary<string, object?>
            {
                ["structuredContent"] = structured,
            }));
    }
}
