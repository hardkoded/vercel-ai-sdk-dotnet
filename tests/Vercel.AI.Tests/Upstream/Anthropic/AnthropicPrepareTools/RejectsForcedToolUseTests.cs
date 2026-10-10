// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class RejectsForcedToolUseTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-prepare-tools.test.ts::rejectsForcedToolUse::";

    private static List<AnthropicToolDefinition> Tools() => new()
    {
        new AnthropicToolDefinition { Name = "testFunction", Description = "Test", InputSchema = AnthropicParity.Json("{}") },
        new AnthropicToolDefinition { Name = "otherFunction", Description = "Other", InputSchema = AnthropicParity.Json("{}") },
    };

    private static AnthropicPreparedTools Prepare(string toolChoice, string? toolName = null, bool? disableParallelToolUse = null)
        => AnthropicToolPreparer.Prepare(Tools(), toolChoice, toolName, disableParallelToolUse, null, true, true, false, true);

    [Fact]
    [UpstreamTest(Prefix + "should fall back to auto for tool choice \"required\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_fall_back_to_auto_for_tool_choice_required()
    {
        var result = Prepare("required");

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\"}"), result.ToolChoice));
        Assert.Equal(2, result.Tools!.Count);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("toolChoice", warning.Feature);
        Assert.Equal("toolChoice 'required' is not supported by this model because it rejects forced tool use. Using 'auto' instead. Instruct the model to use a tool in the prompt and verify that a tool call was made.", warning.Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should only send the selected tool with auto for tool choice \"tool\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_only_send_the_selected_tool_with_auto_for_tool_choice_tool()
    {
        var result = Prepare("tool", "otherFunction");

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\"}"), result.ToolChoice));
        Assert.Equal(new[] { "otherFunction" }, result.Tools!.Select(tool => tool!["name"]!.GetValue<string>()).ToArray());
        Assert.Single(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve disableParallelToolUse in the auto fallback", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_disableParallelToolUse_in_the_auto_fallback()
    {
        var result = Prepare("required", null, true);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\",\"disable_parallel_tool_use\":true}"), result.ToolChoice));
    }

    [Fact]
    [UpstreamTest(Prefix + "should not affect auto and none tool choices", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_affect_auto_and_none_tool_choices()
    {
        var auto = Prepare("auto");
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"auto\"}"), auto.ToolChoice));
        Assert.Empty(auto.Warnings);

        var none = Prepare("none");
        Assert.Equal(2, none.Tools!.Count);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"type\":\"none\"}"), none.ToolChoice));
        Assert.Empty(none.Warnings);
    }
}
