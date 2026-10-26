---
title: "ToolApprovalResponseModelMessage"
description: "The caller's answer to a GeneratedToolApprovalRequest. Only a response with ProviderExecuted reaches the model; the generation loop drops the others from the..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalResponseModelMessage"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class ToolApprovalResponseModelMessage : ModelMessage
```


The caller's answer to a [GeneratedToolApprovalRequest](/api/Vercel.AI.Provider/GeneratedToolApprovalRequest/). Only a response with
[ProviderExecuted](#Vercel_AI_Provider_ToolApprovalResponseModelMessage_ProviderExecuted) reaches the model; the generation loop drops the others from the prompt.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ModelMessage](/api/Vercel.AI.Provider/ModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1159)

## Constructors

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage__ctor_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage__ctor_System_String_System_Boolean_System_String_System_Boolean_"></a>

### ToolApprovalResponseModelMessage(string, bool, string?, bool)


```csharp
public ToolApprovalResponseModelMessage(string approvalId, bool approved, string? reason = null, bool providerExecuted = false)
```


Creates an approval response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `approved` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerExecuted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1162)

## Properties

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_ApprovalId_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Approval id of the request this answers.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1172)

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_Approved_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_Approved"></a>

### Approved


```csharp
public bool Approved { get; }
```


Whether the call was approved.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1175)

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_DeniedToolResult_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_DeniedToolResult"></a>

### DeniedToolResult


```csharp
public ToolModelMessage? DeniedToolResult { get; init; }
```


For a denied provider-executed call, the `execution-denied` result sent in the same tool message, after the response.
The generation loop sets it while it prepares the prompt.

**Value:** [ToolModelMessage](/api/Vercel.AI.Provider/ToolModelMessage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1187)

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_ProviderExecuted_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; }
```


True when the provider runs the tool, so the model must see the response.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1181)

<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_Reason_"></a>
<a id="Vercel_AI_Provider_ToolApprovalResponseModelMessage_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Reason for the decision, when given.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1178)
