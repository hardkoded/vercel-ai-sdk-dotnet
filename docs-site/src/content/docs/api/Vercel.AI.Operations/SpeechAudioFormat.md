---
title: "SpeechAudioFormat"
description: "Audio format of a speech stream, such as audio/pcm at 24000 Hz or audio/mpeg."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechAudioFormat"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechAudioFormat"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechAudioFormat
```


Audio format of a speech stream, such as `audio/pcm` at 24000 Hz or `audio/mpeg`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L12)

## Constructors

<a id="Vercel_AI_Operations_SpeechAudioFormat__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechAudioFormat__ctor_System_String_System_Nullable_System_Int32__"></a>

### SpeechAudioFormat(string, int?)


```csharp
public SpeechAudioFormat(string type, int? rate = null)
```


Creates a format.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `rate` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L15)

## Properties

<a id="Vercel_AI_Operations_SpeechAudioFormat_Rate_"></a>
<a id="Vercel_AI_Operations_SpeechAudioFormat_Rate"></a>

### Rate


```csharp
public int? Rate { get; }
```


Sample rate in Hz. Only applicable for formats that require a rate.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L25)

<a id="Vercel_AI_Operations_SpeechAudioFormat_Type_"></a>
<a id="Vercel_AI_Operations_SpeechAudioFormat_Type"></a>

### Type


```csharp
public string Type { get; }
```


Audio format type, such as `audio/pcm`, `audio/pcmu`, or `audio/pcma`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L22)
