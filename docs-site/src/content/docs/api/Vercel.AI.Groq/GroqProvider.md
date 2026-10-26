---
title: "GroqProvider"
description: "Groq provider. OpenAI Chat Completions compatible at https://api.groq.com/openai/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "GroqProvider"
  hidden: true
---

<a id="Vercel_AI_Groq_GroqProvider"></a>

Namespace: [Vercel.AI.Groq](/api/Vercel.AI.Groq/)


```csharp
public sealed class GroqProvider : OpenAICompatibleProvider
```


Groq provider. OpenAI Chat Completions compatible at `https://api.groq.com/openai/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L11)

## Constructors

<a id="Vercel_AI_Groq_GroqProvider__ctor_"></a>
<a id="Vercel_AI_Groq_GroqProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### GroqProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public GroqProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L23)

## Fields

<a id="Vercel_AI_Groq_GroqProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "GROQ_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L20)

<a id="Vercel_AI_Groq_GroqProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.groq.com/openai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L17)

<a id="Vercel_AI_Groq_GroqProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "groq"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L14)

## Methods

<a id="Vercel_AI_Groq_GroqProvider_Create_"></a>
<a id="Vercel_AI_Groq_GroqProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static GroqProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [GroqProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Groq/GroqProvider.cs#L29)
