---
title: "ToolApprovalCollection"
description: "Collects tool approvals from the last tool message. Maps to collectToolApprovals."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalCollection"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalCollection"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolApprovalCollection
```


Collects tool approvals from the last tool message. Maps to `collectToolApprovals`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L215)

## Methods

<a id="Vercel_AI_GenerateText_ToolApprovalCollection_CollectToolApprovals_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalCollection_CollectToolApprovals_System_Collections_Generic_IReadOnlyList_Vercel_AI_GenerateText_ApprovalHistoryMessage__"></a>

### `CollectToolApprovals(IReadOnlyList<ApprovalHistoryMessage>)`


```csharp
public static CollectedToolApprovalSet CollectToolApprovals(IReadOnlyList<ApprovalHistoryMessage> messages)
```


Reads approval responses from the last message when it is a tool message.
Returns empty lists for every other ending.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `messages` | [IReadOnlyList&lt;ApprovalHistoryMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [CollectedToolApprovalSet](/api/Vercel.AI.GenerateText/CollectedToolApprovalSet/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L221)
