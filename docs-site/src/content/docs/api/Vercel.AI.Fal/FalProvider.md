---
title: "FalProvider"
description: "Fal provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalProvider"
  hidden: true
---

<a id="Vercel_AI_Fal_FalProvider"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public sealed class FalProvider : OpenAICompatibleProvider
```


Fal provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L15)

## Constructors

<a id="Vercel_AI_Fal_FalProvider__ctor_"></a>
<a id="Vercel_AI_Fal_FalProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### FalProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public FalProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L26)

## Fields

<a id="Vercel_AI_Fal_FalProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://fal.run"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L21)

<a id="Vercel_AI_Fal_FalProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "fal"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L18)

## Methods

<a id="Vercel_AI_Fal_FalProvider_Create_"></a>
<a id="Vercel_AI_Fal_FalProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static FalProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [FalProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L39)

<a id="Vercel_AI_Fal_FalProvider_GenerateImageAsync_"></a>
<a id="Vercel_AI_Fal_FalProvider_GenerateImageAsync_System_String_Vercel_AI_Provider_ImageCallOptions_System_Collections_Generic_IDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `GenerateImageAsync(string, ImageCallOptions, IDictionary<string, string>?, CancellationToken)`


```csharp
public Task<ImageGenerationResult> GenerateImageAsync(string modelId, ImageCallOptions options, IDictionary<string, string>? headers, CancellationToken cancellationToken)
```


Posts &lt;code class="paramref"&gt;options&lt;/code&gt; to `{base}/{modelId}`. &lt;code class="paramref"&gt;headers&lt;/code&gt; override the provider headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [ImageCallOptions](/api/Vercel.AI.Provider/ImageCallOptions/) |  |
| `headers` | [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ImageGenerationResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L74)

<a id="Vercel_AI_Fal_FalProvider_ImageModel_"></a>
<a id="Vercel_AI_Fal_FalProvider_ImageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L65)

<a id="Vercel_AI_Fal_FalProvider_LanguageModel_"></a>
<a id="Vercel_AI_Fal_FalProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L33)

<a id="Vercel_AI_Fal_FalProvider_SpeechModel_"></a>
<a id="Vercel_AI_Fal_FalProvider_SpeechModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L68)

<a id="Vercel_AI_Fal_FalProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Fal_FalProvider_TranscriptionModel_System_String_"></a>

### TranscriptionModel(string)


```csharp
public override ITranscriptionModel TranscriptionModel(string modelId)
```


Returns a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalProvider.cs#L71)
