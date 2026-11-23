---
title: "GenerateSpeechAudioInfo"
description: "Metadata about the generated audio of a GenerateSpeechEndEvent. Raw audio is intentionally not included."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechAudioInfo"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateSpeechAudioInfo
```


Metadata about the generated audio of a [GenerateSpeechEndEvent](/api/Vercel.AI.Operations/GenerateSpeechEndEvent/). Raw audio is intentionally not included.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L186)

## Constructors

<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo__ctor_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo__ctor_System_Int32_System_String_System_String_"></a>

### GenerateSpeechAudioInfo(int, string, string)


```csharp
public GenerateSpeechAudioInfo(int byteLength, string mediaType, string format)
```


Creates the metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `byteLength` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `format` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L189)

## Properties

<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_ByteLength_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_ByteLength"></a>

### ByteLength


```csharp
public int ByteLength { get; }
```


Size of the audio in bytes.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L197)

<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_Format_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_Format"></a>

### Format


```csharp
public string Format { get; }
```


Format of the audio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L203)

<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_MediaType_"></a>
<a id="Vercel_AI_Operations_GenerateSpeechAudioInfo_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type of the audio.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L200)
