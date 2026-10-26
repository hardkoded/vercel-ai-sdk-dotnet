---
title: "InvalidToolApprovalException"
description: "An approval response names an unknown approval id."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidToolApprovalException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_InvalidToolApprovalException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class InvalidToolApprovalException : AiSdkException, ISerializable
```


An approval response names an unknown approval id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L183)

## Constructors

<a id="Vercel_AI_GenerateText_InvalidToolApprovalException__ctor_"></a>
<a id="Vercel_AI_GenerateText_InvalidToolApprovalException__ctor_System_String_"></a>

### InvalidToolApprovalException(string)


```csharp
public InvalidToolApprovalException(string approvalId)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `approvalId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L186)

## Properties

<a id="Vercel_AI_GenerateText_InvalidToolApprovalException_ApprovalId_"></a>
<a id="Vercel_AI_GenerateText_InvalidToolApprovalException_ApprovalId"></a>

### ApprovalId


```csharp
public string ApprovalId { get; }
```


Unknown approval id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/CollectToolApprovals.cs#L193)
