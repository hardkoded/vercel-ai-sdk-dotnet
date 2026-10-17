// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class AllowedToolsProviderOptionTests
{
    private const string Id = Suite + " > allowedTools provider option::";

    private static LanguageModelTool WeatherTool() => Function("get_weather", "Get weather");

    private static LanguageModelTool TimeTool() => Function("get_time", "Get time");

    private static LanguageModelTool WebSearchTool() => ProviderTool("openai.web_search", "search", "{}");

    // Mirrors the mapping the language model builds from its provider tool names, so canonical names resolve as in a request.
    private static ToolNameMapping WebSearchMapping() => Mapping(new[] { WebSearchTool() }, ("openai.web_search", "web_search"));

    [Fact]
    [UpstreamTest(Id + "should emit allowed_tools with default auto mode", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_allowed_tools_with_default_auto_mode()
    {
        var result = Prepare(Allowed(null, new[] { "get_weather" }), null, WeatherTool(), TimeTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"}]}""");
        Assert.Equal(2, result.Tools!.Count);
    }

    [Fact]
    [UpstreamTest(Id + "should emit allowed_tools with required mode", Coverage = UpstreamCoverage.Covered)]
    public void Should_emit_allowed_tools_with_required_mode()
    {
        var result = Prepare(Allowed("required", new[] { "get_weather", "get_time" }), null, WeatherTool(), TimeTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"required","tools":[{"type":"function","name":"get_weather"},{"type":"function","name":"get_time"}]}""");
        Assert.Equal(2, result.Tools!.Count);
    }

    [Fact]
    [UpstreamTest(Id + "should override request-level toolChoice when allowedTools is set", Coverage = UpstreamCoverage.Covered)]
    public void Should_override_request_level_toolChoice_when_allowedTools_is_set()
    {
        var result = Prepare(Allowed(null, new[] { "get_weather" }), ToolChoice.Required, WeatherTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should preserve the order of mixed function and built-in entries without deduplicating", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_the_order_of_mixed_function_and_built_in_entries_without_deduplicating()
    {
        var result = Prepare(Allowed(null, new[] { "search", "get_weather", "search" }), null, WeatherTool(), WebSearchTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"web_search"},{"type":"function","name":"get_weather"},{"type":"web_search"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should resolve a built-in tool by its canonical provider name", Coverage = UpstreamCoverage.Covered)]
    public void Should_resolve_a_built_in_tool_by_its_canonical_provider_name()
    {
        var result = Prepare(Allowed(null, new[] { "web_search" }, WebSearchMapping()), null, WebSearchTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"web_search"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should prefer the user tool name over a canonical name alias on collision", Coverage = UpstreamCoverage.Covered)]
    public void Should_prefer_the_user_tool_name_over_a_canonical_name_alias_on_collision()
    {
        var result = Prepare(
            Allowed(null, new[] { "web_search" }, WebSearchMapping()),
            null,
            Function("web_search", "A function that happens to be named web_search"),
            WebSearchTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"web_search"}]}""");
        AssertWarnings(result, ("unsupported", "allowedTools entry \"web_search\"", "this name is both a tool name and the provider tool name of another tool in this request; the tool with this name is allowed"));
    }

    [Fact]
    [UpstreamTest(Id + "should not resolve a function tool by the literal name \"function\"", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_resolve_a_function_tool_by_the_literal_name_function()
    {
        var result = Prepare(Allowed(null, new[] { "function" }), null, WeatherTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"function"}]}""");
        AssertWarnings(result, ("unsupported", "allowedTools entry \"function\"", "the tool is not part of the tools for this request and is sent as a function tool"));
    }

    [Fact]
    [UpstreamTest(Id + "should warn and still send an entry for a tool that is not part of the request", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_and_still_send_an_entry_for_a_tool_that_is_not_part_of_the_request()
    {
        var result = Prepare(Allowed(null, new[] { "get_weather", "typo_tool" }), null, WeatherTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"get_weather"},{"type":"function","name":"typo_tool"}]}""");
        AssertWarnings(result, ("unsupported", "allowedTools entry \"typo_tool\"", "the tool is not part of the tools for this request and is sent as a function tool"));
    }

    [Fact]
    [UpstreamTest(Id + "should apply the tool name mapping for a tool that is not part of the request", Coverage = UpstreamCoverage.Covered)]
    public void Should_apply_the_tool_name_mapping_for_a_tool_that_is_not_part_of_the_request()
    {
        var mapping = ToolNames.CreateToolNameMapping(
            new[] { new MappedTool("provider", "my_tool", "test.mapped") },
            new Dictionary<string, string> { ["test.mapped"] = "mapped_tool" });

        var result = Prepare(Allowed(null, new[] { "my_tool" }, mapping), null, WeatherTool());

        AssertToolChoice(result, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"mapped_tool"}]}""");
    }
}
