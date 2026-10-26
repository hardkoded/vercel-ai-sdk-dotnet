---
title: "FireworksProvider"
description: "Fireworks provider. OpenAI Chat Completions compatible at https://api.fireworks.ai/inference/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "FireworksProvider"
  hidden: true
---

<a id="Vercel_AI_Fireworks_FireworksProvider"></a>

Namespace: [Vercel.AI.Fireworks](/api/Vercel.AI.Fireworks/)


```csharp
public sealed class FireworksProvider : OpenAICompatibleProvider
```


Fireworks provider. OpenAI Chat Completions compatible at `https://api.fireworks.ai/inference/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L12)

## Constructors

<a id="Vercel_AI_Fireworks_FireworksProvider__ctor_"></a>
<a id="Vercel_AI_Fireworks_FireworksProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### FireworksProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public FireworksProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L24)

## Fields

<a id="Vercel_AI_Fireworks_FireworksProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "FIREWORKS_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L21)

<a id="Vercel_AI_Fireworks_FireworksProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.fireworks.ai/inference/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L18)

<a id="Vercel_AI_Fireworks_FireworksProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "fireworks"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L15)

## Methods

<a id="Vercel_AI_Fireworks_FireworksProvider_Create_"></a>
<a id="Vercel_AI_Fireworks_FireworksProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static FireworksProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [FireworksProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L36)

<a id="Vercel_AI_Fireworks_FireworksProvider_ImageModel_"></a>
<a id="Vercel_AI_Fireworks_FireworksProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fireworks/FireworksProvider.cs#L30)
