---
title: "SpeechStreamPart"
description: "A part of FullStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechStreamPart"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechStreamPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechStreamPart
```


A part of [FullStream](/api/Vercel.AI.Operations/StreamSpeechResult/#Vercel_AI_Operations_StreamSpeechResult_FullStream).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L177)

## Properties

<a id="Vercel_AI_Operations_SpeechStreamPart_Audio_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; set; }
```


Audio chunk in generation order. Chunks are not necessarily standalone files; concatenate them to reconstruct the audio.
A chunk is empty only when it carries [ProviderMetadata](#Vercel_AI_Operations_SpeechStreamPart_ProviderMetadata) (for example a turn marker).

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L189)

<a id="Vercel_AI_Operations_SpeechStreamPart_AudioFormat_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_AudioFormat"></a>

### AudioFormat


```csharp
public SpeechAudioFormat? AudioFormat { get; set; }
```


Format of an `audio` chunk.

**Value:** [SpeechAudioFormat](/api/Vercel.AI.Operations/SpeechAudioFormat/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L192)

<a id="Vercel_AI_Operations_SpeechStreamPart_Error_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_Error"></a>

### Error


```csharp
public Exception? Error { get; set; }
```


Provider error of an `error` part.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L201)

<a id="Vercel_AI_Operations_SpeechStreamPart_Id_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Provider identifier of an `audio` chunk.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L183)

<a id="Vercel_AI_Operations_SpeechStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata of an `audio` chunk.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L195)

<a id="Vercel_AI_Operations_SpeechStreamPart_RawValue_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_RawValue"></a>

### RawValue


```csharp
public JsonElement? RawValue { get; set; }
```


Raw provider chunk of a `raw` part.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L198)

<a id="Vercel_AI_Operations_SpeechStreamPart_Type_"></a>
<a id="Vercel_AI_Operations_SpeechStreamPart_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Part type: `audio`, `raw`, or `error`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/StreamSpeech.cs#L180)
