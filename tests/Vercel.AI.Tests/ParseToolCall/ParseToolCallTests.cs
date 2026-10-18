// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;
using static Vercel.AI.Tests.ParseToolCall.ParseToolCallFixture;

namespace Vercel.AI.Tests.ParseToolCall;

/// <summary>Port of <c>parse-tool-call.test.ts</c> &gt; <c>parseToolCall</c>.</summary>
public sealed class ParseToolCallTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should successfully parse a valid tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSuccessfullyParseAValidToolCall()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}"),
            new[] { TestTool(ParamsSchema) });

        Assert.False(result.Invalid);
        Assert.False(result.Dynamic);
        Assert.Null(result.Error);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should refine input after successfully parsing a valid tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldRefineInputAfterSuccessfullyParsingAValidToolCall()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"value\": \" raw \"}"),
            new[] { TestTool("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}") },
            refineToolInput: new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["testTool"] = (input, _) => Task.FromResult(Json("{\"value\":\"" + input.GetProperty("value").GetString()!.Trim() + "\"}")),
            });

        Assert.False(result.Invalid);
        JsonAssert.Equal(result.Input, "{\"value\":\"raw\"}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should not treat an inherited object property as a refinement for a tool whose name collides with it",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldNotTreatAnInheritedObjectPropertyAsARefinementForAToolWhoseNameCollidesWithIt()
    {
        // `RefineToolInput` is keyed by tool name. With no entry for `toString`, the raw parsed input is returned unchanged.
        var result = await ParseAsync(
            new GeneratedToolCall("123", "toString", "{\"value\": \"raw\"}"),
            new[] { TestTool("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}", "toString") },
            refineToolInput: new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
            {
                ["testTool"] = (_, _) => Task.FromResult(Json("{\"value\":\"refined\"}")),
            });

        JsonAssert.Equal(result.Input, "{\"value\":\"raw\"}");
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should successfully parse a valid provider-executed dynamic tool call",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSuccessfullyParseAValidProviderExecutedDynamicToolCall()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}", Json("{\"testProvider\":{\"signature\":\"sig\"}}"))
            {
                ProviderExecuted = true,
                Dynamic = true,
            },
            Array.Empty<Tool>());

        Assert.False(result.Invalid);
        Assert.True(result.Dynamic);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
        Assert.True(result.ProviderExecuted);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"testProvider\":{\"signature\":\"sig\"}}");
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should successfully parse a valid tool call with provider metadata",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSuccessfullyParseAValidToolCallWithProviderMetadata()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}", Json("{\"testProvider\":{\"signature\":\"sig\"}}")),
            new[] { TestTool(ParamsSchema) });

        Assert.False(result.Invalid);
        Assert.False(result.Dynamic);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
        Assert.False(result.ProviderExecuted);
        JsonAssert.Equal(result.ProviderMetadata!.Value, "{\"testProvider\":{\"signature\":\"sig\"}}");
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should successfully process empty tool calls for tools that have no inputSchema",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSuccessfullyProcessEmptyToolCallsForToolsThatHaveNoInputSchema()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", ""),
            new[] { TestTool(EmptyObjectSchema) });

        Assert.False(result.Invalid);
        JsonAssert.Equal(result.Input, "{}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should successfully process empty object tool calls for tools that have no inputSchema",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSuccessfullyProcessEmptyObjectToolCallsForToolsThatHaveNoInputSchema()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{}"),
            new[] { TestTool(EmptyObjectSchema) });

        Assert.False(result.Invalid);
        JsonAssert.Equal(result.Input, "{}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should throw NoSuchToolError when tools is null",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowNoSuchToolErrorWhenToolsIsNull()
    {
        var result = await ParseAsync(new GeneratedToolCall("123", "testTool", "{}"), null);

        Assert.True(result.Dynamic);
        Assert.True(result.Invalid);
        Assert.True(NoSuchToolError.IsInstance(result.Error));
        Assert.Equal("Model tried to call unavailable tool 'testTool'. No tools are available.", result.Error!.Message);
        JsonAssert.Equal(result.Input, "{}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should throw NoSuchToolError when tool is not found",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldThrowNoSuchToolErrorWhenToolIsNotFound()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "nonExistentTool", "{}"),
            new[] { TestTool(ParamsSchema) });

        Assert.True(result.Dynamic);
        Assert.True(result.Invalid);
        Assert.True(NoSuchToolError.IsInstance(result.Error));
        Assert.Equal("Model tried to call unavailable tool 'nonExistentTool'. Available tools: testTool.", result.Error!.Message);
        JsonAssert.Equal(result.Input, "{}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("nonExistentTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should throw InvalidToolInputError when args are invalid",
        Coverage = UpstreamCoverage.Partial,
        Note = "The error message comes from the built-in JSON Schema validator, not the Zod error list. See #604.")]
    public async Task ShouldThrowInvalidToolInputErrorWhenArgsAreInvalid()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\"}"), // Missing required param2
            new[] { TestTool(ParamsSchema) });

        Assert.True(result.Dynamic);
        Assert.True(result.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(result.Error));
        Assert.StartsWith("Invalid input for tool testTool: ", result.Error!.Message);
        Assert.Contains("param2", result.Error.Message);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\"}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/parse-tool-call.test.ts::parseToolCall::should set dynamic to true for dynamic tools",
        Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldSetDynamicToTrueForDynamicTools()
    {
        var result = await ParseAsync(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\", \"param2\": 42}"),
            new[] { Tool.DynamicTool("testTool", null, ParamsSchema, (_, _) => Task.FromResult("result")) });

        Assert.False(result.Invalid);
        Assert.True(result.Dynamic);
        JsonAssert.Equal(result.Input, "{\"param1\":\"test\",\"param2\":42}");
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Title);
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }
}
