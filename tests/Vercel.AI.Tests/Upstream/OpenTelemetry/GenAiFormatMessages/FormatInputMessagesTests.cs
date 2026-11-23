// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class FormatInputMessagesTests
{
    private const string Test =
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::";

    [Fact]
    [UpstreamTest(
        Test + "should convert user text messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_convert_user_text_messages()
    {
        var prompt = new ModelMessage[] { new UserModelMessage("What is the weather?") };
        SemConvJson.AssertEqual(
            """[{"parts":[{"content":"What is the weather?","type":"text"}],"role":"user"}]""",
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should preserve system messages in prompt order",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_preserve_system_messages_in_prompt_order()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage("First"),
            new SystemModelMessage("Be helpful"),
            new UserModelMessage("Second"),
        };
        SemConvJson.AssertEqual(
            """
            [
              {"parts":[{"content":"First","type":"text"}],"role":"user"},
              {"parts":[{"content":"Be helpful","type":"text"}],"role":"system"},
              {"parts":[{"content":"Second","type":"text"}],"role":"user"}
            ]
            """,
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert assistant messages with tool calls",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_convert_assistant_messages_with_tool_calls()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantToolCallPart(
                        new GeneratedToolCall("call_123", "get_weather", """{"city":"Paris"}""")
                    ),
                }
            ),
        };
        SemConvJson.AssertEqual(
            """
            [{"parts":[{"arguments":{"city":"Paris"},"id":"call_123","name":"get_weather","type":"tool_call"}],"role":"assistant"}]
            """,
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert tool result messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_convert_tool_result_messages()
    {
        var prompt = new ModelMessage[]
        {
            new ToolModelMessage("call_123", "get_weather", "Sunny, 72°F", false, "text"),
        };
        SemConvJson.AssertEqual(
            """[{"parts":[{"id":"call_123","response":"Sunny, 72°F","type":"tool_call_response"}],"role":"tool"}]""",
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert file parts to blob parts",
        Coverage = UpstreamCoverage.Partial,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71",
        Note = "The port holds file data as bytes, so the part carries the base64 of the bytes instead of an already-encoded base64 string."
    )]
    public void Should_convert_file_parts_to_blob_parts()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart(
                        "image/png",
                        null,
                        Encoding.UTF8.GetBytes("base64data"),
                        null
                    ),
                }
            ),
        };
        SemConvJson.AssertEqual(
            """
            [{"parts":[{"content":"YmFzZTY0ZGF0YQ==","mime_type":"image/png","modality":"image","type":"blob"}],"role":"user"}]
            """,
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert URL file parts to uri parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_convert_URL_file_parts_to_uri_parts()
    {
        var prompt = new ModelMessage[]
        {
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("image/png", "https://example.com/image.png", null, null),
                }
            ),
        };
        SemConvJson.AssertEqual(
            """
            [{"parts":[{"mime_type":"image/png","modality":"image","type":"uri","uri":"https://example.com/image.png"}],"role":"user"}]
            """,
            GenAiConventions.FormatInputMessages(prompt)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert reasoning parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_convert_reasoning_parts()
    {
        var prompt = new ModelMessage[]
        {
            new AssistantModelMessage(
                new AssistantContentPart[]
                {
                    new AssistantReasoningPart("Let me think about this..."),
                }
            ),
        };
        SemConvJson.AssertEqual(
            """[{"parts":[{"content":"Let me think about this...","type":"reasoning"}],"role":"assistant"}]""",
            GenAiConventions.FormatInputMessages(prompt)
        );
    }
}
