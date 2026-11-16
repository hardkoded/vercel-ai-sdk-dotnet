---
title: "XaiTranscriptionModelOptions"
description: "xAI transcription options, read from providerOptions.xai. Maps to XaiTranscriptionModelOptions of xai-transcription-model-options.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiTranscriptionModelOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiTranscriptionModelOptions
```


xAI transcription options, read from `providerOptions.xai`. Maps to `XaiTranscriptionModelOptions` of
`xai-transcription-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L31)

## Properties

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_AudioFormat_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_AudioFormat"></a>

### AudioFormat


```csharp
public string? AudioFormat { get; set; }
```


Input audio encoding for raw, headerless audio: `pcm`, `mulaw`, or `alaw`. Container formats such as
Ogg/Opus are auto-detected, so do not set this for them. `opus` is for streaming only, with one raw Opus packet per chunk.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L39)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Channels_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Channels"></a>

### Channels


```csharp
public int? Channels { get; set; }
```


Number of interleaved audio channels. Between 2 and 8.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L54)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Diarize_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Diarize"></a>

### Diarize


```csharp
public bool? Diarize { get; set; }
```


Enable speaker diarization.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L57)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_FillerWords_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_FillerWords"></a>

### FillerWords


```csharp
public bool? FillerWords { get; set; }
```


Include filler words such as "uh" and "um" in the transcript.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L63)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Format_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Format"></a>

### Format


```csharp
public bool? Format { get; set; }
```


Enable inverse text normalization. Requires [Language](#Vercel_AI_Xai_XaiTranscriptionModelOptions_Language).

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L48)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Keyterm_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Keyterm"></a>

### Keyterm


```csharp
public IReadOnlyList<string>? Keyterm { get; set; }
```


Terms to bias transcription toward.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L60)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Language_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language code used for inverse text normalization.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L45)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Multichannel_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Multichannel"></a>

### Multichannel


```csharp
public bool? Multichannel { get; set; }
```


Enable per-channel transcription for multichannel audio.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L51)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_SampleRate_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_SampleRate"></a>

### SampleRate


```csharp
public int? SampleRate { get; set; }
```


Sample rate of the input audio in Hz.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L42)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Streaming_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_Streaming"></a>

### Streaming


```csharp
public XaiTranscriptionStreamingOptions? Streaming { get; set; }
```


Options for streaming speech-to-text over WebSocket.

**Value:** [XaiTranscriptionStreamingOptions](/api/Vercel.AI.Xai/XaiTranscriptionStreamingOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L69)

<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_VadThreshold_"></a>
<a id="Vercel_AI_Xai_XaiTranscriptionModelOptions_VadThreshold"></a>

### VadThreshold


```csharp
public double? VadThreshold { get; set; }
```


Voice activity detection threshold between 0 and 1. Defaults to 0.5 for `transcribe` and 0.08 for streaming. 0 disables the filter.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTranscriptionModelOptions.cs#L66)
