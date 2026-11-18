---
title: "ToolApprovalResponsePart"
description: "A tool approval request was answered. Maps to the tool-approval-response stream part."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalResponsePart"
  hidden: true
---

<a id="Vercel_AI_ToolApprovalResponsePart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolApprovalResponsePart : TextStreamPart
```


A tool approval request was answered. Maps to the `tool-approval-response` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1915)

## Constructors

<a id="Vercel_AI_ToolApprovalResponsePart__ctor_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart__ctor_Vercel_AI_Provider_GeneratedToolApprovalResponse_"></a>

### ToolApprovalResponsePart(GeneratedToolApprovalResponse)


```csharp
public ToolApprovalResponsePart(GeneratedToolApprovalResponse response)
```


Creates a tool-approval-response part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [GeneratedToolApprovalResponse](/api/Vercel.AI.Provider/GeneratedToolApprovalResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1918)

## Properties

<a id="Vercel_AI_ToolApprovalResponsePart_ApprovalId_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Id of the approval request this answers.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1934)

<a id="Vercel_AI_ToolApprovalResponsePart_Approved_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart_Approved"></a>

### Approved


```csharp
public bool Approved { get; }
```


Whether the approval was granted or denied.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1940)

<a id="Vercel_AI_ToolApprovalResponsePart_ProviderExecuted_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; }
```


True when the tool call is provider-executed.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1946)

<a id="Vercel_AI_ToolApprovalResponsePart_Reason_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Reason for the approval or denial.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1943)

<a id="Vercel_AI_ToolApprovalResponsePart_ToolCall_"></a>
<a id="Vercel_AI_ToolApprovalResponsePart_ToolCall"></a>

### ToolCall


```csharp
public GeneratedToolCall ToolCall { get; }
```


The tool call the response is for.

**Value:** [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1937)
