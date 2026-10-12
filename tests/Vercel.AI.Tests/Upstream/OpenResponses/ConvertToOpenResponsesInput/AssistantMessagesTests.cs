// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>
/// Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>convertToOpenResponsesInput &gt; assistant messages</c>.
/// An assistant message holds one text, one reasoning, and its tool calls, with no part order and no provider data on the text,
/// so the cases with several text parts, interleaved parts, text annotations, or a text item id are not ported (issue #364).
/// </summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix = "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::convertToOpenResponsesInput > assistant messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert messages with only a text part to output_text content", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_messages_with_only_a_text_part_to_output_text_content()
    {
        var result = Convert(AssistantText("Hello from assistant"));

        JsonAssert.Equal(result.Input, """
            [{"content":[{"text":"Hello from assistant","type":"output_text"}],"role":"assistant","type":"message"}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert reasoning parts to reasoning items", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_reasoning_parts_to_reasoning_items()
    {
        var result = Convert(new AssistantModelMessage(null, null, "Analyzing the problem step by step"));

        JsonAssert.Equal(result.Input, """
            [{"type":"reasoning","summary":[],"content":[{"type":"reasoning_text","text":"Analyzing the problem step by step"}]}]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve reasoning item provider data", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_reasoning_item_provider_data()
    {
        var result = Convert(
            "test-provider",
            new AssistantModelMessage(
                null,
                null,
                "safe summary",
                UpstreamChat.Bag("test-provider", """{"itemId":"rs_123","reasoningSummary":[{"type":"summary_text","text":"safe summary"}],"reasoningEncryptedContent":"encrypted-state"}""")));

        JsonAssert.Equal(result.Input, """
            [
              {
                "id":"rs_123",
                "type":"reasoning",
                "summary":[{"type":"summary_text","text":"safe summary"}],
                "content":[{"type":"reasoning_text","text":"safe summary"}],
                "encrypted_content":"encrypted-state"
              }
            ]
            """);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use an easy input message without generating an ID in strict mode", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_an_easy_input_message_without_generating_an_ID_in_strict_mode()
    {
        var result = ConvertStrict(AssistantText("Hello from assistant"));

        JsonAssert.Equal(result.Input, """[{"type":"message","role":"assistant","content":"Hello from assistant"}]""");
    }
}
