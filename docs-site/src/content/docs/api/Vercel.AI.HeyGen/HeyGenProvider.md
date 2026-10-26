---
title: "HeyGenProvider"
description: "HeyGen provider. It supports video generation only."
editUrl: false
prev: false
next: false
sidebar:
  label: "HeyGenProvider"
  hidden: true
---

<a id="Vercel_AI_HeyGen_HeyGenProvider"></a>

Namespace: [Vercel.AI.HeyGen](/api/Vercel.AI.HeyGen/)


```csharp
public sealed class HeyGenProvider : OpenAICompatibleProvider
```


HeyGen provider. It supports video generation only.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L12)

## Constructors

<a id="Vercel_AI_HeyGen_HeyGenProvider__ctor_"></a>
<a id="Vercel_AI_HeyGen_HeyGenProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### HeyGenProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public HeyGenProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider. The API key is read when a model is used, not here.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L23)

## Fields

<a id="Vercel_AI_HeyGen_HeyGenProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.heygen.com"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L18)

<a id="Vercel_AI_HeyGen_HeyGenProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "heygen"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L15)

## Methods

<a id="Vercel_AI_HeyGen_HeyGenProvider_Create_"></a>
<a id="Vercel_AI_HeyGen_HeyGenProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static HeyGenProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [HeyGenProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L30)

<a id="Vercel_AI_HeyGen_HeyGenProvider_LanguageModel_"></a>
<a id="Vercel_AI_HeyGen_HeyGenProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L37)

<a id="Vercel_AI_HeyGen_HeyGenProvider_Video_"></a>
<a id="Vercel_AI_HeyGen_HeyGenProvider_Video_System_String_"></a>

### Video(string)


```csharp
public IVideoModel Video(string modelId)
```


Creates a video model. Same as [VideoModel(string)](#Vercel_AI_HeyGen_HeyGenProvider_VideoModel_System_String_).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IVideoModel](/api/Vercel.AI.Provider/IVideoModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L46)

<a id="Vercel_AI_HeyGen_HeyGenProvider_VideoModel_"></a>
<a id="Vercel_AI_HeyGen_HeyGenProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenProvider.cs#L43)
