// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class AllowedToolsCannotBeAllowListedTests
{
    private const string Id = Suite + " > allowedTools provider option > tools that cannot be allow-listed::";

    private static LanguageModelTool WeatherTool() => Function("get_weather", "Get weather");

    private static LanguageModelTool McpTool(string name, string serverLabel) =>
        ProviderTool("openai.mcp", name, $$"""{"serverLabel":"{{serverLabel}}","serverUrl":"https://{{serverLabel}}.example.com/mcp"}""");

    private static ToolNameMapping McpMapping() =>
        Mapping(new[] { McpTool("alpha", "alpha"), McpTool("beta", "beta") }, ("openai.mcp", "mcp"));

    [Fact]
    [UpstreamTest(Id + "should drop %s from the allow-list and warn", Coverage = UpstreamCoverage.Covered)]
    public void Should_drop_each_unlistable_tool_from_the_allow_list_and_warn()
    {
        var cases = new (LanguageModelTool Tool, string Details)[]
        {
            (
                Function("problem_tool", "Deferred", openaiOptionsJson: """{"deferLoading":true}"""),
                "deferred tools are not visible to tool_choice.allowed_tools; the tool is removed from the allowed tools"),
            (
                Function("problem_tool", "Namespaced", openaiOptionsJson: """{"namespace":{"name":"crm","description":"CRM tools"}}"""),
                "tools inside an OpenAI tool namespace are not visible to tool_choice.allowed_tools; the tool is removed from the allowed tools"),
            (
                ProviderTool("openai.tool_search", "problem_tool", "{}"),
                "OpenAI does not support tool_search tools in tool_choice.allowed_tools; the tool is removed from the allowed tools"),
        };

        foreach (var (problemTool, details) in cases)
        {
            var result = Prepare(Allowed(null, new[] { "get_weather", "problem_tool" }), null, WeatherTool(), problemTool);

            AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"}]}""");
            AssertWarnings(result, ("unsupported", "allowedTools entry \"problem_tool\"", details));
        }
    }

    [Fact]
    [UpstreamTest(Id + "should drop an unlistable provider tool referenced by its canonical name", Coverage = UpstreamCoverage.Covered)]
    public void Should_drop_an_unlistable_provider_tool_referenced_by_its_canonical_name()
    {
        var searchTool = ProviderTool("openai.tool_search", "my_search", "{}");

        var result = Prepare(
            Allowed(null, new[] { "get_weather", "tool_search" }, Mapping(new[] { searchTool }, ("openai.tool_search", "tool_search"))),
            null,
            WeatherTool(),
            searchTool);

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"}]}""");
        AssertWarnings(result, ("unsupported", "allowedTools entry \"tool_search\"", "OpenAI does not support tool_search tools in tool_choice.allowed_tools; the tool is removed from the allowed tools"));
    }

    [Fact]
    [UpstreamTest(Id + "should drop an ambiguous canonical name when several tools share it", Coverage = UpstreamCoverage.Covered)]
    public void Should_drop_an_ambiguous_canonical_name_when_several_tools_share_it()
    {
        var result = Prepare(Allowed(null, new[] { "get_weather", "mcp" }, McpMapping()), null, WeatherTool(), McpTool("alpha", "alpha"), McpTool("beta", "beta"));

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"}]}""");
        AssertWarnings(result, ("unsupported", "allowedTools entry \"mcp\"", "several tools in this request share this provider tool name; use the tool name from the tools for this request instead"));
    }

    [Fact]
    [UpstreamTest(Id + "should still resolve each mcp server by its own tool name", Coverage = UpstreamCoverage.Covered)]
    public void Should_still_resolve_each_mcp_server_by_its_own_tool_name()
    {
        var result = Prepare(Allowed(null, new[] { "beta" }, McpMapping()), null, McpTool("alpha", "alpha"), McpTool("beta", "beta"));

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"mcp","server_label":"beta"}]}""");
        Assert.Empty(result.ToolWarnings);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when no allowed tool can be expressed", Coverage = UpstreamCoverage.Covered)]
    public void Should_throw_when_no_allowed_tool_can_be_expressed()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => Prepare(
            Allowed(null, new[] { "tool_search", "deferred_tool" }),
            null,
            ProviderTool("openai.tool_search", "tool_search", "{}"),
            Function("deferred_tool", "Deferred", openaiOptionsJson: """{"deferLoading":true}""")));

        Assert.Contains("allowedTools with only tools that cannot be allow-listed (tool_search, deferred_tool)", error.Message);
    }
}
