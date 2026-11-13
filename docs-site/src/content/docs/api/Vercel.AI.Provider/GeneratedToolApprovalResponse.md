---
title: "GeneratedToolApprovalResponse"
description: "The answer to a tool approval request that the generation loop produced itself, such as an automatic approval or denial. Maps to the tool-approval-response o..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedToolApprovalResponse"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedToolApprovalResponse : GeneratedContent
```


The answer to a tool approval request that the generation loop produced itself, such as an automatic approval or denial.
Maps to the `tool-approval-response` output part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L290)

## Constructors

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse__ctor_System_String_Vercel_AI_Provider_GeneratedToolCall_System_Boolean_System_String_System_Boolean_"></a>

### GeneratedToolApprovalResponse(string, GeneratedToolCall, bool, string?, bool)


```csharp
public GeneratedToolApprovalResponse(string approvalId, GeneratedToolCall toolCall, bool approved, string? reason = null, bool providerExecuted = false)
```


Creates an approval response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCall` | [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/) |  |
| `approved` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerExecuted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L293)

## Properties

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ApprovalId_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Id of the approval request this answers.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L304)

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_Approved_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_Approved"></a>

### Approved


```csharp
public bool Approved { get; }
```


Whether the approval was granted or denied.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L310)

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; }
```


True when the tool call is provider-executed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L316)

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_Reason_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Reason for the approval or denial.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L313)

<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ToolCall_"></a>
<a id="Vercel_AI_Provider_GeneratedToolApprovalResponse_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call the response is for.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L307)
