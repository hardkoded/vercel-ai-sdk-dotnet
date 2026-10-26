---
title: "XaiImageGenerationAction"
description: "Restricts what the xai.imagegeneration tool can do. Maps to the action argument."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiImageGenerationAction"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiImageGenerationAction"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public enum XaiImageGenerationAction
```


Restricts what the `xai.image_generation` tool can do. Maps to the `action` argument.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L11)

## Fields

<a id="Vercel_AI_Xai_XaiImageGenerationAction_Auto"></a>

### Auto


```csharp
Auto = 0
```


The model can generate and edit images.

**Value:** [XaiImageGenerationAction](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L14)

<a id="Vercel_AI_Xai_XaiImageGenerationAction_Edit"></a>

### Edit


```csharp
Edit = 2
```


Image editing only.

**Value:** [XaiImageGenerationAction](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L20)

<a id="Vercel_AI_Xai_XaiImageGenerationAction_Generate"></a>

### Generate


```csharp
Generate = 1
```


Text-to-image generation only.

**Value:** [XaiImageGenerationAction](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L17)
