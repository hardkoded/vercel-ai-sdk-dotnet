---
title: "AssistantToolApprovalRequestPart"
description: "A request to approve a tool call, kept in an assistant message."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssistantToolApprovalRequestPart"
  hidden: true
---

<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AssistantToolApprovalRequestPart : AssistantContentPart
```


A request to approve a tool call, kept in an assistant message.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [AssistantContentPart](/api/Vercel.AI.Provider/AssistantContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L893)

## Constructors

<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart__ctor_"></a>
<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart__ctor_System_String_System_String_System_Nullable_System_Boolean__"></a>

### AssistantToolApprovalRequestPart(string, string, bool?)


```csharp
public AssistantToolApprovalRequestPart(string approvalId, string toolCallId, bool? isAutomatic = null)
```


Creates a tool-approval-request part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isAutomatic` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L896)

## Properties

<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_ApprovalId_"></a>
<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L905)

<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_IsAutomatic_"></a>
<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_IsAutomatic"></a>

### IsAutomatic


```csharp
public bool? IsAutomatic { get; }
```


Whether the approval was decided automatically, when known.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L911)

<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_ToolCallId_"></a>
<a id="Vercel_AI_Provider_AssistantToolApprovalRequestPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Id of the tool call that needs approval.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L908)
