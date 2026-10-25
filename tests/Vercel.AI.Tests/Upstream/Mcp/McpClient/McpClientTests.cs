// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mcp;
using Vercel.AI.Prompt;
using Client = Vercel.AI.Mcp.McpClient;

namespace Vercel.AI.Tests.Upstream.Mcp.McpClient;

/// <summary>Port of <c>mcp-client.test.ts</c> &gt; <c>MCPClient</c>.</summary>
public sealed class McpClientTests
{
    [Theory]
    [InlineData("\"Hello world\"", "text", "Hello world")]
    [InlineData("\"\"", "text", "")]
    [InlineData("7", "json", "7")]
    [InlineData("0", "json", "0")]
    [InlineData("true", "json", "true")]
    [InlineData("false", "json", "false")]
    [InlineData("null", "json", "null")]
    [InlineData(null, "json", "null")]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert non-object output $output via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "The output is the JSON text of the value, because Tool.Execute returns a JSON string. A null output stands for undefined.")]
    public async Task ShouldConvertNonObjectOutputViaToModelOutput(string? output, string expectedType, string expectedValue)
    {
        var tools = await Client.ListToolsAsync(new MockMcpTransport());
        var tool = Assert.Single(tools);

        var result = tool.ToModelOutput!(new ToolModelOutputCall("1", JsonDocument.Parse("{}").RootElement, output));

        Assert.Equal(expectedType, result.Type);
        if (result is TextToolModelOutput text)
        {
            Assert.Equal(expectedValue, text.Value);
        }
        else
        {
            Assert.Equal(expectedValue, ((JsonToolModelOutput)result).Value.GetRawText());
        }
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert a string returned by a wrapped MCP execute function",
        Coverage = UpstreamCoverage.Covered,
        Note = "The wrapper returns the JSON text of a string, as Tool.Execute returns JSON text, so the string is the original output.")]
    public async Task ShouldConvertAStringReturnedByAWrappedMcpExecuteFunction()
    {
        var tools = await Client.ListToolsAsync(new MockMcpTransport());
        var originalTool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");
        var original = await originalTool.Execute!(input.RootElement, CancellationToken.None);

        // JSON.stringify(await originalTool.execute(...)) is a string that holds the JSON of the result.
        var wrappedOutput = JsonSerializer.Serialize(original);

        Assert.Equal(
            "{\"content\":[{\"type\":\"text\",\"text\":\"Mock tool call result\"}],\"isError\":false}",
            original);
        var result = originalTool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, wrappedOutput));
        Assert.Equal(original, Assert.IsType<TextToolModelOutput>(result).Value);
    }

    private sealed class MockMcpTransport : IMcpTransport
    {
        public Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
        {
            var json = method switch
            {
                "tools/call" => "{\"content\":[{\"type\":\"text\",\"text\":\"Mock tool call result\"}],\"isError\":false}",
                "tools/list" => "{\"tools\":[{\"name\":\"mock-tool\",\"description\":\"A mock tool for testing\",\"inputSchema\":{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}}]}",
                _ => "{}",
            };
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }
    }
}
