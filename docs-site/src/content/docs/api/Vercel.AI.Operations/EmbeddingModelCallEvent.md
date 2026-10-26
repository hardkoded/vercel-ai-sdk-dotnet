---
title: "EmbeddingModelCallEvent"
description: "One inner doEmbed telemetry event."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingModelCallEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class EmbeddingModelCallEvent
```


One inner `doEmbed` telemetry event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L227)

## Constructors

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent__ctor_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_System_Double____Vercel_AI_Operations_OperationUsage_System_Nullable_System_Int32__"></a>

### `EmbeddingModelCallEvent(string, string, string, string, string, IReadOnlyList<string>, IReadOnlyList<double[]>?, OperationUsage?, int?)`


```csharp
public EmbeddingModelCallEvent(string callId, string embedCallId, string operationId, string provider, string modelId, IReadOnlyList<string> values, IReadOnlyList<double[]>? embeddings, OperationUsage? usage, int? dimensions = null)
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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L230)

## Properties

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_CallId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Operation call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L244)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Dimensions_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Dimensions"></a>

### Dimensions


```csharp
public int? Dimensions { get; }
```


Requested output dimensions, when specified.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L262)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_EmbedCallId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_EmbedCallId"></a>

### EmbedCallId


```csharp
public string EmbedCallId { get; }
```


Id of this `doEmbed` invocation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L247)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Embeddings_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<double[]>? Embeddings { get; }
```


Embeddings, on the end event.

**Value:** [IReadOnlyList&lt;double\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L265)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L256)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.embed.doEmbed` or `ai.embedMany.doEmbed`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L250)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Provider_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L253)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Usage_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; }
```


Usage, on the end event.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L268)

<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Values_"></a>
<a id="Vercel_AI_Operations_EmbeddingModelCallEvent_Values"></a>

### Values


```csharp
public IReadOnlyList<string> Values { get; }
```


Values in this call.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Embedding.cs#L259)
