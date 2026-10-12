// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.ConvertToOpenResponsesInputTests.ConvertToOpenResponsesInputHelpers;

namespace Vercel.AI.Tests.ConvertToOpenResponsesInputTests;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > assistant messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert messages with only a text part to output_text content", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_messages_with_only_a_text_part_to_output_text_content()
    {
        var result = Convert(Assistant(new AssistantTextPart("Hello from assistant")));

        AssertInput("[{\"content\":[{\"text\":\"Hello from assistant\",\"type\":\"output_text\"}],\"role\":\"assistant\",\"type\":\"message\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert messages with multiple text parts", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_messages_with_multiple_text_parts()
    {
        var result = Convert(Assistant(new AssistantTextPart("First part"), new AssistantTextPart("Second part")));

        AssertInput(
            "[{\"content\":[{\"text\":\"First part\",\"type\":\"output_text\"},{\"text\":\"Second part\",\"type\":\"output_text\"}],\"role\":\"assistant\",\"type\":\"message\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert reasoning parts to reasoning items", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_reasoning_parts_to_reasoning_items()
    {
        var result = Convert(Assistant(new AssistantReasoningPart("Analyzing the problem step by step")));

        AssertInput(
            "[{\"type\":\"reasoning\",\"summary\":[],\"content\":[{\"type\":\"reasoning_text\",\"text\":\"Analyzing the problem step by step\"}]}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve interleaved assistant content order", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_interleaved_assistant_content_order()
    {
        var result = Convert(Assistant(
            new AssistantReasoningPart("Analyzing the problem"),
            Call("call_123", "search", "{}"),
            new AssistantTextPart("Answer after the call")));

        Assert.Equal(
            new[] { "reasoning", "function_call", "message" },
            result.Input.Select(item => (string?)item!["type"]).ToArray());
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve reasoning item provider data", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_reasoning_item_provider_data()
    {
        var result = Convert(
            "test-provider",
            false,
            Assistant(new AssistantReasoningPart(
                "safe summary",
                UpstreamChat.Bag(
                    "test-provider",
                    "{\"itemId\":\"rs_123\",\"reasoningSummary\":[{\"type\":\"summary_text\",\"text\":\"safe summary\"}],\"reasoningEncryptedContent\":\"encrypted-state\"}"))));

        AssertInput(
            "[{\"id\":\"rs_123\",\"type\":\"reasoning\",\"summary\":[{\"type\":\"summary_text\",\"text\":\"safe summary\"}],"
            + "\"content\":[{\"type\":\"reasoning_text\",\"text\":\"safe summary\"}],\"encrypted_content\":\"encrypted-state\"}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve output text annotations from provider data", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_output_text_annotations_from_provider_data()
    {
        var result = Convert(
            "test-provider",
            false,
            Assistant(new AssistantTextPart(
                "Sourced answer",
                UpstreamChat.Bag(
                    "test-provider",
                    "{\"itemId\":\"msg_123\",\"annotations\":[{\"type\":\"url_citation\",\"start_index\":0,\"end_index\":7,\"url\":\"https://example.com/source\",\"title\":\"Example source\"}]}"))));

        AssertInput(
            "[{\"id\":\"msg_123\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"output_text\",\"text\":\"Sourced answer\","
            + "\"annotations\":[{\"type\":\"url_citation\",\"start_index\":0,\"end_index\":7,\"url\":\"https://example.com/source\",\"title\":\"Example source\"}]}]}]",
            result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use an easy input message without generating an ID in strict mode", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_an_easy_input_message_without_generating_an_ID_in_strict_mode()
    {
        var result = Convert("open-responses", true, Assistant(new AssistantTextPart("Hello from assistant")));

        AssertInput("[{\"type\":\"message\",\"role\":\"assistant\",\"content\":\"Hello from assistant\"}]", result);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve a genuine item ID as a complete output message in strict mode", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_a_genuine_item_ID_as_a_complete_output_message_in_strict_mode()
    {
        var result = Convert(
            "test-provider",
            true,
            Assistant(new AssistantTextPart("Hello from assistant", UpstreamChat.Bag("test-provider", "{\"itemId\":\"msg_123\"}"))));

        AssertInput(
            "[{\"id\":\"msg_123\",\"type\":\"message\",\"status\":\"completed\",\"role\":\"assistant\","
            + "\"content\":[{\"type\":\"output_text\",\"text\":\"Hello from assistant\",\"annotations\":[],\"logprobs\":[]}]}]",
            result);
    }
}
