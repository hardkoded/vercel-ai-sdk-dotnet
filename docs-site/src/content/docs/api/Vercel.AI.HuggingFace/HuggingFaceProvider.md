---
title: "HuggingFaceProvider"
description: "HuggingFace provider. OpenAI Chat Completions compatible at https://router.huggingface.co/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "HuggingFaceProvider"
  hidden: true
---

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider"></a>

Namespace: [Vercel.AI.HuggingFace](/api/Vercel.AI.HuggingFace/)


```csharp
public sealed class HuggingFaceProvider : OpenAICompatibleProvider
```


HuggingFace provider. OpenAI Chat Completions compatible at `https://router.huggingface.co/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L12)

## Constructors

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider__ctor_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### HuggingFaceProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public HuggingFaceProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L24)

## Fields

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "HUGGINGFACE_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L21)

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://router.huggingface.co/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L18)

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "huggingface"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L15)

## Methods

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_Create_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static HuggingFaceProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [HuggingFaceProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L48)

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L36)

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ImageModel_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L42)

<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ResponsesModel_"></a>
<a id="Vercel_AI_HuggingFace_HuggingFaceProvider_ResponsesModel_System_String_"></a>

### ResponsesModel(string)


```csharp
public HuggingFaceResponsesLanguageModel ResponsesModel(string modelId)
```


Creates a Responses API model. [LanguageModel(string)](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/#Vercel_AI_OpenAICompatible_OpenAICompatibleProvider_LanguageModel_System_String_) stays on Chat Completions.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [HuggingFaceResponsesLanguageModel](/api/Vercel.AI.HuggingFace/HuggingFaceResponsesLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HuggingFace/HuggingFaceProvider.cs#L30)
