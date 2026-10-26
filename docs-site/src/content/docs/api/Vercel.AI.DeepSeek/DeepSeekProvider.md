---
title: "DeepSeekProvider"
description: "DeepSeek provider. OpenAI Chat Completions compatible at https://api.deepseek.com."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekProvider"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekProvider"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekProvider : OpenAICompatibleProvider
```


DeepSeek provider. OpenAI Chat Completions compatible at `https://api.deepseek.com`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L12)

## Constructors

<a id="Vercel_AI_DeepSeek_DeepSeekProvider__ctor_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### DeepSeekProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public DeepSeekProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L24)

## Fields

<a id="Vercel_AI_DeepSeek_DeepSeekProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "DEEPSEEK_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L21)

<a id="Vercel_AI_DeepSeek_DeepSeekProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.deepseek.com"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L18)

<a id="Vercel_AI_DeepSeek_DeepSeekProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "deepseek"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L15)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekProvider_Create_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static DeepSeekProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [DeepSeekProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L36)

<a id="Vercel_AI_DeepSeek_DeepSeekProvider_FileStore_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekProvider_FileStore"></a>

### FileStore()


```csharp
public override IFileStore FileStore()
```


Returns a file store.

**Returns:** [IFileStore](/api/Vercel.AI.Provider/IFileStore/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekProvider.cs#L30)
