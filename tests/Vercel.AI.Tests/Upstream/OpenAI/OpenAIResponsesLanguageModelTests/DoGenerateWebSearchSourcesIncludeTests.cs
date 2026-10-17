// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; web search sources include</c>.</summary>
public sealed class DoGenerateWebSearchSourcesIncludeTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > web search sources include::";

    private static LanguageModelTool WebSearch(string id) =>
        LanguageModelTool.ProviderDefined(id, "web_search", JsonDocument.Parse("{}").RootElement.Clone());

    private static async Task<JsonNode> Request(string toolId, string? providerOptions, bool supportsInclude = true, bool mantle = false)
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"id\":\"resp_test\",\"created_at\":0,\"model\":\"m\",\"output\":[]}",
        };
        var callOptions = new LanguageModelCallOptions
        {
            Prompt = new List<ModelMessage> { new UserModelMessage("Question") },
            Tools = new[] { WebSearch(toolId) },
            ProviderOptions = providerOptions == null
                ? null
                : new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json(providerOptions) },
        };
        ILanguageModel model = mantle
            ? BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" }, capture).Responses("gpt-5")
            : new OpenAIResponsesLanguageModel(OpenAIUpstream.Provider(capture), "gpt-5") { SupportsWebSearchSourcesInclude = supportsInclude };
        await model.DoGenerateAsync(callOptions, CancellationToken.None);
        return JsonNode.Parse(capture.Body)!;
    }

    [Theory]
    [InlineData("openai.web_search")]
    [InlineData("openai.web_search_preview")]
    [UpstreamTest(Prefix + "should add web_search_call.action.sources to include when a web search tool is present", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_add_web_search_call_action_sources_to_include_when_a_web_search_tool_is_present(string toolId)
    {
        var body = await Request(toolId, null);
        OpenAIUpstream.Equal(body["include"], "[\"web_search_call.action.sources\"]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not add web_search_call.action.sources when includeWebSearchSources is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_add_web_search_call_action_sources_when_includeWebSearchSources_is_false()
    {
        var body = await Request("openai.web_search", "{\"includeWebSearchSources\":false}");
        Assert.Null(body["include"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not add web_search_call.action.sources when supportsWebSearchSourcesInclude is false", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_add_web_search_call_action_sources_when_supportsWebSearchSourcesInclude_is_false()
    {
        var body = await Request("openai.web_search", null, supportsInclude: false);
        Assert.Null(body["include"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not duplicate web_search_call.action.sources when already included", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_duplicate_web_search_call_action_sources_when_already_included()
    {
        var body = await Request("openai.web_search", "{\"include\":[\"web_search_call.action.sources\"]}");
        OpenAIUpstream.Equal(body["include"], "[\"web_search_call.action.sources\"]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should not add web_search_call.action.sources on the Bedrock Mantle responses model", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_add_web_search_call_action_sources_on_the_Bedrock_Mantle_responses_model()
    {
        var body = await Request("openai.web_search", null, mantle: true);
        Assert.Null(body["include"]);
    }
}
