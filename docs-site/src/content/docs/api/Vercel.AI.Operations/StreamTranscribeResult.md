---
title: "StreamTranscribeResult"
description: "Streaming transcription result."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranscribeResult"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranscribeResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamTranscribeResult
```


Streaming transcription result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L250)

## Properties

<a id="Vercel_AI_Operations_StreamTranscribeResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public Task<double?> DurationInSeconds { get; }
```


Duration. Accessing it claims the stream.

**Value:** [Task&lt;double?&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L295)

<a id="Vercel_AI_Operations_StreamTranscribeResult_FullStream_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_FullStream"></a>

### FullStream


```csharp
public OperationFullStream<ModelStreamPart> FullStream { get; }
```


Single-consumer part stream.

**Value:** `OperationFullStream&lt;ModelStreamPart&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L319)

<a id="Vercel_AI_Operations_StreamTranscribeResult_Language_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_Language"></a>

### Language


```csharp
public Task<string?> Language { get; }
```


Language. Accessing it claims the stream.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L289)

<a id="Vercel_AI_Operations_StreamTranscribeResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement> ProviderMetadata { get; }
```


Provider metadata. Accessing it claims the stream.

**Value:** [Task&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L313)

<a id="Vercel_AI_Operations_StreamTranscribeResult_Responses_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_Responses"></a>

### Responses


```csharp
public Task<IReadOnlyList<ProviderResponse>> Responses { get; }
```


Responses. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;ProviderResponse&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L307)

<a id="Vercel_AI_Operations_StreamTranscribeResult_Segments_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_Segments"></a>

### Segments


```csharp
public Task<IReadOnlyList<TranscriptSegment>> Segments { get; }
```


Segments. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;TranscriptSegment&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L283)

<a id="Vercel_AI_Operations_StreamTranscribeResult_Text_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_Text"></a>

### Text


```csharp
public Task<string> Text { get; }
```


Final transcript. Accessing it claims the stream.

**Value:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L277)

<a id="Vercel_AI_Operations_StreamTranscribeResult_Warnings_"></a>
<a id="Vercel_AI_Operations_StreamTranscribeResult_Warnings"></a>

### Warnings


```csharp
public Task<IReadOnlyList<OperationWarning>> Warnings { get; }
```


Warnings. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;OperationWarning&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L301)
