---
title: "GatewayProvider"
description: "Vercel AI Gateway provider. Language, embedding, image, and video calls use the Gateway V4 routes (/language-model, /embedding-model, /image-model, /video-mo..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayProvider"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayProvider"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayProvider : ProviderBase, IBatchProvider, IDecisionProvider, IEvaluationProvider
```


Vercel AI Gateway provider. Language, embedding, image, and video calls use the Gateway V4 routes
(`/language-model`, `/embedding-model`, `/image-model`, `/video-model`).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

**Implements:** [IBatchProvider](/api/Vercel.AI.Operations/IBatchProvider/), [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/), [IEvaluationProvider](/api/Vercel.AI.Operations/IEvaluationProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L41)

## Constructors

<a id="Vercel_AI_Gateway_GatewayProvider__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider__ctor_Vercel_AI_Gateway_GatewayOptions_System_Net_Http_HttpClient_"></a>

### GatewayProvider(GatewayOptions?, HttpClient)


```csharp
public GatewayProvider(GatewayOptions? options, HttpClient httpClient)
```


Creates a Gateway provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L51)

## Fields

<a id="Vercel_AI_Gateway_GatewayProvider_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "AI_GATEWAY_API_KEY"
```


Environment variable for the Gateway key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L48)

<a id="Vercel_AI_Gateway_GatewayProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "gateway"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L45)

## Properties

<a id="Vercel_AI_Gateway_GatewayProvider_Http_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L68)

<a id="Vercel_AI_Gateway_GatewayProvider_Options_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_Options"></a>

### Options


```csharp
public GatewayOptions Options { get; }
```


Options.

**Value:** [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L59)

## Methods

<a id="Vercel_AI_Gateway_GatewayProvider_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_Create_Vercel_AI_Gateway_GatewayOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(GatewayOptions?, HttpMessageHandler?)


```csharp
public static GatewayProvider Create(GatewayOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GatewayProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L71)

<a id="Vercel_AI_Gateway_GatewayProvider_DecisionModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_DecisionModel_System_String_"></a>

### DecisionModel(string)


```csharp
public GatewayDecisionModel DecisionModel(string modelId)
```


Decision model routed through `/decision-model`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayDecisionModel](/api/Vercel.AI.Gateway/GatewayDecisionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L108)

<a id="Vercel_AI_Gateway_GatewayProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_EmbeddingModel_System_String_"></a>

### EmbeddingModel(string)


```csharp
public override IEmbeddingModel EmbeddingModel(string modelId)
```


Returns an embedding model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IEmbeddingModel](/api/Vercel.AI.Provider/IEmbeddingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L84)

<a id="Vercel_AI_Gateway_GatewayProvider_EvaluationModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_EvaluationModel_System_String_"></a>

### EvaluationModel(string)


```csharp
[Obsolete("Use DecisionModel instead.")]
public GatewayDecisionModel EvaluationModel(string modelId)
```


Deprecated alias of [DecisionModel(string)](#Vercel_AI_Gateway_GatewayProvider_DecisionModel_System_String_). Maps to `evaluationModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayDecisionModel](/api/Vercel.AI.Gateway/GatewayDecisionModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L120)

<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalBatch_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalBatch"></a>

### ExperimentalBatch()


```csharp
public IBatchApi? ExperimentalBatch()
```


Gateway batch API. Downloads batch results.

**Returns:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L62)

<a id="Vercel_AI_Gateway_GatewayProvider_GetGenerationInfoAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_GetGenerationInfoAsync_Vercel_AI_Gateway_GatewayGenerationInfoParams_System_Threading_CancellationToken_"></a>

### GetGenerationInfoAsync(GatewayGenerationInfoParams, CancellationToken)


```csharp
public Task<GatewayGenerationInfo> GetGenerationInfoAsync(GatewayGenerationInfoParams parameters, CancellationToken cancellationToken = default)
```


Gets the info of one generation, including its cost breakdown.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `parameters` | [GatewayGenerationInfoParams](/api/Vercel.AI.Gateway/GatewayGenerationInfoParams/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayGenerationInfo&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L217)

<a id="Vercel_AI_Gateway_GatewayProvider_ImageModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_ImageModel_System_String_"></a>

### ImageModel(string)


```csharp
public override IImageModel ImageModel(string modelId)
```


Returns an image model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L90)

<a id="Vercel_AI_Gateway_GatewayProvider_LanguageModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L78)

<a id="Vercel_AI_Gateway_GatewayProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public override ITranscriptionModel TranscriptionModel(string modelId)
```


Returns a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L102)

<a id="Vercel_AI_Gateway_GatewayProvider_VideoModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public override IVideoModel VideoModel(string modelId)
```


Returns a video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L96)
