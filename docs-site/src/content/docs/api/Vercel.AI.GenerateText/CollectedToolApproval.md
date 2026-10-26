---
title: "CollectedToolApproval"
description: "One approval that still needs execution or denial handling."
editUrl: false
prev: false
next: false
sidebar:
  label: "CollectedToolApproval"
  hidden: true
---

<a id="Vercel_AI_GenerateText_CollectedToolApproval"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class CollectedToolApproval
```


One approval that still needs execution or denial handling.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L137)

## Constructors

<a id="Vercel_AI_GenerateText_CollectedToolApproval__ctor_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApproval__ctor_Vercel_AI_GenerateText_ApprovalRequestPart_Vercel_AI_GenerateText_ApprovalResponsePart_Vercel_AI_GenerateText_ApprovalToolCallPart_Vercel_AI_GenerateText_ApprovalToolResultPart_"></a>

### CollectedToolApproval(ApprovalRequestPart, ApprovalResponsePart, ApprovalToolCallPart, ApprovalToolResultPart?)


```csharp
public CollectedToolApproval(ApprovalRequestPart approvalRequest, ApprovalResponsePart approvalResponse, ApprovalToolCallPart toolCall, ApprovalToolResultPart? existingToolResult = null)
```


Creates a collected approval.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalRequest` | [ApprovalRequestPart](/api/Vercel.AI.GenerateText/ApprovalRequestPart/) |  |
| `approvalResponse` | [ApprovalResponsePart](/api/Vercel.AI.GenerateText/ApprovalResponsePart/) |  |
| `toolCall` | [ApprovalToolCallPart](/api/Vercel.AI.GenerateText/ApprovalToolCallPart/) |  |
| `existingToolResult` | [ApprovalToolResultPart](/api/Vercel.AI.GenerateText/ApprovalToolResultPart/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L140)

## Properties

<a id="Vercel_AI_GenerateText_CollectedToolApproval_ApprovalRequest_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApproval_ApprovalRequest"></a>

### ApprovalRequest


```csharp
public ApprovalRequestPart ApprovalRequest { get; }
```


Matching approval request.

**Value:** [ApprovalRequestPart](/api/Vercel.AI.GenerateText/ApprovalRequestPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L153)

<a id="Vercel_AI_GenerateText_CollectedToolApproval_ApprovalResponse_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApproval_ApprovalResponse"></a>

### ApprovalResponse


```csharp
public ApprovalResponsePart ApprovalResponse { get; }
```


Approval response.

**Value:** [ApprovalResponsePart](/api/Vercel.AI.GenerateText/ApprovalResponsePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L156)

<a id="Vercel_AI_GenerateText_CollectedToolApproval_ExistingToolResult_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApproval_ExistingToolResult"></a>

### ExistingToolResult


```csharp
public ApprovalToolResultPart? ExistingToolResult { get; }
```


Existing tool result, when a denied call already has an execution-denied output.

**Value:** [ApprovalToolResultPart](/api/Vercel.AI.GenerateText/ApprovalToolResultPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L162)

<a id="Vercel_AI_GenerateText_CollectedToolApproval_ToolCall_"></a>
<a id="Vercel_AI_GenerateText_CollectedToolApproval_ToolCall"></a>

### ToolCall


```csharp
public ApprovalToolCallPart ToolCall { get; }
```


Tool call the approval refers to.

**Value:** [ApprovalToolCallPart](/api/Vercel.AI.GenerateText/ApprovalToolCallPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L159)
