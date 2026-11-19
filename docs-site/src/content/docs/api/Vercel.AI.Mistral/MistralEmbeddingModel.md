---
title: "MistralEmbeddingModel"
description: "Mistral text embedding model. Maps to MistralEmbeddingModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralEmbeddingModel"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralEmbeddingModel : IEmbeddingModel
```


Mistral text embedding model. Maps to `MistralEmbeddingModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L15)

## Constructors

<a id="Vercel_AI_Mistral_MistralEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_"></a>

### MistralEmbeddingModel(OpenAICompatibleProvider, string)


```csharp
public MistralEmbeddingModel(OpenAICompatibleProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L20)

## Properties

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; }
```


Maximum inputs accepted by one request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L36)

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L33)

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L30)

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L27)

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_SupportsParallelCalls_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_SupportsParallelCalls"></a>

### SupportsParallelCalls


```csharp
public bool SupportsParallelCalls { get; }
```


Whether the model embeds in parallel when a call has more values than [MaxEmbeddingsPerCall](#Vercel_AI_Mistral_MistralEmbeddingModel_MaxEmbeddingsPerCall).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L39)

## Methods

<a id="Vercel_AI_Mistral_MistralEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_Mistral_MistralEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralEmbeddingModel.cs#L42)
