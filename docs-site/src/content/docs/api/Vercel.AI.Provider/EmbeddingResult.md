---
title: "EmbeddingResult"
description: "Embedding vector result."
editUrl: false
prev: false
next: false
sidebar:
  label: "EmbeddingResult"
  hidden: true
---

<a id="Vercel_AI_Provider_EmbeddingResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class EmbeddingResult
```


Embedding vector result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L29)

## Constructors

<a id="Vercel_AI_Provider_EmbeddingResult__ctor_"></a>
<a id="Vercel_AI_Provider_EmbeddingResult__ctor_System_Collections_Generic_IReadOnlyList_System_Single____System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_CallWarning__Vercel_AI_Provider_EmbeddingResponseInfo_"></a>

### `EmbeddingResult(IReadOnlyList<float[]>, int?, IReadOnlyList<CallWarning>?, EmbeddingResponseInfo?)`


```csharp
public EmbeddingResult(IReadOnlyList<float[]> embeddings, int? tokens, IReadOnlyList<CallWarning>? warnings = null, EmbeddingResponseInfo? response = null)
```


Creates an embedding result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `embeddings` | [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `tokens` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `warnings` | [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `response` | [EmbeddingResponseInfo](/api/Vercel.AI.Provider/EmbeddingResponseInfo/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L32)

## Properties

<a id="Vercel_AI_Provider_EmbeddingResult_Embeddings_"></a>
<a id="Vercel_AI_Provider_EmbeddingResult_Embeddings"></a>

### Embeddings


```csharp
public IReadOnlyList<float[]> Embeddings { get; }
```


One vector per input value.

**Value:** [IReadOnlyList&lt;float\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L47)

<a id="Vercel_AI_Provider_EmbeddingResult_Response_"></a>
<a id="Vercel_AI_Provider_EmbeddingResult_Response"></a>

### Response


```csharp
public EmbeddingResponseInfo? Response { get; }
```


Raw HTTP response of this call, when the provider reports it.

**Value:** [EmbeddingResponseInfo](/api/Vercel.AI.Provider/EmbeddingResponseInfo/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L44)

<a id="Vercel_AI_Provider_EmbeddingResult_Tokens_"></a>
<a id="Vercel_AI_Provider_EmbeddingResult_Tokens"></a>

### Tokens


```csharp
public int? Tokens { get; }
```


Token usage, when the provider reports it.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L50)

<a id="Vercel_AI_Provider_EmbeddingResult_Warnings_"></a>
<a id="Vercel_AI_Provider_EmbeddingResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Warnings of this call.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L41)
