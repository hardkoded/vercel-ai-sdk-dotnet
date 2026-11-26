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

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatInputMessages</c>.</summary>
public sealed class FormatInputMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert user text messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_user_text_messages()
    {
        ModelMessage[] prompt = [new UserModelMessage("What is the weather?")];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"What is the weather?","type":"text"}],"role":"user"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should preserve system messages in prompt order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_system_messages_in_prompt_order()
    {
        ModelMessage[] prompt =
        [
            new UserModelMessage("First"),
            new SystemModelMessage("Be helpful"),
            new UserModelMessage("Second"),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"First","type":"text"}],"role":"user"},{"parts":[{"content":"Be helpful","type":"text"}],"role":"system"},{"parts":[{"content":"Second","type":"text"}],"role":"user"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert assistant messages with tool calls",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_assistant_messages_with_tool_calls()
    {
        ModelMessage[] prompt =
        [
            new AssistantModelMessage([
                new AssistantToolCallPart(
                    new GeneratedToolCall("call_123", "get_weather", "{\"city\":\"Paris\"}")
                ),
            ]),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"arguments":{"city":"Paris"},"id":"call_123","name":"get_weather","type":"tool_call"}],"role":"assistant"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert tool result messages",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_tool_result_messages()
    {
        ModelMessage[] prompt =
        [
            new ToolModelMessage("call_123", "get_weather", "Sunny, 72°F", false, "text"),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"id":"call_123","response":"Sunny, 72°F","type":"tool_call_response"}],"role":"tool"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert file parts to blob parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_file_parts_to_blob_parts()
    {
        ModelMessage[] prompt =
        [
            new UserModelMessage([new FileDataContentPart("base64data", "image/png")]),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"base64data","mime_type":"image/png","modality":"image","type":"blob"}],"role":"user"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert URL file parts to uri parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_URL_file_parts_to_uri_parts()
    {
        ModelMessage[] prompt =
        [
            new UserModelMessage([
                new FileUrlContentPart("https://example.com/image.png", "image/png"),
            ]),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"mime_type":"image/png","modality":"image","type":"uri","uri":"https://example.com/image.png"}],"role":"user"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert reasoning parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_reasoning_parts()
    {
        ModelMessage[] prompt =
        [
            new AssistantModelMessage([new AssistantReasoningPart("Let me think about this...")]),
        ];
        OtelJson.Equal(
            """
            [{"parts":[{"content":"Let me think about this...","type":"reasoning"}],"role":"assistant"}]
            """,
            Formatter.FormatInputMessages(prompt)
        );
    }
}
