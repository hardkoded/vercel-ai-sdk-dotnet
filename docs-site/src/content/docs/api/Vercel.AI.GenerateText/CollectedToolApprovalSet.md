---
title: "CollectedToolApprovalSet"
description: "Approved and denied tool approvals from the last tool message."
editUrl: false
prev: false
next: false
sidebar:
  label: "CollectedToolApprovalSet"
  hidden: true
---

<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class CollectedToolApprovalSet
```


Approved and denied tool approvals from the last tool message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L166)

## Constructors

<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet__ctor_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_CollectedToolApproval__System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_CollectedToolApproval__"></a>

### `CollectedToolApprovalSet(IReadOnlyList<CollectedToolApproval>, IReadOnlyList<CollectedToolApproval>)`


```csharp
public CollectedToolApprovalSet(IReadOnlyList<CollectedToolApproval> approvedToolApprovals, IReadOnlyList<CollectedToolApproval> deniedToolApprovals)
```


Creates an empty or filled set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvedToolApprovals` | [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `deniedToolApprovals` | [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L169)

## Properties

<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet_ApprovedToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet_ApprovedToolApprovals"></a>

### ApprovedToolApprovals


```csharp
public IReadOnlyList<CollectedToolApproval> ApprovedToolApprovals { get; }
```


Approvals the caller accepted and that have not already produced a result.

**Value:** [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L176)

<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet_DeniedToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApprovalSet_DeniedToolApprovals"></a>

### DeniedToolApprovals


```csharp
public IReadOnlyList<CollectedToolApproval> DeniedToolApprovals { get; }
```


Approvals the caller denied and that still need an execution-denied result.

**Value:** [IReadOnlyList&lt;CollectedToolApproval&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L179)
