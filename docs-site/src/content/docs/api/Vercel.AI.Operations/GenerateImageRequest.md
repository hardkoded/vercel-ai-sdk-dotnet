---
title: "GenerateImageRequest"
description: "Options for CancellationToken)."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateImageRequest"
  hidden: true
---

<a id="Vercel_AI_Operations_GenerateImageRequest"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class GenerateImageRequest : OperationRequest
```


Options for `CancellationToken)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OperationRequest](/api/Vercel.AI.Operations/OperationRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L312)

## Properties

<a id="Vercel_AI_Operations_GenerateImageRequest_AspectRatio_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_AspectRatio"></a>

### AspectRatio


```csharp
public string? AspectRatio { get; set; }
```


Aspect ratio such as `16:9`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L333)

<a id="Vercel_AI_Operations_GenerateImageRequest_MaxImagesPerCall_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int? MaxImagesPerCall { get; set; }
```


Overrides the model's maximum images per call.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L327)

<a id="Vercel_AI_Operations_GenerateImageRequest_Model_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_Model"></a>

### Model


```csharp
public IImageCaller? Model { get; set; }
```


Image model.

**Value:** [IImageCaller](/api/Vercel.AI.Operations/IImageCaller/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L315)

<a id="Vercel_AI_Operations_GenerateImageRequest_N_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_N"></a>

### N


```csharp
public int N { get; set; }
```


Number of images. Default 1.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L324)

<a id="Vercel_AI_Operations_GenerateImageRequest_Prompt_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_Prompt"></a>

### Prompt


```csharp
public ImagePrompt? Prompt { get; set; }
```


Text or edit prompt.

**Value:** [ImagePrompt](/api/Vercel.AI.Operations/ImagePrompt/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L321)

<a id="Vercel_AI_Operations_GenerateImageRequest_Seed_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Seed.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L336)

<a id="Vercel_AI_Operations_GenerateImageRequest_Size_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size such as `1024x1024`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L330)

<a id="Vercel_AI_Operations_GenerateImageRequest_Text_"></a>
<a id="Vercel_AI_Operations_GenerateImageRequest_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text prompt. Ignored when [Prompt](#Vercel_AI_Operations_GenerateImageRequest_Prompt) is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateImage.cs#L318)
