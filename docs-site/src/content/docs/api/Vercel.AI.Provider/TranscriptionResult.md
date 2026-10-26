---
title: "TranscriptionResult"
description: "Transcription result."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionResult"
  hidden: true
---

<a id="Vercel_AI_Provider_TranscriptionResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class TranscriptionResult
```


Transcription result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L319)

## Constructors

<a id="Vercel_AI_Provider_TranscriptionResult__ctor_"></a>
<a id="Vercel_AI_Provider_TranscriptionResult__ctor_System_String_"></a>

### TranscriptionResult(string)


```csharp
public TranscriptionResult(string text)
```


Creates a transcription with no usage.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L322)

<a id="Vercel_AI_Provider_TranscriptionResult__ctor_System_String_Vercel_AI_Provider_AudioUsage_"></a>

### TranscriptionResult(string, AudioUsage?)


```csharp
public TranscriptionResult(string text, AudioUsage? usage)
```


Creates a transcription.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usage` | [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L328)

## Properties

<a id="Vercel_AI_Provider_TranscriptionResult_Text_"></a>
<a id="Vercel_AI_Provider_TranscriptionResult_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L335)

<a id="Vercel_AI_Provider_TranscriptionResult_Usage_"></a>
<a id="Vercel_AI_Provider_TranscriptionResult_Usage"></a>

### Usage


```csharp
public AudioUsage? Usage { get; }
```


Usage reported by the call. Absent unless the caller supplies it.

**Value:** [AudioUsage](/api/Vercel.AI.Provider/AudioUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L338)
