---
title: "LumaProvider"
description: "Luma provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "LumaProvider"
  hidden: true
---

<a id="Vercel_AI_Luma_LumaProvider"></a>

Namespace: [Vercel.AI.Luma](/api/Vercel.AI.Luma/)


```csharp
public sealed class LumaProvider : OpenAICompatibleProvider
```


Luma provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L13)

## Constructors

<a id="Vercel_AI_Luma_LumaProvider__ctor_"></a>
<a id="Vercel_AI_Luma_LumaProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### LumaProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public LumaProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L24)

## Fields

<a id="Vercel_AI_Luma_LumaProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.lumalabs.ai"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L19)

<a id="Vercel_AI_Luma_LumaProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "luma"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L16)

## Methods

<a id="Vercel_AI_Luma_LumaProvider_Create_"></a>
<a id="Vercel_AI_Luma_LumaProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static LumaProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [LumaProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L43)

<a id="Vercel_AI_Luma_LumaProvider_GenerateImageAsync_"></a>
<a id="Vercel_AI_Luma_LumaProvider_GenerateImageAsync_System_String_Vercel_AI_Luma_LumaImageRequest_System_Threading_CancellationToken_"></a>

### GenerateImageAsync(string, LumaImageRequest, CancellationToken)


```csharp
public Task<LumaImageGeneration> GenerateImageAsync(string modelId, LumaImageRequest request, CancellationToken cancellationToken)
```


Generates one image with Dream Machine.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `request` | [LumaImageRequest](/api/Vercel.AI.Luma/LumaImageRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;LumaImageGeneration&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L75)

<a id="Vercel_AI_Luma_LumaProvider_ImageModel_"></a>
<a id="Vercel_AI_Luma_LumaProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L72)

<a id="Vercel_AI_Luma_LumaProvider_LanguageModel_"></a>
<a id="Vercel_AI_Luma_LumaProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L31)

<a id="Vercel_AI_Luma_LumaProvider_VideoModel_"></a>
<a id="Vercel_AI_Luma_LumaProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Luma/LumaProvider.cs#L37)
