---
title: "SpeechResult"
description: "Speech synthesis result."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechResult"
  hidden: true
---

<a id="Vercel_AI_Provider_SpeechResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class SpeechResult
```


Speech synthesis result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L263)

## Constructors

<a id="Vercel_AI_Provider_SpeechResult__ctor_"></a>
<a id="Vercel_AI_Provider_SpeechResult__ctor_System_Byte___System_String_"></a>

### SpeechResult(byte[], string)


```csharp
public SpeechResult(byte[] audio, string mediaType)
```


Creates a speech result with no usage.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L266)

<a id="Vercel_AI_Provider_SpeechResult__ctor_System_Byte___System_String_Vercel_AI_Provider_AudioUsage_"></a>

### SpeechResult(byte[], string, AudioUsage?)


```csharp
public SpeechResult(byte[] audio, string mediaType, AudioUsage? usage)
```


Creates a speech result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `audio` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usage` | [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L272)

## Properties

<a id="Vercel_AI_Provider_SpeechResult_Audio_"></a>
<a id="Vercel_AI_Provider_SpeechResult_Audio"></a>

### Audio


```csharp
public byte[] Audio { get; }
```


Audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L280)

<a id="Vercel_AI_Provider_SpeechResult_MediaType_"></a>
<a id="Vercel_AI_Provider_SpeechResult_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L283)

<a id="Vercel_AI_Provider_SpeechResult_Usage_"></a>
<a id="Vercel_AI_Provider_SpeechResult_Usage"></a>

### Usage


```csharp
public AudioUsage? Usage { get; }
```


Usage reported by the call. Absent unless the caller supplies it.

**Value:** [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L286)
