---
title: "GeneratedToolApprovalRequest"
description: "A request to approve a tool call, such as one a provider-executed MCP tool needs. A model returns it with ToolCallId only; the generation loop fills in ToolC..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedToolApprovalRequest"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedToolApprovalRequest : GeneratedContent
```


A request to approve a tool call, such as one a provider-executed MCP tool needs. A model returns it with
[ToolCallId](#Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCallId) only; the generation loop fills in [ToolCall](#Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCall).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L282)

## Constructors

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest__ctor_System_String_System_String_"></a>

### GeneratedToolApprovalRequest(string, string)


```csharp
public GeneratedToolApprovalRequest(string approvalId, string toolCallId)
```


Creates an approval request.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L285)

## Properties

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ApprovalId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id the response must carry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L293)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_IsAutomatic_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_IsAutomatic"></a>

### IsAutomatic


```csharp
public bool IsAutomatic { get; init; }
```


True when the request was approved or denied automatically, so nobody needs to answer it.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L305)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_Reason_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_Reason"></a>

### Reason


```csharp
public string? Reason { get; init; }
```


Why the tool call needs approval. It can be shown to the approver.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L302)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_Signature_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_Signature"></a>

### Signature


```csharp
public string? Signature { get; init; }
```


HMAC-SHA256 signature that binds this request to its tool call, when an approval secret is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L308)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCall_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall? ToolCall { get; init; }
```


The tool call that needs approval, once the generation loop resolved it.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L299)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCallId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call that needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L296)
