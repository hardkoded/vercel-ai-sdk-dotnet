---
title: "DeepgramTranscriptionResult"
description: "Deepgram transcription with word timings and response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramTranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramTranscriptionResult
```


Deepgram transcription with word timings and response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L199)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__Vercel_AI_Deepgram_DeepgramResponse_"></a>

### `DeepgramTranscriptionResult(string, IReadOnlyList<TranscriptSegment>, string?, double?, IReadOnlyList<ModelWarning>, DeepgramResponse)`


```csharp
public DeepgramTranscriptionResult(string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<ModelWarning> warnings, DeepgramResponse response)
```


Creates a transcription result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `warnings` | [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `response` | [DeepgramResponse](/api/Vercel.AI.Deepgram/DeepgramResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L202)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Audio duration from the response metadata.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L222)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Language_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Detected language, when language detection ran.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L219)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Response_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Response"></a>

### Response


```csharp
public DeepgramResponse Response { get; }
```


Response metadata.

**Value:** [DeepgramResponse](/api/Vercel.AI.Deepgram/DeepgramResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L228)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Segments_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


One segment per word.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L216)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Text_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript of the first channel.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L213)

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Warnings_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L225)
