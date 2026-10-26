---
title: "VoyageProvider.VoyageEmbeddingModel"
description: "Voyage embedding model."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageProvider.VoyageEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageProvider.VoyageEmbeddingModel : IEmbeddingModel
```


Voyage embedding model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L195)

## Constructors

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel__ctor_Vercel_AI_Voyage_VoyageProvider_System_String_"></a>

### VoyageEmbeddingModel(VoyageProvider, string)


```csharp
public VoyageEmbeddingModel(VoyageProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [VoyageProvider](/api/Vercel.AI.Voyage/VoyageProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L200)

## Properties

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; }
```


Maximum values in one call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L207)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L225)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L219)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L213)

## Methods

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L228)

<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_EmbedAsync_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_VoyageEmbeddingModel_EmbedAsync_Vercel_AI_Voyage_VoyageEmbeddingRequest_System_Threading_CancellationToken_"></a>

### EmbedAsync(VoyageEmbeddingRequest, CancellationToken)


```csharp
public Task<VoyageEmbeddingResult> EmbedAsync(VoyageEmbeddingRequest request, CancellationToken cancellationToken)
```


Embeds values and returns usage plus the raw response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [VoyageEmbeddingRequest](/api/Vercel.AI.Voyage/VoyageEmbeddingRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;VoyageEmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L243)
