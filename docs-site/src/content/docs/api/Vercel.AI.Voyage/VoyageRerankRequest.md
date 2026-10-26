---
title: "VoyageRerankRequest"
description: "Voyage rerank request."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageRerankRequest"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageRerankRequest"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageRerankRequest
```


Voyage rerank request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L89)

## Properties

<a id="Vercel_AI_Voyage_VoyageRerankRequest_Headers_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankRequest_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L104)

<a id="Vercel_AI_Voyage_VoyageRerankRequest_ObjectDocuments_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankRequest_ObjectDocuments"></a>

### ObjectDocuments


```csharp
public IReadOnlyList<JsonObject>? ObjectDocuments { get; set; }
```


JSON documents. They are stringified before they are sent.

**Value:** [IReadOnlyList&lt;JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L98)

<a id="Vercel_AI_Voyage_VoyageRerankRequest_Query_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankRequest_Query"></a>

### Query


```csharp
public string Query { get; set; }
```


Query text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L92)

<a id="Vercel_AI_Voyage_VoyageRerankRequest_TextDocuments_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankRequest_TextDocuments"></a>

### TextDocuments


```csharp
public IReadOnlyList<string>? TextDocuments { get; set; }
```


Plain-text documents.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L95)

<a id="Vercel_AI_Voyage_VoyageRerankRequest_TopK_"></a>
<a id="Vercel_AI_Voyage_VoyageRerankRequest_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


`top_k`, when set.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L101)
