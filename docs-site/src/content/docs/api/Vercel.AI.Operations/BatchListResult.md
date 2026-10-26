---
title: "BatchListResult"
description: "A page of batches."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchListResult"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchListResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchListResult
```


A page of batches.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L545)

## Constructors

<a id="Vercel_AI_Operations_BatchListResult__ctor_"></a>
<a id="Vercel_AI_Operations_BatchListResult__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ListedBatch__System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### `BatchListResult(IReadOnlyList<ListedBatch>, string?, JsonElement?)`


```csharp
public BatchListResult(IReadOnlyList<ListedBatch> batches, string? nextCursor = null, JsonElement? providerMetadata = null)
```


Creates a page.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `batches` | [IReadOnlyList&lt;ListedBatch&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `nextCursor` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L548)

## Properties

<a id="Vercel_AI_Operations_BatchListResult_Batches_"></a>
<a id="Vercel_AI_Operations_BatchListResult_Batches"></a>

### Batches


```csharp
public IReadOnlyList<ListedBatch> Batches { get; }
```


Batches.

**Value:** [IReadOnlyList&lt;ListedBatch&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L556)

<a id="Vercel_AI_Operations_BatchListResult_NextCursor_"></a>
<a id="Vercel_AI_Operations_BatchListResult_NextCursor"></a>

### NextCursor


```csharp
public string? NextCursor { get; }
```


Next page cursor.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L559)

<a id="Vercel_AI_Operations_BatchListResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchListResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L562)
