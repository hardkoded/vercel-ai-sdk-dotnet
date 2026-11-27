// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; assistant message</c>.</summary>
[Collection("LogWarnings")]
public sealed class AssistantMessageTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > assistant message::";

    [UpstreamTest(
        Suite + "should convert an assistant message with reasoning",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_an_assistant_message_with_reasoning()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"reasoning","text":"Thinking...","providerMetadata":{"testProvider":{"signature":"1234567890"}},"state":"done"},{"type":"reasoning","text":"redacted-data","providerMetadata":{"testProvider":{"isRedacted":true}},"state":"done"},{"type":"text","text":"Hello, human!","state":"done"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"type":"reasoning","text":"Thinking...","providerOptions":{"testProvider":{"signature":"1234567890"}}},{"type":"reasoning","text":"redacted-data","providerOptions":{"testProvider":{"isRedacted":true}}},{"type":"text","text":"Hello, human!"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert an assistant message with file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_an_assistant_message_with_file_parts()
    {
        var result = Convert(
            new[] { Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")) }
        );

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should include filename for assistant file parts when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_include_filename_for_assistant_file_parts_when_provided()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")
                    {
                        Filename = "test.png",
                    }
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "filename": "test.png"
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should handle assistant message file parts with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_assistant_message_file_parts_with_provider_metadata()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")
                    {
                        ProviderMetadata = new Dictionary<string, JsonElement>
                        {
                            ["testProvider"] = Json("{\"signature\":\"test-signature\"}"),
                        },
                    }
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "providerOptions": { "testProvider": { "signature": "test-signature" } }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should use providerReference as data for assistant file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_use_providerReference_as_data_for_assistant_file_parts()
    {
        var result = Convert(
            new[]
            {
                Assistant(
                    new FileUIPart("application/pdf", "data:application/pdf;base64,xyz")
                    {
                        Filename = "doc.pdf",
                        ProviderReference = new Dictionary<string, string>
                        {
                            ["anthropic"] = "file-xyz789",
                        },
                    }
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "application/pdf",
                    "filename": "doc.pdf",
                    "data": { "type": "reference", "reference": { "anthropic": "file-xyz789" } }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert custom assistant parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_custom_assistant_parts()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"custom","kind":"test-provider.compaction","providerMetadata":{"openai":{"itemId":"cmp_123"}}}]}]
                """
            )
        );

        AssertProviderExecuted(
            """
            [{"role":"assistant","content":[{"type":"custom","kind":"test-provider.compaction","providerOptions":{"openai":{"itemId":"cmp_123"}}}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a simple assistant text message",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_simple_assistant_text_message()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"text","text":"Hello, human!","state":"done"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"type":"text","text":"Hello, human!"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert a simple assistant text message with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_a_simple_assistant_text_message_with_provider_metadata()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"text","text":"Hello, human!","state":"done","providerMetadata":{"testProvider":{"signature":"1234567890"}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"providerOptions":{"testProvider":{"signature":"1234567890"}},"text":"Hello, human!","type":"text"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should preserve an Anthropic compaction signature through UI message persistence",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_preserve_an_Anthropic_compaction_signature_through_UI_message_persistence()
    {
        var recordedMessage = await RecordAssistantMessageJsonFromChunks(
            """
            [
              { "type": "start", "messageId": "msg-123" },
              { "type": "start-step" },
              {
                "type": "text-start",
                "id": "compaction",
                "providerMetadata": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
              },
              { "type": "text-delta", "id": "compaction", "delta": "Summary of the conversation." },
              { "type": "text-end", "id": "compaction" },
              { "type": "finish-step" },
              { "type": "finish" }
            ]
            """
        );

        var expectedPart = JsonNode.Parse(
            """
            {
              "type": "text",
              "text": "Summary of the conversation.",
              "state": "done",
              "providerMetadata": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
            }
            """
        )!;
        Assert.Contains(((JsonArray)recordedMessage["parts"]!), part =>
            JsonNode.DeepEquals(part, expectedPart)
        );

        var result = Convert(new[] { UIMessageJson.FromJson(recordedMessage) });

        Assert.Contains(Render(result).AsArray(), message => JsonNode.DeepEquals(
                message,
                JsonNode.Parse(
                    """
                    {
                      "role": "assistant",
                      "content": [
                        {
                          "type": "text",
                          "text": "Summary of the conversation.",
                          "providerOptions": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
                        }
                      ]
                    }
                    """
                )
            ));
    }

    [UpstreamTest(
        Suite + "should handle assistant message with tool output available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_assistant_message_with_tool_output_available()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"Let me calculate that for you."},{"type":"tool-calculator","state":"output-available","toolCallId":"call1","input":{"operation":"add","numbers":[1,2]},"output":"3","callProviderMetadata":{"testProvider":{"signature":"1234567890"}}}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"text":"Let me calculate that for you.","type":"text"},{"input":{"numbers":[1,2],"operation":"add"},"providerOptions":{"testProvider":{"signature":"1234567890"}},"toolCallId":"call1","toolName":"calculator","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"3"},"providerOptions":{"testProvider":{"signature":"1234567890"}},"toolCallId":"call1","toolName":"calculator","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should omit persisted output when a static tool is no longer available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public async Task Should_omit_persisted_output_when_a_static_tool_is_no_longer_available()
    {
        var historicalTool = new Tool(
            "search",
            null,
            Json(
                """{"type":"object","properties":{"query":{"type":"string"}},"required":["query"]}"""
            ),
            null
        )
        {
            OutputSchema = Json(
                """{"type":"object","properties":{"summary":{"type":"string"},"privateMetadata":{"type":"string"}},"required":["summary","privateMetadata"]}"""
            ),
            ToModelOutput = call => new TextToolModelOutput(
                JsonDocument
                    .Parse((string)call.Output!)
                    .RootElement.GetProperty("summary")
                    .GetString()!
            ),
        };
        var tools = new Dictionary<string, Tool> { ["search"] = historicalTool };

        var history = new[]
        {
            new UIMessage(
                UIMessageRole.Assistant,
                new UIMessagePart[]
                {
                    new ToolUIPart("search", "call-1", ToolUIPartState.OutputAvailable)
                    {
                        Input = Json("""{"query":"weather"}"""),
                        Output = Json(
                            """{"summary":"sunny","privateMetadata":"must-not-reach-the-model"}"""
                        ),
                    },
                }
            )
            {
                Id = "assistant-1",
            },
        };

        var before = Convert(
            await Ai.ValidateUIMessagesAsync(
                new ValidateUIMessagesOptions { Messages = history, Tools = tools }
            ),
            new ConvertToModelMessagesOptions { Tools = tools }
        );

        var validatedWithoutTool = await Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions
            {
                Messages = history,
                Tools = new Dictionary<string, Tool>(),
            }
        );

        var unavailablePart = Assert.IsType<ToolUIPart>(validatedWithoutTool[0].Parts[0]);
        Assert.True(unavailablePart.IsDynamic);
        Assert.False(unavailablePart.Dynamic);

        foreach (
            var messages in new[] { Reload(validatedWithoutTool), Reload(validatedWithoutTool) }
        )
        {
            var revalidatedMessages = await Ai.ValidateUIMessagesAsync(
                new ValidateUIMessagesOptions
                {
                    Messages = messages,
                    Tools = new Dictionary<string, Tool>(),
                }
            );

            AssertEqual(
                """
                [
                  {
                    "role": "assistant",
                    "content": [
                      { "type": "tool-call", "toolCallId": "call-1", "toolName": "search", "input": { "query": "weather" } }
                    ]
                  },
                  {
                    "role": "tool",
                    "content": [
                      {
                        "type": "tool-result",
                        "toolCallId": "call-1",
                        "toolName": "search",
                        "output": { "type": "text", "value": "Tool output omitted because the tool is no longer available." }
                      }
                    ]
                  }
                ]
                """,
                Convert(
                    revalidatedMessages,
                    new ConvertToModelMessagesOptions { Tools = new Dictionary<string, Tool>() }
                )
            );

            Assert.True(
                JsonNode.DeepEquals(
                    Render(before),
                    Render(
                        Convert(
                            revalidatedMessages,
                            new ConvertToModelMessagesOptions { Tools = tools }
                        )
                    )
                )
            );
        }
    }

    [UpstreamTest(
        Suite + "should handle assistant message with provider-executed tool output available",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_assistant_message_with_provider_executed_tool_output_available()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"Let me calculate that for you."},{"type":"tool-calculator","state":"output-available","toolCallId":"call1","input":{"operation":"add","numbers":[1,2]},"output":"3","providerExecuted":true}]}]
                """
            )
        );

        AssertProviderExecuted(
            """
            [{"role":"assistant","content":[{"text":"Let me calculate that for you.","type":"text"},{"input":{"numbers":[1,2],"operation":"add"},"providerExecuted":true,"toolCallId":"call1","toolName":"calculator","type":"tool-call"},{"output":{"type":"text","value":"3"},"toolCallId":"call1","toolName":"calculator","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should handle assistant message with provider-executed tool output error",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_assistant_message_with_provider_executed_tool_output_error()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"Let me calculate that for you."},{"type":"tool-calculator","state":"output-error","toolCallId":"call1","input":{"operation":"add","numbers":[1,2]},"errorText":"Error: Invalid input","providerExecuted":true}]}]
                """
            )
        );

        AssertProviderExecuted(
            """
            [{"role":"assistant","content":[{"text":"Let me calculate that for you.","type":"text"},{"input":{"numbers":[1,2],"operation":"add"},"providerExecuted":true,"toolCallId":"call1","toolName":"calculator","type":"tool-call"},{"output":{"type":"error-json","value":"Error: Invalid input"},"toolCallId":"call1","toolName":"calculator","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should propagate provider metadata to provider-executed tool-result",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_propagate_provider_metadata_to_provider_executed_tool_result()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"tool-calculator","state":"output-available","toolCallId":"call1","input":{"operation":"multiply","numbers":[3,4]},"output":"12","providerExecuted":true,"callProviderMetadata":{"testProvider":{"executionTime":75}}}]}]
                """
            )
        );

        AssertProviderExecuted(
            """
            [{"role":"assistant","content":[{"input":{"numbers":[3,4],"operation":"multiply"},"providerExecuted":true,"providerOptions":{"testProvider":{"executionTime":75}},"toolCallId":"call1","toolName":"calculator","type":"tool-call"},{"output":{"type":"text","value":"12"},"providerOptions":{"testProvider":{"executionTime":75}},"toolCallId":"call1","toolName":"calculator","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should prefer result provider metadata over call provider metadata for provider-executed tool-result",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_prefer_result_provider_metadata_over_call_provider_metadata_for_provider_executed_tool_result()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"tool-calculator","state":"output-available","toolCallId":"call1","input":{"operation":"multiply","numbers":[3,4]},"output":"12","providerExecuted":true,"callProviderMetadata":{"testProvider":{"itemId":"call-item"}},"resultProviderMetadata":{"testProvider":{"itemId":"result-item"}}}]}]
                """
            )
        );

        AssertProviderExecuted(
            """
            [{"role":"assistant","content":[{"input":{"operation":"multiply","numbers":[3,4]},"providerExecuted":true,"providerOptions":{"testProvider":{"itemId":"call-item"}},"toolCallId":"call1","toolName":"calculator","type":"tool-call"},{"output":{"type":"text","value":"12"},"providerOptions":{"testProvider":{"itemId":"result-item"}},"toolCallId":"call1","toolName":"calculator","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should handle assistant message with tool invocations that have multi-part responses",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_assistant_message_with_tool_invocations_that_have_multi_part_responses()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"Let me calculate that for you."},{"type":"tool-screenshot","state":"output-available","toolCallId":"call1","input":{},"output":"imgbase64"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"text":"Let me calculate that for you.","type":"text"},{"input":{},"toolCallId":"call1","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"imgbase64"},"toolCallId":"call1","toolName":"screenshot","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should handle conversation with an assistant message that has empty tool invocations",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_conversation_with_an_assistant_message_that_has_empty_tool_invocations()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"user","parts":[{"type":"text","text":"text1"}]},{"role":"assistant","parts":[{"type":"text","text":"text2","state":"done"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"text":"text1","type":"text"}]},{"role":"assistant","content":[{"text":"text2","type":"text"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite
            + "should handle conversation with multiple tool invocations that have step information",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_conversation_with_multiple_tool_invocations_that_have_step_information()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"response","state":"done"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-1","input":{"value":"value-1"},"output":"result-1"},{"type":"step-start"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-2","input":{"value":"value-2"},"output":"result-2"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-3","input":{"value":"value-3"},"output":"result-3"},{"type":"step-start"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-4","input":{"value":"value-4"},"output":"result-4"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"text":"response","type":"text"},{"input":{"value":"value-1"},"toolCallId":"call-1","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-1"},"toolCallId":"call-1","toolName":"screenshot","type":"tool-result"}]},{"role":"assistant","content":[{"input":{"value":"value-2"},"toolCallId":"call-2","toolName":"screenshot","type":"tool-call"},{"input":{"value":"value-3"},"toolCallId":"call-3","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-2"},"toolCallId":"call-2","toolName":"screenshot","type":"tool-result"},{"output":{"type":"text","value":"result-3"},"toolCallId":"call-3","toolName":"screenshot","type":"tool-result"}]},{"role":"assistant","content":[{"input":{"value":"value-4"},"toolCallId":"call-4","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-4"},"toolCallId":"call-4","toolName":"screenshot","type":"tool-result"}]}]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should handle conversation with mix of tool invocations and text",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_conversation_with_mix_of_tool_invocations_and_text()
    {
        var result = Convert(
            UIMessagesFromJson(
                """
                [{"role":"assistant","parts":[{"type":"step-start"},{"type":"text","text":"i am gonna use tool1","state":"done"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-1","input":{"value":"value-1"},"output":"result-1"},{"type":"step-start"},{"type":"text","text":"i am gonna use tool2 and tool3","state":"done"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-2","input":{"value":"value-2"},"output":"result-2"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-3","input":{"value":"value-3"},"output":"result-3"},{"type":"step-start"},{"type":"tool-screenshot","state":"output-available","toolCallId":"call-4","input":{"value":"value-4"},"output":"result-4"},{"type":"step-start"},{"type":"text","text":"final response","state":"done"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"assistant","content":[{"text":"i am gonna use tool1","type":"text"},{"input":{"value":"value-1"},"toolCallId":"call-1","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-1"},"toolCallId":"call-1","toolName":"screenshot","type":"tool-result"}]},{"role":"assistant","content":[{"text":"i am gonna use tool2 and tool3","type":"text"},{"input":{"value":"value-2"},"toolCallId":"call-2","toolName":"screenshot","type":"tool-call"},{"input":{"value":"value-3"},"toolCallId":"call-3","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-2"},"toolCallId":"call-2","toolName":"screenshot","type":"tool-result"},{"output":{"type":"text","value":"result-3"},"toolCallId":"call-3","toolName":"screenshot","type":"tool-result"}]},{"role":"assistant","content":[{"input":{"value":"value-4"},"toolCallId":"call-4","toolName":"screenshot","type":"tool-call"}]},{"role":"tool","content":[{"output":{"type":"text","value":"result-4"},"toolCallId":"call-4","toolName":"screenshot","type":"tool-result"}]},{"role":"assistant","content":[{"text":"final response","type":"text"}]}]
            """,
            result
        );
    }

    private static void AssertProviderExecuted(
        string expectedJson,
        IReadOnlyList<ModelMessage> actual
    )
    {
        // Provider-executed tool calls and their results live in the assistant message content.
        var content = new JsonArray();
        var message = Assert.IsType<AssistantModelMessage>(Assert.Single(actual));
        foreach (var part in message.Parts)
        {
            switch (part)
            {
                case AssistantToolCallPart call:
                    var node = new JsonObject
                    {
                        ["input"] = JsonNode.Parse(call.ToolCall.ArgumentsJson),
                        ["toolCallId"] = call.ToolCall.ToolCallId,
                        ["toolName"] = call.ToolCall.ToolName,
                        ["type"] = "tool-call",
                    };
                    if (call.ToolCall.ProviderExecuted)
                    {
                        node["providerExecuted"] = true;
                    }

                    AddOptions(node, call.ProviderOptions);
                    content.Add(node);
                    break;
                case AssistantToolResultPart result:
                    var output = new JsonObject { ["type"] = result.Output.OutputType };
                    output["value"] = result.Output.OutputType is "json" or "error-json"
                        ? JsonNode.Parse(result.Output.OutputJson)
                        : result.Output.OutputJson;
                    var resultNode = new JsonObject
                    {
                        ["output"] = output,
                        ["toolCallId"] = result.ToolCallId,
                        ["toolName"] = result.ToolName,
                        ["type"] = "tool-result",
                    };
                    AddOptions(resultNode, result.ProviderOptions);
                    content.Add(resultNode);
                    break;
                case AssistantTextPart text:
                    content.Add(new JsonObject { ["text"] = text.Text, ["type"] = "text" });
                    break;
                case AssistantCustomPart custom:
                    var customNode = new JsonObject { ["type"] = "custom", ["kind"] = custom.Kind };
                    AddOptions(customNode, custom.ProviderOptions);
                    content.Add(customNode);
                    break;
                default:
                    throw new InvalidOperationException(part.Type);
            }
        }

        var rendered = new JsonArray(
            new JsonObject { ["content"] = content, ["role"] = "assistant" }
        );
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expectedJson), rendered),
            "Expected: " + expectedJson + "\nActual: " + rendered.ToJsonString()
        );
    }

    private static void AddOptions(
        JsonObject node,
        IReadOnlyDictionary<string, JsonElement>? options
    )
    {
        if (options != null)
        {
            node["providerOptions"] = JsonSerializer.SerializeToNode(options);
        }
    }
}
