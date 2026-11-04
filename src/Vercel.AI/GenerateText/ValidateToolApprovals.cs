// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateText;

/// <summary>An approved tool call whose input failed validation. Maps to a collected approval with an <c>error</c>.</summary>
public sealed class InvalidToolApproval
{
    /// <summary>Creates the entry.</summary>
    public InvalidToolApproval(CollectedToolApproval approval, InvalidToolInputError error)
    {
        Approval = approval ?? throw new ArgumentNullException(nameof(approval));
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    /// <summary>The approval that was collected from the message history.</summary>
    public CollectedToolApproval Approval { get; }

    /// <summary>Why the input is invalid.</summary>
    public InvalidToolInputError Error { get; }
}

/// <summary>The options of <see cref="ToolApprovalValidation.ValidateApprovedToolApprovalsAsync"/>.</summary>
public sealed class ValidateApprovedToolApprovalsOptions
{
    /// <summary>Creates the options.</summary>
    public ValidateApprovedToolApprovalsOptions(IReadOnlyList<CollectedToolApproval> approvedToolApprovals)
    {
        ApprovedToolApprovals = approvedToolApprovals ?? throw new ArgumentNullException(nameof(approvedToolApprovals));
    }

    /// <summary>The approvals that the history reports as approved.</summary>
    public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }

    /// <summary>The available tools.</summary>
    public IReadOnlyList<Tool>? Tools { get; init; }

    /// <summary>User-defined approval configuration. The approval policy is evaluated again on the approved input.</summary>
    public ToolApprovalConfiguration? ToolApproval { get; init; }

    /// <summary>Messages that were sent to the language model, without the instructions.</summary>
    public IReadOnlyList<ModelMessage> Messages { get; init; } = Array.Empty<ModelMessage>();

    /// <summary>Secret for the HMAC signature of the approval requests. Maps to <c>toolApprovalSecret</c> given as a string.</summary>
    public string? ToolApprovalSecret { get; init; }

    /// <summary>The same secret given as bytes. It is used when <see cref="ToolApprovalSecret"/> is not set.</summary>
    public byte[]? ToolApprovalSecretBytes { get; init; }

    /// <summary>Refines the parsed input of a tool, keyed by tool name. Maps to <c>refineToolInput</c>.</summary>
    public IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>? RefineToolInput { get; init; }
}

/// <summary>The outcome of <see cref="ToolApprovalValidation.ValidateApprovedToolApprovalsAsync"/>.</summary>
public sealed class ValidatedToolApprovals
{
    /// <summary>Creates the outcome.</summary>
    public ValidatedToolApprovals(
        IReadOnlyList<CollectedToolApproval> approvedToolApprovals,
        IReadOnlyList<CollectedToolApproval> deniedToolApprovals,
        IReadOnlyList<InvalidToolApproval> invalidToolApprovals)
    {
        ApprovedToolApprovals = approvedToolApprovals;
        DeniedToolApprovals = deniedToolApprovals;
        InvalidToolApprovals = invalidToolApprovals;
    }

    /// <summary>Approvals that may run.</summary>
    public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }

    /// <summary>Approvals that the approval policy denies now, with the denial in their response.</summary>
    public IReadOnlyList<CollectedToolApproval> DeniedToolApprovals { get; }

    /// <summary>Approvals whose input does not match the tool's input schema.</summary>
    public IReadOnlyList<InvalidToolApproval> InvalidToolApprovals { get; }
}

/// <summary>Validates approvals that come from message history. Maps to <c>validateApprovedToolApprovals</c>.</summary>
public static class ToolApprovalValidation
{
    /// <summary>
    /// Re-validates approved tool approvals reconstructed from client-supplied message history before they run.
    /// It checks the HMAC signature (when a secret is set), the input schema, and the approval policy.
    /// A missing or wrong signature throws <see cref="InvalidToolApprovalSignatureError"/>.
    /// </summary>
    public static async Task<ValidatedToolApprovals> ValidateApprovedToolApprovalsAsync(ValidateApprovedToolApprovalsOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var approved = new List<CollectedToolApproval>();
        var denied = new List<CollectedToolApproval>();
        var invalid = new List<InvalidToolApproval>();

        foreach (var approval in options.ApprovedToolApprovals)
        {
            var approvalRequest = approval.ApprovalRequest;
            var toolCall = approval.ToolCall;

            // The tool name comes from client-supplied history, so the tool is looked up by exact name only.
            var tool = FindTool(options.Tools, toolCall.ToolName);

            if (options.ToolApprovalSecret != null || options.ToolApprovalSecretBytes != null)
            {
                if (approvalRequest.Signature == null)
                {
                    throw new InvalidToolApprovalSignatureError(approvalRequest.ApprovalId, toolCall.ToolCallId, "missing signature");
                }

                var valid = options.ToolApprovalSecret != null
                    ? ToolApprovalSignatures.VerifyToolApprovalSignature(options.ToolApprovalSecret, approvalRequest.Signature, approvalRequest.ApprovalId, toolCall.ToolCallId, toolCall.ToolName, toolCall.Input)
                    : ToolApprovalSignatures.VerifyToolApprovalSignature(options.ToolApprovalSecretBytes!, approvalRequest.Signature, approvalRequest.ApprovalId, toolCall.ToolCallId, toolCall.ToolName, toolCall.Input);
                if (!valid)
                {
                    throw new InvalidToolApprovalSignatureError(approvalRequest.ApprovalId, toolCall.ToolCallId, "invalid signature");
                }
            }

            if (tool != null && (tool.Execute != null || tool.ExecuteWithOptions != null) && tool.InputSchema.ValueKind == JsonValueKind.Object)
            {
                var value = approvalRequest.InputSchemaInput ?? toolCall.Input;
                Exception? validationError = null;
                var errors = ToolInputSchemaValidator.Validate(tool.InputSchema, value);
                if (errors.Count > 0)
                {
                    validationError = new FormatException("Type validation failed: Value: " + value.GetRawText() + ".\nError message: " + string.Join("; ", errors));
                }
                else
                {
                    try
                    {
                        var revalidatedInput = value;
                        if (options.RefineToolInput != null && options.RefineToolInput.TryGetValue(toolCall.ToolName, out var refine) && refine != null)
                        {
                            revalidatedInput = await refine(value, cancellationToken).ConfigureAwait(false);
                        }

                        // Revalidation must never change the operation that was approved,
                        // including when older or projected history omits the schema input.
                        if (!JsonDeepEqual(revalidatedInput, toolCall.Input))
                        {
                            validationError = new FormatException("Approved tool input does not match the validated schema output.");
                        }
                    }
                    catch (Exception error) when (!cancellationToken.IsCancellationRequested)
                    {
                        validationError = error;
                    }
                }

                if (validationError != null)
                {
                    invalid.Add(new InvalidToolApproval(approval, new InvalidToolInputError(toolCall.ToolName, toolCall.Input.GetRawText(), validationError)));
                    continue;
                }
            }

            var generatedCall = new GeneratedToolCall(toolCall.ToolCallId, toolCall.ToolName, toolCall.Input.GetRawText()) { ProviderExecuted = toolCall.ProviderExecuted };
            var approvalStatus = await ToolApprovalResolution.ResolveToolApprovalAsync(options.Tools, generatedCall, options.ToolApproval, options.Messages).ConfigureAwait(false);

            if (approvalStatus.Type == ToolApprovalStatus.DeniedType)
            {
                denied.Add(new CollectedToolApproval(
                    approvalRequest,
                    new ApprovalResponsePart(approval.ApprovalResponse.ApprovalId, false, approvalStatus.Reason ?? approval.ApprovalResponse.Reason),
                    toolCall,
                    approval.ExistingToolResult));
            }
            else
            {
                approved.Add(approval);
            }
        }

        return new ValidatedToolApprovals(approved, denied, invalid);
    }

    private static Tool? FindTool(IReadOnlyList<Tool>? tools, string name)
    {
        if (tools != null)
        {
            foreach (var tool in tools)
            {
                if (string.Equals(tool.Name, name, StringComparison.Ordinal))
                {
                    return tool;
                }
            }
        }

        return null;
    }

    private static bool JsonDeepEqual(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            return false;
        }

        switch (left.ValueKind)
        {
            case JsonValueKind.Object:
                var rightCount = 0;
                foreach (var _ in right.EnumerateObject())
                {
                    rightCount++;
                }

                var leftCount = 0;
                foreach (var property in left.EnumerateObject())
                {
                    leftCount++;
                    if (!right.TryGetProperty(property.Name, out var other) || !JsonDeepEqual(property.Value, other))
                    {
                        return false;
                    }
                }

                return leftCount == rightCount;
            case JsonValueKind.Array:
                if (left.GetArrayLength() != right.GetArrayLength())
                {
                    return false;
                }

                using (var leftItems = left.EnumerateArray().GetEnumerator())
                using (var rightItems = right.EnumerateArray().GetEnumerator())
                {
                    while (leftItems.MoveNext() && rightItems.MoveNext())
                    {
                        if (!JsonDeepEqual(leftItems.Current, rightItems.Current))
                        {
                            return false;
                        }
                    }
                }

                return true;
            case JsonValueKind.String:
                return string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal);
            case JsonValueKind.Number:
                return double.Parse(left.GetRawText(), CultureInfo.InvariantCulture) == double.Parse(right.GetRawText(), CultureInfo.InvariantCulture);
            default:
                return true;
        }
    }
}
