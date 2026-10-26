---
title: "GeneratedAudio"
description: "Generated speech audio."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedAudio"
  hidden: true
---

<a id="Vercel_AI_Operations_GeneratedAudio"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GeneratedAudio
```


Generated speech audio.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L93)

## Constructors

<a id="Vercel_AI_Operations_GeneratedAudio__ctor_"></a>
<a id="Vercel_AI_Operations_GeneratedAudio__ctor_System_Byte___System_String_"></a>

### GeneratedAudio(byte[], string)


```csharp
public GeneratedAudio(byte[] data, string mediaType)
```


Creates generated audio.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidResponseDataException](/api/Vercel.AI.Operations/InvalidResponseDataException/) | The media type has an empty subtype, such as `audio/`. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L97)

## Properties

<a id="Vercel_AI_Operations_GeneratedAudio_Data_"></a>
<a id="Vercel_AI_Operations_GeneratedAudio_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L115)

<a id="Vercel_AI_Operations_GeneratedAudio_Format_"></a>
<a id="Vercel_AI_Operations_GeneratedAudio_Format"></a>

### Format


```csharp
public string Format { get; }
```


Format subtype. `audio/mpeg` stays `mp3`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L121)

<a id="Vercel_AI_Operations_GeneratedAudio_MediaType_"></a>
<a id="Vercel_AI_Operations_GeneratedAudio_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L118)
