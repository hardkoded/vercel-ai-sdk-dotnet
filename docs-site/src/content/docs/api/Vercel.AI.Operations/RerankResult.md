---
title: "RerankResult"
description: "Result of CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankResult"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankResult
```


Result of `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L165)

## Constructors

<a id="Vercel_AI_Operations_RerankResult__ctor_"></a>
<a id="Vercel_AI_Operations_RerankResult__ctor_System_Collections_Generic_IReadOnlyList_System_Object__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_RankedDocument__System_Nullable_System_Text_Json_JsonElement__Vercel_AI_Operations_RerankResponseInfo_"></a>

### `RerankResult(IReadOnlyList<object?>, IReadOnlyList<RankedDocument>, JsonElement?, RerankResponseInfo)`


```csharp
public RerankResult(IReadOnlyList<object?> originalDocuments, IReadOnlyList<RankedDocument> ranking, JsonElement? providerMetadata, RerankResponseInfo response)
```


Creates a rerank result. The ranking list is copied.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `originalDocuments` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `ranking` | [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `response` | [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L170)

## Properties

<a id="Vercel_AI_Operations_RerankResult_OriginalDocuments_"></a>
<a id="Vercel_AI_Operations_RerankResult_OriginalDocuments"></a>

### OriginalDocuments


```csharp
public IReadOnlyList<object?> OriginalDocuments { get; }
```


Documents that were submitted.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L185)

<a id="Vercel_AI_Operations_RerankResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_RerankResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L206)

<a id="Vercel_AI_Operations_RerankResult_Ranking_"></a>
<a id="Vercel_AI_Operations_RerankResult_Ranking"></a>

### Ranking


```csharp
public IReadOnlyList<RankedDocument> Ranking { get; }
```


Ranking. Mutations of the list passed to `onEnd` do not change this copy.

**Value:** [IReadOnlyList&lt;RankedDocument&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L188)

<a id="Vercel_AI_Operations_RerankResult_RerankedDocuments_"></a>
<a id="Vercel_AI_Operations_RerankResult_RerankedDocuments"></a>

### RerankedDocuments


```csharp
public IReadOnlyList<object?> RerankedDocuments { get; }
```


Documents in ranking order.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L191)

<a id="Vercel_AI_Operations_RerankResult_Response_"></a>
<a id="Vercel_AI_Operations_RerankResult_Response"></a>

### Response


```csharp
public RerankResponseInfo Response { get; }
```


Response metadata.

**Value:** [RerankResponseInfo](/api/Vercel.AI.Operations/RerankResponseInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L209)
