---
title: "GoogleMultimodalEmbedding"
description: "Gemini embedding caller used by EmbedMany. Splits multimodal google.content to the chunk that is about to be sent and posts embedContent or batchEmbedContents."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleMultimodalEmbedding"
  hidden: true
---

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GoogleMultimodalEmbedding : IEmbeddingCaller
```


Gemini embedding caller used by [EmbedMany](/api/Vercel.AI.Operations/EmbedMany/).
Splits multimodal `google.content` to the chunk that is about to be sent
and posts `embedContent` or `batchEmbedContents`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L18)

## Constructors

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding__ctor_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding__ctor_System_String_System_Net_Http_HttpClient_System_String_"></a>

### GoogleMultimodalEmbedding(string, HttpClient, string?)


```csharp
public GoogleMultimodalEmbedding(string modelId, HttpClient http, string? baseUrl = null)
```


Creates a caller that posts to &lt;code class="paramref"&gt;baseUrl&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `http` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L27)

## Fields

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_EmbeddingsPerCall"></a>

### EmbeddingsPerCall


```csharp
public const int EmbeddingsPerCall = 100
```


Values accepted by one Gemini embedding request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L21)

## Properties

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int? MaxEmbeddingsPerCall { get; }
```


Maximum values per call. Null means unlimited.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L44)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_MaxInputBytesPerCall_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_MaxInputBytesPerCall"></a>

### MaxInputBytesPerCall


```csharp
public double? MaxInputBytesPerCall { get; }
```


Maximum UTF-8 input bytes per call. Null and infinity mean unlimited.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L47)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_ModelId_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L38)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_Provider_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L35)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_SpecificationVersion_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. `v4` is current. `v2` omits warnings.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L41)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_SupportsParallelCalls_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether chunks may run at the same time.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L50)

## Methods

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_DoEmbedAsync_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_DoEmbedAsync_Vercel_AI_Operations_EmbeddingModelCall_System_Threading_CancellationToken_"></a>

### DoEmbedAsync(EmbeddingModelCall, CancellationToken)


```csharp
public Task<EmbeddingModelResponse> DoEmbedAsync(EmbeddingModelCall call, CancellationToken cancellationToken)
```


Embeds one chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [EmbeddingModelCall](/api/Vercel.AI.Operations/EmbeddingModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingModelResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L81)

<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_TransformProviderOptionsAsync_"></a>
<a id="Vercel_AI_Operations_GoogleMultimodalEmbedding_TransformProviderOptionsAsync_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_System_String__System_Int32_System_Int32_System_Threading_CancellationToken_"></a>

### `TransformProviderOptionsAsync(JsonElement?, IReadOnlyList<string>, int, int, CancellationToken)`


```csharp
public Task<JsonElement?> TransformProviderOptionsAsync(JsonElement? providerOptions, IReadOnlyList<string> values, int startIndex, int endIndex, CancellationToken cancellationToken)
```


Slices provider options for one chunk. Null leaves the options unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptions` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `startIndex` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `endIndex` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;JsonElement?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GoogleMultimodalEmbedding.cs#L53)
