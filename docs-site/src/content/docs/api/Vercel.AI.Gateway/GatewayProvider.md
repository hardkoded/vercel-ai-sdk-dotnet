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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L47)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L57)

## Fields

<a id="Vercel_AI_Gateway_GatewayProvider_ApiKeyEnvironmentVariable"></a>

### ApiKeyEnvironmentVariable


```csharp
public const string ApiKeyEnvironmentVariable = "AI_GATEWAY_API_KEY"
```


Environment variable for the Gateway key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L54)

<a id="Vercel_AI_Gateway_GatewayProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "gateway"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L51)

## Properties

<a id="Vercel_AI_Gateway_GatewayProvider_Http_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_Http"></a>

### Http


```csharp
public ProviderHttp Http { get; }
```


HTTP helper.

**Value:** [ProviderHttp](/api/Vercel.AI.ProviderUtils/ProviderHttp/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L74)

<a id="Vercel_AI_Gateway_GatewayProvider_Options_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_Options"></a>

### Options


```csharp
public GatewayOptions Options { get; }
```


Options.

**Value:** [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L65)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L77)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L126)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L90)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L138)

<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalBatch_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalBatch"></a>

### ExperimentalBatch()


```csharp
public IBatchApi? ExperimentalBatch()
```


Gateway batch API. Downloads batch results.

**Returns:** [IBatchApi](/api/Vercel.AI.Operations/IBatchApi/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L68)

<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalRealtimeGetTokenAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_ExperimentalRealtimeGetTokenAsync_System_String_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### ExperimentalRealtimeGetTokenAsync(string, int?, CancellationToken)


```csharp
public Task<GatewayRealtimeClientSecret> ExperimentalRealtimeGetTokenAsync(string model, int? expiresAfterSeconds = null, CancellationToken cancellationToken = default)
```


Mints a client secret for a realtime model and returns it with the WebSocket URL. Maps to
`experimental_realtime.getToken`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L120)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L235)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L96)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L84)

<a id="Vercel_AI_Gateway_GatewayProvider_RealtimeModel_"></a>
<a id="Vercel_AI_Gateway_GatewayProvider_RealtimeModel_System_String_"></a>

### RealtimeModel(string)


```csharp
public override IRealtimeModel RealtimeModel(string modelId)
```


Returns a realtime model. Maps to `experimental_realtime(modelId)`. Building the model touches no credential;
only minting a client secret and server connections resolve the Gateway credential.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L111)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayProvider.cs#L102)
