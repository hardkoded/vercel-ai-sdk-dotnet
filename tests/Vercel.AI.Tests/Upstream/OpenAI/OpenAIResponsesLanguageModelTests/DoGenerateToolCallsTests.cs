// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
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
}
