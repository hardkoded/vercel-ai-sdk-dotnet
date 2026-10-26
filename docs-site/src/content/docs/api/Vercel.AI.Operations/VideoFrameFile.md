---
title: "VideoFrameFile"
description: "A normalized frame image."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoFrameFile"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoFrameFile"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoFrameFile
```


A normalized frame image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L66)

## Constructors

<a id="Vercel_AI_Operations_VideoFrameFile__ctor_"></a>
<a id="Vercel_AI_Operations_VideoFrameFile__ctor_Vercel_AI_Operations_VideoModelFile_System_String_"></a>

### VideoFrameFile(VideoModelFile, string)


```csharp
public VideoFrameFile(VideoModelFile image, string frameType)
```


Creates a normalized frame.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `image` | [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/) |  |
| `frameType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L69)

## Properties

<a id="Vercel_AI_Operations_VideoFrameFile_FrameType_"></a>
<a id="Vercel_AI_Operations_VideoFrameFile_FrameType"></a>

### FrameType


```csharp
public string FrameType { get; }
```


Frame role.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L79)

<a id="Vercel_AI_Operations_VideoFrameFile_Image_"></a>
<a id="Vercel_AI_Operations_VideoFrameFile_Image"></a>

### Image


```csharp
public VideoModelFile Image { get; }
```


Normalized image.

**Value:** [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L76)
