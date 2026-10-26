---
title: "RevAIProvider.RevAITranscriptionModel"
description: "Rev.ai asynchronous transcription model."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIProvider.RevAITranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAIProvider.RevAITranscriptionModel : ITranscriptionModel
```


Rev.ai asynchronous transcription model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L220)

## Constructors

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel__ctor_Vercel_AI_RevAI_RevAIProvider_System_String_"></a>

### RevAITranscriptionModel(RevAIProvider, string)


```csharp
public RevAITranscriptionModel(RevAIProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [RevAIProvider](/api/Vercel.AI.RevAI/RevAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L225)

## Properties

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L238)

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_Provider_"></a>
<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L232)

## Methods

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L241)

<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_RevAI_RevAIProvider_RevAITranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_RevAI_RevAITranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, RevAITranscriptionRequest?, CancellationToken)


```csharp
public Task<RevAITranscriptionResult> TranscribeAsync(AudioInput audio, RevAITranscriptionRequest? request, CancellationToken cancellationToken)
```


Submits a job, polls until it is transcribed, and reads the transcript.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [RevAITranscriptionRequest](/api/Vercel.AI.RevAI/RevAITranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RevAITranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L248)
