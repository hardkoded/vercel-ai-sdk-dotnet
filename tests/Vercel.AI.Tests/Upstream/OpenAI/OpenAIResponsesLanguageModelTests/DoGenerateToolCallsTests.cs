// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doGenerate &gt; tool calls</c>.</summary>
public sealed class DoGenerateToolCallsTests
{
    private const string ParallelToolCallInput =
        """{"tool_uses":[{"recipient_name":"functions.weather","parameters":{"location":"San Francisco"}},{"recipient_name":"functions.cityAttractions","parameters":{"city":"Rome"}}]}""";

    private static JsonObject FunctionCall(string id, string callId, string name, string arguments) => new()
    {
        ["type"] = "function_call",
        ["id"] = id,
        ["call_id"] = callId,
        ["name"] = name,
        ["arguments"] = arguments,
        ["status"] = "completed",
    };

    private static JsonObject ToolCallsBody(string model, params JsonObject[] output) => new()
    {
        ["id"] = "resp_67c97c0203188190a025beb4a75242bc",
        ["object"] = "response",
        ["created_at"] = 1741257730,
        ["status"] = "completed",
        ["model"] = model,
        ["output"] = new JsonArray(output.Select(item => (JsonNode)item).ToArray()),
        ["usage"] = new JsonObject
        {
            ["input_tokens"] = 34,
            ["output_tokens"] = 0,
            ["output_tokens_details"] = new JsonObject { ["reasoning_tokens"] = 0 },
            ["total_tokens"] = 34,
        },
    };

    private static JsonObject DefaultBody(string model) => ToolCallsBody(
        model,
        FunctionCall("fc_67caf7f4c1ec8190b27edfb5580cfd31", "call_0NdsJqOS8N3J9l2p0p4WpYU9", "weather", """{"location":"San Francisco"}"""),
        FunctionCall("fc_67caf7f5071c81908209c2909c77af05", "call_gexo0HtjUfmAIW4gjNOgyrcr", "cityAttractions", """{"city":"San Francisco"}"""));

    private static string ParallelToolCallProviderMetadata(int index) =>
        """{"openai":{"parallelToolCall":{"itemId":"fc_parallel","toolCallId":"call_parallel","toolName":"parallel","input":"""
        + System.Text.Json.JsonSerializer.Serialize(ParallelToolCallInput)
        + ""","index":""" + index + ""","count":2}}}""";

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::should generate tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_generate_tool_calls()
    {
        var result = OpenAIResponsesSupport.Generate(DefaultBody("gpt-4o"), OpenAIResponsesSupport.TestTools);

        Assert.Collection(
            result.Content.OfType<GeneratedToolCall>(),
            call =>
            {
                Assert.Equal("""{"location":"San Francisco"}""", call.ArgumentsJson);
                JsonAssert.Equal(call.ProviderMetadata!.Value, """{"openai":{"itemId":"fc_67caf7f4c1ec8190b27edfb5580cfd31"}}""");
                Assert.Equal("call_0NdsJqOS8N3J9l2p0p4WpYU9", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
            },
            call =>
            {
                Assert.Equal("""{"city":"San Francisco"}""", call.ArgumentsJson);
                JsonAssert.Equal(call.ProviderMetadata!.Value, """{"openai":{"itemId":"fc_67caf7f5071c81908209c2909c77af05"}}""");
                Assert.Equal("call_gexo0HtjUfmAIW4gjNOgyrcr", call.ToolCallId);
                Assert.Equal("cityAttractions", call.ToolName);
            });
        Assert.Equal(2, result.Content.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::should expand an internal parallel tool call wrapper",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_expand_an_internal_parallel_tool_call_wrapper()
    {
        var result = OpenAIResponsesSupport.Generate(
            OpenAIResponsesSupport.FixtureBody("parallel-tool-call-wrapper.1.json"),
            OpenAIResponsesSupport.TestTools);

        Assert.Collection(
            result.Content.OfType<GeneratedToolCall>(),
            call =>
            {
                Assert.Equal("call_parallel_0", call.ToolCallId);
                Assert.Equal("weather", call.ToolName);
                Assert.Equal("""{"location":"San Francisco"}""", call.ArgumentsJson);
                JsonAssert.Equal(call.ProviderMetadata!.Value, ParallelToolCallProviderMetadata(0));
            },
            call =>
            {
                Assert.Equal("call_parallel_1", call.ToolCallId);
                Assert.Equal("cityAttractions", call.ToolName);
                Assert.Equal("""{"city":"Rome"}""", call.ArgumentsJson);
                JsonAssert.Equal(call.ProviderMetadata!.Value, ParallelToolCallProviderMetadata(1));
            });
        Assert.Equal(2, result.Content.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::should preserve async mode and namespace on function_call output",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_async_mode_and_namespace_on_function_call_output()
    {
        var item = FunctionCall("fc_ns_1", "call_ns_1", "get_weather", """{"location":"NYC"}""");
        item["async"] = true;
        item["namespace"] = "weather_ns";

        var result = OpenAIResponsesSupport.Generate(ToolCallsBody("gpt-5.4", item), OpenAIResponsesSupport.TestTools);

        var toolCall = result.Content.OfType<GeneratedToolCall>().First();
        var openai = toolCall.ProviderMetadata!.Value.GetProperty("openai");
        Assert.Equal("fc_ns_1", openai.GetProperty("itemId").GetString());
        Assert.True(openai.GetProperty("async").GetBoolean());
        Assert.Equal("weather_ns", openai.GetProperty("namespace").GetString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::should not set namespace when absent from function_call output",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_not_set_namespace_when_absent_from_function_call_output()
    {
        var result = OpenAIResponsesSupport.Generate(DefaultBody("gpt-4o"), OpenAIResponsesSupport.TestTools);

        var toolCall = result.Content.OfType<GeneratedToolCall>().First();
        Assert.False(toolCall.ProviderMetadata!.Value.GetProperty("openai").TryGetProperty("namespace", out _));
    }

    private const string Id = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doGenerate > tool calls::";

    private static readonly LanguageModelTool[] RequestTools =
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
        var request = Prepare("gpt-4o", RequestTools, """{"toolNames":["weather"],"mode":"auto"}""");

        Assert.Equal(new[] { "weather", "cityAttractions" }, request.Body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()));
        AssertToolChoice(request, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"weather"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should send derived allowed_tools entries for function, built-in and mcp tools", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_derived_allowed_tools_entries_for_function_built_in_and_mcp_tools()
    {
        var tools = RequestTools.Concat(new[]
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
        var request = Prepare("gpt-4o", RequestTools, """{"toolNames":["weather","cityAttractions"],"mode":"required"}""");

        AssertToolChoice(request, """{"type":"allowed_tools","mode":"required","tools":[{"type":"function","name":"weather"},{"type":"function","name":"cityAttractions"}]}""");
    }

    [Fact]
    [UpstreamTest(Id + "should override request-level toolChoice when allowedTools is set", Coverage = UpstreamCoverage.Covered)]
    public void Should_override_request_level_toolChoice_when_allowedTools_is_set()
    {
        var request = Prepare("gpt-4o", RequestTools, """{"toolNames":["weather"]}""", ToolChoice.Required);

        AssertToolChoice(request, """{"type":"allowed_tools","mode":"auto","tools":[{"type":"function","name":"weather"}]}""");
    }
}
