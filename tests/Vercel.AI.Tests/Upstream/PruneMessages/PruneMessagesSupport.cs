// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.PruneMessages;

/// <summary>Fixtures of <c>prune-messages.test.ts</c> and a renderer that gives messages the shape of upstream's inline snapshots.</summary>
internal static class PruneMessagesSupport
{
    internal const string Prefix = "packages/ai/src/generate-text/prune-messages.test.ts::";

    internal static IReadOnlyList<ModelMessage> MessagesFixture1 => new ModelMessage[]
    {
        new UserModelMessage("Weather in Tokyo and Busan?"),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantReasoningPart("I need to get the weather in Tokyo and Busan."),
            Call("call-1", "get-weather-tool-1", "{\"city\": \"Tokyo\"}"),
            Call("call-2", "get-weather-tool-2", "{\"city\": \"Busan\"}"),
            new AssistantToolApprovalRequestPart("approval-1", "call-2"),
        }),
        new ToolApprovalResponseModelMessage("approval-1", true),
        new ToolModelMessage("call-1", "get-weather-tool-1", "sunny", false, "text"),
        new ToolModelMessage("call-2", "get-weather-tool-2", "Error: Fetching weather data failed", true, "error-text"),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantReasoningPart("I have got the weather in Tokyo and Busan."),
            new AssistantTextPart("The weather in Tokyo is sunny. I could not get the weather in Busan."),
        }),
    };

    internal static IReadOnlyList<ModelMessage> MessagesFixture2 => new ModelMessage[]
    {
        new UserModelMessage("Weather in Tokyo and Busan?"),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantReasoningPart("I need to get the weather in Tokyo and Busan."),
            Call("call-1", "get-weather-tool-1", "{\"city\": \"Tokyo\"}"),
            Call("call-2", "get-weather-tool-2", "{\"city\": \"Busan\"}"),
            new AssistantToolApprovalRequestPart("approval-1", "call-1"),
        }),
    };

    internal static IReadOnlyList<ModelMessage> PendingApprovalMessagesFixture => new ModelMessage[]
    {
        new UserModelMessage("Echo hello."),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            Call("call-1", "echo", "{\"value\":\"hello\"}"),
            new AssistantToolApprovalRequestPart("approval-1", "call-1"),
        }),
        new ToolApprovalResponseModelMessage("approval-1", true),
    };

    internal static IReadOnlyList<ModelMessage> MultiTurnToolCallMessagesFixture => new ModelMessage[]
    {
        new UserModelMessage("ask me a question"),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            new AssistantTextPart("What can i help you with"),
            Call("toolu_01P9s4havAQSjDmS4eWT1N2V", "AskUserQuestion", "{\"question\":\"What would you like help with today?\",\"options\":[\"Tool 1 Option 1\",\"Tool 1 Option 2\",\"Tool 1 Option 3\"]}"),
        }),
        new ToolModelMessage("toolu_01P9s4havAQSjDmS4eWT1N2V", "AskUserQuestion", "Something else", false, "text"),
        new AssistantModelMessage(new AssistantContentPart[]
        {
            Call("toolu_01TMAuwWKLmBoQtx7K88dxsQ", "AskUserQuestion", "{\"question\":\"Ok what else?\",\"options\":[\"Tool 2 Option 1\",\"Tool 2 Option 2\",\"Tool 2 Option 3\"]}"),
        }),
        new ToolModelMessage("toolu_01TMAuwWKLmBoQtx7K88dxsQ", "AskUserQuestion", "Other - I'll describe it", false, "text"),
        new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("What would you like to discuss or work on?") }),
        new UserModelMessage("never mind. lets end this conversation"),
        new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("ok, have a nice day") }),
        new UserModelMessage("thank you"),
    };

    internal static AssistantToolCallPart Call(string toolCallId, string toolName, string inputJson)
    {
        return new AssistantToolCallPart(new GeneratedToolCall(toolCallId, toolName, inputJson));
    }

    internal static IReadOnlyList<ModelMessage> Prune(
        IReadOnlyList<ModelMessage> messages,
        PruneReasoning reasoning = PruneReasoning.None,
        params PruneToolCallsRule[] toolCalls)
    {
        return Ai.PruneMessages(new PruneMessagesOptions { Messages = messages, Reasoning = reasoning, ToolCalls = toolCalls });
    }

    /// <summary>Renders messages like upstream's inline snapshots. Consecutive tool messages are one message, and tool-call input is parsed JSON.</summary>
    internal static JsonNode Render(IReadOnlyList<ModelMessage> messages)
    {
        var array = new JsonArray();
        JsonArray? toolContent = null;
        foreach (var message in messages)
        {
            if (message.Role != "tool")
            {
                toolContent = null;
            }

            switch (message)
            {
                case UserModelMessage user:
                {
                    var content = new JsonArray();
                    foreach (var part in user.Content)
                    {
                        content.Add(new JsonObject { ["text"] = ((TextContentPart)part).Text, ["type"] = "text" });
                    }

                    array.Add(new JsonObject { ["content"] = content, ["role"] = "user" });
                    break;
                }

                case AssistantModelMessage assistant:
                {
                    var content = new JsonArray();
                    foreach (var part in assistant.Parts)
                    {
                        content.Add(part switch
                        {
                            AssistantReasoningPart r => new JsonObject { ["text"] = r.Text, ["type"] = "reasoning" },
                            AssistantTextPart t => new JsonObject { ["text"] = t.Text, ["type"] = "text" },
                            AssistantToolCallPart c => new JsonObject
                            {
                                ["input"] = JsonNode.Parse(c.ToolCall.ArgumentsJson),
                                ["toolCallId"] = c.ToolCall.ToolCallId,
                                ["toolName"] = c.ToolCall.ToolName,
                                ["type"] = "tool-call",
                            },
                            AssistantToolApprovalRequestPart a => new JsonObject
                            {
                                ["approvalId"] = a.ApprovalId,
                                ["toolCallId"] = a.ToolCallId,
                                ["type"] = "tool-approval-request",
                            },
                            _ => throw new InvalidOperationException(part.Type),
                        });
                    }

                    array.Add(new JsonObject { ["content"] = content, ["role"] = "assistant" });
                    break;
                }

                default:
                {
                    if (toolContent is null)
                    {
                        toolContent = new JsonArray();
                        array.Add(new JsonObject { ["content"] = toolContent, ["role"] = "tool" });
                    }

                    if (message is ToolApprovalResponseModelMessage response)
                    {
                        toolContent.Add(new JsonObject { ["approvalId"] = response.ApprovalId, ["approved"] = response.Approved, ["type"] = "tool-approval-response" });
                    }
                    else
                    {
                        foreach (var result in ((ToolModelMessage)message).Results)
                        {
                            toolContent.Add(new JsonObject
                            {
                                ["output"] = new JsonObject { ["type"] = result.OutputType, ["value"] = result.OutputJson },
                                ["toolCallId"] = result.ToolCallId,
                                ["toolName"] = result.ToolName,
                                ["type"] = "tool-result",
                            });
                        }
                    }

                    break;
                }
            }
        }

        return array;
    }

    internal static void AssertSnapshot(string expectedJson, IReadOnlyList<ModelMessage> actual)
    {
        var expected = JsonNode.Parse(expectedJson)!;
        var rendered = Render(actual);
        Assert.True(JsonNode.DeepEquals(expected, rendered), "Expected: " + expected.ToJsonString() + "\nActual: " + rendered.ToJsonString());
    }
}
