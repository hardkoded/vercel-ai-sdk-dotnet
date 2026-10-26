---
title: "KlingAIProvider"
description: "KlingAI provider. It supports video generation only."
editUrl: false
prev: false
next: false
sidebar:
  label: "KlingAIProvider"
  hidden: true
---

<a id="Vercel_AI_KlingAI_KlingAIProvider"></a>

Namespace: [Vercel.AI.KlingAI](/api/Vercel.AI.KlingAI/)


```csharp
public sealed class KlingAIProvider : OpenAICompatibleProvider
```


KlingAI provider. It supports video generation only.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L29)

## Constructors

<a id="Vercel_AI_KlingAI_KlingAIProvider__ctor_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### KlingAIProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public KlingAIProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider. Pass [KlingAIOptions](/api/Vercel.AI.KlingAI/KlingAIOptions/) to use the legacy access key and secret key.
The credentials are read when a request is sent, so a missing key fails then and not here.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L43)

## Fields

<a id="Vercel_AI_KlingAI_KlingAIProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api-singapore.klingai.com"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L35)

<a id="Vercel_AI_KlingAI_KlingAIProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "klingai"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L32)

## Methods

<a id="Vercel_AI_KlingAI_KlingAIProvider_Create_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static KlingAIProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) | Provider options. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | Optional message handler. The provider follows redirects by hand so a status poll cannot send the credentials to another host. For that reason it sets `AllowAutoRedirect = false` on a supplied [HttpClientHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpclienthandler) or (on .NET 5 and later) `SocketsHttpHandler`, including one wrapped in [DelegatingHandler](https://learn.microsoft.com/dotnet/api/system.net.http.delegatinghandler)s. This changes the handler you pass, and it throws if the handler has already sent a request. Any other handler type that exposes a public settable `bool AllowAutoRedirect` property is changed by reflection. Any remaining handler is left as is. It must not follow redirects on its own. |

**Returns:** [KlingAIProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L75)

<a id="Vercel_AI_KlingAI_KlingAIProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L56)

<a id="Vercel_AI_KlingAI_KlingAIProvider_ImageModel_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L62)

<a id="Vercel_AI_KlingAI_KlingAIProvider_LanguageModel_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_LanguageModel_System_String_"></a>

### LanguageModel(string)


```csharp
public override ILanguageModel LanguageModel(string modelId)
```


Returns a language model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L50)

<a id="Vercel_AI_KlingAI_KlingAIProvider_ResolveApiKey_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_ResolveApiKey"></a>

### ResolveApiKey()


```csharp
protected override string? ResolveApiKey()
```


Resolves the bearer token on every call. An API key is sent as is. A legacy access key and secret key sign a fresh JWT.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L98)

<a id="Vercel_AI_KlingAI_KlingAIProvider_Video_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_Video_System_String_"></a>

### Video(string)


```csharp
public IVideoModel Video(string modelId)
```


Creates a video model. Same as [VideoModel(string)](#Vercel_AI_KlingAI_KlingAIProvider_VideoModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L93)

<a id="Vercel_AI_KlingAI_KlingAIProvider_VideoModel_"></a>
<a id="Vercel_AI_KlingAI_KlingAIProvider_VideoModel_System_String_"></a>

### VideoModel(string)


```csharp
public override IVideoModel VideoModel(string modelId)
```


Returns a video model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/KlingAI/KlingAIProvider.cs#L90)
