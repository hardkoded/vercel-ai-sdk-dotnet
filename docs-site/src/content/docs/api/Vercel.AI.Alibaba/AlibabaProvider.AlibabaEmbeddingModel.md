---
title: "AlibabaProvider.AlibabaEmbeddingModel"
description: "DashScope text-embedding model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AlibabaProvider.AlibabaEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel"></a>

Namespace: [Vercel.AI.Alibaba](/api/Vercel.AI.Alibaba/)


```csharp
public sealed class AlibabaProvider.AlibabaEmbeddingModel : IEmbeddingModel
```


DashScope text-embedding model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L90)

## Constructors

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel__ctor_Vercel_AI_Alibaba_AlibabaProvider_System_String_"></a>

### AlibabaEmbeddingModel(AlibabaProvider, string)


```csharp
public AlibabaEmbeddingModel(AlibabaProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AlibabaProvider](/api/Vercel.AI.Alibaba/AlibabaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L95)

## Properties

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; }
```


Maximum values in one call.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L102)

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L120)

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L114)

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L108)

## Methods

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L123)

<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_EmbedAsync_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_AlibabaEmbeddingModel_EmbedAsync_Vercel_AI_Alibaba_AlibabaEmbeddingRequest_System_Threading_CancellationToken_"></a>

### EmbedAsync(AlibabaEmbeddingRequest, CancellationToken)


```csharp
public Task<AlibabaEmbeddingResult> EmbedAsync(AlibabaEmbeddingRequest request, CancellationToken cancellationToken)
```


Embeds texts with the native DashScope embedding route.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [AlibabaEmbeddingRequest](/api/Vercel.AI.Alibaba/AlibabaEmbeddingRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AlibabaEmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L138)
