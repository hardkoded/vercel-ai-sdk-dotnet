---
title: "GeneratedVideo"
description: "A generated video file."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedVideo"
  hidden: true
---

<a id="Vercel_AI_Operations_GeneratedVideo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GeneratedVideo
```


A generated video file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L162)

## Constructors

<a id="Vercel_AI_Operations_GeneratedVideo__ctor_"></a>
<a id="Vercel_AI_Operations_GeneratedVideo__ctor_System_Byte___System_String_"></a>

### GeneratedVideo(byte[], string)


```csharp
public GeneratedVideo(byte[] data, string mediaType)
```


Creates a video file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L165)

## Properties

<a id="Vercel_AI_Operations_GeneratedVideo_Base64_"></a>
<a id="Vercel_AI_Operations_GeneratedVideo_Base64"></a>

### Base64


```csharp
public string Base64 { get; }
```


Base64 form of [Data](#Vercel_AI_Operations_GeneratedVideo_Data).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L175)

<a id="Vercel_AI_Operations_GeneratedVideo_Data_"></a>
<a id="Vercel_AI_Operations_GeneratedVideo_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


Video bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L172)

<a id="Vercel_AI_Operations_GeneratedVideo_MediaType_"></a>
<a id="Vercel_AI_Operations_GeneratedVideo_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L181)
