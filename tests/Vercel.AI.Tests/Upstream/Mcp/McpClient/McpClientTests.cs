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
    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==";

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert MCP image content to AI SDK format via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion.")]
    public async Task ShouldConvertMcpImageContentToAiSdkFormatViaToModelOutput()
    {
        var output = $"{{\"content\":[{{\"type\":\"image\",\"data\":\"{ImageData}\",\"mimeType\":\"image/png\"}}],\"isError\":false}}";
        var tools = await Client.ListToolsAsync(new MockMcpTransport("get-image", "Returns an image", output));
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output)));
        var file = Assert.IsType<FileToolModelContentPart>(Assert.Single(result.Value));
        Assert.Equal("file", file.Type);
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal("data", file.Data.Type);
        Assert.Equal(ImageData, file.Data.TextData);
        Assert.Null(file.FileName);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert MCP text content to AI SDK format via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion.")]
    public async Task ShouldConvertMcpTextContentToAiSdkFormatViaToModelOutput()
    {
        const string output = "{\"content\":[{\"type\":\"text\",\"text\":\"Hello world\"}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(new MockMcpTransport("get-text", "Returns text", output));
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output)));
        var text = Assert.Single(result.Value);
        Assert.Equal("text", text.Type);
        Assert.Equal("Hello world", text.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert mixed MCP content to AI SDK format via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion.")]
    public async Task ShouldConvertMixedMcpContentToAiSdkFormatViaToModelOutput()
    {
        const string output = "{\"content\":[{\"type\":\"text\",\"text\":\"Here is an image:\"},{\"type\":\"image\",\"data\":\"aGVsbG8=\",\"mimeType\":\"image/png\"}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(new MockMcpTransport("get-mixed", "Returns mixed content", output));
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output)));
        Assert.Collection(
            result.Value,
            text =>
            {
                Assert.Equal("text", text.Type);
                Assert.Equal("Here is an image:", text.Text);
            },
            part =>
            {
                var file = Assert.IsType<FileToolModelContentPart>(part);
                Assert.Equal("file", file.Type);
                Assert.Equal("image/png", file.MediaType);
                Assert.Equal("data", file.Data.Type);
                Assert.Equal("aGVsbG8=", file.Data.TextData);
                Assert.Null(file.FileName);
            });
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should fallback to JSON for unknown content types via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion.")]
    public async Task ShouldFallbackToJsonForUnknownContentTypesViaToModelOutput()
    {
        const string output = "{\"content\":[{\"type\":\"custom\",\"data\":{\"foo\":\"bar\"}}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(new MockMcpTransport("get-unknown", "Returns unknown content", output));
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output)));
        var text = Assert.Single(result.Value);
        Assert.Equal("text", text.Type);
        Assert.Equal("{\"type\":\"custom\",\"data\":{\"foo\":\"bar\"}}", text.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should fallback to JSON when result has no content array via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion.")]
    public async Task ShouldFallbackToJsonWhenResultHasNoContentArrayViaToModelOutput()
    {
        const string output = "{\"toolResult\":{\"value\":42},\"isError\":false}";
        var tools = await Client.ListToolsAsync(new MockMcpTransport("get-raw", "Returns raw result", output));
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<JsonToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output)));
        Assert.Equal(output, result.Value.GetRawText());
    }

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
        private readonly string _toolName;
        private readonly string _description;
        private readonly string _toolCallResult;

        public MockMcpTransport(
            string toolName = "mock-tool",
            string description = "A mock tool for testing",
            string toolCallResult = "{\"content\":[{\"type\":\"text\",\"text\":\"Mock tool call result\"}],\"isError\":false}")
        {
            _toolName = toolName;
            _description = description;
            _toolCallResult = toolCallResult;
        }

        public Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
        {
            var json = method switch
            {
                "tools/call" => _toolCallResult,
                "tools/list" => JsonSerializer.Serialize(new
                {
                    tools = new[]
                    {
                        new
                        {
                            name = _toolName,
                            description = _description,
                            inputSchema = new { type = "object" },
                        },
                    },
                }),
                _ => "{}",
            };
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }
    }
}
