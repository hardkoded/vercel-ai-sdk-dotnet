---
title: "AlibabaEmbeddingResult"
description: "Alibaba embedding result."
editUrl: false
prev: false
next: false
sidebar:
  label: "AlibabaEmbeddingResult"
  hidden: true
---

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult"></a>

Namespace: [Vercel.AI.Alibaba](/api/Vercel.AI.Alibaba/)


```csharp
public sealed class AlibabaEmbeddingResult
```


Alibaba embedding result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L270)

## Constructors

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult__ctor_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult__ctor_System_Collections_Generic_IReadOnlyList_System_Single____System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `AlibabaEmbeddingResult(IReadOnlyList<float[]>, int, IReadOnlyDictionary<string, string>)`


```csharp
public AlibabaEmbeddingResult(IReadOnlyList<float[]> embeddings, int tokens, IReadOnlyDictionary<string, string> headers)
```


Creates an embedding result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `embeddings` | [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `tokens` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L273)

## Properties

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Embeddings_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<float[]> Embeddings { get; }
```


Vectors ordered by `text_index`.

**Value:** [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L281)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Headers_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L287)

<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Tokens_"></a>
<a id="Vercel_AI_Alibaba_AlibabaEmbeddingResult_Tokens"></a>

### Tokens


```csharp
public int Tokens { get; }
```


Token count. Zero when usage is omitted.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L284)
