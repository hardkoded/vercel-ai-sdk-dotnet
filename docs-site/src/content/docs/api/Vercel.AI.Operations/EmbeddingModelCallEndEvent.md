---
title: "EmbeddingModelCallEndEvent"
description: "Event fired after an individual doEmbed call. Maps to EmbeddingModelCallEndEvent."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingModelCallEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbeddingModelCallEndEvent : EmbeddingModelCallStartEvent
```


Event fired after an individual `doEmbed` call. Maps to `EmbeddingModelCallEndEvent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [EmbeddingModelCallStartEvent](/api/Vercel.AI.Operations/EmbeddingModelCallStartEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L325)

## Constructors

<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_Double____Vercel_AI_Operations_OperationUsage_System_Nullable_System_Int32__"></a>

### `EmbeddingModelCallEndEvent(string, string, string, string, string, IReadOnlyList<string>, IReadOnlyList<double[]>, OperationUsage, int?)`


```csharp
public EmbeddingModelCallEndEvent(string callId, string embedCallId, string operationId, string provider, string modelId, IReadOnlyList<string> values, IReadOnlyList<double[]> embeddings, OperationUsage usage, int? dimensions = null)
```


Creates the event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `embedCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `embeddings` | [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `usage` | [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L328)

## Properties

<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent_Embeddings_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<double[]> Embeddings { get; }
```


Embeddings of the call.

**Value:** [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L346)

<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent_Usage_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEndEvent_Usage"></a>

### Usage


```csharp
public OperationUsage Usage { get; }
```


Usage of the call.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L349)
