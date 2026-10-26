---
title: "RerankEndEvent"
description: "End event for rerank."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankEndEvent"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankEndEvent"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankEndEvent
```


End event for rerank.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L266)

## Constructors

<a id="Vercel_AI_Operations_RerankEndEvent__ctor_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__System_String_System_String_System_Collections_Generic_IReadOnlyList_System_Object__System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RankedDocument__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_RerankResponseInfo_"></a>

### `RerankEndEvent(string, string, IReadOnlyDictionary<string, object?>, string, string, IReadOnlyList<object?>, string, IReadOnlyList<RankedDocument>, IReadOnlyList<OperationWarning>, JsonElement?, RerankResponseInfo)`


```csharp
public RerankEndEvent(string callId, string operationId, IReadOnlyDictionary<string, object?> runtimeContext, string provider, string modelId, IReadOnlyList<object?> documents, string query, IReadOnlyList<RankedDocument> ranking, IReadOnlyList<OperationWarning> warnings, JsonElement? providerMetadata, RerankResponseInfo response)
```


Creates an end event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `runtimeContext` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `ranking` | [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L269)

## Properties

<a id="Vercel_AI_Operations_RerankEndEvent_CallId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; }
```


Call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L285)

<a id="Vercel_AI_Operations_RerankEndEvent_Documents_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Documents"></a>

### Documents


```csharp
public IReadOnlyList<object?> Documents { get; }
```


Submitted documents.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L300)

<a id="Vercel_AI_Operations_RerankEndEvent_ModelId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L297)

<a id="Vercel_AI_Operations_RerankEndEvent_OperationId_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_OperationId"></a>

### OperationId


```csharp
public string OperationId { get; }
```


`ai.rerank`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L288)

<a id="Vercel_AI_Operations_RerankEndEvent_Provider_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L294)

<a id="Vercel_AI_Operations_RerankEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L312)

<a id="Vercel_AI_Operations_RerankEndEvent_Query_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Query"></a>

### Query


```csharp
public string Query { get; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L303)

<a id="Vercel_AI_Operations_RerankEndEvent_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RankedDocument> Ranking { get; }
```


Ranking visible to the callback. The result keeps its own copy.

**Value:** [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L306)

<a id="Vercel_AI_Operations_RerankEndEvent_Response_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Response"></a>

### Response


```csharp
public RerankResponseInfo Response { get; }
```


Response metadata.

**Value:** [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L315)

<a id="Vercel_AI_Operations_RerankEndEvent_RuntimeContext_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?> RuntimeContext { get; }
```


Runtime context.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L291)

<a id="Vercel_AI_Operations_RerankEndEvent_Warnings_"></a>
<a id="Vercel_AI_Operations_RerankEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L309)
