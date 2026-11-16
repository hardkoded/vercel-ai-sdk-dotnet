// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class PrepareToolsTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools::";

    [Fact]
    [UpstreamTest(Prefix + "should preserve tools with tool choice \"none\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_tools_with_tool_choice_none()
    {
        var tools = new List<AnthropicToolDefinition>
        {
            new AnthropicToolDefinition { Name = "testFunction", Description = "Test", InputSchema = AnthropicParity.Json("{}") },
        };

        var result = AnthropicToolPreparer.Prepare(tools, "none", null, null, null, true, true);

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("[{\"name\":\"testFunction\",\"description\":\"Test\",\"input_schema\":{}}]"),
            result.Tools));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"none\"}"), result.ToolChoice));
    }

    [Fact]
    [UpstreamTest(Prefix + "does not add the strict-tool beta for providers that only support native final output", Coverage = UpstreamCoverage.Covered, Commit = "4b8f00efff97de78e52667359aa33267a1036c46")]
    public void Does_not_add_the_strict_tool_beta_for_providers_that_only_support_native_final_output()
    {
        var tools = new List<AnthropicToolDefinition>
        {
            new AnthropicToolDefinition { Name = "report", InputSchema = AnthropicParity.Json("{\"type\":\"object\",\"properties\":{}}"), Strict = true },
        };

        var result = AnthropicToolPreparer.Prepare(tools, null, null, null, null, true, false);

        Assert.Empty(result.Betas);
        Assert.Null(result.Tools![0]!["strict"]);
        Assert.Contains(result.Warnings, warning => warning.Type == "unsupported" && warning.Feature == "strict");
    }
}
