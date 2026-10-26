---
title: "AmazonBedrockRerankingModel"
description: "Amazon Bedrock Agent Runtime rerank model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockRerankingModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockRerankingModel : IRerankingModel, IRerankCaller
```


Amazon Bedrock Agent Runtime rerank model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/), [IRerankCaller](/api/Vercel.AI.Operations/IRerankCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L14)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockProvider_System_String_"></a>

### AmazonBedrockRerankingModel(AmazonBedrockProvider, string)


```csharp
public AmazonBedrockRerankingModel(AmazonBedrockProvider provider, string modelId)
```


Creates a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L19)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L29)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L26)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankAsync_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__System_Threading_CancellationToken_"></a>

### `DoRerankAsync(string, IReadOnlyList<string>, int?, JsonElement?, CancellationToken)`


```csharp
public Task<RerankResult> DoRerankAsync(string query, IReadOnlyList<string> documents, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
```


Reranks text documents. &lt;code class="paramref"&gt;providerOptions&lt;/code&gt; is the V4 provider-options object
(`amazonBedrock` or legacy `bedrock`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L42)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankAsync_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L32)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankAsync_Vercel_AI_Operations_RerankModelCall_System_Threading_CancellationToken_"></a>

### DoRerankAsync(RerankModelCall, CancellationToken)


```csharp
public Task<RerankModelResponse> DoRerankAsync(RerankModelCall call, CancellationToken cancellationToken)
```


Reranks text or object documents. Call headers are sent before SigV4 signing.
The response carries the headers and raw body, with no warnings or provider metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [RerankModelCall](/api/Vercel.AI.Operations/RerankModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankModelResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L59)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankObjectsAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockRerankingModel_DoRerankObjectsAsync_System_String_System_Collections_Generic_IReadOnlyList_System_Text_Json_JsonElement__System_Nullable_System_Int32__System_Nullable_System_Text_Json_JsonElement__System_Threading_CancellationToken_"></a>

### `DoRerankObjectsAsync(string, IReadOnlyList<JsonElement>, int?, JsonElement?, CancellationToken)`


```csharp
public Task<RerankResult> DoRerankObjectsAsync(string query, IReadOnlyList<JsonElement> documents, int? topN, JsonElement? providerOptions, CancellationToken cancellationToken)
```


Reranks JSON documents. Each value is sent as an inline JSON document.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `query` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `documents` | [IReadOnlyList&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `topN` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RerankResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockRerankingModel.cs#L49)
