---
title: "ApprovalRequestPart"
description: "A request that a tool call be approved."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApprovalRequestPart"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ApprovalRequestPart"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ApprovalRequestPart : ApprovalHistoryPart
```


A request that a tool call be approved.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ApprovalHistoryPart](/api/Vercel.AI.GenerateText/ApprovalHistoryPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L49)

## Constructors

<a id="Vercel_AI_GenerateText_ApprovalRequestPart__ctor_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart__ctor_System_String_System_String_"></a>

### ApprovalRequestPart(string, string)


```csharp
public ApprovalRequestPart(string approvalId, string toolCallId)
```


Creates an approval request part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L52)

## Properties

<a id="Vercel_AI_GenerateText_ApprovalRequestPart_ApprovalId_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L60)

<a id="Vercel_AI_GenerateText_ApprovalRequestPart_InputSchemaInput_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart_InputSchemaInput"></a>

### InputSchemaInput


```csharp
public JsonElement? InputSchemaInput { get; init; }
```


The input of the call before the input schema transformed it, when it differs from the call's input. Maps to `inputSchemaInput`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L72)

<a id="Vercel_AI_GenerateText_ApprovalRequestPart_Reason_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart_Reason"></a>

### Reason


```csharp
public string? Reason { get; init; }
```


Why the call needed approval, when a reason was given.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L69)

<a id="Vercel_AI_GenerateText_ApprovalRequestPart_Signature_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart_Signature"></a>

### Signature


```csharp
public string? Signature { get; init; }
```


HMAC-SHA256 signature that binds this request to its tool call, when the request was signed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L66)

<a id="Vercel_AI_GenerateText_ApprovalRequestPart_ToolCallId_"></a>
<a id="Vercel_AI_GenerateText_ApprovalRequestPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L63)
