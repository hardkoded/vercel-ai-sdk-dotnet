// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.ParseToolCall.ParseToolCallFixture;

namespace Vercel.AI.Tests.ParseToolCall;

/// <summary>Port of <c>parse-tool-call.test.ts</c> &gt; <c>parseToolCall</c> &gt; <c>tool metadata propagation</c>.</summary>
public sealed class ToolMetadataPropagationTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool metadata propagation::should propagate tool metadata onto a parsed dynamic tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPropagateToolMetadataOntoAParsedDynamicToolCall()
    {
        var tool = Tool.DynamicTool("weather", "Get weather", LocationSchema, (_, _) => Task.FromResult("sunny"));
        tool.Metadata = new Dictionary<string, JsonElement> { ["clientName"] = Json("\"MyMCPClient\"") };

        var result = await ParseAsync(
            new GeneratedToolCall("call-1", "weather", "{\"location\":\"Paris\"}"),
            new[] { tool });

        Assert.True(result.Dynamic);
        JsonAssert.Equal(result.Input, "{\"location\":\"Paris\"}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("MyMCPClient", result.ToolMetadata!["clientName"].GetString());
        Assert.Single(result.ToolMetadata);
        Assert.Equal("weather", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool metadata propagation::should propagate tool metadata onto a parsed static tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPropagateToolMetadataOntoAParsedStaticToolCall()
    {
        var tool = Tool.Function(
            "calculator",
            "Calculate",
            "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"},\"b\":{\"type\":\"number\"}},\"required\":[\"a\",\"b\"]}",
            (_, _) => Task.FromResult("8"));
        tool.Metadata = new Dictionary<string, JsonElement> { ["clientName"] = Json("\"MyMCPClient\"") };

        var result = await ParseAsync(
            new GeneratedToolCall("call-2", "calculator", "{\"a\":5,\"b\":3}"),
            new[] { tool });

        Assert.False(result.Dynamic);
        JsonAssert.Equal(result.Input, "{\"a\":5,\"b\":3}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("call-2", result.ToolCallId);
        Assert.Equal("MyMCPClient", result.ToolMetadata!["clientName"].GetString());
        Assert.Single(result.ToolMetadata);
        Assert.Equal("calculator", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool metadata propagation::should keep tool metadata separate from model-supplied providerMetadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldKeepToolMetadataSeparateFromModelSuppliedProviderMetadata()
    {
        var tool = Tool.DynamicTool("weather", "Get weather", LocationSchema, (_, _) => Task.FromResult("sunny"));
        tool.Metadata = new Dictionary<string, JsonElement> { ["clientName"] = Json("\"MyMCPClient\"") };

        var result = await ParseAsync(
            new GeneratedToolCall("call-3", "weather", "{\"location\":\"Paris\"}", Json("{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}")),
            new[] { tool });

        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"anthropic\":{\"cacheControl\":{\"type\":\"ephemeral\"}}}");
        Assert.Equal("MyMCPClient", result.ToolMetadata!["clientName"].GetString());
        Assert.Single(result.ToolMetadata);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool metadata propagation::should propagate tool metadata onto an invalid tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPropagateToolMetadataOntoAnInvalidToolCall()
    {
        var tool = Tool.DynamicTool(
            "weather",
            "Get weather",
            "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"],\"additionalProperties\":false}",
            (_, _) => Task.FromResult("sunny"));
        tool.Metadata = new Dictionary<string, JsonElement> { ["clientName"] = Json("\"MyMCPClient\"") };

        var result = await ParseAsync(
            new GeneratedToolCall("call-4", "weather", "invalid json"),
            new[] { tool });

        Assert.True(result.Invalid);
        Assert.Equal("MyMCPClient", result.ToolMetadata!["clientName"].GetString());
        Assert.Single(result.ToolMetadata);
    }
}
