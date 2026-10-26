---
title: "VoyageEmbeddingResult"
description: "Voyage embedding result, including the raw response."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageEmbeddingResult"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageEmbeddingResult
```


Voyage embedding result, including the raw response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L43)

## Constructors

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingResult__ctor_System_Collections_Generic_IReadOnlyList_System_Single____System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Text_Json_JsonElement_"></a>

### `VoyageEmbeddingResult(IReadOnlyList<float[]>, int, IReadOnlyDictionary<string, string>, JsonElement)`


```csharp
public VoyageEmbeddingResult(IReadOnlyList<float[]> embeddings, int tokens, IReadOnlyDictionary<string, string> headers, JsonElement body)
```


Creates an embedding result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `embeddings` | [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `tokens` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `body` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L46)

## Properties

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Body_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Body"></a>

### Body


```csharp
public JsonElement Body { get; }
```


Raw response body.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L64)

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Embeddings_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<float[]> Embeddings { get; }
```


Vectors in input order.

**Value:** [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L55)

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Headers_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L61)

<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Tokens_"></a>
<a id="Vercel_AI_Voyage_VoyageEmbeddingResult_Tokens"></a>

### Tokens


```csharp
public int Tokens { get; }
```


Token count. Zero when the provider omits usage.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L58)
