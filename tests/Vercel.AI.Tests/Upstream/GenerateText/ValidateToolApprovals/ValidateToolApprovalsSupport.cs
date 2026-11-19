// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals;

internal static class ValidateToolApprovalsSupport
{
    public const string ValueSchema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static Tool ExecutableTool(string name = "tool1", string schema = ValueSchema) =>
        Tool.Function(name, null, schema, (_, _) => Task.FromResult("ok"));

    public static CollectedToolApproval CreateApproval(
        string toolName,
        string inputJson,
        string? signature = null,
        string? inputSchemaInputJson = null,
        string toolCallId = "call-1",
        string approvalId = "approval-1"
    ) =>
        new(
            new ApprovalRequestPart(approvalId, toolCallId)
            {
                Signature = signature,
                InputSchemaInput = inputSchemaInputJson == null ? null : Json(inputSchemaInputJson),
            },
            new ApprovalResponsePart(approvalId, true),
            new ApprovalToolCallPart(toolCallId, toolName, Json(inputJson))
        );

    public static Task<ValidatedToolApprovals> ValidateAsync(
        CollectedToolApproval approval,
        IReadOnlyList<Tool> tools,
        ToolApprovalConfiguration? toolApproval = null,
        string? secret = null,
        IReadOnlyDictionary<
            string,
            Func<JsonElement, CancellationToken, Task<JsonElement>>
        >? refine = null,
        (string ToolName, Func<JsonElement, JsonElement> Transform)? transform = null
    )
    {
        if (transform is { } t)
        {
            // upstream gives the tool a schema with a transform. Here the tool carries the transform
            foreach (var tool in tools.Where(tool => tool.Name == t.ToolName))
            {
                tool.InputTransform = t.Transform;
            }
        }

        return ToolApprovalValidation.ValidateApprovedToolApprovalsAsync(
            new ValidateApprovedToolApprovalsOptions(new[] { approval })
            {
                Tools = tools,
                ToolApproval = toolApproval,
                ToolApprovalSecret = secret,
                RefineToolInput = refine,
            }
        );
    }

    public static (string ToolName, Func<JsonElement, JsonElement> Transform) Transform(
        string toolName,
        Func<JsonElement, JsonElement> transform
    ) => (toolName, transform);
}
