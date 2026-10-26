---
title: "TogetherAIRerankingModel"
description: "Together reranking model. Posts /rerank and reads relevancescore."
editUrl: false
prev: false
next: false
sidebar:
  label: "TogetherAIRerankingModel"
  hidden: true
---

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel"></a>

Namespace: [Vercel.AI.TogetherAI](/api/Vercel.AI.TogetherAI/)


```csharp
public sealed class TogetherAIRerankingModel : IRerankingModel, IRerankCaller
```


Together reranking model. Posts `/rerank` and reads `relevance_score`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/), [IRerankCaller](/api/Vercel.AI.Operations/IRerankCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L12)

## Constructors

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel__ctor_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel__ctor_Vercel_AI_TogetherAI_TogetherAIProvider_System_String_"></a>

### TogetherAIRerankingModel(TogetherAIProvider, string)


```csharp
public TogetherAIRerankingModel(TogetherAIProvider provider, string modelId)
```


Creates a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [TogetherAIProvider](/api/Vercel.AI.TogetherAI/TogetherAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L17)

## Properties

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_ModelId_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L27)

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_Provider_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L24)

## Methods

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_DoRerankAsync_"></a>
<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_DoRerankAsync_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L30)

<a id="Vercel_AI_TogetherAI_TogetherAIRerankingModel_DoRerankAsync_Vercel_AI_Operations_RerankModelCall_System_Threading_CancellationToken_"></a>

### DoRerankAsync(RerankModelCall, CancellationToken)


```csharp
public Task<RerankModelResponse> DoRerankAsync(RerankModelCall call, CancellationToken cancellationToken)
```


Reranks text or object documents. `togetherai.rankFields` is sent as `rank_fields`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [RerankModelCall](/api/Vercel.AI.Operations/RerankModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankModelResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/TogetherAI/TogetherAIRerankingModel.cs#L38)
