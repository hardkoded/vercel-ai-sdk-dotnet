---
title: "BlackForestLabsProvider"
description: "BlackForestLabs provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "BlackForestLabsProvider"
  hidden: true
---

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider"></a>

Namespace: [Vercel.AI.BlackForestLabs](/api/Vercel.AI.BlackForestLabs/)


```csharp
public sealed class BlackForestLabsProvider : OpenAICompatibleProvider
```


BlackForestLabs provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L13)

## Constructors

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider__ctor_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### BlackForestLabsProvider(HttpClient, OpenAICompatibleOptions?, int?, int?)


```csharp
public BlackForestLabsProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null, int? pollIntervalMillis = null, int? pollTimeoutMillis = null)
```


Creates a provider. The poll settings apply to every image and video model it creates.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `pollIntervalMillis` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `pollTimeoutMillis` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L22)

## Fields

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.bfl.ai/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L19)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "black-forest-labs"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L16)

## Properties

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_HttpClient_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client used for submit, poll, and download requests.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L31)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_PollIntervalMillis_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_PollIntervalMillis"></a>

### PollIntervalMillis


```csharp
public int? PollIntervalMillis { get; }
```


Poll interval in milliseconds between status checks. Null uses 500ms for images and 2s for video.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L34)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_PollTimeoutMillis_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_PollTimeoutMillis"></a>

### PollTimeoutMillis


```csharp
public int? PollTimeoutMillis { get; }
```


Overall timeout in milliseconds for polling before giving up. Null uses 60s for images and 10 minutes for video.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L37)

## Methods

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Create_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?, int?, int?)


```csharp
public static BlackForestLabsProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null, int? pollIntervalMillis = null, int? pollTimeoutMillis = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) | Provider options. |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) | Optional message handler. The provider follows redirects by hand so a status poll cannot send the API key to another host. For that reason it sets `AllowAutoRedirect = false` on a supplied [HttpClientHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpclienthandler) or (on .NET 5 and later) `SocketsHttpHandler`, including one wrapped in [DelegatingHandler](https://learn.microsoft.com/dotnet/api/system.net.http.delegatinghandler)s. This changes the handler you pass, and it throws if the handler has already sent a request. Any other handler type that exposes a public settable `bool AllowAutoRedirect` property is changed by reflection. Any remaining handler is left as is. It must not follow redirects on its own. |
| `pollIntervalMillis` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Poll interval in milliseconds between status checks. |
| `pollTimeoutMillis` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) | Overall polling timeout in milliseconds. |

**Returns:** [BlackForestLabsProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L61)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L46)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Image_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Image_System_String_"></a>

### Image(string)


```csharp
public IImageModel Image(string modelId)
```


Creates an image model. Same as [ImageModel(string)](#Vercel_AI_BlackForestLabs_BlackForestLabsProvider_ImageModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IImageModel](/api/Vercel.AI.Provider/IImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L129)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_ImageModel_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L126)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_LanguageModel_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L40)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Video_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_Video_System_String_"></a>

### Video(string)


```csharp
public IVideoModel Video(string modelId)
```


Creates a video model. Same as [VideoModel(string)](#Vercel_AI_BlackForestLabs_BlackForestLabsProvider_VideoModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L135)

<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_VideoModel_"></a>
<a id="Vercel_AI_BlackForestLabs_BlackForestLabsProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/BlackForestLabs/BlackForestLabsProvider.cs#L132)
