---
title: "GoogleVertexGeminiTranscriptionModel"
description: "Vertex Gemini transcription through generateContent."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleVertexGeminiTranscriptionModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleVertexGeminiTranscriptionModel : ITranscriptionModel, ITranscriptionCaller
```


Vertex Gemini transcription through generateContent.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/), [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L214)

## Constructors

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel__ctor_Vercel_AI_Google_GoogleVertexProvider_System_String_"></a>

### GoogleVertexGeminiTranscriptionModel(GoogleVertexProvider, string)


```csharp
public GoogleVertexGeminiTranscriptionModel(GoogleVertexProvider provider, string modelId)
```


Creates a Gemini transcription model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L222)

## Properties

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_CanStream_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_CanStream"></a>

### CanStream


```csharp
public bool CanStream { get; }
```


False. The Vertex Live WebSocket is not opened by this port.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L244)

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L235)

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L229)

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L238)

## Methods

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoGenerateAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoGenerateAsync_Vercel_AI_Operations_TranscriptionModelCall_System_Threading_CancellationToken_"></a>

### DoGenerateAsync(TranscriptionModelCall, CancellationToken)


```csharp
public Task<TranscriptionModelResult> DoGenerateAsync(TranscriptionModelCall call, CancellationToken cancellationToken)
```


Transcribes audio. Options are read from `googleVertex`, then `vertex`, then `google`.
Text parts win over the `audioTranscription` text, which also carries the language and word timings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionModelCall](/api/Vercel.AI.Operations/TranscriptionModelCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionModelResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L261)

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoStreamAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoStreamAsync_Vercel_AI_Operations_TranscriptionStreamCall_System_Threading_CancellationToken_"></a>

### DoStreamAsync(TranscriptionStreamCall, CancellationToken)


```csharp
public Task<TranscriptionStreamStart?> DoStreamAsync(TranscriptionStreamCall call, CancellationToken cancellationToken)
```


Returns null. The Vertex Live WebSocket is not opened by this port.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `call` | [TranscriptionStreamCall](/api/Vercel.AI.Operations/TranscriptionStreamCall/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;TranscriptionStreamStart&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L358)

<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoTranscribeAsync_"></a>
<a id="Vercel_AI_Google_GoogleVertexGeminiTranscriptionModel_DoTranscribeAsync_Vercel_AI_Provider_AudioInput_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleVertexMedia.cs#L250)
