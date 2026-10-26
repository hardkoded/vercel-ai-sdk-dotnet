---
title: "CohereRerankingModel"
description: "Cohere rerank model."
editUrl: false
prev: false
next: false
sidebar:
  label: "CohereRerankingModel"
  hidden: true
---

<a id="Vercel_AI_Cohere_CohereRerankingModel"></a>

Namespace: [Vercel.AI.Cohere](/api/Vercel.AI.Cohere/)


```csharp
public sealed class CohereRerankingModel : IRerankingModel
```


Cohere rerank model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L199)

## Constructors

<a id="Vercel_AI_Cohere_CohereRerankingModel__ctor_"></a>
<a id="Vercel_AI_Cohere_CohereRerankingModel__ctor_Vercel_AI_Cohere_CohereProvider_System_String_"></a>

### CohereRerankingModel(CohereProvider, string)


```csharp
public CohereRerankingModel(CohereProvider provider, string modelId)
```


Creates a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [CohereProvider](/api/Vercel.AI.Cohere/CohereProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L204)

## Properties

<a id="Vercel_AI_Cohere_CohereRerankingModel_ModelId_"></a>
<a id="Vercel_AI_Cohere_CohereRerankingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L214)

<a id="Vercel_AI_Cohere_CohereRerankingModel_Provider_"></a>
<a id="Vercel_AI_Cohere_CohereRerankingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L211)

## Methods

<a id="Vercel_AI_Cohere_CohereRerankingModel_DoRerankAsync_"></a>
<a id="Vercel_AI_Cohere_CohereRerankingModel_DoRerankAsync_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `DoRerankAsync(string, IReadOnlyList<string>, int?, CancellationToken, IReadOnlyDictionary<string, string?>?)`


```csharp
public Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, CancellationToken cancellationToken, IReadOnlyDictionary<string, string?>? headers = null)
```


Reranks documents for a query. &lt;code class="paramref"&gt;headers&lt;/code&gt; are sent with this call and override the provider headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Task&lt;RerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L217)

<a id="Vercel_AI_Cohere_CohereRerankingModel_RerankAsync_"></a>
<a id="Vercel_AI_Cohere_CohereRerankingModel_RerankAsync_Vercel_AI_Cohere_CohereRerankRequest_System_Threading_CancellationToken_"></a>

### RerankAsync(CohereRerankRequest, CancellationToken)


```csharp
public Task<CohereRerankResult> RerankAsync(CohereRerankRequest request, CancellationToken cancellationToken)
```


Reranks text or JSON documents and returns the response body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [CohereRerankRequest](/api/Vercel.AI.Cohere/CohereRerankRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;CohereRerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cohere/CohereProvider.cs#L224)
