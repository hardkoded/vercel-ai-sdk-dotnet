// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; mcp</c>.</summary>
public sealed class PrepareResponsesToolsMcpTests
{
    private const string McpArgs = """{"serverUrl":"https://example.com/mcp","serverLabel":"test-server"}""";
    private const string Prefix = TestFile + " > mcp::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare mcp tool with required args only", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_mcp_tool_with_required_args_only()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.mcp", "mcp", McpArgs) }, null);

        JsonAssert.Equal(result.Tools, """[{"server_label":"test-server","server_url":"https://example.com/mcp","type":"mcp"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should prepare mcp tool with all optional args", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_mcp_tool_with_all_optional_args()
    {
        var args = """
            {"serverUrl":"https://example.com/mcp","serverLabel":"test-server","serverDescription":"A test MCP server",
             "allowedTools":["tool1","tool2"],"headers":{"X-Custom":"value"},"authorization":"Bearer token123"}
            """;

        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.mcp", "mcp", args) }, null);

        JsonAssert.Equal(result.Tools, """
            [{"allowed_tools":["tool1","tool2"],"authorization":"Bearer token123","headers":{"X-Custom":"value"},
              "server_description":"A test MCP server","server_label":"test-server","server_url":"https://example.com/mcp","type":"mcp"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when trying to force mcp tool via toolChoice", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_when_trying_to_force_mcp_tool_via_toolChoice()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.mcp", "mcp", McpArgs) }, ToolChoice.Tool("mcp"));

        Assert.Null(result.ToolChoice);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "toolChoice for server-side tool \"mcp\"");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple tools including mcp", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_mcp()
    {
        var result = XaiResponsesTools.Prepare(
            new[] { ProviderTool("xai.web_search", "web_search"), ProviderTool("xai.mcp", "mcp", McpArgs), Function("calculator", "calculate numbers") },
            null);

        Assert.Equal(new[] { "web_search", "mcp", "function" }, Types(result));
    }
}
