---
title: "RevAITranscriptionResult"
description: "Rev.ai transcription, including segments and response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAITranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAITranscriptionResult"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAITranscriptionResult
```


Rev.ai transcription, including segments and response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L136)

## Constructors

<a id="Vercel_AI_RevAI_RevAITranscriptionResult__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult__ctor_System_String_System_Collections_Generic_IReadOnlyList_Vercel_AI_RevAI_RevAITranscriptionSegment__System_String_System_Double_Vercel_AI_RevAI_RevAITranscriptionResponse_"></a>

### `RevAITranscriptionResult(string, IReadOnlyList<RevAITranscriptionSegment>, string?, double, RevAITranscriptionResponse)`


```csharp
public RevAITranscriptionResult(string text, IReadOnlyList<RevAITranscriptionSegment> segments, string? language, double durationInSeconds, RevAITranscriptionResponse response)
```


Creates a transcription result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `segments` | [IReadOnlyList&lt;RevAITranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `durationInSeconds` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `response` | [RevAITranscriptionResponse](/api/Vercel.AI.RevAI/RevAITranscriptionResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L139)

## Properties

<a id="Vercel_AI_RevAI_RevAITranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double DurationInSeconds { get; }
```


Latest word end time, in seconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L163)

<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Language_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Language reported on the job, when present.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L160)

<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Response_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Response"></a>

### Response


```csharp
public RevAITranscriptionResponse Response { get; }
```


Response metadata.

**Value:** [RevAITranscriptionResponse](/api/Vercel.AI.RevAI/RevAITranscriptionResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L166)

<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Segments_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<RevAITranscriptionSegment> Segments { get; }
```


Timed fragments.

**Value:** [IReadOnlyList&lt;RevAITranscriptionSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L157)

<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Text_"></a>
<a id="Vercel_AI_RevAI_RevAITranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Full transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L154)
