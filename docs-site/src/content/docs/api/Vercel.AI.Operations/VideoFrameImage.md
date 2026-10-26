---
title: "VideoFrameImage"
description: "A role-tagged frame image."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoFrameImage"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoFrameImage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoFrameImage
```


A role-tagged frame image.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L49)

## Constructors

<a id="Vercel_AI_Operations_VideoFrameImage__ctor_"></a>
<a id="Vercel_AI_Operations_VideoFrameImage__ctor_System_Object_System_String_"></a>

### VideoFrameImage(object?, string)


```csharp
public VideoFrameImage(object? image, string frameType)
```


Creates a frame.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `image` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `frameType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L52)

## Properties

<a id="Vercel_AI_Operations_VideoFrameImage_FrameType_"></a>
<a id="Vercel_AI_Operations_VideoFrameImage_FrameType"></a>

### FrameType


```csharp
public string FrameType { get; }
```


`first_frame` or `last_frame`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L62)

<a id="Vercel_AI_Operations_VideoFrameImage_Image_"></a>
<a id="Vercel_AI_Operations_VideoFrameImage_Image"></a>

### Image


```csharp
public object? Image { get; }
```


Image bytes, base64, data URL, or http URL.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L59)
