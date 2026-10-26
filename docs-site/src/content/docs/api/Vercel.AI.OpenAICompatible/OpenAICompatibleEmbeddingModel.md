---
title: "OpenAICompatibleEmbeddingModel"
description: "OpenAI-compatible embeddings model."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public sealed class OpenAICompatibleEmbeddingModel : IEmbeddingModel
```


OpenAI-compatible embeddings model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L544)

## Constructors

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel__ctor_Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_System_String_"></a>

### OpenAICompatibleEmbeddingModel(OpenAICompatibleProvider, string)


```csharp
public OpenAICompatibleEmbeddingModel(OpenAICompatibleProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L549)

## Properties

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_Endpoint_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_Endpoint"></a>

### Endpoint


```csharp
public Uri? Endpoint { get; set; }
```


When set, the embedding call uses this URL instead of the provider embeddings route.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L569)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; set; }
```


Maximum inputs accepted by one request.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L566)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L563)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L560)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L557)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L572)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Nullable_System_Int32__System_String_System_Threading_CancellationToken_"></a>

### `DoEmbedAsync(IReadOnlyList<string>, int?, string?, CancellationToken)`


```csharp
public Task<EmbeddingResult> DoEmbedAsync(IReadOnlyList<string> values, int? dimensions, string? user, CancellationToken cancellationToken)
```


Embeds values, sending &lt;code class="paramref"&gt;dimensions&lt;/code&gt; and &lt;code class="paramref"&gt;user&lt;/code&gt; when they are set. A `dimensions` provider option wins over &lt;code class="paramref"&gt;dimensions&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `dimensions` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `user` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L584)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleEmbeddingModel_DoEmbedAsync_Vercel_AI_Provider_EmbeddingCallOptions_System_Threading_CancellationToken_"></a>

### DoEmbedAsync(EmbeddingCallOptions, CancellationToken)


```csharp
public Task<EmbeddingResult> DoEmbedAsync(EmbeddingCallOptions options, CancellationToken cancellationToken)
```


Embeds values with per-call options. Nothing is kept on the model, so concurrent calls do not see each other.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [EmbeddingCallOptions](/api/Vercel.AI.Provider/EmbeddingCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleProvider.cs#L578)
