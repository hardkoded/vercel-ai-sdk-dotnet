---
title: "RerankModelDocuments"
description: "Documents sent to a reranking model."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankModelDocuments"
  hidden: true
---

<a id="Vercel_AI_Operations_RerankModelDocuments"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class RerankModelDocuments
```


Documents sent to a reranking model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_RerankModelDocuments__ctor_"></a>
<a id="Vercel_AI_Operations_RerankModelDocuments__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_Object__"></a>

### `RerankModelDocuments(string, IReadOnlyList<object?>)`


```csharp
public RerankModelDocuments(string type, IReadOnlyList<object?> values)
```


Creates a document batch.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `values` | [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L13)

## Properties

<a id="Vercel_AI_Operations_RerankModelDocuments_Type_"></a>
<a id="Vercel_AI_Operations_RerankModelDocuments_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text` or `object`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L20)

<a id="Vercel_AI_Operations_RerankModelDocuments_Values_"></a>
<a id="Vercel_AI_Operations_RerankModelDocuments_Values"></a>

### Values


```csharp
public IReadOnlyList<object?> Values { get; }
```


Document values.

**Value:** [IReadOnlyList&lt;object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Rerank.cs#L23)
