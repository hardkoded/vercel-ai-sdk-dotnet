// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doGenerate &gt; tools</c>.</summary>
public sealed class DoGenerateToolsTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doGenerate > tools::";

    private readonly UpstreamCapture _capture = new UpstreamCapture();
    private readonly LanguageModelGenerateResult _result;

    public DoGenerateToolsTests()
    {
        PrepareJsonFixtureResponse(_capture, "lmstudio-basic.1");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool(
                "get_weather",
                "Get the current weather for a location",
                UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"location\":{\"type\":\"string\",\"description\":\"The city and state\"}},\"required\":[\"location\"]}")),
            new LanguageModelTool(
                "search",
                "Search for information",
                UpstreamChat.Json("{\"type\":\"object\",\"properties\":{\"query\":{\"type\":\"string\"}},\"required\":[\"query\"]}"),
                strict: true),
            LanguageModelTool.ProviderDefined("openai.web_search", "web_search", UpstreamChat.Json("{}")),
            LanguageModelTool.ProviderDefined("openai.file_search", "file_search", UpstreamChat.Json("{\"vectorStoreIds\":[\"vs_123\"]}")),
        };
        _result = CreateModel(_capture).DoGenerateAsync(options, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_correct_request_body_with_tools()
    {
        AssertJson(
            Obj("{\"input\":[{\"content\":[{\"text\":\"Hello\",\"type\":\"input_text\"}],\"role\":\"user\",\"type\":\"message\"}],\"model\":\"gemma-7b-it\",\"tools\":[{\"description\":\"Get the current weather for a location\",\"name\":\"get_weather\",\"parameters\":{\"properties\":{\"location\":{\"description\":\"The city and state\",\"type\":\"string\"}},\"required\":[\"location\"],\"type\":\"object\"},\"type\":\"function\"},{\"description\":\"Search for information\",\"name\":\"search\",\"parameters\":{\"properties\":{\"query\":{\"type\":\"string\"}},\"required\":[\"query\"],\"type\":\"object\"},\"strict\":true,\"type\":\"function\"}]}"),
            RequestBody(_capture, 0));
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn for each unsupported provider-defined tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_for_each_unsupported_provider_defined_tool()
    {
        Assert.Equal(
            new[] { ("unsupported", "provider-defined tool openai.web_search"), ("unsupported", "provider-defined tool openai.file_search") },
            _result.Warnings.Select(warning => (warning.Type, warning.Message)).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should encode registered provider tools and preserve warnings for unregistered tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_encode_registered_provider_tools_and_preserve_warnings_for_unregistered_tools()
    {
        var capture = new UpstreamCapture();
        PrepareJsonFixtureResponse(capture, "lmstudio-basic.1");
        var options = TestPrompt();
        options.Tools = new[]
        {
            new LanguageModelTool("lookup", null, UpstreamChat.Json("{\"type\":\"object\",\"properties\":{}}")),
            LanguageModelTool.ProviderDefined("acme.document_search", "documentSearch", UpstreamChat.Json("{\"index\":\"docs\"}")),
            LanguageModelTool.ProviderDefined("acme.unregistered", "unregistered", UpstreamChat.Json("{}")),
        };
        options.ToolChoice = ToolChoice.Tool("documentSearch");

        var result = await CreateModel(capture, "gemma-7b-it", CreateDocumentSearchExtension(providerExecuted: true)).DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture, capture.Requests.Count - 1);
        AssertJson(
            Arr("[{\"type\":\"function\",\"name\":\"lookup\",\"parameters\":{\"type\":\"object\",\"properties\":{}}},{\"type\":\"acme:document_search\",\"name\":\"documentSearch\",\"index\":\"docs\"}]"),
            requestBody["tools"]);
        AssertJson(Obj("{\"type\":\"acme:document_search\",\"name\":\"documentSearch\"}"), requestBody["tool_choice"]);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal(("unsupported", "provider-defined tool acme.unregistered"), (warning.Type, warning.Message));
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn and omit a registered provider tool and its selected choice when it cannot be encoded", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_warn_and_omit_a_registered_provider_tool_and_its_selected_choice_when_it_cannot_be_encoded()
    {
        var capture = new UpstreamCapture();
        PrepareJsonFixtureResponse(capture, "lmstudio-basic.1");
        var extension = CreateDocumentSearchExtension(providerExecuted: true);
        extension.EncodeTool = _ => null;
        var options = TestPrompt();
        options.Tools = new[] { LanguageModelTool.ProviderDefined("acme.document_search", "documentSearch", UpstreamChat.Json("{}")) };
        options.ToolChoice = ToolChoice.Tool("documentSearch");

        var result = await CreateModel(capture, "gemma-7b-it", extension).DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture, capture.Requests.Count - 1);
        Assert.False(requestBody.ContainsKey("tools"));
        Assert.False(requestBody.ContainsKey("tool_choice"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal(("unsupported", "provider-defined tool acme.document_search"), (warning.Type, warning.Message));
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit a selected unregistered provider tool choice", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_a_selected_unregistered_provider_tool_choice()
    {
        var capture = new UpstreamCapture();
        PrepareJsonFixtureResponse(capture, "lmstudio-basic.1");
        var options = TestPrompt();
        options.Tools = new[] { LanguageModelTool.ProviderDefined("acme.unregistered", "unregistered", UpstreamChat.Json("{}")) };
        options.ToolChoice = ToolChoice.Tool("unregistered");

        var result = await CreateModel(capture).DoGenerateAsync(options, CancellationToken.None);

        var requestBody = RequestBody(capture, capture.Requests.Count - 1);
        Assert.False(requestBody.ContainsKey("tools"));
        Assert.False(requestBody.ContainsKey("tool_choice"));
        var warning = Assert.Single(result.Warnings);
        Assert.Equal(("unsupported", "provider-defined tool acme.unregistered"), (warning.Type, warning.Message));
    }
}
