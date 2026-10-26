---
title: "StreamTranscribeRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranscribeRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranscribeRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranscribeRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L228)

## Properties

<a id="Vercel_AI_Operations_StreamTranscribeRequest_Audio_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_Audio"></a>

### Audio


```csharp
public AudioChunkStream? Audio { get; set; }
```


Audio stream.

**Value:** [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L237)

<a id="Vercel_AI_Operations_StreamTranscribeRequest_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; set; }
```


Requests raw provider chunks.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L243)

<a id="Vercel_AI_Operations_StreamTranscribeRequest_InputAudioFormat_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_InputAudioFormat"></a>

### InputAudioFormat


```csharp
public string InputAudioFormat { get; set; }
```


Input audio format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L240)

<a id="Vercel_AI_Operations_StreamTranscribeRequest_Model_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_Model"></a>

### Model


```csharp
public ITranscriptionCaller? Model { get; set; }
```


Model instance. Takes precedence over [ModelId](#Vercel_AI_Operations_StreamTranscribeRequest_ModelId).

**Value:** [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L231)

<a id="Vercel_AI_Operations_StreamTranscribeRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Model id resolved through [Default](/api/Vercel.AI.Operations/TranscriptionModels/#Vercel_AI_Operations_TranscriptionModels_Default).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L234)

<a id="Vercel_AI_Operations_StreamTranscribeRequest_ModelWasString_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeRequest_ModelWasString"></a>

### ModelWasString


```csharp
public bool ModelWasString { get; set; }
```


True when the model was resolved from [ModelId](#Vercel_AI_Operations_StreamTranscribeRequest_ModelId).

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L246)
