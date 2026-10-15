// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; assistant messages &gt; reasoning models (store: true)</c>.</summary>
public sealed class ReasoningModelsStoreTrueTests
{
    private static IReadOnlyDictionary<string, JsonElement> ItemId(string id) =>
        new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse("{\"itemId\":\"" + id + "\"}").RootElement };

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > assistant messages > reasoning models (store: true)::should use item_reference for text parts preceded by reasoning",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_use_item_reference_for_text_parts_preceded_by_reasoning()
    {
        var prompt = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Thinking...", ItemId("rs_123")),
                new AssistantTextPart("Hello", ItemId("msg_123")),
            }),
        };

        var result = OpenAIResponsesLanguageModel.ConvertInput(prompt, "developer", false, null, "openai", false, true, isReasoningModel: true);

        var expected = new JsonArray(
            new JsonObject { ["type"] = "item_reference", ["id"] = "rs_123" },
            new JsonObject { ["type"] = "item_reference", ["id"] = "msg_123" });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > assistant messages > reasoning models (store: true)::should send text parts with content when their reasoning has been removed",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_send_text_parts_with_content_when_their_reasoning_has_been_removed()
    {
        var prompt = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello", ItemId("msg_123")) }),
        };

        var result = OpenAIResponsesLanguageModel.ConvertInput(prompt, "developer", false, null, "openai", false, true, isReasoningModel: true);

        var expected = new JsonArray(new JsonObject { ["role"] = "assistant", ["content"] = "Hello" });
        Assert.True(JsonNode.DeepEquals(expected, result.Input), result.Input.ToJsonString());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > assistant messages > reasoning models (store: true)::should skip text parts that already exist in the conversation",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_text_parts_that_already_exist_in_the_conversation()
    {
        var prompt = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello", ItemId("msg_123")) }),
        };

        var result = OpenAIResponsesLanguageModel.ConvertInput(prompt, "developer", false, null, "openai", false, true, isReasoningModel: true, hasConversation: true);

        Assert.Empty(result.Input);
    }
}
