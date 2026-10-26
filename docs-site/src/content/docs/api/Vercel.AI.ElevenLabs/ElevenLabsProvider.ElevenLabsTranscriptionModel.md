---
title: "ElevenLabsProvider.ElevenLabsTranscriptionModel"
description: "ElevenLabs batch speech-to-text model."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsProvider.ElevenLabsTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsProvider.ElevenLabsTranscriptionModel : ITranscriptionModel
```


ElevenLabs batch speech-to-text model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L453)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel__ctor_Vercel_AI_ElevenLabs_ElevenLabsProvider_System_String_"></a>

### ElevenLabsTranscriptionModel(ElevenLabsProvider, string)


```csharp
public ElevenLabsTranscriptionModel(ElevenLabsProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [ElevenLabsProvider](/api/Vercel.AI.ElevenLabs/ElevenLabsProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L458)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L468)

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L465)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L471)

<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsProvider_ElevenLabsTranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_ElevenLabs_ElevenLabsTranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, ElevenLabsTranscriptionRequest?, CancellationToken)


```csharp
public Task<ElevenLabsTranscriptionResult> TranscribeAsync(AudioInput audio, ElevenLabsTranscriptionRequest? request, CancellationToken cancellationToken)
```


Uploads audio to `/v1/speech-to-text` and reads the transcript.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [ElevenLabsTranscriptionRequest](/api/Vercel.AI.ElevenLabs/ElevenLabsTranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ElevenLabsTranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L478)
