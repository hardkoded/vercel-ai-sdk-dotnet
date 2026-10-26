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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L257)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L260)

## Properties

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ApprovalId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id the response must carry.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L268)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCall_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall? ToolCall { get; init; }
```


The tool call that needs approval, once the generation loop resolved it.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L274)

<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCallId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalRequest_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call that needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L271)
