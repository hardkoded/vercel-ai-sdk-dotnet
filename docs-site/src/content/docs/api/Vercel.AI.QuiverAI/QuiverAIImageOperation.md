---
title: "QuiverAIImageOperation"
description: "The operation of an image call. Maps to the operation union of QuiverAIImageModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageOperation"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageOperation"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public enum QuiverAIImageOperation
```


The operation of an image call. Maps to the `operation` union of `QuiverAIImageModelOptions`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L11)

## Fields

<a id="Vercel_AI_QuiverAI_QuiverAIImageOperation_Animate"></a>

### Animate


```csharp
Animate = 2
```


`animate`: animate an input SVG.

**Value:** [QuiverAIImageOperation](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L20)

<a id="Vercel_AI_QuiverAI_QuiverAIImageOperation_Edit"></a>

### Edit


```csharp
Edit = 3
```


`edit`: edit a single SVG using the prompt text as the instruction.

**Value:** [QuiverAIImageOperation](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L23)

<a id="Vercel_AI_QuiverAI_QuiverAIImageOperation_Generate"></a>

### Generate


```csharp
Generate = 0
```


`generate`: text-to-SVG generation.

**Value:** [QuiverAIImageOperation](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L14)

<a id="Vercel_AI_QuiverAI_QuiverAIImageOperation_Vectorize"></a>

### Vectorize


```csharp
Vectorize = 1
```


`vectorize`: convert an input raster image into an SVG.

**Value:** [QuiverAIImageOperation](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L17)
