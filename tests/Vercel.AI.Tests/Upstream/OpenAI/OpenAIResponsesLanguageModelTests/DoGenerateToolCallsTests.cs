// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>
/// Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; tool calls</c>:
/// the tests about the request tools. The tests that parse tool calls from a response are not ported yet.
/// </summary>
public sealed class DoGenerateToolCallsTests
{
    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::";

    private static readonly LanguageModelTool[] TestTools =
    {
        Tool("weather", """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}""", null),
        Tool("cityAttractions", """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false}""", null),
    };

    private static LanguageModelTool Tool(string name, string schemaJson, string? openaiOptionsJson)
    {
        using var schema = JsonDocument.Parse(schemaJson);
        if (openaiOptionsJson == null)
        {
            return new LanguageModelTool(name, null, schema.RootElement.Clone());
        }

        using var options = JsonDocument.Parse("{\"openai\":" + openaiOptionsJson + "}");
        return new LanguageModelTool(name, null, schema.RootElement.Clone()) { ProviderOptions = options.RootElement.Clone() };
    }

    private static LanguageModelTool ProviderTool(string id, string name, string argsJson)
    {
        using var args = JsonDocument.Parse(argsJson);
        return LanguageModelTool.ProviderDefined(id, name, args.RootElement.Clone());
    }

    private static OpenAIResponsesPreparedRequest Prepare(string modelId, IReadOnlyList<LanguageModelTool> tools, string? allowedToolsJson = null, ToolChoice? toolChoice = null)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Tools = tools,
            ToolChoice = toolChoice,
        };
        if (allowedToolsJson != null)
        {
            using var providerOptions = JsonDocument.Parse("{\"allowedTools\":" + allowedToolsJson + "}");
            options.ProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = providerOptions.RootElement.Clone() };
        }

        return OpenAIResponsesLanguageModel.Prepare(modelId, options, false);
    }

    private static void AssertToolChoice(OpenAIResponsesPreparedRequest request, string expectedJson)
    {
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), request.Body["tool_choice"]),
            "Expected " + expectedJson + " but was " + request.Body["tool_choice"]?.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Id + "should gate async tools to GPT-6 and later models", Coverage = UpstreamCoverage.Covered)]
    public void Should_gate_async_tools_to_GPT_6_and_later_models()
    {
        var asyncTools = new[] { Tool("weather", """{"type":"object"}""", """{"async":true}""") };

        var unsupported = Prepare("gpt-5.6", asyncTools);

        Assert.Null(unsupported.Body["tools"]![0]!["async"]);
        var warning = Assert.Single(unsupported.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("async tool calling for \"weather\"", warning.Feature);
        Assert.Equal("Async tool calling is only supported by GPT-6 and later models.", warning.Details);

        var supported = Prepare("gpt-99", asyncTools);

        Assert.True(supported.Body["tools"]![0]!["async"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(Id + "should JSON-encode error outputs for tools with an output schema", Coverage = UpstreamCoverage.Covered)]
    public void Should_JSON_encode_error_outputs_for_tools_with_an_output_schema()
    {
        const string OutputSchema = """{"type":"object","properties":{"temperature":{"type":"number"}},"required":["temperature"],"additionalProperties":false}""";
        var request = OpenAIResponsesLanguageModel.Prepare(
            "gpt-4o",
            new LanguageModelCallOptions
            {
                Prompt = new ModelMessage[]
                {
                    new AssistantModelMessage(null, new[] { new GeneratedToolCall("call_123", "weather", """{"location":"San Francisco"}""") }, null),
                    new ToolModelMessage("call_123", "weather", "Error: boom", isError: true, outputType: "error-text"),
                },
                Tools = new[] { Tool("weather", """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}""", $$"""{"outputSchema":{{OutputSchema}}}""") },
            },
            false);

        var input = request.Body["input"]!.AsArray();
        Assert.Equal("function_call", input[0]!["type"]!.GetValue<string>());
        Assert.Equal("call_123", input[0]!["call_id"]!.GetValue<string>());
        Assert.Equal("weather", input[0]!["name"]!.GetValue<string>());
        Assert.Equal("""{"location":"San Francisco"}""", input[0]!["arguments"]!.GetValue<string>());
        Assert.Equal("function_call_output", input[1]!["type"]!.GetValue<string>());
        Assert.Equal("call_123", input[1]!["call_id"]!.GetValue<string>());
        Assert.Equal("""{"error":"Error: boom"}""", input[1]!["output"]!.GetValue<string>());
        var tool = request.Body["tools"]![0]!;
        Assert.Equal("function", tool["type"]!.GetValue<string>());
        Assert.Equal("weather", tool["name"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(OutputSchema), tool["output_schema"]));
    }

    [Fact]
    [UpstreamTest(Id + "should send full tools list and allowed_tools tool_choice when allowedTools provider option is set", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_full_tools_list_and_allowed_tools_tool_choice_when_allowedTools_provider_option_is_set()
    {
        var request = Prepare("gpt-4o", TestTools, """{"toolNames":["weather"],"mode":"auto"}""");

        Assert.Equal(new[] { "weather", "cityAttractions" }, request.Body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()));
        AssertToolChoice(request, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"weather"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should send derived allowed_tools entries for function, built-in and mcp tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_derived_allowed_tools_entries_for_function_built_in_and_mcp_tools()
    {
        var tools = TestTools.Concat(new[]
        {
            ProviderTool("openai.web_search", "search", "{}"),
            ProviderTool("openai.mcp", "deepwiki", """{"serverLabel":"deepwiki","serverUrl":"https://mcp.deepwiki.com/mcp"}"""),
        }).ToArray();

        var request = Prepare("gpt-4o", tools, """{"toolNames":["weather","search","deepwiki"]}""");

        Assert.Equal(
            new[] { "weather", "cityAttractions", "web_search", "mcp" },
            request.Body["tools"]!.AsArray().Select(tool => tool!["name"]?.GetValue<string>() ?? tool["type"]!.GetValue<string>()));
        AssertToolChoice(request, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"weather"},{"type":"web_search"},{"type":"mcp","server_label":"deepwiki"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should send allowed_tools with required mode when allowedTools.mode is required", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_allowed_tools_with_required_mode_when_allowedTools_mode_is_required()
    {
        var request = Prepare("gpt-4o", TestTools, """{"toolNames":["weather","cityAttractions"],"mode":"required"}""");

        AssertToolChoice(request, """{"type":"allowed_tools","mode":"required","tools":[{"type":"function","name":"weather"},{"type":"function","name":"cityAttractions"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should override request-level toolChoice when allowedTools is set", Coverage = UpstreamCoverage.Covered)]
    public void Should_override_request_level_toolChoice_when_allowedTools_is_set()
    {
        var request = Prepare("gpt-4o", TestTools, """{"toolNames":["weather"]}""", ToolChoice.Required);

        AssertToolChoice(request, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"weather"}]}""");
    }
}
