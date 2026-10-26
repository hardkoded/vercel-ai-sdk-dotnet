---
title: "OpenAITranscriptionResult"
description: "Transcript with timed segments and response metadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAITranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAITranscriptionResult
```


Transcript with timed segments and response metadata.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L60)

## Properties

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_DurationInSeconds_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; }
```


Audio duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L90)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Language_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Language"></a>

### Language


```csharp
public string? Language { get; }
```


ISO-639-1 code, when OpenAI reports a two-letter code, a known language name, or a single detected language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L87)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_ProviderMetadata_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Speaker segments under `openai.segments` for diarized responses and detected languages under `openai.languages`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L93)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Response_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Response"></a>

### Response


```csharp
public OpenAITranscriptionResponse Response { get; }
```


Response metadata.

**Value:** [OpenAITranscriptionResponse](/api/Vercel.AI.OpenAI/OpenAITranscriptionResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L99)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Segments_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment> Segments { get; }
```


Segments, or words when the response has no segments.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L84)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Text_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L81)

<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAITranscriptionResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the request. OpenAI transcription reports none today.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Transcription/OpenAITranscriptionModel.cs#L96)
