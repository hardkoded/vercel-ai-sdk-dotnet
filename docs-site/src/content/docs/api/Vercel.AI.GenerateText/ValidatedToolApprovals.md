---
title: "ValidatedToolApprovals"
description: "The outcome of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ValidatedToolApprovals"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ValidatedToolApprovals"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ValidatedToolApprovals
```


The outcome of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L61)

## Constructors

<a id="Vercel_AI_GenerateText_ValidatedToolApprovals__ctor_"></a>
<a id="Vercel_AI_GenerateText_ValidatedToolApprovals__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_CollectedToolApproval__System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_CollectedToolApproval__System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_InvalidToolApproval__"></a>

### `ValidatedToolApprovals(IReadOnlyList<CollectedToolApproval>, IReadOnlyList<CollectedToolApproval>, IReadOnlyList<InvalidToolApproval>)`


```csharp
public ValidatedToolApprovals(IReadOnlyList<CollectedToolApproval> approvedToolApprovals, IReadOnlyList<CollectedToolApproval> deniedToolApprovals, IReadOnlyList<InvalidToolApproval> invalidToolApprovals)
```


Creates the outcome.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvedToolApprovals` | [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `deniedToolApprovals` | [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `invalidToolApprovals` | [IReadOnlyList&lt;InvalidToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L64)

## Properties

<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_ApprovedToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_ApprovedToolApprovals"></a>

### ApprovedToolApprovals


```csharp
public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }
```


Approvals that may run.

**Value:** [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L75)

<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_DeniedToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_DeniedToolApprovals"></a>

### DeniedToolApprovals


```csharp
public IReadOnlyList<CollectedToolApproval> DeniedToolApprovals { get; }
```


Approvals that the approval policy denies now, with the denial in their response.

**Value:** [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L78)

<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_InvalidToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_ValidatedToolApprovals_InvalidToolApprovals"></a>

### InvalidToolApprovals


```csharp
public IReadOnlyList<InvalidToolApproval> InvalidToolApprovals { get; }
```


Approvals whose input does not match the tool's input schema.

**Value:** [IReadOnlyList&lt;InvalidToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ValidateToolApprovals.cs#L81)
