---
title: "ByteDanceProvider"
description: "ByteDance provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ByteDanceProvider"
  hidden: true
---

<a id="Vercel_AI_ByteDance_ByteDanceProvider"></a>

Namespace: [Vercel.AI.ByteDance](/api/Vercel.AI.ByteDance/)


```csharp
public sealed class ByteDanceProvider : OpenAICompatibleProvider
```


ByteDance provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L12)

## Constructors

<a id="Vercel_AI_ByteDance_ByteDanceProvider__ctor_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### ByteDanceProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public ByteDanceProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L23)

## Fields

<a id="Vercel_AI_ByteDance_ByteDanceProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://ark.ap-southeast.bytepluses.com/api/v3"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L18)

<a id="Vercel_AI_ByteDance_ByteDanceProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "bytedance"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L15)

## Methods

<a id="Vercel_AI_ByteDance_ByteDanceProvider_Create_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static ByteDanceProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [ByteDanceProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L36)

<a id="Vercel_AI_ByteDance_ByteDanceProvider_GenerateImageAsync_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider_GenerateImageAsync_System_String_Vercel_AI_Provider_ImageCallOptions_System_Collections_Generic_IDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `GenerateImageAsync(string, ImageCallOptions, IDictionary<string, string>?, CancellationToken)`


```csharp
public Task<ImageGenerationResult> GenerateImageAsync(string modelId, ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
```


Posts an image generation. &lt;code class="paramref"&gt;headers&lt;/code&gt; override the provider headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [ImageCallOptions](/api/Vercel.AI.Provider/ImageCallOptions/) |  |
| `headers` | [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ImageGenerationResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L62)

<a id="Vercel_AI_ByteDance_ByteDanceProvider_ImageModel_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L56)

<a id="Vercel_AI_ByteDance_ByteDanceProvider_LanguageModel_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L30)

<a id="Vercel_AI_ByteDance_ByteDanceProvider_VideoModel_"></a>
<a id="Vercel_AI_ByteDance_ByteDanceProvider_VideoModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ByteDance/ByteDanceProvider.cs#L59)
