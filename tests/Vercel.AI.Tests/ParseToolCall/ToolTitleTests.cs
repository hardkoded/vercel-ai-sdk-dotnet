// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ParseToolCall.ParseToolCallFixture;

namespace Vercel.AI.Tests.ParseToolCall;

/// <summary>Port of <c>parse-tool-call.test.ts</c> &gt; <c>parseToolCall</c> &gt; <c>tool title</c>.</summary>
public sealed class ToolTitleTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool title::should include title in parsed dynamic tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeTitleInParsedDynamicToolCall()
    {
        var tool = Tool.DynamicTool("weather", "Get weather", LocationSchema, (_, _) => Task.FromResult("sunny"));
        tool.Title = "Weather Information";

        var result = await ParseAsync(
            new GeneratedToolCall("call-1", "weather", "{\"location\":\"Paris\"}"),
            new[] { tool });

        Assert.Equal("Weather Information", result.Title);
        Assert.True(result.Dynamic);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool title::should include title in parsed static tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeTitleInParsedStaticToolCall()
    {
        var tool = Tool.Function(
            "calculator",
            "Calculate",
            "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"number\"},\"b\":{\"type\":\"number\"}},\"required\":[\"a\",\"b\"]}",
            (_, _) => Task.FromResult("8"));
        tool.Title = "Calculator";

        var result = await ParseAsync(
            new GeneratedToolCall("call-2", "calculator", "{\"a\":5,\"b\":3}"),
            new[] { tool });

        Assert.Equal("Calculator", result.Title);
        Assert.False(result.Dynamic);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall > tool title::should include title in invalid tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldIncludeTitleInInvalidToolCall()
    {
        var tool = Tool.Function(
            "invalidTool",
            "Tool that will fail",
            "{\"type\":\"object\",\"properties\":{\"required\":{\"type\":\"string\"}},\"required\":[\"required\"]}",
            (_, _) => Task.FromResult("result"));
        tool.Title = "Invalid Tool";

        var result = await ParseAsync(
            new GeneratedToolCall("call-4", "invalidTool", "invalid json"),
            new[] { tool });

        Assert.True(result.Invalid);
        Assert.Equal("Invalid Tool", result.Title);
    }
}
