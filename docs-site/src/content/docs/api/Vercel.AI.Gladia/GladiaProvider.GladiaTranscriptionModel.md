---
title: "GladiaProvider.GladiaTranscriptionModel"
description: "Uploads audio, starts a pre-recorded job, and reads the result."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaProvider.GladiaTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaProvider.GladiaTranscriptionModel : ITranscriptionModel
```


Uploads audio, starts a pre-recorded job, and reads the result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L232)

## Constructors

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel__ctor_Vercel_AI_Gladia_GladiaProvider_System_String_"></a>

### GladiaTranscriptionModel(GladiaProvider, string)


```csharp
public GladiaTranscriptionModel(GladiaProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GladiaProvider](/api/Vercel.AI.Gladia/GladiaProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L237)

## Properties

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L250)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L244)

## Methods

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L253)

<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_Gladia_GladiaProvider_GladiaTranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_Gladia_GladiaTranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, GladiaTranscriptionRequest?, CancellationToken)


```csharp
public Task<GladiaTranscriptionResult> TranscribeAsync(AudioInput audio, GladiaTranscriptionRequest? request, CancellationToken cancellationToken)
```


Transcribes audio through upload, pre-recorded init, and result polling.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [GladiaTranscriptionRequest](/api/Vercel.AI.Gladia/GladiaTranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GladiaTranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L260)
