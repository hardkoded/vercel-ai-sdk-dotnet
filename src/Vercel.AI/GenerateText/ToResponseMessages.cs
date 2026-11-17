// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.GenerateText;

/// <summary>Maps to <c>to-response-messages.ts</c>: converts the content of a step to response messages.</summary>
internal static class ToResponseMessages
{
    /// <summary>
    /// Builds the content of the assistant message from the content of a step, in order. Maps to the assistant
    /// <c>content</c> that <c>toResponseMessages</c> builds. Sources and files are not part of it. The content is the model's own, so every tool result in it is provider-executed.
    /// </summary>
    internal static List<AssistantContentPart> ToAssistantContent(
        IReadOnlyList<GeneratedContent> inputContent
    )
    {
        var content = new List<AssistantContentPart>();
        foreach (var part in inputContent)
        {
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
                        }
                    );
                    break;
                case GeneratedToolCall call:
                    content.Add(new AssistantToolCallPart(call));
                    break;
                case GeneratedText generatedText:
                    content.Add(
                        new AssistantTextPart(
                            generatedText.Text,
                            Generation.ToProviderOptions(generatedText.ProviderMetadata)
                        )
                    );
                    break;
                case GeneratedToolResult providerResult:
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
}
