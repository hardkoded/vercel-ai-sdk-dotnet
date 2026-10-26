---
title: "AlibabaProvider"
description: "Alibaba provider. OpenAI Chat Completions compatible at https://dashscope-intl.aliyuncs.com/compatible-mode/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "AlibabaProvider"
  hidden: true
---

<a id="Vercel_AI_Alibaba_AlibabaProvider"></a>

Namespace: [Vercel.AI.Alibaba](/api/Vercel.AI.Alibaba/)


```csharp
public sealed class AlibabaProvider : OpenAICompatibleProvider
```


Alibaba provider. OpenAI Chat Completions compatible at `https://dashscope-intl.aliyuncs.com/compatible-mode/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L15)

## Constructors

<a id="Vercel_AI_Alibaba_AlibabaProvider__ctor_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### AlibabaProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public AlibabaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L32)

## Fields

<a id="Vercel_AI_Alibaba_AlibabaProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "ALIBABA_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L24)

<a id="Vercel_AI_Alibaba_AlibabaProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L21)

<a id="Vercel_AI_Alibaba_AlibabaProvider_EmbeddingBaseUrl"></a>

### EmbeddingBaseUrl


```csharp
public const string EmbeddingBaseUrl = "https://dashscope-intl.aliyuncs.com/api/v1"
```


Native DashScope embedding origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L27)

<a id="Vercel_AI_Alibaba_AlibabaProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "alibaba"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L18)

## Methods

<a id="Vercel_AI_Alibaba_AlibabaProvider_Create_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static AlibabaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [AlibabaProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L45)

<a id="Vercel_AI_Alibaba_AlibabaProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Alibaba_AlibabaProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Alibaba/AlibabaProvider.cs#L39)
