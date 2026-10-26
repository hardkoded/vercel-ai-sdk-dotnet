---
title: "BasetenProvider"
description: "Baseten provider. OpenAI Chat Completions compatible at https://inference.baseten.co/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "BasetenProvider"
  hidden: true
---

<a id="Vercel_AI_Baseten_BasetenProvider"></a>

Namespace: [Vercel.AI.Baseten](/api/Vercel.AI.Baseten/)


```csharp
public sealed class BasetenProvider : OpenAICompatibleProvider
```


Baseten provider. OpenAI Chat Completions compatible at `https://inference.baseten.co/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L13)

## Constructors

<a id="Vercel_AI_Baseten_BasetenProvider__ctor_"></a>
<a id="Vercel_AI_Baseten_BasetenProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### BasetenProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public BasetenProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L25)

## Fields

<a id="Vercel_AI_Baseten_BasetenProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "BASETEN_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L22)

<a id="Vercel_AI_Baseten_BasetenProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://inference.baseten.co/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L19)

<a id="Vercel_AI_Baseten_BasetenProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "baseten"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L16)

## Methods

<a id="Vercel_AI_Baseten_BasetenProvider_Create_"></a>
<a id="Vercel_AI_Baseten_BasetenProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static BasetenProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [BasetenProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L77)

<a id="Vercel_AI_Baseten_BasetenProvider_CreateChatModel_"></a>
<a id="Vercel_AI_Baseten_BasetenProvider_CreateChatModel_System_String_"></a>

### CreateChatModel(string)


```csharp
public override OpenAICompatibleLanguageModel CreateChatModel(string modelId)
```


Creates a chat model. Providers override this when support differs by model id.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAICompatibleLanguageModel](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L31)

<a id="Vercel_AI_Baseten_BasetenProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Baseten_BasetenProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Baseten/BasetenProvider.cs#L50)
