---
title: "RerankItem"
description: "One reranked document."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankItem"
  hidden: true
---

<a id="Vercel_AI_Provider_RerankItem"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RerankItem
```


One reranked document.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L442)

## Constructors

<a id="Vercel_AI_Provider_RerankItem__ctor_"></a>
<a id="Vercel_AI_Provider_RerankItem__ctor_System_Int32_System_Double_"></a>

### RerankItem(int, double)


```csharp
public RerankItem(int index, double score)
```


Creates a rerank item.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `index` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `score` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L445)

## Properties

<a id="Vercel_AI_Provider_RerankItem_Index_"></a>
<a id="Vercel_AI_Provider_RerankItem_Index"></a>

### Index


```csharp
public int Index { get; }
```


Index in the original document list.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L452)

<a id="Vercel_AI_Provider_RerankItem_Score_"></a>
<a id="Vercel_AI_Provider_RerankItem_Score"></a>

### Score


```csharp
public double Score { get; }
```


Relevance score.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L455)
