---
title: "OpenAIEmbeddingCallResult"
description: "Embedding vectors plus the raw response."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIEmbeddingCallResult"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIEmbeddingCallResult
```


Embedding vectors plus the raw response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L146)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


HTTP response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L159)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_RawBody_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_RawBody"></a>

### RawBody


```csharp
public string RawBody { get; }
```


Raw response body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L162)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_Result_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingCallResult_Result"></a>

### Result


```csharp
public EmbeddingResult Result { get; }
```


Embedding vectors and token count.

**Value:** [EmbeddingResult](/api/Vercel.AI.Provider/EmbeddingResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L156)
