---
title: "ToolApprovalRequestPart"
description: "A provider-executed tool call needs approval before the provider runs it."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalRequestPart"
  hidden: true
---

<a id="Vercel_AI_ToolApprovalRequestPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolApprovalRequestPart : TextStreamPart
```


A provider-executed tool call needs approval before the provider runs it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1189)

## Constructors

<a id="Vercel_AI_ToolApprovalRequestPart__ctor_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart__ctor_Vercel_AI_Provider_GeneratedToolApprovalRequest_"></a>

### ToolApprovalRequestPart(GeneratedToolApprovalRequest)


```csharp
public ToolApprovalRequestPart(GeneratedToolApprovalRequest request)
```


Creates a tool-approval-request part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GeneratedToolApprovalRequest](/api/Vercel.AI.Provider/GeneratedToolApprovalRequest/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1192)

## Properties

<a id="Vercel_AI_ToolApprovalRequestPart_ApprovalId_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id the response must carry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1200)

<a id="Vercel_AI_ToolApprovalRequestPart_ToolCall_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call that needs approval.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1203)
