---
title: "DeepgramProvider"
description: "Deepgram provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramProvider"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramProvider"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramProvider : OpenAICompatibleProvider
```


Deepgram provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ProviderBase](/api/Vercel.AI.Provider/ProviderBase/) → [OpenAICompatibleProvider](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L232)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramProvider__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider__ctor_System_Net_Http_HttpClient_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_"></a>

### DeepgramProvider(HttpClient, OpenAICompatibleOptions?)


```csharp
public DeepgramProvider(HttpClient httpClient, OpenAICompatibleOptions? options = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L243)

## Fields

<a id="Vercel_AI_Deepgram_DeepgramProvider_DefaultBaseUrl"></a>

### DefaultBaseUrl


```csharp
public const string DefaultBaseUrl = "https://api.deepgram.com"
```


Default API origin.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L238)

<a id="Vercel_AI_Deepgram_DeepgramProvider_ProviderId"></a>

### ProviderId


```csharp
public const string ProviderId = "deepgram"
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L235)

## Methods

<a id="Vercel_AI_Deepgram_DeepgramProvider_Create_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_Create_Vercel_AI_OpenAICompatible_OpenAICompatibleOptions_System_Net_Http_HttpMessageHandler_"></a>

### Create(OpenAICompatibleOptions?, HttpMessageHandler?)


```csharp
public static DeepgramProvider Create(OpenAICompatibleOptions? options = null, HttpMessageHandler? handler = null)
```


Creates a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/) |  |
| `handler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [DeepgramProvider](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L256)

<a id="Vercel_AI_Deepgram_DeepgramProvider_LanguageModel_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_LanguageModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L250)

<a id="Vercel_AI_Deepgram_DeepgramProvider_SpeechModel_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_SpeechModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L279)

<a id="Vercel_AI_Deepgram_DeepgramProvider_TranscribeAsync_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_TranscribeAsync_System_String_Vercel_AI_Provider_AudioInput_Vercel_AI_Deepgram_DeepgramTranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(string, AudioInput, DeepgramTranscriptionRequest?, CancellationToken)


```csharp
public Task<DeepgramTranscriptionResult> TranscribeAsync(string modelId, AudioInput audio, DeepgramTranscriptionRequest? request, CancellationToken cancellationToken)
```


Posts raw audio to `/v1/listen?model=`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [DeepgramTranscriptionRequest](/api/Vercel.AI.Deepgram/DeepgramTranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DeepgramTranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L282)

<a id="Vercel_AI_Deepgram_DeepgramProvider_TranscriptionModel_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_TranscriptionModel_System_String_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L276)
