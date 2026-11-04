// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Helpers of <c>convert-to-model-messages.test.ts</c> and a renderer that gives messages the shape of upstream's expectations.</summary>
internal static class ConvertToModelMessagesSupport
{
    internal const string Prefix = "packages/ai/src/ui/convert-to-model-messages.test.ts::";

    internal static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    internal static UIMessage Assistant(params UIMessagePart[] parts) => new(UIMessageRole.Assistant, parts);

    internal static UIMessage User(params UIMessagePart[] parts) => new(UIMessageRole.User, parts);

    internal static IReadOnlyList<ModelMessage> Convert(IReadOnlyList<UIMessage> messages, ConvertToModelMessagesOptions? options = null)
    {
        return Ai.ConvertToModelMessages(messages, options);
    }

    /// <summary>
    /// Renders messages like upstream's expectations. The converter emits one tool message per tool part, while upstream's
    /// tool message holds several parts, so consecutive tool messages are rendered as one message. Members that are undefined upstream are omitted.
    /// </summary>
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
                case SystemModelMessage system:
                    array.Add(WithProviderOptions(new JsonObject { ["content"] = system.Content, ["role"] = "system" }, system.ProviderOptions));
                    break;
                case UserModelMessage user:
                    {
                        var content = new JsonArray();
                        foreach (var part in user.Content)
                        {
                            content.Add(part switch
                            {
                                TextContentPart text => WithProviderOptions(new JsonObject { ["text"] = text.Text, ["type"] = "text" }, text.ProviderOptions),
                                FileContentPart file => RenderFile(file.MediaType, file.FileName, file.Url, file.ProviderReference, file.ProviderOptions),
                                _ => throw new InvalidOperationException(part.GetType().Name),
                            });
                        }

                        array.Add(new JsonObject { ["content"] = content, ["role"] = "user" });
                        break;
                    }

                case AssistantModelMessage assistant:
                    {
                        var content = new JsonArray();
                        foreach (var part in assistant.Parts)
                        {
                            content.Add(RenderAssistantPart(part));
                        }

                        array.Add(new JsonObject { ["content"] = content, ["role"] = "assistant" });
                        break;
                    }

                case ToolApprovalResponseModelMessage response:
                    {
                        toolContent ??= NewToolMessage(array);
                        var node = new JsonObject { ["approvalId"] = response.ApprovalId, ["approved"] = response.Approved, ["type"] = "tool-approval-response" };
                        if (response.Reason != null)
                        {
                            node["reason"] = response.Reason;
                        }

                        if (response.ProviderExecuted)
                        {
                            node["providerExecuted"] = true;
                        }

                        toolContent.Add(node);
                        break;
                    }

                case ToolModelMessage tool:
                    {
                        toolContent ??= NewToolMessage(array);
                        foreach (var result in tool.Results)
                        {
                            toolContent.Add(RenderToolResult(result));
                        }

                        break;
                    }

                default:
                    throw new InvalidOperationException(message.GetType().Name);
            }
        }

        return array;
    }

    internal static void AssertEqual(JsonNode expected, IReadOnlyList<ModelMessage> actual)
    {
        var rendered = Render(actual);
        Assert.True(JsonNode.DeepEquals(expected, rendered), "Expected: " + expected.ToJsonString() + "\nActual: " + rendered.ToJsonString());
    }

    internal static void AssertEqual(string expectedJson, IReadOnlyList<ModelMessage> actual)
    {
        AssertEqual(JsonNode.Parse(expectedJson)!, actual);
    }

    private static JsonArray NewToolMessage(JsonArray array)
    {
        var content = new JsonArray();
        array.Add(new JsonObject { ["content"] = content, ["role"] = "tool" });
        return content;
    }

    private static JsonObject RenderAssistantPart(AssistantContentPart part)
    {
        switch (part)
        {
            case AssistantTextPart text:
                return WithProviderOptions(new JsonObject { ["text"] = text.Text, ["type"] = "text" }, text.ProviderOptions);
            case AssistantReasoningPart reasoning:
                return WithProviderOptions(new JsonObject { ["text"] = reasoning.Text, ["type"] = "reasoning" }, reasoning.ProviderOptions);
            case AssistantReasoningFilePart reasoningFile:
                return WithProviderOptions(new JsonObject { ["data"] = new JsonObject { ["type"] = "url", ["url"] = reasoningFile.Url }, ["mediaType"] = reasoningFile.MediaType, ["type"] = "reasoning-file" }, reasoningFile.ProviderOptions);
            case AssistantFilePart file:
                return RenderFile(file.MediaType, file.FileName, file.Url, file.ProviderReference, file.ProviderOptions);
            case AssistantToolCallPart call:
                return WithProviderOptions(
                    new JsonObject
                    {
                        ["input"] = ToolInput(call.ToolCall.ArgumentsJson),
                        ["toolCallId"] = call.ToolCall.ToolCallId,
                        ["toolName"] = call.ToolCall.ToolName,
                        ["type"] = "tool-call",
                    },
                    call.ProviderOptions);
            case AssistantToolApprovalRequestPart request:
                {
                    var node = new JsonObject { ["approvalId"] = request.ApprovalId, ["toolCallId"] = request.ToolCallId, ["type"] = "tool-approval-request" };
                    if (request.IsAutomatic != null)
                    {
                        node["isAutomatic"] = request.IsAutomatic;
                    }

                    if (request.Reason != null)
                    {
                        node["reason"] = request.Reason;
                    }

                    if (request.InputSchemaInput is { } inputSchemaInput)
                    {
                        node["inputSchemaInput"] = JsonNode.Parse(inputSchemaInput.GetRawText());
                    }

                    if (request.Signature != null)
                    {
                        node["signature"] = request.Signature;
                    }

                    return node;
                }

            default:
                throw new InvalidOperationException(part.Type);
        }
    }

    private static JsonNode? ToolInput(string argumentsJson)
    {
        try
        {
            return JsonNode.Parse(argumentsJson);
        }
        catch (JsonException)
        {
            // Not JSON: the input is the string itself.
            return argumentsJson;
        }
    }

    private static JsonObject RenderFile(
        string mediaType,
        string? fileName,
        string? url,
        IReadOnlyDictionary<string, string>? providerReference,
        IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        var node = new JsonObject
        {
            ["data"] = providerReference != null
                ? new JsonObject { ["reference"] = JsonSerializer.SerializeToNode(providerReference), ["type"] = "reference" }
                : new JsonObject { ["type"] = "url", ["url"] = url },
            ["mediaType"] = mediaType,
            ["type"] = "file",
        };
        if (fileName != null)
        {
            node["filename"] = fileName;
        }

        return WithProviderOptions(node, providerOptions);
    }

    private static JsonObject RenderToolResult(ToolModelMessage result)
    {
        var output = new JsonObject { ["type"] = result.OutputType };
        output["value"] = result.OutputType is "json" or "error-json" ? JsonNode.Parse(result.OutputJson) : result.OutputJson;
        return WithProviderOptions(
            new JsonObject
            {
                ["output"] = output,
                ["toolCallId"] = result.ToolCallId,
                ["toolName"] = result.ToolName,
                ["type"] = "tool-result",
            },
            result.ProviderOptions);
    }

    private static JsonObject WithProviderOptions(JsonObject node, IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        if (providerOptions != null)
        {
            node["providerOptions"] = JsonSerializer.SerializeToNode(providerOptions);
        }

        return node;
    }
}

/// <summary>Stands in for upstream's <c>globalThis.AI_SDK_LOG_WARNINGS = false</c> in <c>beforeEach</c> and its <c>delete</c> in <c>afterEach</c>.</summary>
public abstract class WarningsOff : IDisposable
{
    private readonly object? _previous = Vercel.AI.Util.LogWarnings.Logger;

    protected WarningsOff()
    {
        Vercel.AI.Util.LogWarnings.Logger = false;
    }

    public void Dispose()
    {
        Vercel.AI.Util.LogWarnings.Logger = _previous;
    }
}
