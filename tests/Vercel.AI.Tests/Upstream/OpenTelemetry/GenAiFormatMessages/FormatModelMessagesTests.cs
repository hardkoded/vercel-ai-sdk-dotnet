// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Formatter = Vercel.AI.OpenTelemetry.GenAiFormatMessages;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatModelMessages</c>.</summary>
public sealed class FormatModelMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert a prompt string to a user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_prompt_string_to_a_user_message()
    {
        OtelJson.Equal(
            """
            [{"parts":[{"content":"Hello","type":"text"}],"role":"user"}]
            """,
            Formatter.FormatModelMessages("Hello", (IReadOnlyList<ModelMessage>?)null)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert ModelMessage array from prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_ModelMessage_array_from_prompt()
    {
        IReadOnlyList<ModelMessage> prompt =
        [
            new UserModelMessage("Hi there"),
            new AssistantModelMessage([
                new AssistantToolCallPart(
                    new GeneratedToolCall("tc1", "weather", "{\"city\":\"NYC\"}")
                ),
            ]),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"Hi there","type":"text"}],"role":"user"},{"parts":[{"arguments":{"city":"NYC"},"id":"tc1","name":"weather","type":"tool_call"}],"role":"assistant"}]
            """,
            Formatter.FormatModelMessages(prompt, (IReadOnlyList<ModelMessage>?)null)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should preserve system messages in message order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_system_messages_in_message_order()
    {
        IReadOnlyList<ModelMessage> messages =
        [
            new UserModelMessage("First"),
            new SystemModelMessage("Be helpful"),
            new UserModelMessage("Second"),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"First","type":"text"}],"role":"user"},{"parts":[{"content":"Be helpful","type":"text"}],"role":"system"},{"parts":[{"content":"Second","type":"text"}],"role":"user"}]
            """,
            Formatter.FormatModelMessages((string?)null, messages)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert tool messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_tool_messages()
    {
        IReadOnlyList<ModelMessage> messages =
        [
            new ToolModelMessage("tc1", "weather", "Sunny", false, "text"),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"id":"tc1","response":"Sunny","type":"tool_call_response"}],"role":"tool"}]
            """,
            Formatter.FormatModelMessages((string?)null, messages)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should combine prompt and messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_combine_prompt_and_messages()
    {
        OtelJson.Equal(
            """
            [{"parts":[{"content":"First message","type":"text"}],"role":"user"},{"parts":[{"content":"Second message","type":"text"}],"role":"user"}]
            """,
            Formatter.FormatModelMessages("First message", [new UserModelMessage("Second message")])
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should return empty array when both prompt and messages are undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_empty_array_when_both_prompt_and_messages_are_undefined()
    {
        OtelJson.Equal(
            """
            []
            """,
            Formatter.FormatModelMessages((string?)null, (IReadOnlyList<ModelMessage>?)null)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should format all assistant content parts in order",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public void Should_format_all_assistant_content_parts_in_order()
    {
        IReadOnlyList<ModelMessage> messages =
        [
            new AssistantModelMessage([
                new AssistantTextPart("Before"),
                new AssistantReasoningPart("Thinking"),
                new AssistantReasoningFilePart(
                    "image/png",
                    null,
                    Convert.FromBase64String("cmVhc29u")
                ),
                new AssistantCustomPart("anthropic.compaction"),
                new AssistantFilePart("image/jpeg", null, Convert.FromBase64String("aW1hZ2U=")),
                new AssistantToolCallPart(
                    new GeneratedToolCall("call-1", "search", "{\"q\":\"test\"}")
                ),
                new AssistantToolResultPart(
                    "call-1",
                    "search",
                    new ToolModelMessage("call-1", "search", "{\"hits\":1}", false, "json")
                ),
                new AssistantToolApprovalRequestPart("approval-1", "call-2"),
                new AssistantTextPart("After"),
            ]),
            new ToolApprovalResponseModelMessage("approval-1", false, "not allowed"),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"Before","type":"text"},{"content":"Thinking","type":"reasoning"},{"content":"cmVhc29u","mime_type":"image/png","modality":"image","type":"blob"},{"kind":"anthropic.compaction","type":"custom"},{"content":"aW1hZ2U=","mime_type":"image/jpeg","modality":"image","type":"blob"},{"arguments":{"q":"test"},"id":"call-1","name":"search","type":"tool_call"},{"id":"call-1","response":{"hits":1},"type":"tool_call_response"},{"approval_id":"approval-1","reason":null,"tool_call_id":"call-2","type":"tool_approval_request"},{"content":"After","type":"text"}],"role":"assistant"},{"parts":[{"approval_id":"approval-1","approved":false,"reason":"not allowed","type":"tool_approval_response"}],"role":"tool"}]
            """,
            Formatter.FormatModelMessages((string?)null, messages)
        );
    }
}
