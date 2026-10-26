---
title: "FalTranscriptionModel"
description: "Fal transcription model. Submits the audio to the Fal queue and polls until the transcript is ready."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Fal_FalTranscriptionModel"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public sealed class FalTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Fal transcription model. Submits the audio to the Fal queue and polls until the transcript is ready.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L15)

## Constructors

<a id="Vercel_AI_Fal_FalTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel__ctor_Vercel_AI_Fal_FalProvider_System_String_System_Func_System_DateTime__"></a>

### `FalTranscriptionModel(FalProvider, string, Func<DateTime>?)`


```csharp
public FalTranscriptionModel(FalProvider provider, string modelId, Func<DateTime>? clock = null)
```


Creates a transcription model. &lt;code class="paramref"&gt;clock&lt;/code&gt; sets the response timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [FalProvider](/api/Vercel.AI.Fal/FalProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `clock` | [Func&lt;DateTime&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L25)

## Properties

<a id="Vercel_AI_Fal_FalTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


True when `CancellationToken)` is implemented.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L42)

<a id="Vercel_AI_Fal_FalTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L36)

<a id="Vercel_AI_Fal_FalTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L33)

<a id="Vercel_AI_Fal_FalTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L39)

## Methods

<a id="Vercel_AI_Fal_FalTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L58)

<a id="Vercel_AI_Fal_FalTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L109)

<a id="Vercel_AI_Fal_FalTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Fal_FalTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalTranscriptionModel.cs#L45)
