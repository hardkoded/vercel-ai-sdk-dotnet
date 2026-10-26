---
title: "ZaiProvider"
description: "Zai provider. OpenAI Chat Completions compatible at https://api.z.ai/api/paas/v4."
editUrl: false
prev: false
next: false
sidebar:
  label: "ZaiProvider"
  hidden: true
---

<a id="Vercel_AI_Zai_ZaiProvider"></a>

Namespace: [Vercel.AI.Zai](/api/Vercel.AI.Zai/)


```csharp
public sealed class ZaiProvider : OpenAICompatibleProvider
```


Zai provider. OpenAI Chat Completions compatible at `https://api.z.ai/api/paas/v4`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L11)

## Constructors

<a id="Vercel_AI_Zai_ZaiProvider__ctor_"></a>
<a id="Vercel_AI_Zai_ZaiProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### ZaiProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public ZaiProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L23)

## Fields

<a id="Vercel_AI_Zai_ZaiProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "ZAI_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L20)

<a id="Vercel_AI_Zai_ZaiProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.z.ai/api/paas/v4"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L17)

<a id="Vercel_AI_Zai_ZaiProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "zai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L14)

## Methods

<a id="Vercel_AI_Zai_ZaiProvider_Create_"></a>
<a id="Vercel_AI_Zai_ZaiProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static ZaiProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ZaiProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Zai/ZaiProvider.cs#L29)
