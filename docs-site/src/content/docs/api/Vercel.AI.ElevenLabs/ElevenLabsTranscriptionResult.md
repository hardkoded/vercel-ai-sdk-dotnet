---
title: "ElevenLabsTranscriptionResult"
description: "ElevenLabs transcription with word timings and response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsTranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsTranscriptionResult
```


ElevenLabs transcription with word timings and response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L180)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_TranscriptSegment__System_String_System_Nullable_System_Double__System_Collections_Generic_IReadOnlyList_Vercel_AI_Util_ModelWarning__Vercel_AI_ElevenLabs_ElevenLabsResponse_"></a>

### `ElevenLabsTranscriptionResult(string, IReadOnlyList<TranscriptSegment>, string?, double?, IReadOnlyList<ModelWarning>, ElevenLabsResponse)`


```csharp
public ElevenLabsTranscriptionResult(string text, IReadOnlyList<TranscriptSegment> segments, string? language, double? durationInSeconds, IReadOnlyList<ModelWarning> warnings, ElevenLabsResponse response)
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
| `response` | [ElevenLabsResponse](/api/Vercel.AI.ElevenLabs/ElevenLabsResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L183)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


End time of the last word, in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L209)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Language_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Detected language code.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L206)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Response_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Response"></a>

### Response


```csharp
public ElevenLabsResponse Response { get; }
```


Response metadata.

**Value:** [ElevenLabsResponse](/api/Vercel.AI.ElevenLabs/ElevenLabsResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L215)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Segments_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


One segment per word or spacing item.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L203)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Text_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Full transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L200)

<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Warnings_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsTranscriptionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Warnings for ignored settings.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L212)
