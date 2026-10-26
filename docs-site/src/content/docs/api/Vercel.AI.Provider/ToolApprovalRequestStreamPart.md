---
title: "ToolApprovalRequestStreamPart"
description: "A request to approve a tool call."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalRequestStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolApprovalRequestStreamPart : LanguageModelStreamPart
```


A request to approve a tool call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1736)

## Constructors

<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart__ctor_System_String_System_String_"></a>

### ToolApprovalRequestStreamPart(string, string)


```csharp
public ToolApprovalRequestStreamPart(string approvalId, string toolCallId)
```


Creates a tool-approval-request part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1739)

## Properties

<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart_ApprovalId_"></a>
<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id the response must carry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1747)

<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart_ToolCallId_"></a>
<a id="Vercel_AI_Provider_ToolApprovalRequestStreamPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call that needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1750)
