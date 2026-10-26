---
title: "EmbedMany"
description: "Embeds many values, splitting on count and UTF-8 byte limits. Maps to embedMany."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbedMany"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbedMany"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class EmbedMany
```


Embeds many values, splitting on count and UTF-8 byte limits. Maps to `embedMany`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L551)

## Methods

<a id="Vercel_AI_Operations_EmbedMany_EmbedManyAsync_"></a>
<a id="Vercel_AI_Operations_EmbedMany_EmbedManyAsync_Vercel_AI_Operations_EmbedManyRequest_System_Threading_CancellationToken_"></a>

### EmbedManyAsync(EmbedManyRequest, CancellationToken)


```csharp
public static Task<EmbedManyResult> EmbedManyAsync(EmbedManyRequest request, CancellationToken cancellationToken = default)
```


Embeds [Values](/api/Vercel.AI.Operations/EmbedManyRequest/#Vercel_AI_Operations_EmbedManyRequest_Values).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [EmbedManyRequest](/api/Vercel.AI.Operations/EmbedManyRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbedManyResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L554)

<a id="Vercel_AI_Operations_EmbedMany_SplitByLimits_"></a>
<a id="Vercel_AI_Operations_EmbedMany_SplitByLimits_System_Collections_Generic_IReadOnlyList_System_String__System_Int32_System_Double_"></a>

### `SplitByLimits(IReadOnlyList<string>, int, double)`


```csharp
public static IReadOnlyList<IReadOnlyList<string>> SplitByLimits(IReadOnlyList<string> values, int maxEmbeddingsPerCall, double maxInputBytesPerCall)
```


Splits values by embedding count and UTF-8 byte budget.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `maxEmbeddingsPerCall` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxInputBytesPerCall` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

**Returns:** [IReadOnlyList&lt;IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L694)

<a id="Vercel_AI_Operations_EmbedMany_Wrap_"></a>
<a id="Vercel_AI_Operations_EmbedMany_Wrap_Vercel_AI_Operations_IEmbeddingCaller_System_Func_Vercel_AI_Operations_EmbeddingModelCall_Vercel_AI_Operations_IEmbeddingCaller_System_Threading_CancellationToken_System_Threading_Tasks_Task_Vercel_AI_Operations_EmbeddingModelCall___"></a>

### `Wrap(IEmbeddingCaller, Func<EmbeddingModelCall, IEmbeddingCaller, CancellationToken, Task<EmbeddingModelCall>>?)`


```csharp
public static IEmbeddingCaller Wrap(IEmbeddingCaller model, Func<EmbeddingModelCall, IEmbeddingCaller, CancellationToken, Task<EmbeddingModelCall>>? transformParams)
```


Wraps a model so middleware can rewrite each `doEmbed` call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/) |  |
| `transformParams` | [Func&lt;EmbeddingModelCall, IEmbeddingCaller, CancellationToken, Task&lt;EmbeddingModelCall&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-4) |  |

**Returns:** [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L646)
