---
title: "ElevenLabsMusicWordTimestamp"
description: "One lyric word with its start and end time in milliseconds."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsMusicWordTimestamp"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsMusicWordTimestamp
```


One lyric word with its start and end time in milliseconds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L15)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp__ctor_System_String_System_Double_System_Double_"></a>

### ElevenLabsMusicWordTimestamp(string, double, double)


```csharp
public ElevenLabsMusicWordTimestamp(string word, double startMs, double endMs)
```


Creates a word timestamp.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `word` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `startMs` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `endMs` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L18)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_EndMs_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_EndMs"></a>

### EndMs


```csharp
public double EndMs { get; }
```


End time in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L32)

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_StartMs_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_StartMs"></a>

### StartMs


```csharp
public double StartMs { get; }
```


Start time in milliseconds.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L29)

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_Word_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicWordTimestamp_Word"></a>

### Word


```csharp
public string Word { get; }
```


Lyric word.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L26)
