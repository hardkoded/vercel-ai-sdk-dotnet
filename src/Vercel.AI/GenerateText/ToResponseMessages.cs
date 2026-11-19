// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>Maps to <c>to-response-messages.ts</c>: converts the content of a step to response messages.</summary>
internal static class ToResponseMessages
{
    /// <summary>
    /// Converts the content of a step to response messages: the assistant message and one tool message with the approval
    /// responses and the results in the order of the tool calls. Maps to <c>toResponseMessages</c>.
    /// </summary>
    internal static List<ModelMessage> Convert(
        IReadOnlyList<GeneratedContent> inputContent,
        IReadOnlyList<Tool>? tools
    )
    {
        var responseMessages = new List<ModelMessage>();
        var toolCallOrder = new Dictionary<string, int>(StringComparer.Ordinal);

        var content = ToAssistantContent(inputContent, toolCallOrder);
        if (content.Count > 0)
        {
            responseMessages.Add(new AssistantModelMessage(content));
        }

        var approvalResponses = new List<ToolApprovalResponseModelMessage>();
        var toolResultContent = new List<ToolModelMessage>();
        foreach (var part in inputContent)
        {
            switch (part)
            {
                case GeneratedToolApprovalResponse approvalResponse:
                    approvalResponses.Add(
                        new ToolApprovalResponseModelMessage(
                            approvalResponse.ApprovalId,
                            approvalResponse.Approved,
                            approvalResponse.Reason,
                            approvalResponse.ProviderExecuted
                        )
                    );

                    // When the tool approval is denied, there is no tool result for the call, so it gets an execution-denied one.
                    if (!approvalResponse.Approved)
                    {
                        toolResultContent.Add(
                            new ToolModelMessage(
                                approvalResponse.ToolCall.ToolCallId,
                                approvalResponse.ToolCall.ToolName,
                                approvalResponse.Reason,
                                false,
                                "execution-denied"
                            )
                        );
                    }

                    break;
                case GeneratedToolResult { ProviderExecuted: false } result:
                    toolResultContent.Add(CreateToolMessage(result, tools));
                    break;
            }
        }

        if (toolResultContent.Count > 0)
        {
            var sorted = SortToolResultContentByToolCallOrder(toolResultContent, toolCallOrder);
            var toolMessage = sorted.Count == 1 ? sorted[0] : new ToolModelMessage(sorted);
            responseMessages.Add(
                approvalResponses.Count > 0
                    ? new ToolModelMessage(toolMessage.Results)
                    {
                        ApprovalResponses = approvalResponses,
                    }
                    : toolMessage
            );
        }
        else if (approvalResponses.Count > 0)
        {
            responseMessages.Add(new ToolModelMessage(approvalResponses));
        }

        return responseMessages;
    }

    /// <summary>
    /// Builds the content of the assistant message from the content of a step, in order. Maps to the assistant
    /// <c>content</c> that <c>toResponseMessages</c> builds. Sources, empty text without metadata, and the results of client tools are not part of it.
    /// </summary>
    internal static List<AssistantContentPart> ToAssistantContent(
        IReadOnlyList<GeneratedContent> inputContent
    ) => ToAssistantContent(inputContent, new Dictionary<string, int>(StringComparer.Ordinal));

    private static List<AssistantContentPart> ToAssistantContent(
        IReadOnlyList<GeneratedContent> inputContent,
        Dictionary<string, int> toolCallOrder
    )
    {
        var content = new List<AssistantContentPart>();
        foreach (var part in inputContent)
        {
            // Skip empty text without provider metadata.
            if (part is GeneratedText { Text.Length: 0, ProviderMetadata: null })
            {
                continue;
            }

            switch (part)
            {
                case GeneratedToolApprovalRequest approvalRequest:
                    content.Add(
                        new AssistantToolApprovalRequestPart(
                            approvalRequest.ApprovalId,
                            approvalRequest.ToolCallId,
                            approvalRequest.IsAutomatic ? true : null
                        )
                        {
                            Reason = approvalRequest.Reason,
                            Signature = approvalRequest.Signature,
                            InputSchemaInput = ChangedInputSchemaInput(approvalRequest.ToolCall),
                        }
                    );
                    break;
                case GeneratedToolCall call:
                    if (!toolCallOrder.ContainsKey(call.ToolCallId))
                    {
                        toolCallOrder[call.ToolCallId] = toolCallOrder.Count;
                    }

                    content.Add(new AssistantToolCallPart(SanitizeInvalidInput(call)));
                    break;
                case GeneratedText generatedText:
                    content.Add(
                        new AssistantTextPart(
                            generatedText.Text,
                            Generation.ToProviderOptions(generatedText.ProviderMetadata)
                        )
                    );
                    break;
                case GeneratedFile file:
                    content.Add(new AssistantFilePart(file.MediaType, null, file.Data));
                    break;
                case GeneratedToolResult { ProviderExecuted: true } providerResult:
                    // The provider ran the tool, so the next step replays the result in the same assistant message.
                    content.Add(
                        new AssistantToolResultPart(
                            providerResult.ToolCallId,
                            providerResult.ToolName,
                            new ToolModelMessage(
                                providerResult.ToolCallId,
                                providerResult.ToolName,
                                providerResult.ResultJson,
                                providerResult.IsError
                            ),
                            Generation.ToProviderOptions(providerResult.ProviderMetadata)
                        )
                    );
                    break;
                case GeneratedCustom custom:
                    content.Add(
                        new AssistantCustomPart(
                            custom.Kind,
                            Generation.ToProviderOptions(custom.ProviderMetadata)
                        )
                    );
                    break;
                case GeneratedReasoning generatedReasoning:
                    content.Add(
                        new AssistantReasoningPart(
                            generatedReasoning.Text,
                            Generation.ToProviderOptions(generatedReasoning.ProviderMetadata)
                        )
                    );
                    break;
                case GeneratedReasoningFile reasoningFile:
                    content.Add(
                        new AssistantReasoningFilePart(
                            reasoningFile.MediaType,
                            null,
                            reasoningFile.Data,
                            Generation.ToProviderOptions(reasoningFile.ProviderMetadata)
                        )
                    );
                    break;
            }
        }

        return content;
    }

    // The input before the schema transformed it, when it differs from the approved input. Maps to the inputSchemaInput check.
    private static JsonElement? ChangedInputSchemaInput(GeneratedToolCall? toolCall)
    {
        if (toolCall?.InputSchemaInput is not { } schemaInput)
        {
            return null;
        }

        using var input = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(toolCall.ArgumentsJson) ? "{}" : toolCall.ArgumentsJson
        );
        return DeepEqual.IsDeepEqualData(schemaInput, input.RootElement) ? null : schemaInput;
    }

    // An invalid call whose input is not an object is replayed with an empty object, as upstream does.
    private static GeneratedToolCall SanitizeInvalidInput(GeneratedToolCall call)
    {
        if (!call.Invalid)
        {
            return call;
        }

        try
        {
            using var document = JsonDocument.Parse(call.ArgumentsJson);
            if (
                document.RootElement.ValueKind
                is JsonValueKind.Object
                    or JsonValueKind.Array
                    or JsonValueKind.Null
            )
            {
                return call;
            }
        }
        catch (JsonException) { }

        return new GeneratedToolCall(call.ToolCallId, call.ToolName, "{}", call.ProviderMetadata)
        {
            ProviderExecuted = call.ProviderExecuted,
            Dynamic = call.Dynamic,
            Invalid = call.Invalid,
            Error = call.Error,
            Title = call.Title,
            ToolMetadata = call.ToolMetadata,
            InputSchemaInput = call.InputSchemaInput,
        };
    }

    // The error of a result without an exception: a JSON string is the message itself, as the error-text mode sends a string error.
    private static string ErrorText(GeneratedToolResult result)
    {
        try
        {
            using var document = JsonDocument.Parse(result.ResultJson);
            if (document.RootElement.ValueKind == JsonValueKind.String)
            {
                return document.RootElement.GetString() ?? string.Empty;
            }
        }
        catch (JsonException) { }

        return result.ResultJson;
    }

    // Maps to the createToolModelOutput step: the model output of a client tool result or error.
    internal static ToolModelMessage CreateToolMessage(
        GeneratedToolResult result,
        IReadOnlyList<Tool>? tools
    )
    {
        if (result.IsError)
        {
            return new ToolModelMessage(
                result.ToolCallId,
                result.ToolName,
                result.Error != null ? ErrorMessage.ToErrorString(result.Error) : ErrorText(result),
                true,
                "error-text"
            );
        }

        var tool = tools?.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, result.ToolName, StringComparison.Ordinal)
        );
        if (tool?.ToModelOutput == null)
        {
            return new ToolModelMessage(
                result.ToolCallId,
                result.ToolName,
                result.ResultJson,
                false
            );
        }

        using var input = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(result.InputJson) ? "{}" : result.InputJson
        );
        var output = tool.ToModelOutput(
            new ToolModelOutputCall(result.ToolCallId, input.RootElement.Clone(), result.ResultJson)
        );
        return output is ContentToolModelOutput contentOutput
            ? new ToolModelMessage(
                result.ToolCallId,
                result.ToolName,
                Generation.ToUserContent(contentOutput.Value)
            )
            : new ToolModelMessage(
                result.ToolCallId,
                result.ToolName,
                Generation.ModelOutputText(output) ?? result.ResultJson,
                false,
                output.Type
            );
    }

    private static List<ToolModelMessage> SortToolResultContentByToolCallOrder(
        List<ToolModelMessage> toolResultContent,
        Dictionary<string, int> toolCallOrder
    ) =>
        toolResultContent
            .Select((message, index) => (Message: message, Index: index))
            .OrderBy(entry =>
                toolCallOrder.TryGetValue(entry.Message.ToolCallId, out var position)
                    ? position
                    : int.MaxValue
            )
            .ThenBy(entry => entry.Index)
            .Select(entry => entry.Message)
            .ToList();
}
