// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.ParseToolCall;

public sealed class ParseToolCallTests
{
    private const string EmptyObjectSchema = "{\"type\":\"object\",\"properties\":{}}";

    private static Tool TestTool(string schema) =>
        Tool.Function("testTool", null, schema, (_, _) => Task.FromResult("null"));

    [Fact]
    public void ShouldSuccessfullyProcessEmptyToolCallsForToolsThatHaveNoInputSchema()
    {
        var result = ToolCallParser.ParseToolCall(
            new GeneratedToolCall("123", "testTool", string.Empty),
            new[] { TestTool(EmptyObjectSchema) });

        Assert.False(result.Invalid);
        Assert.Equal("{}", result.Input.GetRawText());
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
        Assert.False(result.ProviderExecuted);
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    public void ShouldSuccessfullyProcessEmptyObjectToolCallsForToolsThatHaveNoInputSchema()
    {
        var result = ToolCallParser.ParseToolCall(
            new GeneratedToolCall("123", "testTool", "{}"),
            new[] { TestTool(EmptyObjectSchema) });

        Assert.False(result.Invalid);
        Assert.Equal("{}", result.Input.GetRawText());
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }

    [Fact]
    public void ShouldThrowInvalidToolInputErrorWhenArgsAreInvalid()
    {
        var result = ToolCallParser.ParseToolCall(
            new GeneratedToolCall("123", "testTool", "{\"param1\": \"test\"}"),
            new[]
            {
                TestTool("{\"type\":\"object\",\"properties\":{\"param1\":{\"type\":\"string\"},\"param2\":{\"type\":\"number\"}},\"required\":[\"param1\",\"param2\"]}"),
            });

        Assert.True(result.Invalid);
        Assert.True(InvalidToolInputError.IsInstance(result.Error));
        var error = (InvalidToolInputError)result.Error!;
        Assert.Equal("testTool", error.ToolName);
        Assert.StartsWith("Invalid input for tool testTool: ", error.Message);
        Assert.Contains("param2", error.Message);
        Assert.Equal("test", result.Input.GetProperty("param1").GetString());
        Assert.Equal("123", result.ToolCallId);
        Assert.Equal("testTool", result.ToolName);
    }
}
