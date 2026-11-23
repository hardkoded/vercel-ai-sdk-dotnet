// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class FormatModelMessagesTests
{
    private const string Test =
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::";

    [Fact]
    [UpstreamTest(
        Test + "should convert a prompt string to a user message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_a_prompt_string_to_a_user_message()
    {
        SemConvJson.AssertEqual(
            """[{"parts":[{"content":"Hello","type":"text"}],"role":"user"}]""",
            GenAiConventions.FormatModelMessages("Hello", null)
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should convert ModelMessage array from prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_ModelMessage_array_from_prompt()
    {
        SemConvJson.AssertEqual(
            """
            [
              {"parts":[{"content":"Hi there","type":"text"}],"role":"user"},
              {"parts":[{"arguments":{"city":"NYC"},"id":"tc1","name":"weather","type":"tool_call"}],"role":"assistant"}
            ]
            """,
            GenAiConventions.FormatModelMessages(
                null,
                new ModelMessage[]
                {
                    new UserModelMessage("Hi there"),
                    new AssistantModelMessage(
                        new AssistantContentPart[]
                        {
                            new AssistantToolCallPart(
                                new GeneratedToolCall("tc1", "weather", """{"city":"NYC"}""")
                            ),
                        }
                    ),
                },
                null
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should preserve system messages in message order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_system_messages_in_message_order()
    {
        SemConvJson.AssertEqual(
            """
            [
              {"parts":[{"content":"First","type":"text"}],"role":"user"},
              {"parts":[{"content":"Be helpful","type":"text"}],"role":"system"},
              {"parts":[{"content":"Second","type":"text"}],"role":"user"}
            ]
            """,
            GenAiConventions.FormatModelMessages(
                null,
                new ModelMessage[]
                {
                    new UserModelMessage("First"),
                    new SystemModelMessage("Be helpful"),
                    new UserModelMessage("Second"),
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(Test + "should convert tool messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_messages()
    {
        SemConvJson.AssertEqual(
            """[{"parts":[{"id":"tc1","response":"Sunny","type":"tool_call_response"}],"role":"tool"}]""",
            GenAiConventions.FormatModelMessages(
                null,
                new ModelMessage[]
                {
                    new ToolModelMessage("tc1", "weather", "Sunny", false, "text"),
                }
            )
        );
    }

    [Fact]
    [UpstreamTest(Test + "should combine prompt and messages", Coverage = UpstreamCoverage.Covered)]
    public void Should_combine_prompt_and_messages()
    {
        SemConvJson.AssertEqual(
            """
            [
              {"parts":[{"content":"First message","type":"text"}],"role":"user"},
              {"parts":[{"content":"Second message","type":"text"}],"role":"user"}
            ]
            """,
            GenAiConventions.FormatModelMessages(
                "First message",
                new ModelMessage[] { new UserModelMessage("Second message") }
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should return empty array when both prompt and messages are undefined",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_empty_array_when_both_prompt_and_messages_are_undefined()
    {
        Assert.Empty(GenAiConventions.FormatModelMessages(null, null));
    }

    [Fact]
    [UpstreamTest(
        Test + "should format all assistant content parts in order",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_all_assistant_content_parts_in_order()
    {
        SemConvJson.AssertEqual(
            """
            [
              {"parts":[
                {"content":"Before","type":"text"},
                {"content":"Thinking","type":"reasoning"},
                {"content":"cmVhc29u","mime_type":"image/png","modality":"image","type":"blob"},
                {"kind":"anthropic.compaction","type":"custom"},
                {"content":"aW1hZ2U=","mime_type":"image/jpeg","modality":"image","type":"blob"},
                {"arguments":{"q":"test"},"id":"call-1","name":"search","type":"tool_call"},
                {"id":"call-1","response":{"hits":1},"type":"tool_call_response"},
                {"approval_id":"approval-1","tool_call_id":"call-2","type":"tool_approval_request"},
                {"content":"After","type":"text"}
              ],"role":"assistant"},
              {"parts":[
                {"approval_id":"approval-1","approved":false,"reason":"not allowed","type":"tool_approval_response"}
              ],"role":"tool"}
            ]
            """,
            GenAiConventions.FormatModelMessages(
                null,
                new ModelMessage[]
                {
                    new AssistantModelMessage(
                        new AssistantContentPart[]
                        {
                            new AssistantTextPart("Before"),
                            new AssistantReasoningPart("Thinking"),
                            new AssistantReasoningFilePart(
                                "image/png",
                                null,
                                Convert.FromBase64String("cmVhc29u")
                            ),
                            new AssistantCustomPart("anthropic.compaction"),
                            new AssistantFilePart(
                                "image/jpeg",
                                null,
                                Convert.FromBase64String("aW1hZ2U=")
                            ),
                            new AssistantToolCallPart(
                                new GeneratedToolCall("call-1", "search", """{"q":"test"}""")
                            ),
                            new AssistantToolResultPart(
                                "call-1",
                                "search",
                                new ToolModelMessage(
                                    "call-1",
                                    "search",
                                    """{"hits":1}""",
                                    false,
                                    "json"
                                )
                            ),
                            new AssistantToolApprovalRequestPart("approval-1", "call-2"),
                            new AssistantTextPart("After"),
                        }
                    ),
                    new ToolApprovalResponseModelMessage("approval-1", false, "not allowed"),
                }
            )
        );
    }
}
