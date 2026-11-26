---
title: "QuiverAIImageCapabilities"
description: "The image capabilities of a QuiverAI model: the shared ImageCapabilities plus animation and editing. Maps to capabilities.image of getQuiverAIImageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageCapabilities"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIImageCapabilities
```


The image capabilities of a QuiverAI model: the shared [ImageCapabilities](/api/Vercel.AI/ImageCapabilities/) plus animation and editing. Maps to `capabilities.image` of `getQuiverAIImageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L77)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_Animation_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_Animation"></a>

### Animation


```csharp
public ModelCapability Animation { get; init; }
```


Maps to `animation`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L92)

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_Editing_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_Editing"></a>

### Editing


```csharp
public ModelCapability Editing { get; init; }
```


Maps to `editing`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L95)

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_FileInput_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_FileInput"></a>

### FileInput


```csharp
public ModelCapability FileInput { get; init; }
```


Maps to `fileInput`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L83)

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_MaskInput_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_MaskInput"></a>

### MaskInput


```csharp
public ModelCapability MaskInput { get; init; }
```


Maps to `maskInput`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L86)

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_MaxImagesPerCall_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_MaxImagesPerCall"></a>

### MaxImagesPerCall


```csharp
public int MaxImagesPerCall { get; init; }
```


Maps to `maxImagesPerCall`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L89)

<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_SizesDescription_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageCapabilities_SizesDescription"></a>

### SizesDescription


```csharp
public string SizesDescription { get; init; }
```


Maps to `sizes.description`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L80)
