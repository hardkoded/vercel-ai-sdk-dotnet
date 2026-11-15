---
title: "ToolApprovalValidation"
description: "Validates approvals that come from message history. Maps to validateApprovedToolApprovals."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalValidation"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalValidation"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolApprovalValidation
```


Validates approvals that come from message history. Maps to `validateApprovedToolApprovals`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L85)

## Methods

<a id="Vercel_AI_GenerateText_ToolApprovalValidation_ValidateApprovedToolApprovalsAsync_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalValidation_ValidateApprovedToolApprovalsAsync_Vercel_AI_GenerateText_ValidateApprovedToolApprovalsOptions_System_Threading_CancellationToken_"></a>

### ValidateApprovedToolApprovalsAsync(ValidateApprovedToolApprovalsOptions, CancellationToken)


```csharp
public static Task<ValidatedToolApprovals> ValidateApprovedToolApprovalsAsync(ValidateApprovedToolApprovalsOptions options, CancellationToken cancellationToken = default)
```


Re-validates approved tool approvals reconstructed from client-supplied message history before they run.
It checks the HMAC signature (when a secret is set), the input schema, and the approval policy.
A missing or wrong signature throws [InvalidToolApprovalSignatureError](/api/Vercel.AI.Util/InvalidToolApprovalSignatureError/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ValidateApprovedToolApprovalsOptions](/api/Vercel.AI.GenerateText/ValidateApprovedToolApprovalsOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ValidatedToolApprovals&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L92)
