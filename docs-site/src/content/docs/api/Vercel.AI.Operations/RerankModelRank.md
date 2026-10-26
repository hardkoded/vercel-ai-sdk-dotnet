---
title: "RerankModelRank"
description: "One ranked index from the model."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankModelRank"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankModelRank"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankModelRank
```


One ranked index from the model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L27)

## Constructors

<a id="Vercel_AI_Operations_RerankModelRank__ctor_"></a>
<a id="Vercel_AI_Operations_RerankModelRank__ctor_System_Int32_System_Double_"></a>

### RerankModelRank(int, double)


```csharp
public RerankModelRank(int index, double relevanceScore)
```


Creates a rank.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `index` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `relevanceScore` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L30)

## Properties

<a id="Vercel_AI_Operations_RerankModelRank_Index_"></a>
<a id="Vercel_AI_Operations_RerankModelRank_Index"></a>

### Index


```csharp
public int Index { get; }
```


Index in the submitted document list.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L37)

<a id="Vercel_AI_Operations_RerankModelRank_RelevanceScore_"></a>
<a id="Vercel_AI_Operations_RerankModelRank_RelevanceScore"></a>

### RelevanceScore


```csharp
public double RelevanceScore { get; }
```


Relevance score.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L40)
