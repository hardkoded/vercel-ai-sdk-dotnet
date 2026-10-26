---
title: "AudioInput"
description: "Audio sent to a transcription or translation model."
editUrl: false
prev: false
next: false
sidebar:
  label: "AudioInput"
  hidden: true
---

<a id="Vercel_AI_Provider_AudioInput"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class AudioInput
```


Audio sent to a transcription or translation model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L342)

## Constructors

<a id="Vercel_AI_Provider_AudioInput__ctor_"></a>
<a id="Vercel_AI_Provider_AudioInput__ctor_System_Byte___System_String_System_String_"></a>

### AudioInput(byte[], string, string?)


```csharp
public AudioInput(byte[] data, string mediaType, string? fileName)
```


Creates audio input.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L345)

## Properties

<a id="Vercel_AI_Provider_AudioInput_Data_"></a>
<a id="Vercel_AI_Provider_AudioInput_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L353)

<a id="Vercel_AI_Provider_AudioInput_FileName_"></a>
<a id="Vercel_AI_Provider_AudioInput_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name sent in multipart bodies.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L359)

<a id="Vercel_AI_Provider_AudioInput_MediaType_"></a>
<a id="Vercel_AI_Provider_AudioInput_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L356)
