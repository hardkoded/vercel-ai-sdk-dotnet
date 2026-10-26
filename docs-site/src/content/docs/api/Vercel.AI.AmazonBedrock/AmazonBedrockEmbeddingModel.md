---
title: "AmazonBedrockEmbeddingModel"
description: "Amazon Bedrock InvokeModel embedding model for Titan, Cohere, and Nova."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockEmbeddingModel"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public sealed class AmazonBedrockEmbeddingModel : IEmbeddingModel
```


Amazon Bedrock `InvokeModel` embedding model for Titan, Cohere, and Nova.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L15)

## Constructors

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel__ctor_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockProvider_System_String_"></a>

### AmazonBedrockEmbeddingModel(AmazonBedrockProvider, string)


```csharp
public AmazonBedrockEmbeddingModel(AmazonBedrockProvider provider, string modelId)
```


Creates an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L22)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel__ctor_Vercel_AI_AmazonBedrock_AmazonBedrockProvider_System_String_Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModelSettings_"></a>

### AmazonBedrockEmbeddingModel(AmazonBedrockProvider, string, AmazonBedrockEmbeddingModelSettings?)


```csharp
public AmazonBedrockEmbeddingModel(AmazonBedrockProvider provider, string modelId, AmazonBedrockEmbeddingModelSettings? settings)
```


Creates an embedding model with &lt;code class="paramref"&gt;settings&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `settings` | [AmazonBedrockEmbeddingModelSettings](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModelSettings/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L28)

## Properties

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_MaxEmbeddingsPerCall_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_MaxEmbeddingsPerCall"></a>

### MaxEmbeddingsPerCall


```csharp
public int MaxEmbeddingsPerCall { get; }
```


Maximum values per call. Cohere models take 96. Titan and Nova take one.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L51)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_ModelId_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L48)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_Provider_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L45)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_SpecificationVersion_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `V4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L42)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_DoEmbedAsync_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_DoEmbedAsync_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Text_Json_JsonElement__System_Threading_CancellationToken_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L56)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockEmbeddingModel_DoEmbedAsync_Vercel_AI_Provider_EmbeddingCallOptions_System_Threading_CancellationToken_"></a>

### DoEmbedAsync(EmbeddingCallOptions, CancellationToken)


```csharp
public Task<EmbeddingResult> DoEmbedAsync(EmbeddingCallOptions call, CancellationToken cancellationToken)
```


Embeds each value. Values, provider options, headers, and dimensions travel in &lt;code class="paramref"&gt;call&lt;/code&gt;, so concurrent calls do not share state.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [EmbeddingCallOptions](/api/Vercel.AI.Provider/EmbeddingCallOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EmbeddingResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockEmbeddingModel.cs#L62)
