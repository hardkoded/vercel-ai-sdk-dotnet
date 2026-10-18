// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; programmatic tool calling</c>.</summary>
public sealed class ProgrammaticToolCallingTests
{
    private const string P = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > programmatic tool calling::";

    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "program", "openai.programmatic_tool_calling") },
        new Dictionary<string, string> { ["openai.programmatic_tool_calling"] = "programmatic_tool_calling" });

    private static IReadOnlyDictionary<string, JsonElement> Options(string openaiObject) =>
        new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse(openaiObject).RootElement };

    [Fact]
    [UpstreamTest(P + "should preserve the program output item reference from provider metadata", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_the_program_output_item_reference_from_provider_metadata()
    {
        var output = new ToolModelMessage(
            "program_call_1",
            "program",
            """{"result":"{\"availableUnits\":42}","status":"completed"}""",
            false,
            "json",
            JsonDocument.Parse("""{"openai":{"itemId":"program_output_item_1"}}""").RootElement);
        var message = new AssistantModelMessage(new AssistantContentPart[] { new AssistantToolResultPart("program_call_1", "program", output) });

        var result = OpenAIResponsesLanguageModel.ConvertInput(new ModelMessage[] { message }, "system", false, null, "openai", false, true, toolNameMapping: Mapping);

        var expected = new JsonArray(new JsonObject { ["type"] = "item_reference", ["id"] = "program_output_item_1" });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(P + "should replay program items and preserve nested function caller linkage", Coverage = UpstreamCoverage.Covered)]
    public void Should_replay_program_items_and_preserve_nested_function_caller_linkage()
    {
        var programCall = new GeneratedToolCall(
            "program_call_1",
            "program",
            """{"code":"const value = await tools.get_inventory({ sku: \"A\" });","fingerprint":"fingerprint_1"}""")
        {
            ProviderExecuted = true,
        };
        var functionCall = new GeneratedToolCall("function_call_1", "get_inventory", """{"sku":"A"}""");
        var callerOptions = Options("""{"itemId":"function_item_1","caller":{"type":"program","callerId":"program_call_1"}}""");
        var first = new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantToolCallPart(programCall, Options("""{"itemId":"program_item_1"}""")),
            new AssistantToolCallPart(functionCall, callerOptions),
        });
        var toolResult = new ToolModelMessage(
            "function_call_1",
            "get_inventory",
            """{"sku":"A","availableUnits":42}""",
            false,
            "json",
            JsonDocument.Parse("""{"openai":{"caller":{"type":"program","callerId":"program_call_1"}}}""").RootElement);
        var programOutput = new ToolModelMessage(
            "program_call_1",
            "program",
            """{"result":"{\"availableUnits\":42}","status":"completed"}""",
            false,
            "json");
        var last = new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantToolResultPart("program_call_1", "program", programOutput, Options("""{"itemId":"program_output_item_1"}""")),
        });

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[] { first, toolResult, last },
            "system",
            false,
            null,
            "openai",
            false,
            false,
            toolNameMapping: Mapping);

        var expected = JsonNode.Parse("""
            [
              {"type":"program","id":"program_item_1","call_id":"program_call_1","code":"const value = await tools.get_inventory({ sku: \"A\" });","fingerprint":"fingerprint_1"},
              {"type":"function_call","call_id":"function_call_1","name":"get_inventory","arguments":"{\"sku\":\"A\"}","caller":{"type":"program","caller_id":"program_call_1"}},
              {"type":"function_call_output","call_id":"function_call_1","output":"{\"sku\":\"A\",\"availableUnits\":42}","caller":{"type":"program","caller_id":"program_call_1"}},
              {"type":"program_output","id":"program_output_item_1","call_id":"program_call_1","result":"{\"availableUnits\":42}","status":"completed"}
            ]
            """);
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }
}
