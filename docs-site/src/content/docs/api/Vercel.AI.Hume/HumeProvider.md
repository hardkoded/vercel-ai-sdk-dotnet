---
title: "HumeProvider"
description: "Hume text-to-speech provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeProvider"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeProvider"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeProvider : OpenAICompatibleProvider
```


Hume text-to-speech provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L182)

## Constructors

<a id="Vercel_AI_Hume_HumeProvider__ctor_"></a>
<a id="Vercel_AI_Hume_HumeProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### HumeProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public HumeProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L193)

## Fields

<a id="Vercel_AI_Hume_HumeProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.hume.ai"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L188)

<a id="Vercel_AI_Hume_HumeProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "hume"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L185)

## Methods

<a id="Vercel_AI_Hume_HumeProvider_Create_"></a>
<a id="Vercel_AI_Hume_HumeProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static HumeProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [HumeProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L200)

<a id="Vercel_AI_Hume_HumeProvider_EmbeddingModel_"></a>
<a id="Vercel_AI_Hume_HumeProvider_EmbeddingModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L218)

<a id="Vercel_AI_Hume_HumeProvider_ImageModel_"></a>
<a id="Vercel_AI_Hume_HumeProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L224)

<a id="Vercel_AI_Hume_HumeProvider_LanguageModel_"></a>
<a id="Vercel_AI_Hume_HumeProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L212)

<a id="Vercel_AI_Hume_HumeProvider_Speech_"></a>
<a id="Vercel_AI_Hume_HumeProvider_Speech"></a>

### Speech()


```csharp
public HumeProvider.HumeSpeechModel Speech()
```


Creates the speech model. Hume does not take a model id.

**Returns:** [HumeProvider.HumeSpeechModel](/api/Vercel.AI.Hume/HumeProvider.HumeSpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L230)

<a id="Vercel_AI_Hume_HumeProvider_SpeechModel_"></a>
<a id="Vercel_AI_Hume_HumeProvider_SpeechModel_System_String_"></a>

### SpeechModel(string)


```csharp
public override ISpeechModel SpeechModel(string modelId)
```


Returns a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L236)
