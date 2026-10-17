// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ToolSearchTests
{
    private const string Id = Suite + " > tool search::";

    private const string WeatherSchema = """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}""";
    private const string CustomerSchema = """{"type":"object","properties":{"customer_id":{"type":"string"}},"required":["customer_id"],"additionalProperties":false}""";
    private const string CrmNamespace = "\"namespace\":{\"name\":\"crm\",\"description\":\"CRM tools for customer lookup and order management.\"}";

    [Fact]
    [UpstreamTest(Id + "should prepare tool_search tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_tool_search_tool()
    {
        var result = Prepare(null, ProviderTool("openai.tool_search", "toolSearch", "{}"));

        AssertTools(result, """[{"type":"tool_search"}]""");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should prepare tool_search alongside function tools with defer_loading", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_tool_search_alongside_function_tools_with_defer_loading()
    {
        var result = Prepare(
            null,
            ProviderTool("openai.tool_search", "toolSearch", "{}"),
            Function("get_weather", "Get the current weather", WeatherSchema, """{"deferLoading":true}"""));

        AssertTools(result, $$"""[{"type":"tool_search"},{"type":"function","name":"get_weather","description":"Get the current weather","parameters":{{WeatherSchema}},"strict":false,"defer_loading":true}]""");
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should group function tools by OpenAI namespace provider option", Coverage = UpstreamCoverage.Covered)]
    public void Should_group_function_tools_by_OpenAI_namespace_provider_option()
    {
        var result = Prepare(
            null,
            ProviderTool("openai.tool_search", "toolSearch", "{}"),
            Function("get_customer_profile", "Fetch a customer profile by customer ID.", CustomerSchema, "{" + CrmNamespace + "}"),
            Function("get_weather", "Get the current weather", WeatherSchema),
            Function("list_open_orders", "List open orders for a customer ID.", CustomerSchema, "{\"deferLoading\":true," + CrmNamespace + "}", strict: true));

        AssertTools(
            result,
            $$"""
            [{"type":"tool_search"},
             {"type":"namespace","name":"crm","description":"CRM tools for customer lookup and order management.","tools":[
               {"type":"function","name":"get_customer_profile","description":"Fetch a customer profile by customer ID.","parameters":{{CustomerSchema}},"strict":false},
               {"type":"function","name":"list_open_orders","description":"List open orders for a customer ID.","parameters":{{CustomerSchema}},"strict":true,"defer_loading":true}]},
             {"type":"function","name":"get_weather","description":"Get the current weather","parameters":{{WeatherSchema}},"strict":false}]
            """);
        AssertNoToolChoice(result);
    }

    [Fact]
    [UpstreamTest(Id + "should reject conflicting descriptions for the same OpenAI namespace", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_conflicting_descriptions_for_the_same_OpenAI_namespace()
    {
        var error = Assert.Throws<UnsupportedFunctionalityException>(() => Prepare(
            null,
            Function("get_customer_profile", "Fetch a customer profile by customer ID.", openaiOptionsJson: """{"namespace":{"name":"crm","description":"CRM tools."}}"""),
            Function("list_open_orders", "List open orders for a customer ID.", openaiOptionsJson: """{"namespace":{"name":"crm","description":"Different CRM tools."}}""")));

        Assert.Contains("conflicting descriptions for OpenAI tool namespace \"crm\"", error.Message);
    }
}
