---
title: "OpenAIImageGenerationToolOptions"
description: "Options of ImageGeneration(OpenAIImageGenerationToolOptions). Maps to the upstream imageGeneration args."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIImageGenerationToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIImageGenerationToolOptions
```


Options of [ImageGeneration(OpenAIImageGenerationToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_ImageGeneration_Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_). Maps to the upstream `imageGeneration` args.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L12)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Action_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Action"></a>

### Action


```csharp
public string? Action { get; set; }
```


Whether to generate a new image or edit an existing one: `generate`, `edit`, or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L15)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Background_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Background"></a>

### Background


```csharp
public string? Background { get; set; }
```


Background type: `auto`, `opaque`, or `transparent`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L18)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputFidelity_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputFidelity"></a>

### InputFidelity


```csharp
public string? InputFidelity { get; set; }
```


Input fidelity: `low` or `high`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L21)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputImageMaskFileId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputImageMaskFileId"></a>

### InputImageMaskFileId


```csharp
public string? InputImageMaskFileId { get; set; }
```


File id of the inpainting mask.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L24)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputImageMaskImageUrl_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_InputImageMaskImageUrl"></a>

### InputImageMaskImageUrl


```csharp
public string? InputImageMaskImageUrl { get; set; }
```


Base64-encoded inpainting mask.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L27)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Model_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Model"></a>

### Model


```csharp
public string? Model { get; set; }
```


The image generation model. Default: `gpt-image-1`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L30)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Moderation_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Moderation"></a>

### Moderation


```csharp
public string? Moderation { get; set; }
```


Moderation level: `auto` or `low`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L33)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_OutputCompression_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_OutputCompression"></a>

### OutputCompression


```csharp
public int? OutputCompression { get; set; }
```


Compression level of the output image, 0 to 100.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L36)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_OutputFormat_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Output format: `png`, `jpeg`, or `webp`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L39)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_PartialImages_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_PartialImages"></a>

### PartialImages


```csharp
public int? PartialImages { get; set; }
```


Number of partial images in streaming mode, 0 to 3.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L42)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Quality_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Quality"></a>

### Quality


```csharp
public string? Quality { get; set; }
```


Quality: `auto`, `low`, `medium`, `high`, `xhigh`, or `max`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L45)

<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Size_"></a>
<a id="Vercel_AI_OpenAI_OpenAIImageGenerationToolOptions_Size"></a>

### Size


```csharp
public string? Size { get; set; }
```


Size: `auto`, `1024x1024`, `1024x1536`, `1536x1024`, or `WIDTHxHEIGHT`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L48)
