---
title: "StreamSpeechResult"
description: "Result of CancellationToken). Maps to StreamSpeechResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamSpeechResult"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamSpeechResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamSpeechResult
```


Result of `CancellationToken)`. Maps to `StreamSpeechResult`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L358)

## Properties

<a id="Vercel_AI_Operations_StreamSpeechResult_AudioStream_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_AudioStream"></a>

### AudioStream


```csharp
public OperationFullStream<byte[]> AudioStream { get; }
```


Stream of audio bytes in generation order. [AudioStream](#Vercel_AI_Operations_StreamSpeechResult_AudioStream) and [FullStream](#Vercel_AI_Operations_StreamSpeechResult_FullStream) share one live
stream: only one of them can be accessed, and only once. Access it before any result promise.

**Value:** `OperationFullStream&lt;byte\[\]&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L424)

<a id="Vercel_AI_Operations_StreamSpeechResult_FullStream_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_FullStream"></a>

### FullStream


```csharp
public OperationFullStream<SpeechStreamPart> FullStream { get; }
```


Full stream of speech parts (audio, raw, and error parts). [AudioStream](#Vercel_AI_Operations_StreamSpeechResult_AudioStream) and [FullStream](#Vercel_AI_Operations_StreamSpeechResult_FullStream)
share one live stream: only one of them can be accessed, and only once. Access it before any result promise.

**Value:** `OperationFullStream&lt;SpeechStreamPart&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L437)

<a id="Vercel_AI_Operations_StreamSpeechResult_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public Task<JsonElement> ProviderMetadata { get; }
```


Additional provider-specific metadata. Accessing it claims the stream.

**Value:** [Task&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L415)

<a id="Vercel_AI_Operations_StreamSpeechResult_Responses_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_Responses"></a>

### Responses


```csharp
public Task<IReadOnlyList<ProviderResponse>> Responses { get; }
```


Response metadata. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;ProviderResponse&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L409)

<a id="Vercel_AI_Operations_StreamSpeechResult_Usage_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_Usage"></a>

### Usage


```csharp
public Task<SpeechStreamUsage> Usage { get; }
```


Usage reported for the call, if available. Accessing it claims the stream.

**Value:** [Task&lt;SpeechStreamUsage&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L397)

<a id="Vercel_AI_Operations_StreamSpeechResult_Warnings_"></a>
<a id="Vercel_AI_Operations_StreamSpeechResult_Warnings"></a>

### Warnings


```csharp
public Task<IReadOnlyList<OperationWarning>> Warnings { get; }
```


Warnings for the call, such as unsupported settings. Accessing it claims the stream.

**Value:** [Task&lt;IReadOnlyList&lt;OperationWarning&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L403)
