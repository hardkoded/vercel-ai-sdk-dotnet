---
title: "AnthropicBatchCounts"
description: "Request counts derived from an Anthropic message batch."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicBatchCounts"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicBatchCounts
```


Request counts derived from an Anthropic message batch.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L12)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchCounts__ctor_System_Int32_System_Int32_System_Int32_System_Int32_"></a>

### AnthropicBatchCounts(int, int, int, int)


```csharp
public AnthropicBatchCounts(int total, int pending, int completed, int failed)
```


Creates counts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `total` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `pending` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `completed` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `failed` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L15)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Completed_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Completed"></a>

### Completed


```csharp
public int Completed { get; }
```


Succeeded requests.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L30)

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Failed_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Failed"></a>

### Failed


```csharp
public int Failed { get; }
```


errored + canceled + expired.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L33)

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Pending_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Pending"></a>

### Pending


```csharp
public int Pending { get; }
```


Requests still processing.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L27)

<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Total_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBatchCounts_Total"></a>

### Total


```csharp
public int Total { get; }
```


processing + succeeded + errored + canceled + expired.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicBatch.cs#L24)
