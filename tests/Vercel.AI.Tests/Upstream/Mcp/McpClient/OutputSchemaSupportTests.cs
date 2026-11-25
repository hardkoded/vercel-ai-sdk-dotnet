// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mcp;
using Vercel.AI.Prompt;
using Client = Vercel.AI.Mcp.McpClient;

namespace Vercel.AI.Tests.Upstream.Mcp.McpClient;

/// <summary>Port of <c>mcp-client.test.ts</c> &gt; <c>MCPClient</c> &gt; <c>outputSchema support</c>.</summary>
public sealed class OutputSchemaSupportTests
{
    private const string EmptyInput = "{\"type\":\"object\",\"properties\":{}}";

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should convert a scalar returned by execute with an outputSchema",
        Coverage = UpstreamCoverage.Covered,
        Note = "The schemas are JSON Schema strings, and Tool.Execute returns the JSON text of the output."
    )]
    public async Task ShouldConvertAScalarReturnedByExecuteWithAnOutputSchema()
    {
        var tool = await ToolAsync(
            "mock-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"7\"}]}",
            EmptyInput,
            "{\"type\":\"number\"}"
        );

        var output = await Execute(tool, "{}");

        Assert.Equal("7", output);
        using var input = JsonDocument.Parse("{}");
        var modelOutput = tool.ToModelOutput!(
            new ToolModelOutputCall("1", input.RootElement, output)
        );
        Assert.Equal("7", Assert.IsType<JsonToolModelOutput>(modelOutput).Value.GetRawText());
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should return typed output when outputSchema is provided with structuredContent",
        Coverage = UpstreamCoverage.Partial,
        Note = "The compile-time expectTypeOf checks have no C# equivalent."
    )]
    public async Task ShouldReturnTypedOutputWhenOutputSchemaIsProvidedWithStructuredContent()
    {
        var tool = await ToolAsync(
            "weather-tool",
            "{\"structuredContent\":{\"temperature\":22.5,\"conditions\":\"Sunny\"}}",
            "{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\"}},\"required\":[\"location\"]}",
            "{\"type\":\"object\",\"properties\":{\"temperature\":{\"type\":\"number\"},\"conditions\":{\"type\":\"string\"}},\"required\":[\"temperature\",\"conditions\"]}"
        );

        var result = await Execute(tool, "{\"location\":\"New York\"}");

        Assert.Equal("{\"temperature\":22.5,\"conditions\":\"Sunny\"}", result);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should fallback to parsing text content when structuredContent is not present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldFallbackToParsingTextContentWhenStructuredContentIsNotPresent()
    {
        var tool = await ToolAsync(
            "json-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"value\\\": 42, \\\"name\\\": \\\"test\\\"}\"}]}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\"},\"name\":{\"type\":\"string\"}},\"required\":[\"value\",\"name\"]}"
        );

        Assert.Equal("{\"value\":42,\"name\":\"test\"}", await Execute(tool, "{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should return CallToolResult when outputSchema is not provided",
        Coverage = UpstreamCoverage.Partial,
        Note = "The compile-time expectTypeOf check has no C# equivalent."
    )]
    public async Task ShouldReturnCallToolResultWhenOutputSchemaIsNotProvided()
    {
        var tool = await ToolAsync(
            "untyped-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"Some text result\"}]}",
            "{\"type\":\"object\",\"properties\":{\"input\":{\"type\":\"string\"}}}",
            null
        );

        Assert.Equal(
            "{\"content\":[{\"type\":\"text\",\"text\":\"Some text result\"}],\"isError\":false}",
            await Execute(tool, "{\"input\":\"test\"}")
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should throw error when structuredContent does not match outputSchema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowErrorWhenStructuredContentDoesNotMatchOutputSchema()
    {
        var tool = await ToolAsync(
            "bad-output-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"wrong\\\": \\\"data\\\"}\"}],\"structuredContent\":{\"wrong\":\"data\"}}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"expected\":{\"type\":\"number\"}},\"required\":[\"expected\"]}"
        );

        await Assert.ThrowsAsync<MCPClientError>(() => Execute(tool, "{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should throw error when text content is not valid JSON but outputSchema is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowErrorWhenTextContentIsNotValidJsonButOutputSchemaIsProvided()
    {
        var tool = await ToolAsync(
            "invalid-json-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"not valid json\"}]}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}"
        );

        await Assert.ThrowsAsync<MCPClientError>(() => Execute(tool, "{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should throw error when parsed JSON does not match outputSchema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowErrorWhenParsedJsonDoesNotMatchOutputSchema()
    {
        var tool = await ToolAsync(
            "mismatched-json-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"{\\\"differentField\\\": \\\"value\\\"}\"}]}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"requiredField\":{\"type\":\"string\"}},\"required\":[\"requiredField\"]}"
        );

        await Assert.ThrowsAsync<MCPClientError>(() => Execute(tool, "{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should work with automatic schema discovery (no outputSchema)",
        Coverage = UpstreamCoverage.Partial,
        Note = "The compile-time expectTypeOf check has no C# equivalent. The mock transport replaces the SSE transport."
    )]
    public async Task ShouldWorkWithAutomaticSchemaDiscoveryNoOutputSchema()
    {
        var tools = await Client.ListToolsAsync(
            new MockTransport(
                "mock-tool",
                "{\"content\":[{\"type\":\"text\",\"text\":\"Mock tool call result\"}]}",
                EmptyInput
            )
        );

        var result = await Execute(Assert.Single(tools), "{\"foo\":\"bar\"}");

        using var document = JsonDocument.Parse(result);
        Assert.Equal(
            "Mock tool call result",
            document.RootElement.GetProperty("content")[0].GetProperty("text").GetString()
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should handle complex nested outputSchema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldHandleComplexNestedOutputSchema()
    {
        const string data =
            "{\"users\":[{\"id\":1,\"name\":\"Alice\"},{\"id\":2,\"name\":\"Bob\"}],\"metadata\":{\"total\":2,\"page\":1}}";
        var tool = await ToolAsync(
            "complex-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":"
                + JsonSerializer.Serialize(data)
                + "}],\"structuredContent\":"
                + data
                + "}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"users\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"number\"},\"name\":{\"type\":\"string\"}},\"required\":[\"id\",\"name\"]}},\"metadata\":{\"type\":\"object\",\"properties\":{\"total\":{\"type\":\"number\"},\"page\":{\"type\":\"number\"}},\"required\":[\"total\",\"page\"]}},\"required\":[\"users\",\"metadata\"]}"
        );

        Assert.Equal(data, await Execute(tool, "{}"));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should bypass outputSchema validation when isError is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldBypassOutputSchemaValidationWhenIsErrorIsTrue()
    {
        var tool = await ToolAsync(
            "error-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"Workflow with name \\\"test\\\" already exists\"}],\"isError\":true}",
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"]}",
            "{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"number\"},\"status\":{\"type\":\"string\"}},\"required\":[\"id\",\"status\"]}"
        );

        Assert.Equal(
            "{\"content\":[{\"type\":\"text\",\"text\":\"Workflow with name \\\"test\\\" already exists\"}],\"isError\":true}",
            await Execute(tool, "{\"name\":\"test\"}")
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should bypass outputSchema validation for isError with structuredContent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldBypassOutputSchemaValidationForIsErrorWithStructuredContent()
    {
        var tool = await ToolAsync(
            "error-structured-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"Something went wrong\"}],\"structuredContent\":{\"error_code\":\"DUPLICATE\",\"message\":\"Already exists\"},\"isError\":true}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"number\"},\"status\":{\"type\":\"string\"}},\"required\":[\"id\",\"status\"]}"
        );

        Assert.Equal(
            "{\"content\":[{\"type\":\"text\",\"text\":\"Something went wrong\"}],\"structuredContent\":{\"error_code\":\"DUPLICATE\",\"message\":\"Already exists\"},\"isError\":true}",
            await Execute(tool, "{}")
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient > outputSchema support::should still validate outputSchema when isError is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldStillValidateOutputSchemaWhenIsErrorIsFalse()
    {
        var tool = await ToolAsync(
            "non-error-tool",
            "{\"content\":[{\"type\":\"text\",\"text\":\"not valid json\"}],\"isError\":false}",
            EmptyInput,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}"
        );

        await Assert.ThrowsAsync<MCPClientError>(() => Execute(tool, "{}"));
    }

    private static async Task<Tool> ToolAsync(
        string name,
        string callResult,
        string inputSchema,
        string? outputSchema
    )
    {
        var client = await Client.CreateAsync(new MockTransport(name, callResult, inputSchema));
        var tools = await client.ListToolsAsync(
            new Dictionary<string, McpToolSchema>
            {
                [name] = new McpToolSchema(inputSchema) { OutputSchema = outputSchema },
            }
        );
        return Assert.Single(tools);
    }

    private static Task<string> Execute(Tool tool, string arguments)
    {
        using var input = JsonDocument.Parse(arguments);
        return tool.Execute!(input.RootElement.Clone(), CancellationToken.None);
    }

    private sealed class MockTransport(string toolName, string callResult, string inputSchema)
        : IMcpTransport
    {
        public Task<JsonElement> CallAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            var json = method switch
            {
                "initialize" =>
                    "{\"protocolVersion\":\"2025-11-25\",\"capabilities\":{},\"serverInfo\":{\"name\":\"mock\",\"version\":\"1.0.0\"}}",
                "tools/call" => callResult,
                "tools/list" => "{\"tools\":[{\"name\":"
                    + JsonSerializer.Serialize(toolName)
                    + ",\"description\":\"d\",\"inputSchema\":"
                    + inputSchema
                    + "}]}",
                _ => "{}",
            };
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }
    }
}
