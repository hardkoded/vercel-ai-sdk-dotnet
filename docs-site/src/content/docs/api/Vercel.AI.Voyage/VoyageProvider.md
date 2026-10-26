---
title: "VoyageProvider"
description: "Voyage embeddings and reranking."
editUrl: false
prev: false
next: false
sidebar:
  label: "VoyageProvider"
  hidden: true
---

<a id="Vercel_AI_Voyage_VoyageProvider"></a>

Namespace: [Vercel.AI.Voyage](/api/Vercel.AI.Voyage/)


```csharp
public sealed class VoyageProvider : OpenAICompatibleProvider
```


Voyage embeddings and reranking.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L133)

## Constructors

<a id="Vercel_AI_Voyage_VoyageProvider__ctor_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### VoyageProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public VoyageProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L144)

## Fields

<a id="Vercel_AI_Voyage_VoyageProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.voyageai.com/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L139)

<a id="Vercel_AI_Voyage_VoyageProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "voyage"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L136)

## Methods

<a id="Vercel_AI_Voyage_VoyageProvider_Create_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static VoyageProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [VoyageProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L151)

<a id="Vercel_AI_Voyage_VoyageProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L170)

<a id="Vercel_AI_Voyage_VoyageProvider_ImageModel_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L164)

<a id="Vercel_AI_Voyage_VoyageProvider_LanguageModel_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L158)

<a id="Vercel_AI_Voyage_VoyageProvider_RerankingModel_"></a>
<a id="Vercel_AI_Voyage_VoyageProvider_RerankingModel_System_String_"></a>

### RerankingModel(string)


```csharp
public override IRerankingModel RerankingModel(string modelId)
```


Returns a reranking model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IRerankingModel](/api/Vercel.AI.Provider/IRerankingModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Voyage/VoyageProvider.cs#L176)
