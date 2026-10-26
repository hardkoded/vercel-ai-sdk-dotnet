---
title: "PerplexityEmbeddingModel"
description: "Perplexity embeddings at {base}/v1/embeddings. Perplexity returns quantized vectors as base64, so the model decodes base64int8 (the default) or base64binary..."
editUrl: false
prev: false
next: false
sidebar:
  label: "PerplexityEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel"></a>

Namespace: [Vercel.AI.Perplexity](/api/Vercel.AI.Perplexity/)


```csharp
public sealed class PerplexityEmbeddingModel : IEmbeddingModel, IEmbeddingCaller
```


Perplexity embeddings at `{base}/v1/embeddings`. Perplexity returns quantized vectors as base64,
so the model decodes `base64_int8` (the default) or `base64_binary` into numbers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/), [IEmbeddingCaller](/api/Vercel.AI.Operations/IEmbeddingCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L18)

## Constructors

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel__ctor_Vercel_AI_Perplexity_PerplexityProvider_System_String_"></a>

### PerplexityEmbeddingModel(PerplexityProvider, string)


```csharp
public PerplexityEmbeddingModel(PerplexityProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L26)

## Fields

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_MaxValuesPerCall"></a>

### MaxValuesPerCall


```csharp
public const int MaxValuesPerCall = 512
```


Maximum values per call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L21)

## Properties

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int? MaxEmbeddingsPerCall { get; }
```


Maximum values per call. Null means unlimited.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L42)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_MaxInputBytesPerCall_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_MaxInputBytesPerCall"></a>

### MaxInputBytesPerCall


```csharp
public double? MaxInputBytesPerCall { get; }
```


Maximum UTF-8 input bytes per call. Null and infinity mean unlimited.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L45)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L36)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L33)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L39)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_SupportsParallelCalls_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether chunks may run at the same time.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L48)

## Methods

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `DoEmbedAsync(IReadOnlyList<string>, IReadOnlyDictionary<string, JsonElement>?, CancellationToken, int?, IReadOnlyDictionary<string, string?>?)`


```csharp
public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, IReadOnlyDictionary<string, JsonElement>? providerOptions, CancellationToken cancellationToken, int? dimensions = null, IReadOnlyDictionary<string, string?>? headers = null)
```


Embeds each value. Maps to `doEmbed`. &lt;code class="paramref"&gt;providerOptions&lt;/code&gt; is keyed by provider id. &lt;code class="paramref"&gt;dimensions&lt;/code&gt; is the requested vector width. A provider option for the same setting wins. &lt;code class="paramref"&gt;headers&lt;/code&gt; are sent with this call and override the provider headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `providerOptions` | [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L57)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_DoEmbedAsync_Vercel_AI_Operations_EmbeddingModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L70)

<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_TransformProviderOptionsAsync_"></a>
<a id="Vercel_AI_Perplexity_PerplexityEmbeddingModel_TransformProviderOptionsAsync_System_Nullable_System_Text_Json_JsonElement__System_Collections_Generic_IReadOnlyList_System_String__System_Int32_System_Int32_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Perplexity/PerplexityEmbeddingModel.cs#L51)
