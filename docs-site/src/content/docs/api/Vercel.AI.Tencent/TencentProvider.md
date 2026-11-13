---
title: "TencentProvider"
description: "Tencent Cloud provider: TokenHub chat models (OpenAI-compatible) and Kling (TokenHub) and VOD AIGC video models."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentProvider"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentProvider"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentProvider : OpenAICompatibleProvider
```


Tencent Cloud provider: TokenHub chat models (OpenAI-compatible) and Kling (TokenHub) and VOD AIGC video models.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L43)

## Constructors

<a id="Vercel_AI_Tencent_TencentProvider__ctor_"></a>
<a id="Vercel_AI_Tencent_TencentProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### TencentProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public TencentProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider. Pass `TencentOptions` to use VOD video models.
Credentials are read when a request is sent, so a missing key fails then and not here.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L60)

## Fields

<a id="Vercel_AI_Tencent_TencentProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://tokenhub-intl.tencentcloudmaas.com/v1"
```


Default TokenHub API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L49)

<a id="Vercel_AI_Tencent_TencentProvider_DefaultVodBaseUrl"></a>

### DefaultVodBaseUrl


```csharp
public const string DefaultVodBaseUrl = "https://vod.intl.tencentcloudapi.com"
```


Default VOD API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L52)

<a id="Vercel_AI_Tencent_TencentProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "tencent"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L46)

## Methods

<a id="Vercel_AI_Tencent_TencentProvider_Chat_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_Chat_System_String_"></a>

### Chat(string)


```csharp
public ILanguageModel Chat(string modelId)
```


Creates a TokenHub model for text generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L103)

<a id="Vercel_AI_Tencent_TencentProvider_Create_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static TencentProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) | Provider options. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | Optional message handler. The provider follows redirects by hand so a status poll cannot send the credentials to another host. It sets `AllowAutoRedirect = false` on a supplied handler that exposes it (see `HttpMessageHandler)`). |

**Returns:** [TencentProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L88)

<a id="Vercel_AI_Tencent_TencentProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L71)

<a id="Vercel_AI_Tencent_TencentProvider_ImageModel_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L77)

<a id="Vercel_AI_Tencent_TencentProvider_Video_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_Video_System_String_"></a>

### Video(string)


```csharp
public IVideoModel Video(string modelId)
```


Creates a model for video generation. Same as [VideoModel(string)](#Vercel_AI_Tencent_TencentProvider_VideoModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L109)

<a id="Vercel_AI_Tencent_TencentProvider_VideoModel_"></a>
<a id="Vercel_AI_Tencent_TencentProvider_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public override IVideoModel VideoModel(string modelId)
```


Creates a model for video generation. TokenHub Kling models use their TokenHub id; VOD AIGC models use `vod/<ModelName>/<ModelVersion>`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentProvider.cs#L106)
