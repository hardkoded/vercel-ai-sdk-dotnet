---
title: "VideoCallInputs"
description: "Normalized prompt, frames, and references."
editUrl: false
prev: false
next: false
sidebar:
  label: "VideoCallInputs"
  hidden: true
---

<a id="Vercel_AI_Operations_VideoCallInputs"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class VideoCallInputs
```


Normalized prompt, frames, and references.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L398)

## Constructors

<a id="Vercel_AI_Operations_VideoCallInputs__ctor_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs__ctor_System_String_Vercel_AI_Operations_VideoModelFile_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoFrameFile__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoModelFile__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__"></a>

### `VideoCallInputs(string?, VideoModelFile?, IReadOnlyList<VideoFrameFile>?, IReadOnlyList<VideoModelFile>?, IReadOnlyList<OperationWarning>)`


```csharp
public VideoCallInputs(string? prompt, VideoModelFile? image, IReadOnlyList<VideoFrameFile>? frameImages, IReadOnlyList<VideoModelFile>? inputReferences, IReadOnlyList<OperationWarning> warnings)
```


Creates normalized inputs.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `image` | [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/) |  |
| `frameImages` | [IReadOnlyList&lt;VideoFrameFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `inputReferences` | [IReadOnlyList&lt;VideoModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L401)

## Properties

<a id="Vercel_AI_Operations_VideoCallInputs_FrameImages_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs_FrameImages"></a>

### FrameImages


```csharp
public IReadOnlyList<VideoFrameFile>? FrameImages { get; }
```


Frames.

**Value:** [IReadOnlyList&lt;VideoFrameFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L417)

<a id="Vercel_AI_Operations_VideoCallInputs_Image_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs_Image"></a>

### Image


```csharp
public VideoModelFile? Image { get; }
```


Start image. A `first_frame` wins over the prompt image.

**Value:** [VideoModelFile](/api/Vercel.AI.Operations/VideoModelFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L414)

<a id="Vercel_AI_Operations_VideoCallInputs_InputReferences_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs_InputReferences"></a>

### InputReferences


```csharp
public IReadOnlyList<VideoModelFile>? InputReferences { get; }
```


References. Null when frames are present.

**Value:** [IReadOnlyList&lt;VideoModelFile&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L420)

<a id="Vercel_AI_Operations_VideoCallInputs_Prompt_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs_Prompt"></a>

### Prompt


```csharp
public string? Prompt { get; }
```


Prompt text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L411)

<a id="Vercel_AI_Operations_VideoCallInputs_Warnings_"></a>
<a id="Vercel_AI_Operations_VideoCallInputs_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Normalization warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L423)
