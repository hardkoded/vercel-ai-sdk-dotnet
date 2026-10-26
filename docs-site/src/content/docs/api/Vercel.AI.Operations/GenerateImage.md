---
title: "GenerateImage"
description: "Generates images. Maps to generateImage."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateImage"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateImage"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class GenerateImage
```


Generates images. Maps to `generateImage`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L340)

## Methods

<a id="Vercel_AI_Operations_GenerateImage_GenerateImageAsync_"></a>
<a id="Vercel_AI_Operations_GenerateImage_GenerateImageAsync_Vercel_AI_Operations_GenerateImageRequest_System_Threading_CancellationToken_"></a>

### GenerateImageAsync(GenerateImageRequest, CancellationToken)


```csharp
public static Task<GenerateImageResult> GenerateImageAsync(GenerateImageRequest request, CancellationToken cancellationToken = default)
```


Generates images and merges parallel calls.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [GenerateImageRequest](/api/Vercel.AI.Operations/GenerateImageRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GenerateImageResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L348)

<a id="Vercel_AI_Operations_GenerateImage_NormalizePrompt_"></a>
<a id="Vercel_AI_Operations_GenerateImage_NormalizePrompt_Vercel_AI_Operations_ImagePrompt_System_String_"></a>

### NormalizePrompt(ImagePrompt?, string?)


```csharp
public static ImagePromptParts NormalizePrompt(ImagePrompt? prompt, string? text)
```


Normalizes a prompt the way `normalizePrompt` does.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `prompt` | [ImagePrompt](/api/Vercel.AI.Operations/ImagePrompt/) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ImagePromptParts](/api/Vercel.AI.Operations/ImagePromptParts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L410)
