---
title: "TranscriptionAudioInfo"
description: "Metadata about the input audio on a transcription telemetry event."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionAudioInfo"
  hidden: true
---

<a id="Vercel_AI_Operations_TranscriptionAudioInfo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranscriptionAudioInfo
```


Metadata about the input audio on a transcription telemetry event.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L379)

## Constructors

<a id="Vercel_AI_Operations_TranscriptionAudioInfo__ctor_"></a>
<a id="Vercel_AI_Operations_TranscriptionAudioInfo__ctor_System_Nullable_System_Int32__System_String_"></a>

### TranscriptionAudioInfo(int?, string?)


```csharp
public TranscriptionAudioInfo(int? byteLength, string? mediaType)
```


Creates the metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `byteLength` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L382)

## Properties

<a id="Vercel_AI_Operations_TranscriptionAudioInfo_ByteLength_"></a>
<a id="Vercel_AI_Operations_TranscriptionAudioInfo_ByteLength"></a>

### ByteLength


```csharp
public int? ByteLength { get; }
```


Byte length. Unknown for native URL transcription and for inputs that fail to prepare.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L389)

<a id="Vercel_AI_Operations_TranscriptionAudioInfo_MediaType_"></a>
<a id="Vercel_AI_Operations_TranscriptionAudioInfo_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type, when it is known before input preparation completes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Transcribe.cs#L392)
