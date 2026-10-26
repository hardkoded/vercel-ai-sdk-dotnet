---
title: "MistralProvider"
description: "Mistral provider. OpenAI Chat Completions compatible at https://api.mistral.ai/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralProvider"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralProvider"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralProvider : OpenAICompatibleProvider
```


Mistral provider. OpenAI Chat Completions compatible at `https://api.mistral.ai/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L14)

## Constructors

<a id="Vercel_AI_Mistral_MistralProvider__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### MistralProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public MistralProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L26)

## Fields

<a id="Vercel_AI_Mistral_MistralProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "MISTRAL_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L23)

<a id="Vercel_AI_Mistral_MistralProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.mistral.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L20)

<a id="Vercel_AI_Mistral_MistralProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "mistral"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L17)

## Methods

<a id="Vercel_AI_Mistral_MistralProvider_Create_"></a>
<a id="Vercel_AI_Mistral_MistralProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static MistralProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [MistralProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L32)
