// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals;

internal static class ValidateToolApprovalsSupport
{
    public const string ValueSchema = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    public const string TransformNote = "A Zod transform has no JSON Schema equivalent. A refineToolInput function stands in for it, so the schema input and the approved input differ the way a transform makes them differ.";

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static Tool ExecutableTool(string name = "tool1", string schema = ValueSchema) =>
        Tool.Function(name, null, schema, (_, _) => Task.FromResult("ok"));

    public static CollectedToolApproval CreateApproval(string toolName, string inputJson, string? signature = null, string? inputSchemaInputJson = null, string toolCallId = "call-1", string approvalId = "approval-1") =>
        new(
            new ApprovalRequestPart(approvalId, toolCallId)
            {
                Signature = signature,
                InputSchemaInput = inputSchemaInputJson == null ? null : Json(inputSchemaInputJson),
            },
            new ApprovalResponsePart(approvalId, true),
            new ApprovalToolCallPart(toolCallId, toolName, Json(inputJson)));

    public static Task<ValidatedToolApprovals> ValidateAsync(
        CollectedToolApproval approval,
        IReadOnlyList<Tool> tools,
        ToolApprovalConfiguration? toolApproval = null,
        string? secret = null,
        IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? refine = null) =>
        ToolApprovalValidation.ValidateApprovedToolApprovalsAsync(
            new ValidateApprovedToolApprovalsOptions(new[] { approval })
            {
                Tools = tools,
                ToolApproval = toolApproval,
                ToolApprovalSecret = secret,
                RefineToolInput = refine,
            });
}
