---
title: "FishAudioProvider"
description: "FishAudio provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "FishAudioProvider"
  hidden: true
---

<a id="Vercel_AI_FishAudio_FishAudioProvider"></a>

Namespace: [Vercel.AI.FishAudio](/api/Vercel.AI.FishAudio/)


```csharp
public sealed class FishAudioProvider : OpenAICompatibleProvider
```


FishAudio provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L17)

## Constructors

<a id="Vercel_AI_FishAudio_FishAudioProvider__ctor_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### FishAudioProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public FishAudioProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L31)

## Fields

<a id="Vercel_AI_FishAudio_FishAudioProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.fish.audio"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L23)

<a id="Vercel_AI_FishAudio_FishAudioProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "fish-audio"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L20)

<a id="Vercel_AI_FishAudio_FishAudioProvider_ProviderOptionsKey"></a>

### ProviderOptionsKey


```csharp
public const string ProviderOptionsKey = "fishAudio"
```


Key of the Fish Audio entry in provider options.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L26)

## Properties

<a id="Vercel_AI_FishAudio_FishAudioProvider_ErrorSchema_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_ErrorSchema"></a>

### ErrorSchema


```csharp
public static JsonObject ErrorSchema { get; }
```


Schema of a Fish Audio error body.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L38)

## Methods

<a id="Vercel_AI_FishAudio_FishAudioProvider_Create_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static FishAudioProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [FishAudioProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L67)

<a id="Vercel_AI_FishAudio_FishAudioProvider_LanguageModel_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L49)

<a id="Vercel_AI_FishAudio_FishAudioProvider_Speech_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_Speech_System_String_"></a>

### Speech(string)


```csharp
public ISpeechModel Speech(string modelId)
```


Creates a speech model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L55)

<a id="Vercel_AI_FishAudio_FishAudioProvider_SpeechModel_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_SpeechModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L87)

<a id="Vercel_AI_FishAudio_FishAudioProvider_Transcription_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_Transcription"></a>

### Transcription()


```csharp
public ITranscriptionModel Transcription()
```


Creates the default transcription model `transcribe-1`.

**Returns:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L61)

<a id="Vercel_AI_FishAudio_FishAudioProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_TranscriptionModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L90)
