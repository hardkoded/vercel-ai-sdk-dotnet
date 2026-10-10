// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; tool messages &gt; custom tool: %s</c>.</summary>
public sealed class ToolMessagesCustomToolTests
{
    [Theory]
    [InlineData(false, "error-text", "E42", "{\"error\":\"E42\"}")]
    [InlineData(false, "error-json", "{\"code\":\"E42\"}", "{\"error\":{\"code\":\"E42\"}}")]
    [InlineData(true, "error-text", "E42", "{\"error\":\"E42\"}")]
    [InlineData(true, "error-json", "{\"code\":\"E42\"}", "{\"error\":{\"code\":\"E42\"}}")]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > tool messages > custom tool: %s::should wrap tool errors $output",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_wrap_tool_errors(bool isCustomTool, string outputType, string value, string expected)
    {
        foreach (var hasOutputSchema in new[] { false, true })
        {
            foreach (var hasBreakpoint in new[] { false, true })
            {
                var breakpoint = JsonDocument.Parse("{\"openai\":{\"promptCacheBreakpoint\":{\"mode\":\"explicit\"}}}").RootElement;
                var result = OpenAIResponsesLanguageModel.ConvertInput(
                    new ModelMessage[]
                    {
                        new ToolModelMessage("call_error", "deploy", value, isError: true, outputType: outputType, providerMetadata: hasBreakpoint ? breakpoint : null),
                    },
                    "system",
                    false,
                    null,
                    "openai",
                    false,
                    true,
                    customToolNames: isCustomTool ? new HashSet<string> { "deploy" } : null,
                    outputSchemaToolNames: hasOutputSchema ? new HashSet<string> { "deploy" } : null);

                JsonNode output = expected;
                if (hasBreakpoint)
                {
                    output = new JsonArray(new JsonObject
                    {
                        ["type"] = "input_text",
                        ["text"] = expected,
                        ["prompt_cache_breakpoint"] = new JsonObject { ["mode"] = "explicit" },
                    });
                }

                var expectedInput = new JsonArray(new JsonObject
                {
                    ["type"] = isCustomTool ? "custom_tool_call_output" : "function_call_output",
                    ["call_id"] = "call_error",
                    ["output"] = output,
                });
                Assert.True(JsonNode.DeepEquals(expectedInput, result.Input), result.Input.ToJsonString());
                Assert.Empty(result.Warnings);
            }
        }
    }
}
