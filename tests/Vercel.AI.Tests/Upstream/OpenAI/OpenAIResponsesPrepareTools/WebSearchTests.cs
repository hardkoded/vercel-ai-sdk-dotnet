// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class WebSearchTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should prepare web_search tool with no options", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_no_options()
    {
        var result = Prepare(null, ProviderTool("openai.web_search", "web_search", "{}"));

        AssertTools(result, "[{\"type\":\"web_search\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should prepare web_search tool with externalWebAccess set to true", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_externalWebAccess_set_to_true()
    {
        var result = Prepare(null, ProviderTool("openai.web_search", "web_search", "{\"externalWebAccess\":true}"));

        AssertTools(result, "[{\"type\":\"web_search\",\"external_web_access\":true}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should prepare web_search tool with externalWebAccess set to false", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_externalWebAccess_set_to_false()
    {
        var result = Prepare(null, ProviderTool("openai.web_search", "web_search", "{\"externalWebAccess\":false}"));

        AssertTools(result, "[{\"type\":\"web_search\",\"external_web_access\":false}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should prepare web_search tool with all options including externalWebAccess", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_all_options_including_externalWebAccess()
    {
        var result = Prepare(null, ProviderTool("openai.web_search", "web_search", "{\"externalWebAccess\":true,\"filters\":{\"allowedDomains\":[\"example.com\",\"test.org\"],\"blockedDomains\":[\"blocked.example\",\"blocked.test\"]},\"searchContextSize\":\"high\",\"userLocation\":{\"type\":\"approximate\",\"country\":\"US\",\"city\":\"San Francisco\",\"region\":\"California\",\"timezone\":\"America/Los_Angeles\"}}"));

        AssertTools(result, "[{\"type\":\"web_search\",\"external_web_access\":true,\"filters\":{\"allowed_domains\":[\"example.com\",\"test.org\"],\"blocked_domains\":[\"blocked.example\",\"blocked.test\"]},\"search_context_size\":\"high\",\"user_location\":{\"type\":\"approximate\",\"country\":\"US\",\"city\":\"San Francisco\",\"region\":\"California\",\"timezone\":\"America/Los_Angeles\"}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should prepare web_search tool with blocked domains", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_web_search_tool_with_blocked_domains()
    {
        var result = Prepare(null, ProviderTool("openai.web_search", "web_search", "{\"filters\":{\"blockedDomains\":[\"example.com\"]}}"));

        AssertTools(result, "[{\"type\":\"web_search\",\"filters\":{\"blocked_domains\":[\"example.com\"]}}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should handle tool choice selection with web_search", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_selection_with_web_search()
    {
        var result = Prepare(ToolChoice.Tool("web_search"), ProviderTool("openai.web_search", "web_search", "{\"externalWebAccess\":true}"));

        AssertTools(result, "[{\"type\":\"web_search\",\"external_web_access\":true}]");
        AssertToolChoice(result, "{\"type\":\"web_search\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > web search::should handle multiple tools including web_search", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_web_search()
    {
        var result = Prepare(null, FunctionTool(), ProviderTool("openai.web_search", "web_search", "{\"externalWebAccess\":false,\"searchContextSize\":\"medium\"}"));

        AssertTools(result, "[" + FunctionToolJson + ",{\"type\":\"web_search\",\"external_web_access\":false,\"search_context_size\":\"medium\"}]");
    }
}
