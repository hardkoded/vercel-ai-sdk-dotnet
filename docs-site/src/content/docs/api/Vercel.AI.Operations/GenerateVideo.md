---
title: "GenerateVideo"
description: "Generates videos. Maps to experimentalgenerateVideo."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateVideo"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateVideo"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class GenerateVideo
```


Generates videos. Maps to `experimental_generateVideo`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L557)

## Methods

<a id="Vercel_AI_Operations_GenerateVideo_GenerateVideoAsync_"></a>
<a id="Vercel_AI_Operations_GenerateVideo_GenerateVideoAsync_Vercel_AI_Operations_GenerateVideoRequest_System_Threading_CancellationToken_"></a>

### GenerateVideoAsync(GenerateVideoRequest, CancellationToken)


```csharp
public static Task<GenerateVideoResult> GenerateVideoAsync(GenerateVideoRequest request, CancellationToken cancellationToken = default)
```


Generates videos, polling or waiting on a webhook when the model requires it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GenerateVideoRequest](/api/Vercel.AI.Operations/GenerateVideoRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateVideoResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L560)

<a id="Vercel_AI_Operations_GenerateVideo_Normalize_"></a>
<a id="Vercel_AI_Operations_GenerateVideo_Normalize_Vercel_AI_Operations_VideoPrompt_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoFrameImage__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_VideoReference__"></a>

### `Normalize(VideoPrompt?, IReadOnlyList<VideoFrameImage>?, IReadOnlyList<VideoReference>?)`


```csharp
public static VideoCallInputs Normalize(VideoPrompt? prompt, IReadOnlyList<VideoFrameImage>? frameImages, IReadOnlyList<VideoReference>? inputReferences)
```


Normalizes prompt, frames, and references.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [VideoPrompt](/api/Vercel.AI.Operations/VideoPrompt/) |  |
| `frameImages` | [IReadOnlyList&lt;VideoFrameImage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `inputReferences` | [IReadOnlyList&lt;VideoReference&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [VideoCallInputs](/api/Vercel.AI.Operations/VideoCallInputs/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateVideo.cs#L628)
