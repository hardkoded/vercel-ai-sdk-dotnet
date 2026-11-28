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
    private const string ImageData =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==";

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert MCP image content to AI SDK format via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion."
    )]
    public async Task ShouldConvertMcpImageContentToAiSdkFormatViaToModelOutput()
    {
        var output =
            $"{{\"content\":[{{\"type\":\"image\",\"data\":\"{ImageData}\",\"mimeType\":\"image/png\"}}],\"isError\":false}}";
        var tools = await Client.ListToolsAsync(
            new MockMcpTransport("get-image", "Returns an image", output)
        );
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output))
        );
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
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion."
    )]
    public async Task ShouldConvertMcpTextContentToAiSdkFormatViaToModelOutput()
    {
        const string output =
            "{\"content\":[{\"type\":\"text\",\"text\":\"Hello world\"}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(
            new MockMcpTransport("get-text", "Returns text", output)
        );
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output))
        );
        var text = Assert.Single(result.Value);
        Assert.Equal("text", text.Type);
        Assert.Equal("Hello world", text.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should convert mixed MCP content to AI SDK format via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion."
    )]
    public async Task ShouldConvertMixedMcpContentToAiSdkFormatViaToModelOutput()
    {
        const string output =
            "{\"content\":[{\"type\":\"text\",\"text\":\"Here is an image:\"},{\"type\":\"image\",\"data\":\"aGVsbG8=\",\"mimeType\":\"image/png\"}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(
            new MockMcpTransport("get-mixed", "Returns mixed content", output)
        );
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output))
        );
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
            }
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should fallback to JSON for unknown content types via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion."
    )]
    public async Task ShouldFallbackToJsonForUnknownContentTypesViaToModelOutput()
    {
        const string output =
            "{\"content\":[{\"type\":\"custom\",\"data\":{\"foo\":\"bar\"}}],\"isError\":false}";
        var tools = await Client.ListToolsAsync(
            new MockMcpTransport("get-unknown", "Returns unknown content", output)
        );
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<ContentToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output))
        );
        var text = Assert.Single(result.Value);
        Assert.Equal("text", text.Type);
        Assert.Equal("{\"type\":\"custom\",\"data\":{\"foo\":\"bar\"}}", text.Text);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should fallback to JSON when result has no content array via toModelOutput",
        Coverage = UpstreamCoverage.Covered,
        Note = "Tool.Execute returns the MCP result as JSON text; ToModelOutput parses that representation before applying the upstream conversion."
    )]
    public async Task ShouldFallbackToJsonWhenResultHasNoContentArrayViaToModelOutput()
    {
        const string output = "{\"toolResult\":{\"value\":42},\"isError\":false}";
        var tools = await Client.ListToolsAsync(
            new MockMcpTransport("get-raw", "Returns raw result", output)
        );
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        Assert.Equal(output, await tool.Execute!(input.RootElement, CancellationToken.None));

        var result = Assert.IsType<JsonToolModelOutput>(
            tool.ToModelOutput!(new ToolModelOutputCall("1", input.RootElement, output))
        );
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
        Note = "The output is the JSON text of the value, because Tool.Execute returns a JSON string. A null output stands for undefined."
    )]
    public async Task ShouldConvertNonObjectOutputViaToModelOutput(
        string? output,
        string expectedType,
        string expectedValue
    )
    {
        var tools = await Client.ListToolsAsync(new MockMcpTransport());
        var tool = Assert.Single(tools);

        var result = tool.ToModelOutput!(
            new ToolModelOutputCall("1", JsonDocument.Parse("{}").RootElement, output)
        );

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
        Note = "The wrapper returns the JSON text of a string, as Tool.Execute returns JSON text, so the string is the original output."
    )]
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
            original
        );
        var result = originalTool.ToModelOutput!(
            new ToolModelOutputCall("1", input.RootElement, wrappedOutput)
        );
        Assert.Equal(original, Assert.IsType<TextToolModelOutput>(result).Value);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should not retry successful MCP tool results with isError",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the isError result and the single attempt. The port has no maxRetries option, so the retry-enabled client is not covered. Tool.Execute returns the result as JSON text."
    )]
    public async Task ShouldNotRetrySuccessfulMcpToolResultsWithIsError()
    {
        var transport = new MockMcpTransport(
            toolCallResult: "{\"content\":[{\"type\":\"text\",\"text\":\"tool-level error\"}],\"isError\":true}"
        );
        var tools = await Client.ListToolsAsync(transport);
        var tool = Assert.Single(tools);
        using var input = JsonDocument.Parse("{}");

        var result = await tool.Execute!(input.RootElement, CancellationToken.None);

        Assert.Equal(
            "{\"content\":[{\"type\":\"text\",\"text\":\"tool-level error\"}],\"isError\":true}",
            result
        );
        Assert.Equal(1, transport.ToolCallAttempts);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should return tools from all paginated tool list responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldReturnToolsFromAllPaginatedToolListResponses()
    {
        var cursors = new List<string?>();
        var transport = new ScriptedTransport(
            (
                "initialize",
                "{\"protocolVersion\":\"2025-11-25\",\"serverInfo\":{\"name\":\"paginated-tools-server\",\"version\":\"1.0.0\"},\"capabilities\":{\"tools\":{}}}"
            ),
            ("tools/list", null)
        )
        {
            Handler = (method, parameters) =>
            {
                if (method != "tools/list")
                {
                    return null;
                }

                var cursor = parameters.TryGetProperty("cursor", out var value)
                    ? value.GetString()
                    : null;
                cursors.Add(cursor);
                return cursor is null
                    ? "{\"tools\":[{\"name\":\"first-page-tool\",\"inputSchema\":{\"type\":\"object\"}}],\"nextCursor\":\"second-page\"}"
                    : "{\"tools\":[{\"name\":\"second-page-tool\",\"inputSchema\":{\"type\":\"object\"}}]}";
            },
        };

        var tools = await Client.ConnectAsync(transport);

        Assert.Equal(["first-page-tool", "second-page-tool"], tools.Select(tool => tool.Name));
        Assert.Equal([null, "second-page"], cursors);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should throw if the server does not support any tools",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowIfTheServerDoesNotSupportAnyTools()
    {
        var transport = new ScriptedTransport(
            (
                "initialize",
                "{\"protocolVersion\":\"2025-11-25\",\"serverInfo\":{\"name\":\"mock-mcp-server\",\"version\":\"1.0.0\"},\"capabilities\":{\"resources\":{}}}"
            )
        );
        var client = await Client.CreateAsync(transport);

        await Assert.ThrowsAsync<MCPClientError>(() => client.ListToolsAsync());
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should throw if server sends invalid initialize result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowIfServerSendsInvalidInitializeResult()
    {
        var transport = new ScriptedTransport(("initialize", "{}"));

        await Assert.ThrowsAsync<MCPClientError>(() => Client.CreateAsync(transport));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should throw if server sends invalid protocol version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldThrowIfServerSendsInvalidProtocolVersion()
    {
        var transport = new ScriptedTransport(
            (
                "initialize",
                "{\"protocolVersion\":\"foo\",\"serverInfo\":{\"name\":\"mock-mcp-server\",\"version\":\"1.0.0\"},\"capabilities\":{}}"
            )
        );

        await Assert.ThrowsAsync<MCPClientError>(() => Client.CreateAsync(transport));
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should accept server responding with 2025-11-25 protocol version",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldAcceptServerRespondingWith20251125ProtocolVersion()
    {
        var transport = new ScriptedTransport(("initialize", InitializeResult("2025-11-25")));
        var client = await Client.CreateAsync(transport);

        var tools = await client.ListToolsAsync();

        Assert.NotNull(tools);
    }

    [Theory]
    [InlineData("2025-06-18")]
    [InlineData("2025-03-26")]
    [InlineData("2024-11-05")]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should accept server responding with older supported protocol versions",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream loops over the versions in one test; the port runs one theory case per version."
    )]
    public async Task ShouldAcceptServerRespondingWithOlderSupportedProtocolVersions(string version)
    {
        var transport = new ScriptedTransport(("initialize", InitializeResult(version)));
        var client = await Client.CreateAsync(transport);

        var tools = await client.ListToolsAsync();

        Assert.NotNull(tools);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should expose serverInfo from initialization",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldExposeServerInfoFromInitialization()
    {
        var transport = new ScriptedTransport(
            (
                "initialize",
                "{\"protocolVersion\":\"2025-11-25\",\"serverInfo\":{\"name\":\"my-server\",\"version\":\"2.0.0\",\"title\":\"My Server\"},\"capabilities\":{\"tools\":{}}}"
            )
        );

        var client = await Client.CreateAsync(transport);

        Assert.Equal("my-server", client.ServerInfo.Name);
        Assert.Equal("2.0.0", client.ServerInfo.Version);
        Assert.Equal("My Server", client.ServerInfo.Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should expose serverInfo without title when server omits it",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldExposeServerInfoWithoutTitleWhenServerOmitsIt()
    {
        var transport = new ScriptedTransport(("initialize", InitializeResult("2025-11-25")));

        var client = await Client.CreateAsync(transport);

        Assert.Equal("mock-mcp-server", client.ServerInfo.Name);
        Assert.Equal("1.0.0", client.ServerInfo.Version);
        Assert.Null(client.ServerInfo.Title);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should expose instructions from initialization when server provides them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldExposeInstructionsFromInitializationWhenServerProvidesThem()
    {
        var transport = new ScriptedTransport(
            (
                "initialize",
                InitializeResult(
                    "2025-11-25",
                    "Use search tools to resolve IDs — never ask the user."
                )
            )
        );

        var client = await Client.CreateAsync(transport);

        Assert.Equal("Use search tools to resolve IDs — never ask the user.", client.Instructions);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should expose undefined instructions when server omits them",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldExposeUndefinedInstructionsWhenServerOmitsThem()
    {
        var transport = new ScriptedTransport(("initialize", InitializeResult("2025-11-25")));

        var client = await Client.CreateAsync(transport);

        Assert.Null(client.Instructions);
    }

    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-client.test.ts::MCPClient::should not return user-defined tool if it is nonexistent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldNotReturnUserDefinedToolIfItIsNonexistent()
    {
        var transport = new ScriptedTransport(
            ("initialize", InitializeResult("2025-11-25")),
            (
                "tools/list",
                "{\"tools\":[{\"name\":\"mock-tool\",\"inputSchema\":{\"type\":\"object\"}}]}"
            )
        );
        var client = await Client.CreateAsync(transport);

        var tools = await client.ListToolsAsync(
            new Dictionary<string, McpToolSchema>
            {
                ["nonexistent-tool"] = new(
                    "{\"type\":\"object\",\"properties\":{\"bar\":{\"type\":\"string\"}}}"
                ),
            }
        );

        Assert.DoesNotContain(tools, tool => tool.Name == "nonexistent-tool");
    }

    private static string InitializeResult(string version, string? instructions = null) =>
        "{\"protocolVersion\":\""
        + version
        + "\",\"serverInfo\":{\"name\":\"mock-mcp-server\",\"version\":\"1.0.0\"},\"capabilities\":{\"tools\":{}}"
        + (
            instructions is null
                ? string.Empty
                : ",\"instructions\":" + JsonSerializer.Serialize(instructions)
        )
        + "}";

    private sealed class ScriptedTransport(params (string Method, string? Result)[] results)
        : IMcpNotificationTransport
    {
        public Func<string, JsonElement, string?>? Handler { get; init; }

        public Task<JsonElement> CallAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            var json = Handler?.Invoke(method, parameters)
                ?? results.FirstOrDefault(entry => entry.Method == method).Result
                ?? (method == "tools/list" ? "{\"tools\":[]}" : "{}");
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }

        public Task NotifyAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        ) => Task.CompletedTask;
    }

    private sealed class MockMcpTransport : IMcpTransport
    {
        private readonly string _toolName;
        private readonly string _description;
        private readonly string _toolCallResult;

        public int ToolCallAttempts { get; private set; }

        public MockMcpTransport(
            string toolName = "mock-tool",
            string description = "A mock tool for testing",
            string toolCallResult =
                "{\"content\":[{\"type\":\"text\",\"text\":\"Mock tool call result\"}],\"isError\":false}"
        )
        {
            _toolName = toolName;
            _description = description;
            _toolCallResult = toolCallResult;
        }

        public Task<JsonElement> CallAsync(
            string method,
            JsonElement parameters,
            CancellationToken cancellationToken
        )
        {
            if (method == "tools/call")
            {
                ToolCallAttempts++;
            }

            var json = method switch
            {
                "tools/call" => _toolCallResult,
                "tools/list" => JsonSerializer.Serialize(
                    new
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
                    }
                ),
                _ => "{}",
            };
            using var document = JsonDocument.Parse(json);
            return Task.FromResult(document.RootElement.Clone());
        }
    }
}
