---
title: "CerebrasProvider"
description: "Cerebras provider. OpenAI Chat Completions compatible at https://api.cerebras.ai/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasProvider"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasProvider"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasProvider : OpenAICompatibleProvider
```


Cerebras provider. OpenAI Chat Completions compatible at `https://api.cerebras.ai/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L11)

## Constructors

<a id="Vercel_AI_Cerebras_CerebrasProvider__ctor_"></a>
<a id="Vercel_AI_Cerebras_CerebrasProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### CerebrasProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public CerebrasProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L23)

## Fields

<a id="Vercel_AI_Cerebras_CerebrasProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "CEREBRAS_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L20)

<a id="Vercel_AI_Cerebras_CerebrasProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.cerebras.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L17)

<a id="Vercel_AI_Cerebras_CerebrasProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "cerebras"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L14)

## Methods

<a id="Vercel_AI_Cerebras_CerebrasProvider_Create_"></a>
<a id="Vercel_AI_Cerebras_CerebrasProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static CerebrasProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [CerebrasProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasProvider.cs#L29)
