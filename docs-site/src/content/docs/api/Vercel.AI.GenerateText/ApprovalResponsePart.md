---
title: "ApprovalResponsePart"
description: "The caller's decision for an approval request."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalResponsePart"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalResponsePart"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ApprovalResponsePart : ApprovalHistoryPart
```


The caller's decision for an approval request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ApprovalHistoryPart](/api/Vercel.AI.GenerateText/ApprovalHistoryPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L64)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalResponsePart__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalResponsePart__ctor_System_String_System_Boolean_System_String_"></a>

### ApprovalResponsePart(string, bool, string?)


```csharp
public ApprovalResponsePart(string approvalId, bool approved, string? reason = null)
```


Creates an approval response part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approved` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L67)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalResponsePart_ApprovalId_"></a>
<a id="Vercel_AI_GenerateText_ApprovalResponsePart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L76)

<a id="Vercel_AI_GenerateText_ApprovalResponsePart_Approved_"></a>
<a id="Vercel_AI_GenerateText_ApprovalResponsePart_Approved"></a>

### Approved


```csharp
public bool Approved { get; }
```


Whether the call was approved.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L79)

<a id="Vercel_AI_GenerateText_ApprovalResponsePart_Reason_"></a>
<a id="Vercel_AI_GenerateText_ApprovalResponsePart_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Denial reason, when the caller sent one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L82)
