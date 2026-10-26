---
title: "BatchRequestCounts"
description: "Request counts."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchRequestCounts"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchRequestCounts"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchRequestCounts
```


Request counts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L446)

## Constructors

<a id="Vercel_AI_Operations_BatchRequestCounts__ctor_"></a>
<a id="Vercel_AI_Operations_BatchRequestCounts__ctor_System_Int32_System_Int32_System_Int32_System_Int32_"></a>

### BatchRequestCounts(int, int, int, int)


```csharp
public BatchRequestCounts(int total, int pending, int completed, int failed)
```


Creates counts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `total` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `pending` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `completed` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `failed` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L449)

## Properties

<a id="Vercel_AI_Operations_BatchRequestCounts_Completed_"></a>
<a id="Vercel_AI_Operations_BatchRequestCounts_Completed"></a>

### Completed


```csharp
public int Completed { get; }
```


Completed requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L464)

<a id="Vercel_AI_Operations_BatchRequestCounts_Failed_"></a>
<a id="Vercel_AI_Operations_BatchRequestCounts_Failed"></a>

### Failed


```csharp
public int Failed { get; }
```


Failed requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L467)

<a id="Vercel_AI_Operations_BatchRequestCounts_Pending_"></a>
<a id="Vercel_AI_Operations_BatchRequestCounts_Pending"></a>

### Pending


```csharp
public int Pending { get; }
```


Pending requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L461)

<a id="Vercel_AI_Operations_BatchRequestCounts_Total_"></a>
<a id="Vercel_AI_Operations_BatchRequestCounts_Total"></a>

### Total


```csharp
public int Total { get; }
```


Total requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L458)
