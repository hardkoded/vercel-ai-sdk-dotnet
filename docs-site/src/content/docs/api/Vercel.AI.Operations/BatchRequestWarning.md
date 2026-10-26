---
title: "BatchRequestWarning"
description: "A warning tied to a request."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchRequestWarning"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchRequestWarning"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchRequestWarning
```


A warning tied to a request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L471)

## Constructors

<a id="Vercel_AI_Operations_BatchRequestWarning__ctor_"></a>
<a id="Vercel_AI_Operations_BatchRequestWarning__ctor_System_String_Vercel_AI_Operations_OperationWarning_"></a>

### BatchRequestWarning(string?, OperationWarning)


```csharp
public BatchRequestWarning(string? requestId, OperationWarning warning)
```


Creates a warning.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `requestId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `warning` | [OperationWarning](/api/Vercel.AI.Operations/OperationWarning/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L474)

## Properties

<a id="Vercel_AI_Operations_BatchRequestWarning_RequestId_"></a>
<a id="Vercel_AI_Operations_BatchRequestWarning_RequestId"></a>

### RequestId


```csharp
public string? RequestId { get; }
```


Request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L481)

<a id="Vercel_AI_Operations_BatchRequestWarning_Warning_"></a>
<a id="Vercel_AI_Operations_BatchRequestWarning_Warning"></a>

### Warning


```csharp
public OperationWarning Warning { get; }
```


Warning.

**Value:** [OperationWarning](/api/Vercel.AI.Operations/OperationWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L484)
