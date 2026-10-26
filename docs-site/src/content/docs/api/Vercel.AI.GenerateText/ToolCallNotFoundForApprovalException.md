---
title: "ToolCallNotFoundForApprovalException"
description: "An approval request points at a tool call that is not in the history."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallNotFoundForApprovalException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolCallNotFoundForApprovalException : AiSdkException, ISerializable
```


An approval request points at a tool call that is not in the history.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L197)

## Constructors

<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException__ctor_System_String_System_String_"></a>

### ToolCallNotFoundForApprovalException(string, string)


```csharp
public ToolCallNotFoundForApprovalException(string toolCallId, string approvalId)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L200)

## Properties

<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException_ApprovalId_"></a>
<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L211)

<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ToolCallNotFoundForApprovalException_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Missing tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L208)
