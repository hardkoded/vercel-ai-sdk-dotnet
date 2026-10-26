---
title: "AssemblyAIProvider.AssemblyAITranscriptionModel"
description: "Uploads audio, submits a transcript, and polls until it completes."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIProvider.AssemblyAITranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAIProvider.AssemblyAITranscriptionModel : ITranscriptionModel
```


Uploads audio, submits a transcript, and polls until it completes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L234)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel__ctor_Vercel_AI_AssemblyAI_AssemblyAIProvider_System_String_"></a>

### AssemblyAITranscriptionModel(AssemblyAIProvider, string)


```csharp
public AssemblyAITranscriptionModel(AssemblyAIProvider provider, string modelId)
```


Creates a transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [AssemblyAIProvider](/api/Vercel.AI.AssemblyAI/AssemblyAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L272)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L282)

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_Provider_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L279)

## Methods

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L285)

<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_TranscribeAsync_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIProvider_AssemblyAITranscriptionModel_TranscribeAsync_Vercel_AI_Provider_AudioInput_Vercel_AI_AssemblyAI_AssemblyAITranscriptionRequest_System_Threading_CancellationToken_"></a>

### TranscribeAsync(AudioInput, AssemblyAITranscriptionRequest?, CancellationToken)


```csharp
public Task<AssemblyAITranscriptionResult> TranscribeAsync(AudioInput audio, AssemblyAITranscriptionRequest? request, CancellationToken cancellationToken)
```


Uploads audio, submits a transcript, and polls until it completes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [AudioInput](/api/Vercel.AI.Provider/AudioInput/) |  |
| `request` | [AssemblyAITranscriptionRequest](/api/Vercel.AI.AssemblyAI/AssemblyAITranscriptionRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;AssemblyAITranscriptionResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L292)
