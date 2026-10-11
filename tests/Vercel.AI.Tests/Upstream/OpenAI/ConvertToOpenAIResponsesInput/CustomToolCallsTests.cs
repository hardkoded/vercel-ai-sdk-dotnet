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
