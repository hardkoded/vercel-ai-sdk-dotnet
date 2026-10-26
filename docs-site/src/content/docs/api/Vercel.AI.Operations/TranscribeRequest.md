---
title: "TranscribeRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscribeRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscribeRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscribeRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L209)

## Properties

<a id="Vercel_AI_Operations_TranscribeRequest_Audio_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Audio"></a>

### Audio


```csharp
public byte[]? Audio { get; set; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L218)

<a id="Vercel_AI_Operations_TranscribeRequest_AudioUrl_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_AudioUrl"></a>

### AudioUrl


```csharp
public string? AudioUrl { get; set; }
```


Audio URL. Downloaded when set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L221)

<a id="Vercel_AI_Operations_TranscribeRequest_Download_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Download"></a>

### Download


```csharp
public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
```


Downloads URL audio.

**Value:** [Func&lt;string, CancellationToken, Task&lt;DownloadedMedia&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L224)

<a id="Vercel_AI_Operations_TranscribeRequest_Model_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Model"></a>

### Model


```csharp
public ITranscriptionCaller? Model { get; set; }
```


Model instance. Takes precedence over [ModelId](#Vercel_AI_Operations_TranscribeRequest_ModelId).

**Value:** [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L212)

<a id="Vercel_AI_Operations_TranscribeRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Model id resolved through [Default](/api/Vercel.AI.Operations/TranscriptionModels/#Vercel_AI_Operations_TranscriptionModels_Default).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L215)
