// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; additional tools at a message position</c>.</summary>
public sealed class AdditionalToolsAtAMessagePositionTests
{
    private const string Definition = "{\"type\":\"function\",\"name\":\"list_issues\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"defer_loading\":true}";
    private const string CustomDefinition = "{\"type\":\"custom\",\"name\":\"apply_patch\",\"description\":\"Apply a patch\",\"format\":{\"type\":\"grammar\",\"syntax\":\"lark\",\"definition\":\"start: \\\"x\\\"\"}}";
    private const string NamespaceDefinition = "{\"type\":\"namespace\",\"name\":\"linear\",\"description\":\"Linear tools\",\"tools\":[{\"type\":\"function\",\"name\":\"get_issue\",\"parameters\":{\"type\":\"object\",\"properties\":{}},\"defer_loading\":true}]}";

    private static SystemModelMessage Control(string options, string provider = "openai", string content = "") =>
        new(content, new Dictionary<string, JsonElement> { [provider] = JsonDocument.Parse(options).RootElement.Clone() });

    private static JsonArray Convert(IReadOnlyList<ModelMessage> prompt, string providerOptionsName = "openai", string? configurationUpdateUnsupportedReason = null) =>
        OpenAIResponsesLanguageModel.ConvertInput(prompt, "developer", false, configurationUpdateUnsupportedReason, providerOptionsName, false, false).Input;

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    [Theory]
    [InlineData("openai")]
    [InlineData("azure")]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::emits the item where the control message is for %s", Coverage = UpstreamCoverage.Covered)]
    public void Emits_the_item_where_the_control_message_is_for_provider(string provider)
    {
        var tools = "[" + Definition + "," + CustomDefinition + "," + NamespaceDefinition + "]";
        var input = Convert(
            new ModelMessage[]
            {
                new UserModelMessage("Hello"),
                Control("{\"additionalTools\":" + tools + "}", provider),
                new UserModelMessage("Use them"),
            },
            provider);

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":" + tools + "},"
            + "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Use them\"}]}]",
            input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::emits a configuration update before tools from the same message", Coverage = UpstreamCoverage.Covered)]
    public void Emits_a_configuration_update_before_tools_from_the_same_message()
    {
        var input = Convert(new ModelMessage[] { Control("{\"reasoningEffortUpdate\":\"low\",\"additionalTools\":[" + Definition + "]}") });

        AssertJson(
            "[{\"type\":\"configuration_update\",\"reasoning\":{\"effort\":\"low\"}},"
            + "{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition + "]}]",
            input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::does not need configuration update support", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_need_configuration_update_support()
    {
        var input = Convert(
            new ModelMessage[] { Control("{\"additionalTools\":[" + Definition + "]}") },
            configurationUpdateUnsupportedReason: "not supported");

        AssertJson("[{\"type\":\"additional_tools\",\"role\":\"developer\",\"tools\":[" + Definition + "]}]", input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::does not emit an item for an empty list", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_emit_an_item_for_an_empty_list()
    {
        AssertJson("[]", Convert(new ModelMessage[] { Control("{\"additionalTools\":[]}") }));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::requires empty system message content", Coverage = UpstreamCoverage.Covered)]
    public void Requires_empty_system_message_content()
    {
        var exception = Assert.Throws<UnsupportedFunctionalityException>(() =>
            Convert(new ModelMessage[] { Control("{\"additionalTools\":[" + Definition + "]}", content: "Instructions") }));

        Assert.Equal("Message-level additionalTools", exception.Functionality);
        Assert.Equal("Message-level additionalTools requires empty system message content.", exception.Message);
    }

    [Theory]
    [InlineData("[{\"type\":\"namespace\",\"name\":\"ns\",\"description\":\"d\",\"tools\":[]}]")]
    [InlineData("[{\"type\":\"custom\",\"name\":\"c\",\"format\":{\"type\":\"grammar\"}}]")]
    [InlineData("[{\"type\":\"web_search\"}]")]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > additional tools at a message position::rejects malformed definitions: $additionalTools", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_malformed_definitions(string additionalTools)
    {
        Assert.ThrowsAny<Exception>(() => Convert(new ModelMessage[] { Control("{\"additionalTools\":" + additionalTools + "}") }));
    }
}
