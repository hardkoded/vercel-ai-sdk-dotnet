---
title: "OpenAIEmbeddingModel"
description: "OpenAI embeddings model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIEmbeddingModel : IEmbeddingModel
```


OpenAI embeddings model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L13)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIEmbeddingModel(OpenAIProvider, string)


```csharp
public OpenAIEmbeddingModel(OpenAIProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L24)

## Fields

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_DefaultMaxEmbeddingsPerCall"></a>

### DefaultMaxEmbeddingsPerCall


```csharp
public const int DefaultMaxEmbeddingsPerCall = 2048
```


Maximum embedding inputs in one request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L16)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_DefaultMaxInputBytesPerCall"></a>

### DefaultMaxInputBytesPerCall


```csharp
public const int DefaultMaxInputBytesPerCall = 300000
```


Maximum input bytes in one request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L19)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; }
```


Maximum values accepted by one call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L40)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_MaxInputBytesPerCall_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_MaxInputBytesPerCall"></a>

### MaxInputBytesPerCall


```csharp
public int MaxInputBytesPerCall { get; }
```


Maximum combined input size accepted by one call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L43)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L37)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L34)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_SupportsParallelCalls_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether the model accepts parallel embedding calls.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L46)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L49)

<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_EmbedAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIEmbeddingModel_EmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `EmbedAsync(IReadOnlyList<string>, int?, string?, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<OpenAIEmbeddingCallResult> EmbedAsync(IReadOnlyList<string> values, int? dimensions, string? user, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Embeds &lt;code class="paramref"&gt;values&lt;/code&gt;, sending &lt;code class="paramref"&gt;dimensions&lt;/code&gt; and &lt;code class="paramref"&gt;user&lt;/code&gt; when they are set.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `user` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIEmbeddingCallResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Embedding/OpenAIEmbeddingModel.cs#L68)
