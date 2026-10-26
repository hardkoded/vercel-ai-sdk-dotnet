---
title: "RerankResult"
description: "Rerank result."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankResult"
  hidden: true
---

<a id="Vercel_AI_Provider_RerankResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RerankResult
```


Rerank result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L459)

## Constructors

<a id="Vercel_AI_Provider_RerankResult__ctor_"></a>
<a id="Vercel_AI_Provider_RerankResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_RerankItem__"></a>

### `RerankResult(IReadOnlyList<RerankItem>)`


```csharp
public RerankResult(IReadOnlyList<RerankItem> items)
```


Creates a rerank result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `items` | [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L462)

## Properties

<a id="Vercel_AI_Provider_RerankResult_Items_"></a>
<a id="Vercel_AI_Provider_RerankResult_Items"></a>

### Items


```csharp
public IReadOnlyList<RerankItem> Items { get; }
```


Documents ordered by relevance.

**Value:** [IReadOnlyList&lt;RerankItem&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L468)
