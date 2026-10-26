---
title: "VoyageProvider.VoyageRerankingModel"
description: "Voyage reranking model."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageProvider.VoyageRerankingModel"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageProvider.VoyageRerankingModel : IRerankingModel
```


Voyage reranking model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L341)

## Constructors

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel__ctor_Vercel_AI_Voyage_VoyageProvider_System_String_"></a>

### VoyageRerankingModel(VoyageProvider, string)


```csharp
public VoyageRerankingModel(VoyageProvider provider, string modelId)
```


Creates a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [VoyageProvider](/api/Vercel.AI.Voyage/VoyageProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L346)

## Properties

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_ModelId_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L359)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_Provider_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L353)

## Methods

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_DoRerankAsync_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_DoRerankAsync_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L362)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_RerankAsync_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageRerankingModel_RerankAsync_Vercel_AI_Voyage_VoyageRerankRequest_System_Threading_CancellationToken_"></a>

### RerankAsync(VoyageRerankRequest, CancellationToken)


```csharp
public Task<VoyageRerankResult> RerankAsync(VoyageRerankRequest request, CancellationToken cancellationToken)
```


Reranks text or JSON documents.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [VoyageRerankRequest](/api/Vercel.AI.Voyage/VoyageRerankRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VoyageRerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L369)
