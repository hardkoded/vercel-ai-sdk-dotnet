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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L465)

## Properties

<a id="Vercel_AI_Operations_TranscribeRequest_Audio_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Audio"></a>

### Audio


```csharp
public byte[]? Audio { get; set; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L474)

<a id="Vercel_AI_Operations_TranscribeRequest_AudioUrl_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_AudioUrl"></a>

### AudioUrl


```csharp
public string? AudioUrl { get; set; }
```


Audio URL. HTTP(S) URLs go straight to models with native URL support unless [Download](#Vercel_AI_Operations_TranscribeRequest_Download) is set. Otherwise the audio is downloaded.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L477)

<a id="Vercel_AI_Operations_TranscribeRequest_Download_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Download"></a>

### Download


```csharp
public Func<string, CancellationToken, Task<DownloadedMedia>>? Download { get; set; }
```


Downloads URL audio. Setting it forces local downloading, even for models with native URL support.

**Value:** [Func&lt;string, CancellationToken, Task&lt;DownloadedMedia&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L480)

<a id="Vercel_AI_Operations_TranscribeRequest_Model_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Model"></a>

### Model


```csharp
public ITranscriptionCaller? Model { get; set; }
```


Model instance. Takes precedence over [ModelId](#Vercel_AI_Operations_TranscribeRequest_ModelId).

**Value:** [ITranscriptionCaller](/api/Vercel.AI.Operations/ITranscriptionCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L468)

<a id="Vercel_AI_Operations_TranscribeRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Model id resolved through [Default](/api/Vercel.AI.Operations/TranscriptionModels/#Vercel_AI_Operations_TranscriptionModels_Default).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L471)

<a id="Vercel_AI_Operations_TranscribeRequest_Telemetry_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_Telemetry"></a>

### Telemetry


```csharp
public TranscribeTelemetry? Telemetry { get; set; }
```


Telemetry callbacks.

**Value:** [TranscribeTelemetry](/api/Vercel.AI.Operations/TranscribeTelemetry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L486)

<a id="Vercel_AI_Operations_TranscribeRequest_TimestampGranularities_"></a>
<a id="Vercel_AI_Operations_TranscribeRequest_TimestampGranularities"></a>

### TimestampGranularities


```csharp
public IReadOnlyList<string>? TimestampGranularities { get; set; }
```


Timing granularities to request, `word` and `segment`. Maps to `timestampGranularities`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L483)
