// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; additional tools after ordinary tool results</c>.</summary>
public sealed class AdditionalToolsAfterOrdinaryToolResultsTests
{
    private const string Definition = "{\"type\":\"function\",\"name\":\"list_issues\",\"description\":\"List issues\",\"parameters\":{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false},\"strict\":true}";
    private const string CustomDefinition = "{\"type\":\"custom\",\"name\":\"apply_patch\",\"description\":\"Apply a patch\",\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"x\\\"\"}}";
    private const string NamespaceDefinition = "{\"type\":\"namespace\",\"name\":\"linear\",\"description\":\"Linear tools\",\"tools\":[{\"type\":\"function\",\"name\":\"get_issue\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"defer_loading\":true}]}";
    private const string Output = "[{\"connection\":\"linear\"},{\"connection\":\"github\",\"error\":\"Not authorized\"}]";

    private static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, string providerOptionsName = "openai") =>
        OpenAIResponsesLanguageModel.ConvertInput(prompt, "developer", false, null, providerOptionsName, false, false).Input;

    private static ToolModelMessage Result(string additionalTools, string provider = "openai", string toolCallId = "search-call") =>
        new(toolCallId, "search", Output, isError: false, outputType: "json",
            providerMetadata: JsonDocument.Parse("{\"" + provider + "\":{\"additionalTools\":" + additionalTools + "}}").RootElement.Clone());

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    [Theory]
    [InlineData("openai")]
    [InlineData("azure")]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::preserves the result and inserts definitions immediately after it for %s", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_the_result_and_inserts_definitions_immediately_after_it_for_provider(string provider)
    {
        var input = Convert(
            new ModelMessage[]
            {
                Result("[" + Definition + "]", provider),
                new ToolModelMessage("other-call", "other", "other result", isError: false, outputType: "text"),
            },
            provider);

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"search-call\",\"output\":" + JsonSerializer.Serialize(Output) + "},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition + "]},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"other-call\",\"output\":\"other result\"}]",
            input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::preserves multiple insertion points when replaying and extending history", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_multiple_insertion_points_when_replaying_and_extending_history()
    {
        var prompt = new List<ModelMessage> { Result("[" + Definition + "]") };
        var first = Convert(prompt);
        prompt.Add(new UserModelMessage("Search again"));
        prompt.Add(Result("[" + Definition.Replace("list_issues", "get_issue") + "]", toolCallId: "second-search"));
        var second = Convert(prompt);

        for (var index = 0; index < first.Count; index++)
        {
            Assert.True(JsonNode.DeepEquals(first[index], second[index]));
        }

        AssertJson(
            "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition.Replace("list_issues", "get_issue") + "]}",
            second[second.Count - 1]);
        Assert.True(JsonNode.DeepEquals(second, Convert(prompt)));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::does not emit an item for an empty list", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_emit_an_item_for_an_empty_list()
    {
        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"search-call\",\"output\":" + JsonSerializer.Serialize(Output) + "}]",
            Convert(new ModelMessage[] { Result("[]") }));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"invalid\"")]
    [InlineData("[{\"type\":\"function\",\"name\":\"missing_parameters\"}]")]
    [InlineData("[{\"type\":\"function\",\"name\":\"list_issues\",\"parameters\":{},\"strict\":\"yes\"}]")]
    [InlineData("[{\"type\":\"function\",\"name\":\"list_issues\",\"parameters\":{},\"unsupported\":true}]")]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::rejects malformed definitions: $additionalTools", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_malformed_definitions(string additionalTools)
    {
        Assert.ThrowsAny<Exception>(() => Convert(new ModelMessage[] { Result(additionalTools) }));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::reads openai options when using another provider name", Coverage = UpstreamCoverage.Covered, Commit = "a279e869252270b0c7ee9eff0fd263b7c17463b3")]
    public void Reads_openai_options_when_using_another_provider_name()
    {
        var input = Convert(new ModelMessage[] { Result("[" + Definition + "]", "openai") }, "azure");

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"search-call\",\"output\":" + JsonSerializer.Serialize(Output) + "},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition + "]}]",
            input);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::keeps definitions from expanded parallel results with %j", Coverage = UpstreamCoverage.Covered, Commit = "a279e869252270b0c7ee9eff0fd263b7c17463b3")]
    public void Keeps_definitions_from_expanded_parallel_results(bool hasConversation)
    {
        var prompt = ExpandedParallelToolCallPrompt.Create(
            additionalTools: index => JsonNode.Parse("[" + Definition.Replace("list_issues", "tool_" + index) + "]"));

        var input = ExpandedParallelToolCallPrompt.Convert(prompt, "developer", hasConversation, !hasConversation).Input;

        AssertJson(
            "[" + (hasConversation
                ? string.Empty
                : "{\"type\":\"function_call\",\"call_id\":\"call_parallel\",\"name\":\"parallel\",\"arguments\":" + JsonSerializer.Serialize(ExpandedParallelToolCallPrompt.ParallelToolCallInput) + "},")
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":" + JsonSerializer.Serialize(ExpandedParallelToolCallPrompt.ParallelToolCallOutput) + "},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition.Replace("list_issues", "tool_0") + "]},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition.Replace("list_issues", "tool_1") + "]}]",
            input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools after ordinary tool results::accepts custom and namespace definitions", Coverage = UpstreamCoverage.Covered, Commit = "6eff9587dfef6b92924ca8f6349dcb5ad8a8b39e")]
    public void Accepts_custom_and_namespace_definitions()
    {
        var tools = "[" + CustomDefinition + "," + NamespaceDefinition + "]";

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"search-call\",\"output\":" + JsonSerializer.Serialize(Output) + "},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":" + tools + "}]",
            Convert(new ModelMessage[] { Result(tools) }));
    }
}
