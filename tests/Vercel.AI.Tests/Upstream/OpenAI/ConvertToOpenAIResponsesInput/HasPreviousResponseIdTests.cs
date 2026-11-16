// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; hasPreviousResponseId</c>.</summary>
public sealed class HasPreviousResponseIdTests
{
    private static IReadOnlyDictionary<string, JsonElement> ItemId(string id) =>
        new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse("{\"itemId\":\"" + id + "\"}").RootElement };

    private static readonly string UserHello = "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}";

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasPreviousResponseId::should reconstruct expanded parallel tool calls with one wrapper output", Coverage = UpstreamCoverage.Covered)]
    public void Should_reconstruct_expanded_parallel_tool_calls_with_one_wrapper_output()
    {
        var result = Convert(Create(), hasPreviousResponseId: true);

        AssertJson(
            "[{\"type\":\"function_call\",\"call_id\":\"call_parallel\",\"name\":\"parallel\",\"arguments\":" + JsonSerializer.Serialize(ParallelToolCallInput) + "},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":" + JsonSerializer.Serialize(ParallelToolCallOutput) + "}]",
            result.Input);
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasPreviousResponseId::should keep client-executed function calls paired with their outputs when hasPreviousResponseId is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_keep_client_executed_function_calls_paired_with_their_outputs_when_hasPreviousResponseId_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantTextPart("Hi there!", ItemId("msg_existing_123")),
                new AssistantToolCallPart(new GeneratedToolCall("call_123", "getWeather", "{\"location\":\"San Francisco\"}"), ItemId("fc_existing_456")),
            }),
            new ToolModelMessage("call_123", "getWeather", "{\"temp\":72}", false),
        };

        var result = Convert(prompt, hasPreviousResponseId: true);

        AssertJson(
            "[" + UserHello + ",{\"type\":\"item_reference\",\"id\":\"msg_existing_123\"},"
            + "{\"type\":\"function_call\",\"call_id\":\"call_123\",\"name\":\"getWeather\",\"arguments\":\"{\\\"location\\\":\\\"San Francisco\\\"}\"},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_123\",\"output\":\"{\\\"temp\\\":72}\"}]",
            result.Input);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasPreviousResponseId::should skip reasoning parts with item IDs when hasPreviousResponseId is true", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_reasoning_parts_with_item_IDs_when_hasPreviousResponseId_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantReasoningPart("Let me think...", ItemId("rs_existing_789")) }),
        };

        var result = Convert(prompt, hasPreviousResponseId: true);

        AssertJson("[" + UserHello + "]", result.Input);
    }
}
