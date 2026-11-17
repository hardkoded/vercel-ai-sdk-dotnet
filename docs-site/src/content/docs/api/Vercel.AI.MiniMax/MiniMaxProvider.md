---
title: "MiniMaxProvider"
description: "MiniMax provider. Speaks the Anthropic Messages protocol at https://api.minimax.io/anthropic/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "MiniMaxProvider"
  hidden: true
---

<a id="Vercel_AI_MiniMax_MiniMaxProvider"></a>

Namespace: [Vercel.AI.MiniMax](/api/Vercel.AI.MiniMax/)


```csharp
public sealed class MiniMaxProvider : ProviderBase
```


MiniMax provider. Speaks the Anthropic Messages protocol at `https://api.minimax.io/anthropic/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L36)

## Constructors

<a id="Vercel_AI_MiniMax_MiniMaxProvider__ctor_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_MiniMax_MiniMaxOptions_"></a>

### MiniMaxProvider(HttpClient, MiniMaxOptions?)


```csharp
public MiniMaxProvider(HttpClient httpClient, MiniMaxOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L57)

## Fields

<a id="Vercel_AI_MiniMax_MiniMaxProvider_ApiKeyVariable"></a>

### ApiKeyVariable


```csharp
public const string ApiKeyVariable = "MINIMAX_API_KEY"
```


Environment variable for the API key.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L51)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.minimax.io/anthropic/v1"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L42)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_DefaultImageBaseUrl"></a>

### DefaultImageBaseUrl


```csharp
public const string DefaultImageBaseUrl = "https://api.minimax.io/v1"
```


Default API origin for image generation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L48)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_DefaultVideoBaseUrl"></a>

### DefaultVideoBaseUrl


```csharp
public const string DefaultVideoBaseUrl = "https://api.minimax.io"
```


Default API origin for video generation. It has no version suffix.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L45)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "minimax"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L39)

## Properties

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Options_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Options"></a>

### Options


```csharp
public MiniMaxOptions Options { get; }
```


Options.

**Value:** [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L78)

## Methods

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Chat_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Chat_System_String_"></a>

### Chat(string)


```csharp
public MiniMaxChatLanguageModel Chat(string modelId)
```


Creates a MiniMax chat model for text generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MiniMaxChatLanguageModel](/api/Vercel.AI.MiniMax/MiniMaxChatLanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L99)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Create_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Create_Vercel_AI_MiniMax_MiniMaxOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(MiniMaxOptions?, HttpMessageHandler?)


```csharp
public static MiniMaxProvider Create(MiniMaxOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider. Pass a handler from tests.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [MiniMaxProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L81)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Image_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Image_System_String_"></a>

### Image(string)


```csharp
public MiniMaxImageModel Image(string modelId)
```


Creates a MiniMax image model for image generation and editing.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MiniMaxImageModel](/api/Vercel.AI.MiniMax/MiniMaxImageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L131)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_ImageModel_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L125)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_LanguageModel_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L93)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_Video_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_Video_System_String_"></a>

### Video(string)


```csharp
public MiniMaxVideoModel Video(string modelId)
```


Creates a MiniMax video model for video generation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [MiniMaxVideoModel](/api/Vercel.AI.MiniMax/MiniMaxVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L111)

<a id="Vercel_AI_MiniMax_MiniMaxProvider_VideoModel_"></a>
<a id="Vercel_AI_MiniMax_MiniMaxProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/MiniMax/MiniMaxProvider.cs#L105)
