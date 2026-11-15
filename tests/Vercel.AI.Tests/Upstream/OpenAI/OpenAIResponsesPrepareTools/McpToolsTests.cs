// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class McpToolsTests
{
    private const string Tunnel = "tunnel_0123456789abcdef0123456789abcdef";

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > MCP tools::should serialize a remote MCP server URL", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public void Should_serialize_a_remote_MCP_server_URL()
    {
        var result = Prepare(null, ProviderTool("openai.mcp", "mcp", "{\"serverLabel\":\"remote\",\"serverUrl\":\"https://example.com/mcp\"}"));

        AssertTools(result, "[{\"type\":\"mcp\",\"server_label\":\"remote\",\"server_url\":\"https://example.com/mcp\",\"require_approval\":\"never\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > MCP tools::should serialize a Secure MCP tunnel and preserve MCP options", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public void Should_serialize_a_Secure_MCP_tunnel_and_preserve_MCP_options()
    {
        var result = Prepare(
            null,
            ProviderTool(
                "openai.mcp",
                "mcp",
                "{\"serverLabel\":\"secure-tunnel\",\"tunnelId\":\"" + Tunnel + "\",\"serverDescription\":\"Private MCP server\",\"authorization\":\"Bearer test-token\",\"headers\":{\"X-Test\":\"test-value\"},\"allowedTools\":{\"readOnly\":true,\"toolNames\":[\"search\"]},\"requireApproval\":{\"never\":{\"toolNames\":[\"search\"]}}}"));

        AssertTools(
            result,
            "[{\"type\":\"mcp\",\"server_label\":\"secure-tunnel\",\"tunnel_id\":\"" + Tunnel + "\",\"server_description\":\"Private MCP server\",\"authorization\":\"Bearer test-token\",\"headers\":{\"X-Test\":\"test-value\"},\"allowed_tools\":{\"read_only\":true,\"tool_names\":[\"search\"]},\"require_approval\":{\"never\":{\"tool_names\":[\"search\"]}}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > MCP tools::should preserve legacy URL and connector combinations", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public void Should_preserve_legacy_URL_and_connector_combinations()
    {
        var result = Prepare(null, ProviderTool("openai.mcp", "mcp", "{\"serverLabel\":\"legacy\",\"serverUrl\":\"https://example.com/mcp\",\"connectorId\":\"connector_example\"}"));

        var tool = Assert.Single(result.Tools!.AsArray())!;
        Assert.Equal("https://example.com/mcp", tool["server_url"]!.GetValue<string>());
        Assert.Equal("connector_example", tool["connector_id"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("{\"serverLabel\":\"missing-target\"}")]
    // JSON has no undefined: the upstream serverUrl, connectorId, and tunnelId set to undefined are absent keys.
    [InlineData("{\"serverLabel\":\"undefined-targets\"}")]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > MCP tools::should reject missing MCP targets: %j", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public void Should_reject_missing_MCP_targets(string args)
    {
        var error = Assert.Throws<TypeValidationException>(() => Prepare(null, ProviderTool("openai.mcp", "mcp", args)));

        Assert.Contains("One of serverUrl, connectorId, or tunnelId must be provided.", error.Message);
    }

    [Theory]
    [InlineData("{\"serverUrl\":\"https://example.com/mcp\"}")]
    [InlineData("{\"connectorId\":\"connector_example\"}")]
    [InlineData("{\"serverUrl\":\"https://example.com/mcp\",\"connectorId\":\"connector_example\"}")]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > MCP tools::should reject a tunnel combined with another target: %j", Coverage = UpstreamCoverage.Covered, Commit = "7a905226b619e14e705722448fe0e8629de13739")]
    public void Should_reject_a_tunnel_combined_with_another_target(string targets)
    {
        var args = "{\"serverLabel\":\"ambiguous-target\",\"tunnelId\":\"" + Tunnel + "\"," + targets.Trim('{', '}') + "}";

        var error = Assert.Throws<TypeValidationException>(() => Prepare(null, ProviderTool("openai.mcp", "mcp", args)));

        Assert.Contains("tunnelId cannot be combined with serverUrl or connectorId.", error.Message);
    }
}
