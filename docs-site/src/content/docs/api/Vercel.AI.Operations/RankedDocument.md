---
title: "RankedDocument"
description: "One document in the reranked order."
editUrl: false
prev: false
next: false
sidebar:
  label: "RankedDocument"
  hidden: true
---

<a id="Vercel_AI_Operations_RankedDocument"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RankedDocument
```


One document in the reranked order.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L115)

## Constructors

<a id="Vercel_AI_Operations_RankedDocument__ctor_"></a>
<a id="Vercel_AI_Operations_RankedDocument__ctor_System_Int32_System_Double_System_Object_"></a>

### RankedDocument(int, double, object?)


```csharp
public RankedDocument(int originalIndex, double score, object? document)
```


Creates a ranked document.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `originalIndex` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `score` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `document` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L118)

## Properties

<a id="Vercel_AI_Operations_RankedDocument_Document_"></a>
<a id="Vercel_AI_Operations_RankedDocument_Document"></a>

### Document


```csharp
public object? Document { get; }
```


Original document.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L132)

<a id="Vercel_AI_Operations_RankedDocument_OriginalIndex_"></a>
<a id="Vercel_AI_Operations_RankedDocument_OriginalIndex"></a>

### OriginalIndex


```csharp
public int OriginalIndex { get; }
```


Index in [OriginalDocuments](/api/Vercel.AI.Operations/RerankResult/#Vercel_AI_Operations_RerankResult_OriginalDocuments).

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L126)

<a id="Vercel_AI_Operations_RankedDocument_Score_"></a>
<a id="Vercel_AI_Operations_RankedDocument_Score"></a>

### Score


```csharp
public double Score { get; }
```


Relevance score.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L129)
