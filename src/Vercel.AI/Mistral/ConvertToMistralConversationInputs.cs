// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Converts a prompt to Conversations API inputs. Maps to <c>convert-to-mistral-conversation-inputs.ts</c>.</summary>
internal static class ConvertToMistralConversationInputs
{
    /// <summary>Converts the prompt to inputs and instructions. Maps to <c>convertToMistralConversationInputs</c>.</summary>
    public static (JsonArray Inputs, string? Instructions) Convert(
        IReadOnlyList<ModelMessage> prompt,
        ToolNameMapping toolNameMapping
    )
    {
        var inputs = new JsonArray();
        var instructions = new List<string>();
        var executions = new HashSet<string>(StringComparer.Ordinal);

        void AppendToolResult(ToolModelMessage part)
        {
            if (executions.Contains(part.ToolCallId))
            {
                JsonNode? result = null;
                if (part.OutputType == "json")
                {
                    var value = JsonNode.Parse(part.OutputJson);
                    if (value is JsonObject container && container["info"] is JsonObject info)
                    {
                        result = info.ContainsKey("result") ? info["result"] ?? info : info;
                    }
                }

                inputs.Add(
                    new JsonObject
                    {
                        ["type"] = "function.result",
                        ["tool_call_id"] = part.ToolCallId,
                        ["result"] =
                            result is JsonValue text && text.TryGetValue<string>(out var asString)
                                ? asString
                                : result?.ToJsonString(ToolResultOptions) ?? "null",
                    }
                );
                return;
            }

            inputs.Add(
                new JsonObject
                {
                    ["type"] = "function.result",
                    ["tool_call_id"] = part.ToolCallId,
                    ["result"] = ConvertToMistralChatMessages.ToolContent(part),
                }
            );
        }

        void AppendTool(ToolModelMessage message)
        {
            foreach (var part in message.Results)
            {
                AppendToolResult(part);
            }
        }

        foreach (var message in prompt)
        {
            switch (message)
            {
                case SystemModelMessage system:
                    instructions.Add(system.Content);
                    break;
                case UserModelMessage user:
                    var converted = ConvertToMistralChatMessages.Convert(
                        new ModelMessage[] { user }
                    )[0]!.AsObject();
                    inputs.Add(
                        new JsonObject
                        {
                            ["type"] = "message.input",
                            ["role"] = "user",
                            ["content"] = converted["content"]!.DeepClone(),
                        }
                    );
                    break;
                case AssistantModelMessage assistant:
                    var content = new JsonArray();
                    void Flush()
                    {
                        if (content.Count > 0)
                        {
                            inputs.Add(
                                new JsonObject
                                {
                                    ["type"] = "message.input",
                                    ["role"] = "assistant",
                                    ["content"] = content,
                                }
                            );
                            content = new JsonArray();
                        }
                    }

                    foreach (var part in assistant.Parts)
                    {
                        switch (part)
                        {
                            case AssistantTextPart text:
                                content.Add(
                                    new JsonObject { ["type"] = "text", ["text"] = text.Text }
                                );
                                break;
                            case AssistantReasoningPart reasoning:
                                content.Add(ConvertToMistralChatMessages.Thinking(reasoning.Text));
                                break;
                            case AssistantToolCallPart call:
                                Flush();
                                inputs.Add(ToolCall(call, toolNameMapping, executions));
                                break;
                            case AssistantToolResultPart result:
                                Flush();
                                AppendToolResult(result.Output);
                                break;
                            default:
                                throw new UnsupportedFunctionalityException(
                                    "assistant content type: " + part.Type,
                                    "'assistant content type: "
                                        + part.Type
                                        + "' functionality not supported."
                                );
                        }
                    }

                    Flush();
                    break;
                case ToolApprovalResponseModelMessage approval:
                    if (approval.DeniedToolResult != null)
                    {
                        AppendTool(approval.DeniedToolResult);
                    }

                    break;
                case ToolModelMessage tool:
                    AppendTool(tool);
                    break;
            }
        }

        return (inputs, instructions.Count > 0 ? string.Join("\n\n", instructions) : null);
    }

    private static readonly JsonSerializerOptions ToolResultOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static JsonObject ToolCall(
        AssistantToolCallPart part,
        ToolNameMapping toolNameMapping,
        HashSet<string> executions
    )
    {
        var call = part.ToolCall;
        if (!call.ProviderExecuted)
        {
            return new JsonObject
            {
                ["type"] = "function.call",
                ["tool_call_id"] = call.ToolCallId,
                ["name"] = call.ToolName,
                ["arguments"] = ConvertToMistralChatMessages.SerializeMistralToolCallArguments(
                    call.ArgumentsJson
                ),
            };
        }

        string? name = null;
        if (
            part.ProviderOptions != null
            && part.ProviderOptions.TryGetValue("mistral", out var mistral)
            && mistral.ValueKind == JsonValueKind.Object
        )
        {
            name = String(mistral, "function") ?? String(mistral, "name");
        }

        string? arguments = null;
        try
        {
            var input = JsonNode.Parse(call.ArgumentsJson);
            if (
                input is JsonObject container
                && container["arguments"] is JsonValue value
                && value.TryGetValue<string>(out var text)
            )
            {
                arguments = text;
            }
        }
        catch (JsonException)
        {
            // Not JSON, so it is replayed as a string below.
        }

        // Replay built-in executions as calls and results. Entry IDs and tool.execution output entries cannot be sent as inputs.
        executions.Add(call.ToolCallId);
        return new JsonObject
        {
            ["type"] = "function.call",
            ["tool_call_id"] = call.ToolCallId,
            ["name"] = name ?? toolNameMapping.ToProviderToolName(call.ToolName),
            ["arguments"] = arguments
                ?? ConvertToMistralChatMessages.SerializeMistralToolCallArguments(
                    call.ArgumentsJson
                ),
        };
    }

    private static string? String(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
