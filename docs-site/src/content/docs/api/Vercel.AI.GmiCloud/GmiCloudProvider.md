---
title: "GmiCloudProvider"
description: "GmiCloud provider. OpenAI Chat Completions compatible at https://api.gmi-serving.com/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "GmiCloudProvider"
  hidden: true
---

<a id="Vercel_AI_GmiCloud_GmiCloudProvider"></a>

Namespace: [Vercel.AI.GmiCloud](/api/Vercel.AI.GmiCloud/)


```csharp
public sealed class GmiCloudProvider : OpenAICompatibleProvider
```


GmiCloud provider. OpenAI Chat Completions compatible at `https://api.gmi-serving.com/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L11)

## Constructors

<a id="Vercel_AI_GmiCloud_GmiCloudProvider__ctor_"></a>
<a id="Vercel_AI_GmiCloud_GmiCloudProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### GmiCloudProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public GmiCloudProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L23)

## Fields

<a id="Vercel_AI_GmiCloud_GmiCloudProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "GMI_CLOUD_APIKEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L20)

<a id="Vercel_AI_GmiCloud_GmiCloudProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.gmi-serving.com/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L17)

<a id="Vercel_AI_GmiCloud_GmiCloudProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "gmicloud"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L14)

## Methods

<a id="Vercel_AI_GmiCloud_GmiCloudProvider_Create_"></a>
<a id="Vercel_AI_GmiCloud_GmiCloudProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static GmiCloudProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GmiCloudProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GmiCloud/GmiCloudProvider.cs#L29)
