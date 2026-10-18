// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private static readonly JsonElement Breakpoint = JsonDocument.Parse("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}").RootElement;

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > tool messages::should preserve prompt cache breakpoints on scalar tool results",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_prompt_cache_breakpoints_on_scalar_tool_results()
    {
        var scalarOutputs = ScalarOutputs.Cases;
        var messages = new List<ModelMessage>();
        var expected = new JsonArray();
        for (var outputIndex = 0; outputIndex < scalarOutputs.Length; outputIndex++)
        {
            var (type, value, expectedText) = scalarOutputs[outputIndex];
            foreach (var placement in new[] { "output", "tool-result" })
            {
                var id = "call_" + outputIndex + "_" + placement;
                messages.Add(new ToolModelMessage(id, "lookup", value, isError: false, outputType: type, providerMetadata: placement == "tool-result" ? Breakpoint : null)
                {
                    OutputProviderOptions = placement == "output" ? Breakpoint : null,
                });
                expected.Add(new JsonObject
                {
                    ["type"] = "function_call_output",
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

        var result = OpenAIResponsesLanguageModel.ConvertInput(messages, "system", false, null, "openai", false, true);

        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > tool messages::should JSON-encode text outputs only for tools with an output schema",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_JSON_encode_text_outputs_only_for_tools_with_an_output_schema()
    {
        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[]
            {
                new ToolModelMessage("call_text", "search", "The weather is sunny", isError: false, outputType: "text"),
                new ToolModelMessage("call_error", "search", "Error: boom", isError: true, outputType: "error-text"),
                new ToolModelMessage("call_denied", "search", "User denied the tool execution", isError: true, outputType: "execution-denied"),
                new ToolModelMessage("call_without_schema", "lookup", "Error: unchanged", isError: true, outputType: "error-text"),
                new ToolModelMessage("call_schema_breakpoint", "search", "Structured output", isError: false, outputType: "text")
                {
                    OutputProviderOptions = Breakpoint,
                },
            },
            "system",
            false,
            null,
            "openai",
            false,
            true,
            outputSchemaToolNames: new HashSet<string> { "search" });

        var expected = new JsonArray(
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_text", ["output"] = "\"The weather is sunny\"" },
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_error", ["output"] = "{\"error\":\"Error: boom\"}" },
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_denied", ["output"] = "\"User denied the tool execution\"" },
            new JsonObject { ["type"] = "function_call_output", ["call_id"] = "call_without_schema", ["output"] = "{\"error\":\"Error: unchanged\"}" },
            new JsonObject
            {
                ["type"] = "function_call_output",
                ["call_id"] = "call_schema_breakpoint",
                ["output"] = new JsonArray(new JsonObject
                {
                    ["type"] = "input_text",
                    ["text"] = "\"Structured output\"",
                    ["prompt_cache_breakpoint"] = new JsonObject { ["mode"] = "explicit" },
                }),
            });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > tool messages::should reject execution-denied programmatic tool results",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_execution_denied_programmatic_tool_results()
    {
        var call = new GeneratedToolCall("call_denied_123", "search", """{"query":"test"}""");
        var assistant = new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantToolCallPart(
                call,
                new Dictionary<string, JsonElement>
                {
                    ["openai"] = JsonDocument.Parse("""{"caller":{"type":"program","callerId":"program_call_123"}}""").RootElement,
                }),
        });
        var denied = new ToolModelMessage("call_denied_123", "search", "User denied the tool execution", false, "execution-denied");

        Assert.Throws<UnsupportedFunctionalityException>(() => OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[] { assistant, denied }, "system", false, null, "openai", false, true));
    }
}

internal static class ScalarOutputs
{
    // Output type, output value, and the text the request carries.
    public static readonly (string Type, string Value, string ExpectedText)[] Cases =
    {
        ("text", "stable tool output", "stable tool output"),
        ("json", "{\"stable\":true}", "{\"stable\":true}"),
        ("error-text", "tool error", "{\"error\":\"tool error\"}"),
        ("error-json", "{\"error\":\"boom\"}", "{\"error\":{\"error\":\"boom\"}}"),
        ("execution-denied", "execution denied", "execution denied"),
    };
}
