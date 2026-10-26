---
title: "CohereRerankRequest"
description: "Cohere rerank request."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereRerankRequest"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereRerankRequest"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereRerankRequest
```


Cohere rerank request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L320)

## Properties

<a id="Vercel_AI_Cohere_CohereRerankRequest_Headers_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L341)

<a id="Vercel_AI_Cohere_CohereRerankRequest_MaxTokensPerDoc_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_MaxTokensPerDoc"></a>

### MaxTokensPerDoc


```csharp
public int? MaxTokensPerDoc { get; set; }
```


`max_tokens_per_doc`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L335)

<a id="Vercel_AI_Cohere_CohereRerankRequest_ObjectDocuments_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_ObjectDocuments"></a>

### ObjectDocuments


```csharp
public IReadOnlyList<JsonObject>? ObjectDocuments { get; set; }
```


JSON documents. They are stringified before they are sent.

**Value:** [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L329)

<a id="Vercel_AI_Cohere_CohereRerankRequest_Priority_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_Priority"></a>

### Priority


```csharp
public int? Priority { get; set; }
```


`priority`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L338)

<a id="Vercel_AI_Cohere_CohereRerankRequest_Query_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_Query"></a>

### Query


```csharp
public string Query { get; set; }
```


Query text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L323)

<a id="Vercel_AI_Cohere_CohereRerankRequest_TextDocuments_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_TextDocuments"></a>

### TextDocuments


```csharp
public IReadOnlyList<string>? TextDocuments { get; set; }
```


Plain-text documents.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L326)

<a id="Vercel_AI_Cohere_CohereRerankRequest_TopN_"></a>
<a id="Vercel_AI_Cohere_CohereRerankRequest_TopN"></a>

### TopN


```csharp
public int? TopN { get; set; }
```


`top_n`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L332)
