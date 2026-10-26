---
title: "StreamSpeechRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamSpeechRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamSpeechRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class StreamSpeechRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L311)

## Properties

<a id="Vercel_AI_Operations_StreamSpeechRequest_IncludeRawChunks_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_IncludeRawChunks"></a>

### IncludeRawChunks


```csharp
public bool? IncludeRawChunks { get; set; }
```


When true, providers include raw provider chunks in `fullStream`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L344)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Instructions_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Speaking instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L335)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Language_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


ISO 639-1 language code or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L341)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Model_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Model"></a>

### Model


```csharp
public ISpeechCaller? Model { get; set; }
```


Speech model instance. Takes precedence over [ModelId](#Vercel_AI_Operations_StreamSpeechRequest_ModelId). It must support streaming speech.

**Value:** [ISpeechCaller](/api/Vercel.AI.Operations/ISpeechCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L314)

<a id="Vercel_AI_Operations_StreamSpeechRequest_ModelId_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Model id resolved through [Default](/api/Vercel.AI.Operations/SpeechModels/#Vercel_AI_Operations_SpeechModels_Default).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L317)

<a id="Vercel_AI_Operations_StreamSpeechRequest_OnError_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_OnError"></a>

### OnError


```csharp
public Func<Exception, Task>? OnError { get; set; }
```


Called for each error part the provider reports. Error parts are not fatal: `fullStream` includes them and
`audioStream` skips them. The callback is not awaited, and errors it throws are ignored.
Defaults to writing the error to [Error](https://learn.microsoft.com/dotnet/api/system.console.error).

**Value:** [Func&lt;Exception, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L351)

<a id="Vercel_AI_Operations_StreamSpeechRequest_OutputFormat_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_OutputFormat"></a>

### OutputFormat


```csharp
public SpeechAudioFormat? OutputFormat { get; set; }
```


Desired format of the output audio chunks. Supported formats are provider-specific.

**Value:** [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L332)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Speed_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Playback speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L338)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Text_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


The text to convert to speech, as one complete string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L320)

<a id="Vercel_AI_Operations_StreamSpeechRequest_TextParts_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_TextParts"></a>

### TextParts


```csharp
public IEnumerable<SpeechTextPart>? TextParts { get; set; }
```


The text to convert to speech, as text parts. Strings convert implicitly to parts.

**Value:** [IEnumerable&lt;SpeechTextPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L323)

<a id="Vercel_AI_Operations_StreamSpeechRequest_TextStream_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_TextStream"></a>

### TextStream


```csharp
public IAsyncEnumerable<SpeechTextPart>? TextStream { get; set; }
```


The text to convert to speech, as a stream of text deltas or parts that is synthesized as it arrives.

**Value:** [IAsyncEnumerable&lt;SpeechTextPart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L326)

<a id="Vercel_AI_Operations_StreamSpeechRequest_Voice_"></a>
<a id="Vercel_AI_Operations_StreamSpeechRequest_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice to use.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L329)
