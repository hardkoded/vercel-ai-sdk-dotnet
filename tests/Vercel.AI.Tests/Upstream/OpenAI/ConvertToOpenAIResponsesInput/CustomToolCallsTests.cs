// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; custom tool calls</c>.</summary>
public sealed class CustomToolCallsTests
{
    private static readonly JsonElement Breakpoint = JsonDocument.Parse("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}").RootElement;

    private const string P = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > custom tool calls::";

    private static readonly HashSet<string> CustomNames = new() { "write_sql" };

    private static JsonElement ItemId(string itemId) => JsonDocument.Parse("{\"openai\":{\"itemId\":\"" + itemId + "\"}}").RootElement;

    private static OpenAIResponsesLanguageModel.ResponsesInput Convert(GeneratedToolCall call, JsonElement? providerOptions, bool store, ISet<string>? customToolNames)
    {
        var options = providerOptions?.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone());
        var message = new AssistantModelMessage(new AssistantContentPart[] { new AssistantToolCallPart(call, options) });
        return OpenAIResponsesLanguageModel.ConvertInput(new ModelMessage[] { message }, "system", false, null, "openai", false, store, customToolNames: customToolNames);
    }

    [Fact]
    [UpstreamTest(P + "should convert custom tool call to custom_tool_call input item", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_custom_tool_call_to_custom_tool_call_input_item()
    {
        var result = Convert(new GeneratedToolCall("call_custom_001", "write_sql", "\"SELECT * FROM users WHERE age > 25\""), null, true, CustomNames);

        var expected = new JsonArray(new JsonObject
        {
            ["type"] = "custom_tool_call",
            ["call_id"] = "call_custom_001",
            ["name"] = "write_sql",
            ["input"] = "SELECT * FROM users WHERE age > 25",
        });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(P + "should JSON.stringify non-string custom tool call input", Coverage = UpstreamCoverage.Covered)]
    public void Should_JSON_stringify_non_string_custom_tool_call_input()
    {
        var result = Convert(new GeneratedToolCall("call_custom_002", "write_sql", "{ \"query\": \"test\" }"), null, true, CustomNames);

        var expected = new JsonArray(new JsonObject
        {
            ["type"] = "custom_tool_call",
            ["call_id"] = "call_custom_002",
            ["name"] = "write_sql",
            ["input"] = "{\"query\":\"test\"}",
        });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(P + "should convert custom tool call with itemId to item_reference when store: true", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_custom_tool_call_with_itemId_to_item_reference_when_store_true()
    {
        var result = Convert(new GeneratedToolCall("call_custom_003", "write_sql", "\"SELECT 1\""), ItemId("ct_ref_123"), true, CustomNames);

        var expected = new JsonArray(new JsonObject { ["type"] = "item_reference", ["id"] = "ct_ref_123" });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(P + "should not emit custom_tool_call when customProviderToolNames is not provided", Coverage = UpstreamCoverage.Covered)]
    public void Should_not_emit_custom_tool_call_when_customProviderToolNames_is_not_provided()
    {
        var result = Convert(new GeneratedToolCall("call_custom_001", "write_sql", "\"SELECT 1\""), null, true, null);

        var expected = new JsonArray(new JsonObject
        {
            ["type"] = "function_call",
            ["call_id"] = "call_custom_001",
            ["name"] = "write_sql",
            ["arguments"] = "\"SELECT 1\"",
        });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > custom tool calls::should preserve prompt cache breakpoints on scalar custom tool results",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_prompt_cache_breakpoints_on_scalar_custom_tool_results()
    {
        var scalarOutputs = ScalarOutputs.Cases;
        var messages = new List<ModelMessage>();
        var expected = new JsonArray();
        for (var outputIndex = 0; outputIndex < scalarOutputs.Length; outputIndex++)
        {
            var (type, value, expectedText) = scalarOutputs[outputIndex];
            foreach (var placement in new[] { "output", "tool-result" })
            {
                var id = "call_custom_" + outputIndex + "_" + placement;
                messages.Add(new ToolModelMessage(id, "write_sql", value, isError: false, outputType: type, providerMetadata: placement == "tool-result" ? Breakpoint : null)
                {
                    OutputProviderOptions = placement == "output" ? Breakpoint : null,
                });
                expected.Add(new JsonObject
                {
                    ["type"] = "custom_tool_call_output",
                    ["call_id"] = id,
                    ["output"] = new JsonArray(new JsonObject
                    {
                        ["type"] = "input_text",
                        ["text"] = expectedText,
                        ["prompt_cache_breakpoint"] = new JsonObject { ["mode"] = "explicit" },
                    }),
                });
            }
        }

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            messages, "system", false, null, "openai", false, true,
            customToolNames: new HashSet<string> { "write_sql" });

        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
        Assert.Empty(result.Warnings);
    }
}
