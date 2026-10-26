---
title: "ProdiaProvider"
description: "Prodia provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProdiaProvider"
  hidden: true
---

<a id="Vercel_AI_Prodia_ProdiaProvider"></a>

Namespace: [Vercel.AI.Prodia](/api/Vercel.AI.Prodia/)


```csharp
public sealed class ProdiaProvider : OpenAICompatibleProvider
```


Prodia provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L13)

## Constructors

<a id="Vercel_AI_Prodia_ProdiaProvider__ctor_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### ProdiaProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public ProdiaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L22)

## Fields

<a id="Vercel_AI_Prodia_ProdiaProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://inference.prodia.com/v2"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L19)

<a id="Vercel_AI_Prodia_ProdiaProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "prodia"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L16)

## Properties

<a id="Vercel_AI_Prodia_ProdiaProvider_HttpClient_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider_HttpClient"></a>

### HttpClient


```csharp
public HttpClient HttpClient { get; }
```


HTTP client used for job requests and image downloads.

**Value:** [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L29)

## Methods

<a id="Vercel_AI_Prodia_ProdiaProvider_Create_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static ProdiaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ProdiaProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L32)

<a id="Vercel_AI_Prodia_ProdiaProvider_ImageModel_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L48)

<a id="Vercel_AI_Prodia_ProdiaProvider_LanguageModel_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L45)

<a id="Vercel_AI_Prodia_ProdiaProvider_VideoModel_"></a>
<a id="Vercel_AI_Prodia_ProdiaProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prodia/ProdiaProvider.cs#L51)
