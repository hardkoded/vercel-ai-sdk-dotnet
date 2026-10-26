---
title: "DeepgramProvider.DeepgramTranscriptionModel"
description: "Deepgram pre-recorded transcription model."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramProvider.DeepgramTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramProvider.DeepgramTranscriptionModel : ITranscriptionModel
```


Deepgram pre-recorded transcription model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L715)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel__ctor_Vercel_AI_Deepgram_DeepgramProvider_System_String_"></a>

### DeepgramTranscriptionModel(DeepgramProvider, string)


```csharp
public DeepgramTranscriptionModel(DeepgramProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [DeepgramProvider](/api/Vercel.AI.Deepgram/DeepgramProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L742)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L752)

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L749)

## Methods

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

### DoTranscribeAsync(AudioInput, CancellationToken)


```csharp
public Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
```


Transcribes audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L755)

<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_Deepgram_DeepgramProvider_DeepgramTranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_Deepgram_DeepgramTranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, DeepgramTranscriptionRequest?, CancellationToken)


```csharp
public Task<DeepgramTranscriptionResult> TranscribeAsync(AudioInput audio, DeepgramTranscriptionRequest? request, CancellationToken cancellationToken)
```


Posts raw audio to `/v1/listen`. Provider options become query parameters.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [DeepgramTranscriptionRequest](/api/Vercel.AI.Deepgram/DeepgramTranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;DeepgramTranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L762)
