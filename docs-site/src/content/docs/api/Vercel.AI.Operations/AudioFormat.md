---
title: "AudioFormat"
description: "The format of raw input audio. Maps to SharedV4AudioFormat."
editUrl: false
prev: false
next: false
sidebar:
  label: "AudioFormat"
  hidden: true
---

<a id="Vercel_AI_Operations_AudioFormat"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class AudioFormat
```


The format of raw input audio. Maps to `SharedV4AudioFormat`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L31)

## Constructors

<a id="Vercel_AI_Operations_AudioFormat__ctor_"></a>
<a id="Vercel_AI_Operations_AudioFormat__ctor_System_String_System_Nullable_System_Double__"></a>

### AudioFormat(string, double?)


```csharp
public AudioFormat(string type, double? rate = null)
```


Creates an audio format.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `rate` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L34)

## Properties

<a id="Vercel_AI_Operations_AudioFormat_Rate_"></a>
<a id="Vercel_AI_Operations_AudioFormat_Rate"></a>

### Rate


```csharp
public double? Rate { get; }
```


Sample rate in Hz, when the format needs one.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L44)

<a id="Vercel_AI_Operations_AudioFormat_Type_"></a>
<a id="Vercel_AI_Operations_AudioFormat_Type"></a>

### Type


```csharp
public string Type { get; }
```


Media type of the audio, such as `audio/pcm`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L41)

## Operators

<a id="Vercel_AI_Operations_AudioFormat_op_Implicit_"></a>
<a id="Vercel_AI_Operations_AudioFormat_op_Implicit_System_String__Vercel_AI_Operations_AudioFormat"></a>

### implicit operator AudioFormat(string)


```csharp
public static implicit operator AudioFormat(string type)
```


Creates a format with no sample rate.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [AudioFormat](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L47)
