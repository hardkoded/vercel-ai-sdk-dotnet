// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; hasConversation</c>.</summary>
public sealed class HasConversationTests
{
    private static IReadOnlyDictionary<string, JsonElement> ItemId(string id) =>
        new Dictionary<string, JsonElement>
        {
            ["openai"] = JsonDocument.Parse("{\"itemId\":\"" + id + "\"}").RootElement,
        };

    private static readonly string UserHello =
        "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]}";

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should send expanded parallel tool results back to the stored wrapper call",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_send_expanded_parallel_tool_results_back_to_the_stored_wrapper_call()
    {
        var result = Convert(Create(), hasConversation: true);

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":"
                + JsonSerializer.Serialize(ParallelToolCallOutput)
                + "}]",
            result.Input
        );
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should preserve scalar prompt cache breakpoints on expanded parallel tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_scalar_prompt_cache_breakpoints_on_expanded_parallel_tool_results()
    {
        var result = Convert(Create(withPromptCacheBreakpoints: true), hasConversation: true);

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":["
                + "{\"type\":\"input_text\",\"text\":\"{\\\"temperature\\\":72}\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}},"
                + "{\"type\":\"input_text\",\"text\":\"\\nColosseum\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]",
            result.Input
        );
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should skip assistant text messages with item IDs when hasConversation is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_assistant_text_messages_with_item_IDs_when_hasConversation_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Hi there!", ItemId("msg_existing_123")),
                }
            ),
            new UserModelMessage("What is the weather?"),
        };

        var result = Convert(prompt, hasConversation: true);

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Hello\"}]},"
                + "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"What is the weather?\"}]}]",
            result.Input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should skip assistant tool-call messages with item IDs when hasConversation is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_assistant_tool_call_messages_with_item_IDs_when_hasConversation_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("What is the weather?"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall(
                            "call_123",
                            "getWeather",
                            "{\"location\":\"San Francisco\"}"
                        ),
                        ItemId("fc_existing_456")
                    ),
                }
            ),
            new ToolModelMessage("call_123", "getWeather", "{\"temp\":72}", false),
        };

        var result = Convert(prompt, hasConversation: true);

        AssertJson(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"What is the weather?\"}]},"
                + "{\"type\":\"function_call_output\",\"call_id\":\"call_123\",\"output\":\"{\\\"temp\\\":72}\"}]",
            result.Input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should include assistant messages without item IDs when hasConversation is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_assistant_messages_without_item_IDs_when_hasConversation_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(
                new AssistantContentPart[] { new AssistantTextPart("Hi there!") }
            ),
        };

        var result = Convert(prompt, hasConversation: true);

        AssertJson(
            "[" + UserHello + ",{\"role\":\"assistant\",\"content\":\"Hi there!\"}]",
            result.Input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should include assistant messages with item IDs when hasConversation is false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_assistant_messages_with_item_IDs_when_hasConversation_is_false()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantTextPart("Hi there!", ItemId("msg_existing_123")),
                }
            ),
        };

        var result = Convert(prompt, hasConversation: false);

        AssertJson(
            "[" + UserHello + ",{\"type\":\"item_reference\",\"id\":\"msg_existing_123\"}]",
            result.Input
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should skip reasoning parts with item IDs when hasConversation is true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_reasoning_parts_with_item_IDs_when_hasConversation_is_true()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart("Let me think...", ItemId("reasoning_existing_789")),
                }
            ),
        };

        var result = Convert(prompt, hasConversation: true);

        AssertJson("[" + UserHello + "]", result.Input);
    }
}
