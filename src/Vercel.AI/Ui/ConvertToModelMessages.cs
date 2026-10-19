// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Ui;

/// <summary>Options of <see cref="UIMessageConversion.ConvertToModelMessages"/>.</summary>
public sealed class ConvertToModelMessagesOptions
{
    /// <summary>The tools to use, by name.</summary>
    public IReadOnlyDictionary<string, Tool>? Tools { get; set; }

    /// <summary>Whether to ignore incomplete tool calls. Default is <c>false</c>.</summary>
    public bool IgnoreIncompleteToolCalls { get; set; }

    /// <summary>Converts a data part to a text or file part. Returns null if the part should be ignored.</summary>
    public Func<DataUIPart, UserContentPart?>? ConvertDataPart { get; set; }
}

/// <summary>Conversion of UI messages to model messages. Maps to <c>convertToModelMessages</c>.</summary>
public static class UIMessageConversion
{
    /// <summary>
    /// Converts UI messages from a chat UI into model messages that can be used with the AI functions.
    /// A tool message upstream holds several parts. Here its parts are consecutive tool messages.
    /// Assistant file and reasoning-file parts are dropped, as assistant messages here cannot hold files.
    /// </summary>
    /// <param name="messages">The UI messages to convert.</param>
    /// <param name="options">Conversion options.</param>
    /// <returns>The model messages.</returns>
    public static IReadOnlyList<ModelMessage> ConvertToModelMessages(IReadOnlyList<UIMessage> messages, ConvertToModelMessagesOptions? options = null)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var modelMessages = new List<ModelMessage>();

        // A later user message supersedes unresolved approval requests. Keeping
        // those requests would leave unmatched tool calls at the user boundary.
        var lastUserMessageIndex = -1;
        for (var index = 0; index < messages.Count; index++)
        {
            if (messages[index].Role == UIMessageRole.User)
            {
                lastUserMessageIndex = index;
            }
        }

        var ignoreIncomplete = options?.IgnoreIncompleteToolCalls == true;
        var filtered = new List<IReadOnlyList<UIMessagePart>>(messages.Count);
        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            var parts = messages[messageIndex].Parts;
            if (ignoreIncomplete || lastUserMessageIndex > 0)
            {
                var kept = new List<UIMessagePart>();
                foreach (var part in parts)
                {
                    if (part is not ToolUIPart tool
                        || ((tool.State != ToolUIPartState.ApprovalRequested || messageIndex >= lastUserMessageIndex)
                            && (!ignoreIncomplete
                                || tool.State == ToolUIPartState.ApprovalResponded
                                || (tool.State == ToolUIPartState.OutputAvailable && tool.Preliminary != true)
                                || tool.State == ToolUIPartState.OutputError
                                || tool.State == ToolUIPartState.OutputDenied)))
                    {
                        kept.Add(part);
                    }
                }

                parts = kept;
            }

            filtered.Add(parts);
        }

        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            var message = messages[messageIndex];
            var parts = filtered[messageIndex];
            switch (message.Role)
            {
                case UIMessageRole.System:
                    modelMessages.Add(ConvertSystem(parts));
                    break;
                case UIMessageRole.User:
                    modelMessages.Add(ConvertUser(parts, options));
                    break;
                case UIMessageRole.Assistant:
                    ConvertAssistant(parts, options, modelMessages);
                    break;
                default:
                    throw new MessageConversionException(message, "Unsupported role: " + message.Role);
            }
        }

        return modelMessages;
    }

    private static SystemModelMessage ConvertSystem(IReadOnlyList<UIMessagePart> parts)
    {
        var text = new System.Text.StringBuilder();
        var providerOptions = new Dictionary<string, JsonElement>();
        foreach (var part in parts)
        {
            if (part is TextUIPart textPart)
            {
                text.Append(textPart.Text);
                if (textPart.ProviderMetadata != null)
                {
                    foreach (var pair in textPart.ProviderMetadata)
                    {
                        providerOptions[pair.Key] = pair.Value;
                    }
                }
            }
        }

        return new SystemModelMessage(text.ToString(), providerOptions.Count > 0 ? providerOptions : null);
    }

    private static UserModelMessage ConvertUser(IReadOnlyList<UIMessagePart> parts, ConvertToModelMessagesOptions? options)
    {
        var content = new List<UserContentPart>();
        foreach (var part in parts)
        {
            switch (part)
            {
                case TextUIPart text:
                    content.Add(new TextContentPart(text.Text) { ProviderOptions = text.ProviderMetadata });
                    break;
                case FileUIPart file:
                    content.Add(ToFileContentPart(file));
                    break;
                case DataUIPart data:
                    if (options?.ConvertDataPart?.Invoke(data) is { } converted)
                    {
                        content.Add(converted);
                    }

                    break;
            }
        }

        return new UserModelMessage(content);
    }

    private static FileContentPart ToFileContentPart(FileUIPart file)
    {
        return file.ProviderReference != null
            ? FileContentPart.FromProviderReference(file.MediaType, file.ProviderReference, file.Filename, file.ProviderMetadata)
            : new FileContentPart(file.MediaType, file.Url, null, file.Filename) { ProviderOptions = file.ProviderMetadata };
    }

    private static void ConvertAssistant(IReadOnlyList<UIMessagePart> parts, ConvertToModelMessagesOptions? options, List<ModelMessage> modelMessages)
    {
        var block = new List<UIMessagePart>();

        foreach (var part in parts)
        {
            if (part is StepStartUIPart)
            {
                ProcessBlock(block, options, modelMessages);
                block = new List<UIMessagePart>();
            }
            else if (part is CustomContentUIPart or TextUIPart or ReasoningUIPart or ReasoningFileUIPart or FileUIPart or ToolUIPart or DataUIPart)
            {
                block.Add(part);
            }
        }

        ProcessBlock(block, options, modelMessages);
    }

    private static void ProcessBlock(List<UIMessagePart> block, ConvertToModelMessagesOptions? options, List<ModelMessage> modelMessages)
    {
        if (block.Count == 0)
        {
            return;
        }

        var content = new List<AssistantContentPart>();

        foreach (var part in block)
        {
            switch (part)
            {
                case TextUIPart text:
                    content.Add(new AssistantTextPart(text.Text, text.ProviderMetadata));
                    break;
                case CustomContentUIPart custom:
                    content.Add(new AssistantCustomPart(custom.Kind, custom.ProviderMetadata));
                    break;
                case ReasoningUIPart reasoning:
                    content.Add(new AssistantReasoningPart(reasoning.Text, reasoning.ProviderMetadata));
                    break;
                case ToolUIPart tool when tool.State != ToolUIPartState.InputStreaming:
                    AddToolContent(tool, options, content);
                    break;
                case DataUIPart data:
                    if (options?.ConvertDataPart?.Invoke(data) is TextContentPart dataText)
                    {
                        content.Add(new AssistantTextPart(dataText.Text, dataText.ProviderOptions));
                    }

                    break;
            }
        }

        if (content.Count > 0)
        {
            modelMessages.Add(new AssistantModelMessage(content));
        }

        // tool messages with tool results
        // Include non-provider-executed tools, OR provider-executed tools with approval responses
        foreach (var part in block)
        {
            if (part is ToolUIPart tool && (tool.ProviderExecuted != true || tool.Approval?.Approved != null))
            {
                AddToolResults(tool, options, modelMessages);
            }
        }
    }

    private static void AddToolContent(ToolUIPart tool, ConvertToModelMessagesOptions? options, List<AssistantContentPart> content)
    {
        var callProviderMetadata = tool.CallProviderMetadata
            ?? (tool.State == ToolUIPartState.OutputError ? tool.ResultProviderMetadata : null);

        var input = tool.State == ToolUIPartState.OutputError ? tool.Input ?? tool.RawInput : tool.Input;
        var call = new GeneratedToolCall(tool.ToolCallId, tool.ToolName, input?.GetRawText() ?? "{}")
        {
            ProviderExecuted = tool.ProviderExecuted == true,
            Dynamic = tool.IsDynamic && tool.Dynamic != false,
        };
        content.Add(new AssistantToolCallPart(call, callProviderMetadata));

        if (tool.Approval != null)
        {
            content.Add(new AssistantToolApprovalRequestPart(tool.Approval.Id, tool.ToolCallId, tool.Approval.IsAutomatic));
        }

        if (tool.ProviderExecuted == true
            && tool.State != ToolUIPartState.ApprovalResponded
            && (tool.State == ToolUIPartState.OutputAvailable || tool.State == ToolUIPartState.OutputError))
        {
            var resultProviderMetadata = tool.ResultProviderMetadata ?? tool.CallProviderMetadata;
            var isError = tool.State == ToolUIPartState.OutputError;
            var output = CreateModelOutput(tool, options, isError ? JsonString(tool.ErrorText) : tool.Output, isError ? "json" : "none");
            content.Add(new AssistantToolResultPart(tool.ToolCallId, tool.ToolName, ToResultMessage(tool, output, null), resultProviderMetadata));
        }
    }

    private static void AddToolResults(ToolUIPart tool, ConvertToModelMessagesOptions? options, List<ModelMessage> modelMessages)
    {
        // add approval response for approved tool calls:
        if (tool.Approval?.Approved != null)
        {
            modelMessages.Add(new ToolApprovalResponseModelMessage(
                tool.Approval.Id,
                tool.Approval.Approved.Value,
                tool.Approval.Reason,
                tool.ProviderExecuted == true));
        }

        // add synthetic execution-denied result for denied tool approvals
        if (tool.State == ToolUIPartState.ApprovalResponded && tool.Approval?.Approved == false)
        {
            modelMessages.Add(new ToolModelMessage(tool.ToolCallId, tool.ToolName, tool.Approval.Reason, false, "execution-denied")
            {
                ProviderOptions = tool.CallProviderMetadata,
            });
        }

        // For provider-executed tools, the tool result is already in the
        // assistant content. Skip adding to tool message to avoid duplicates
        // (which would create orphaned function_call_output entries).
        if (tool.ProviderExecuted == true)
        {
            return;
        }

        switch (tool.State)
        {
            case ToolUIPartState.OutputDenied:
                modelMessages.Add(new ToolModelMessage(
                    tool.ToolCallId,
                    tool.ToolName,
                    tool.Approval?.Reason ?? "Tool call execution denied.",
                    true,
                    "error-text")
                {
                    ProviderOptions = tool.CallProviderMetadata,
                });
                break;
            case ToolUIPartState.OutputError:
            case ToolUIPartState.OutputAvailable:
                {
                    var isError = tool.State == ToolUIPartState.OutputError;
                    var output = CreateModelOutput(tool, options, isError ? JsonString(tool.ErrorText) : tool.Output, isError ? "text" : "none");
                    modelMessages.Add(ToResultMessage(tool, output, tool.CallProviderMetadata));
                    break;
                }
        }
    }

    private static JsonElement? JsonString(string? value)
    {
        return value is null ? null : JsonSerializer.SerializeToElement(value);
    }

    private static ToolModelMessage ToResultMessage(ToolUIPart tool, ToolModelOutput output, IReadOnlyDictionary<string, JsonElement>? providerOptions)
    {
        string value;
        switch (output)
        {
            case TextToolModelOutput text:
                value = text.Value;
                break;
            case ErrorTextToolModelOutput errorText:
                value = errorText.Value;
                break;
            case JsonToolModelOutput json:
                value = json.Value.GetRawText();
                break;
            case ErrorJsonToolModelOutput errorJson:
                value = errorJson.Value.GetRawText();
                break;
            default:
                throw new NotSupportedException("Tool output type '" + output.Type + "' is not supported.");
        }

        return new ToolModelMessage(tool.ToolCallId, tool.ToolName, value, output.Type.StartsWith("error", StringComparison.Ordinal), output.Type)
        {
            ProviderOptions = providerOptions,
        };
    }

    private static ToolModelOutput CreateModelOutput(ToolUIPart tool, ConvertToModelMessagesOptions? options, JsonElement? output, string errorMode)
    {
        Tool? definition = null;
        options?.Tools?.TryGetValue(tool.ToolName, out definition);

        if (errorMode == "none" && definition == null && tool.IsDynamic && tool.Dynamic == false)
        {
            return new TextToolModelOutput("Tool output omitted because the tool is no longer available.");
        }

        object? value = output is { } element ? JsonElementValue(element) : null;
        return ToolOutputs.CreateToolModelOutput(tool.ToolCallId, tool.Input, value, output != null, null, errorMode);
    }

    private static object? JsonElementValue(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String ? element.GetString() : element;
    }
}
