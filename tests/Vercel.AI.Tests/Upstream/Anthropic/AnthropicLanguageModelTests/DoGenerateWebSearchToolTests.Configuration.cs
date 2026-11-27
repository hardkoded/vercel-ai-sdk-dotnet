// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; web search tool</c>.</summary>
public sealed partial class DoGenerateWebSearchToolTests
{
    private const string ConfigurationPrefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > web search tool::";

    private static LanguageModelCallOptions WebSearchOptions(string id, string name, string args)
    {
        var options = AnthropicParity.Hello("What is the latest news?");
        options.Tools = new[]
        {
            LanguageModelTool.ProviderDefined(id, name, AnthropicParity.Json(args)),
        };
        return options;
    }

    private static CaptureHandler WebSearchTextResponse(string text)
    {
        return AnthropicParity.Ok(
            "{\"type\":\"message\",\"id\":\"msg_test\",\"content\":[{\"type\":\"text\",\"text\":\""
                + text
                + "\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}"
        );
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix
            + "should enable server-side web search when using anthropic.tools.webSearch_20250305",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_enable_server_side_web_search_when_using_anthropic_tools_webSearch_20250305()
    {
        var handler = WebSearchTextResponse("Here are the latest quantum computing breakthroughs.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":3,\"allowedDomains\":[\"arxiv.org\",\"nature.com\",\"mit.edu\"]}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20250305\",\"name\":\"web_search\",\"max_uses\":3,\"allowed_domains\":[\"arxiv.org\",\"nature.com\",\"mit.edu\"]}",
            requestBody["tools"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix
            + "should enable server-side web search when using anthropic.tools.webSearch_20260209",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_enable_server_side_web_search_when_using_anthropic_tools_webSearch_20260209()
    {
        var handler = WebSearchTextResponse("Here are the latest quantum computing breakthroughs.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20260209",
                "web_search",
                "{\"maxUses\":3,\"allowedDomains\":[\"arxiv.org\",\"nature.com\",\"mit.edu\"]}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20260209\",\"name\":\"web_search\",\"max_uses\":3,\"allowed_domains\":[\"arxiv.org\",\"nature.com\",\"mit.edu\"]}",
            requestBody["tools"]![0]
        );
        Assert.Equal("code-execution-web-tools-2026-02-09", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix + "should use web_search_20260318 with response inclusion and map custom tool names",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_web_search_20260318_with_response_inclusion_and_map_custom_tool_names()
    {
        var handler = AnthropicParity.Ok(
            "{\"type\":\"message\",\"id\":\"msg_test\",\"content\":[{\"type\":\"server_tool_use\",\"id\":\"srvtoolu_code\",\"name\":\"code_execution\",\"input\":{\"code\":\"search(\\\"AI SDK\\\")\"},\"caller\":{\"type\":\"direct\"}},{\"type\":\"server_tool_use\",\"id\":\"srvtoolu_search\",\"name\":\"web_search\",\"input\":{\"query\":\"AI SDK\"},\"caller\":{\"type\":\"code_execution_20260120\",\"tool_id\":\"srvtoolu_code\"}}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":10,\"output_tokens\":20}}"
        );

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20260318",
                "research",
                "{\"maxUses\":3,\"responseInclusion\":\"excluded\"}"
            )
        );

        LanguageModelTestSupport.AssertJson(
            "[{\"type\":\"web_search_20260318\",\"name\":\"web_search\",\"max_uses\":3,\"response_inclusion\":\"excluded\"}]",
            LanguageModelTestSupport.RequestBody(handler)["tools"]
        );
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
        Assert.Equal(2, result.Content.Count);
        var codeExecution = Assert.IsType<GeneratedToolCall>(result.Content[0]);
        Assert.Equal("code_execution", codeExecution.ToolName);
        Assert.True(codeExecution.Dynamic);
        var research = Assert.IsType<GeneratedToolCall>(result.Content[1]);
        Assert.Equal("research", research.ToolName);
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix + "should pass web search configuration with blocked domains",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_web_search_configuration_with_blocked_domains()
    {
        var handler = WebSearchTextResponse("Here are the latest stock market trends.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":2,\"blockedDomains\":[\"reddit.com\"]}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20250305\",\"name\":\"web_search\",\"max_uses\":2,\"blocked_domains\":[\"reddit.com\"]}",
            requestBody["tools"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix + "should handle web search with user location",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_web_search_with_user_location()
    {
        var handler = WebSearchTextResponse("Here are local tech events.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":1,\"userLocation\":{\"type\":\"approximate\",\"city\":\"New York\",\"region\":\"New York\",\"country\":\"US\",\"timezone\":\"America/New_York\"}}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20250305\",\"name\":\"web_search\",\"max_uses\":1,\"user_location\":{\"type\":\"approximate\",\"city\":\"New York\",\"region\":\"New York\",\"country\":\"US\",\"timezone\":\"America/New_York\"}}",
            requestBody["tools"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix + "should handle web search with partial user location (city + country)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_web_search_with_partial_user_location_city_plus_country()
    {
        var handler = WebSearchTextResponse("Here are local events.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":1,\"userLocation\":{\"type\":\"approximate\",\"city\":\"London\",\"country\":\"GB\"}}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20250305\",\"name\":\"web_search\",\"max_uses\":1,\"user_location\":{\"type\":\"approximate\",\"city\":\"London\",\"country\":\"GB\"}}",
            requestBody["tools"]![0]
        );
    }

    [Fact]
    [UpstreamTest(
        ConfigurationPrefix + "should handle web search with minimal user location (country only)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_web_search_with_minimal_user_location_country_only()
    {
        var handler = WebSearchTextResponse("Here are global events.");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            WebSearchOptions(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":1,\"userLocation\":{\"type\":\"approximate\",\"country\":\"US\"}}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Single(requestBody["tools"]!.AsArray());
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"web_search_20250305\",\"name\":\"web_search\",\"max_uses\":1,\"user_location\":{\"type\":\"approximate\",\"country\":\"US\"}}",
            requestBody["tools"]![0]
        );
    }
}
