---
title: "ToolApprovalRequestPart"
description: "A tool call needs approval before it runs. Maps to the tool-approval-request stream part."
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


A tool call needs approval before it runs. Maps to the `tool-approval-request` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2071)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2074)

## Properties

<a id="Vercel_AI_ToolApprovalRequestPart_ApprovalId_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id the response must carry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2086)

<a id="Vercel_AI_ToolApprovalRequestPart_IsAutomatic_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_IsAutomatic"></a>

### IsAutomatic


```csharp
public bool IsAutomatic { get; }
```


True when the request was approved or denied automatically, so nobody needs to answer it.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2095)

<a id="Vercel_AI_ToolApprovalRequestPart_Reason_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Why the tool call needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2092)

<a id="Vercel_AI_ToolApprovalRequestPart_Signature_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_Signature"></a>

### Signature


```csharp
public string? Signature { get; }
```


HMAC-SHA256 signature that binds the request to its tool call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2098)

<a id="Vercel_AI_ToolApprovalRequestPart_ToolCall_"></a>
<a id="Vercel_AI_ToolApprovalRequestPart_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call that needs approval.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2089)
