// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenTelemetry;

/// <summary>Maps to <c>gen-ai-format-messages.ts</c>. Each method returns the GenAI SemConv value of a message list.</summary>
internal static class GenAiFormatMessages
{
    /// <summary>Maps to <c>formatSystemInstructions</c>.</summary>
    public static List<Dictionary<string, object?>> FormatSystemInstructions(Instructions system) =>
        system.Text is { } text
            ? [TextInstruction(text)]
            : [.. (system.Messages ?? []).Select(message => TextInstruction(message.Content))];

    /// <summary>Maps to <c>formatModelMessages</c>.</summary>
    public static List<Dictionary<string, object?>> FormatModelMessages(
        string? prompt,
        IReadOnlyList<ModelMessage>? messages
    )
    {
        var result = new List<Dictionary<string, object?>>();
        if (prompt is not null)
        {
            result.Add(Message("user", [Text(prompt)]));
        }

        if (messages is not null)
        {
            foreach (var message in messages)
            {
                result.AddRange(ConvertModelMessageToSemConv(message));
            }
        }

        return result;
    }

    /// <summary>Maps to <c>formatInputMessages</c> for the prompt messages of a model call.</summary>
    public static List<Dictionary<string, object?>> FormatInputMessages(
        IEnumerable<ModelMessage> prompt
    ) => [.. prompt.SelectMany(ConvertModelMessageToSemConv)];

    /// <summary>Maps to <c>formatObjectOutputMessages</c>.</summary>
    public static List<Dictionary<string, object?>> FormatObjectOutputMessages(
        string objectText,
        FinishReason finishReason
    ) =>
        [
            new(StringComparer.Ordinal)
            {
                ["role"] = "assistant",
                ["parts"] = new List<Dictionary<string, object?>> { Text(objectText) },
                ["finish_reason"] = MapFinishReason(finishReason),
            },
        ];

    /// <summary>Maps to <c>mapFinishReason</c>.</summary>
    private static string MapFinishReason(FinishReason finishReason) =>
        finishReason switch
        {
            FinishReason.Length => "length",
            FinishReason.ContentFilter => "content_filter",
            FinishReason.ToolCalls => "tool_call",
            FinishReason.Error => "error",
            FinishReason.Interrupted => "interrupted",
            _ => "stop",
        };

    private static Dictionary<string, object?> TextInstruction(string content) =>
        new(StringComparer.Ordinal) { ["type"] = "text", ["content"] = content };

    private static Dictionary<string, object?> Text(string content) =>
        new(StringComparer.Ordinal) { ["type"] = "text", ["content"] = content };

    private static Dictionary<string, object?> Message(
        string role,
        List<Dictionary<string, object?>> parts
    ) => new(StringComparer.Ordinal) { ["role"] = role, ["parts"] = parts };

    private static IEnumerable<Dictionary<string, object?>> ConvertModelMessageToSemConv(
        ModelMessage message
    )
    {
        switch (message)
        {
            case SystemModelMessage system:
                yield return Message("system", [Text(system.Content)]);
                break;
            case UserModelMessage user:
                yield return Message("user", [.. user.Content.Select(UserPart)]);
                break;
            case AssistantModelMessage assistant:
                yield return Message("assistant", [.. assistant.Parts.Select(AssistantPart)]);
                break;
            case ToolModelMessage tool:
                var parts = new List<Dictionary<string, object?>>();
                parts.AddRange(tool.ApprovalResponses.Select(ApprovalResponse));
                if (tool.ApprovalResponses.Count == 0 || tool.Results.Count > 0)
                {
                    parts.AddRange(
                        tool.Results.Where(r => r.ToolCallId.Length > 0).Select(ToolResult)
                    );
                }

                yield return Message("tool", parts);
                break;
            case ToolApprovalResponseModelMessage approval:
                yield return Message("tool", [ApprovalResponse(approval)]);
                break;
        }
    }

    private static Dictionary<string, object?> ApprovalResponse(
        ToolApprovalResponseModelMessage approval
    ) =>
        new(StringComparer.Ordinal)
        {
            ["type"] = "tool_approval_response",
            ["approval_id"] = approval.ApprovalId,
            ["approved"] = approval.Approved,
            ["reason"] = approval.Reason,
        };

    private static Dictionary<string, object?> ToolResult(ToolModelMessage result) =>
        new(StringComparer.Ordinal)
        {
            ["type"] = "tool_call_response",
            ["id"] = result.ToolCallId,
            ["response"] = ConvertToolResultOutputToResponse(result),
        };

    /// <summary>Maps to <c>convertToolResultOutputToResponse</c>.</summary>
    private static object? ConvertToolResultOutputToResponse(ToolModelMessage result) =>
        result.OutputType switch
        {
            "text" or "error-text" => result.OutputJson,
            "json" or "error-json" => ParseJson(result.OutputJson),
            "execution-denied" => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["denied"] = true,
                ["reason"] = result.ExecutionDeniedReason,
            },
            _ => result.OutputJson,
        };

    private static object? ParseJson(string json)
    {
        try
        {
            return JsonDocument.Parse(json).RootElement.Clone();
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private static Dictionary<string, object?> AssistantPart(AssistantContentPart part) =>
        part switch
        {
            AssistantTextPart text => Text(text.Text),
            AssistantReasoningPart reasoning => new(StringComparer.Ordinal)
            {
                ["type"] = "reasoning",
                ["content"] = reasoning.Text,
            },
            AssistantFilePart file => File(
                file.MediaType,
                file.Url,
                file.Data is { } data ? Convert.ToBase64String(data) : null
            ),
            AssistantCustomPart custom => new(StringComparer.Ordinal)
            {
                ["type"] = "custom",
                ["kind"] = custom.Kind,
            },
            AssistantToolCallPart call => new(StringComparer.Ordinal)
            {
                ["type"] = "tool_call",
                ["id"] = call.ToolCall.ToolCallId,
                ["name"] = call.ToolCall.ToolName,
                ["arguments"] = ParseJson(call.ToolCall.ArgumentsJson),
            },
            AssistantToolResultPart result => new(StringComparer.Ordinal)
            {
                ["type"] = "tool_call_response",
                ["id"] = result.ToolCallId,
                ["response"] = ConvertToolResultOutputToResponse(result.Output),
            },
            AssistantToolApprovalRequestPart request => new(StringComparer.Ordinal)
            {
                ["type"] = "tool_approval_request",
                ["approval_id"] = request.ApprovalId,
                ["tool_call_id"] = request.ToolCallId,
                ["reason"] = request.Reason,
            },
            _ => new(StringComparer.Ordinal) { ["type"] = part.Type },
        };

    private static Dictionary<string, object?> UserPart(UserContentPart part) =>
        part switch
        {
            TextContentPart text => Text(text.Text),
            ImageUrlContentPart image => Uri("image", null, image.Url),
            ImageDataContentPart image => IsHttp(image.Data)
                ? Uri("image", image.MediaType, image.Data)
                : Blob("image", image.MediaType, image.Data),
            FileContentPart file => File(
                file.MediaType,
                file.Url,
                file.Data is { } data ? Convert.ToBase64String(data) : file.Text
            ),
            FileUrlContentPart file => Uri(Modality(file.MediaType), file.MediaType, file.Url),
            FileDataContentPart file => IsHttp(file.Data)
                ? Uri(Modality(file.MediaType), file.MediaType, file.Data)
                : Blob(Modality(file.MediaType), file.MediaType, file.Data),
            _ => new(StringComparer.Ordinal) { ["type"] = part.Type },
        };

    /// <summary>Maps to <c>convertFileDataToSemConv</c>.</summary>
    private static Dictionary<string, object?> File(
        string? mediaType,
        string? url,
        string? content
    ) =>
        url is not null ? Uri(Modality(mediaType), mediaType, url)
        : IsHttp(content) ? Uri(Modality(mediaType), mediaType, content!)
        : Blob(Modality(mediaType), mediaType, content ?? string.Empty);

    private static Dictionary<string, object?> Uri(
        string modality,
        string? mediaType,
        string uri
    ) =>
        new(StringComparer.Ordinal)
        {
            ["type"] = "uri",
            ["modality"] = modality,
            ["mime_type"] = mediaType,
            ["uri"] = uri,
        };

    private static Dictionary<string, object?> Blob(
        string modality,
        string? mediaType,
        string content
    ) =>
        new(StringComparer.Ordinal)
        {
            ["type"] = "blob",
            ["modality"] = modality,
            ["mime_type"] = mediaType,
            ["content"] = content,
        };

    private static bool IsHttp(string? data) =>
        data is not null
        && (
            data.StartsWith("http://", StringComparison.Ordinal)
            || data.StartsWith("https://", StringComparison.Ordinal)
        );

    /// <summary>Maps to <c>getModality</c>.</summary>
    private static string Modality(string? mediaType) =>
        string.IsNullOrEmpty(mediaType) ? "image"
        : mediaType.StartsWith("video/", StringComparison.Ordinal) ? "video"
        : mediaType.StartsWith("audio/", StringComparison.Ordinal) ? "audio"
        : "image";
}
