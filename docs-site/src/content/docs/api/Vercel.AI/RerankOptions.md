---
title: "RerankOptions"
description: "Rerank options."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankOptions"
  hidden: true
---

<a id="Vercel_AI_RerankOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class RerankOptions
```


Rerank options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L331)

## Properties

<a id="Vercel_AI_RerankOptions_Documents_"></a>
<a id="Vercel_AI_RerankOptions_Documents"></a>

### Documents


```csharp
public IReadOnlyList<string> Documents { get; set; }
```


Documents.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L340)

<a id="Vercel_AI_RerankOptions_Headers_"></a>
<a id="Vercel_AI_RerankOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L346)

<a id="Vercel_AI_RerankOptions_Model_"></a>
<a id="Vercel_AI_RerankOptions_Model"></a>

### Model


```csharp
public IRerankingModel? Model { get; set; }
```


Reranking model.

**Value:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L334)

<a id="Vercel_AI_RerankOptions_Query_"></a>
<a id="Vercel_AI_RerankOptions_Query"></a>

### Query


```csharp
public string Query { get; set; }
```


Query.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L337)

<a id="Vercel_AI_RerankOptions_TopN_"></a>
<a id="Vercel_AI_RerankOptions_TopN"></a>

### TopN


```csharp
public int? TopN { get; set; }
```


Maximum results.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L343)
