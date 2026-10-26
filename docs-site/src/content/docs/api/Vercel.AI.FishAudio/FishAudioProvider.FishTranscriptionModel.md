---
title: "FishAudioProvider.FishTranscriptionModel"
description: "Fish Audio transcription model. Posts multipart audio to /v1/asr."
editUrl: false
prev: false
next: false
sidebar:
  label: "FishAudioProvider.FishTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel"></a>

Namespace: [Vercel.AI.FishAudio](/api/Vercel.AI.FishAudio/)


```csharp
public sealed class FishAudioProvider.FishTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Fish Audio transcription model. Posts multipart audio to `/v1/asr`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L294)

## Constructors

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel__ctor_Vercel_AI_FishAudio_FishAudioProvider_System_String_System_Func_System_DateTime__"></a>

### `FishTranscriptionModel(FishAudioProvider, string, Func<DateTime>?)`


```csharp
public FishTranscriptionModel(FishAudioProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FishAudioProvider](/api/Vercel.AI.FishAudio/FishAudioProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L300)

## Properties

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L317)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L314)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L311)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L308)

## Methods

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(TranscriptionModelCall, CancellationToken)


```csharp
public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
```


Transcribes audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionModelCall](/api/Vercel.AI.Operations/TranscriptionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L333)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranscriptionStreamCall, CancellationToken)


```csharp
public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
```


Starts a stream, or null when streaming is unsupported.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L387)

<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_FishAudio_FishAudioProvider_FishTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/FishAudio/FishAudioProvider.cs#L320)
