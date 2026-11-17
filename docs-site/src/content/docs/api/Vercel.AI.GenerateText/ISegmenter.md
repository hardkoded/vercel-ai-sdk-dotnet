---
title: "ISegmenter"
description: "Splits text into segments, like Intl.Segmenter. .NET has no built-in word segmenter, so you provide one (for example a wrapper over ICU) to chunk text with l..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ISegmenter"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ISegmenter"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public interface ISegmenter
```


Splits text into segments, like `Intl.Segmenter`. .NET has no built-in word segmenter, so you provide one
(for example a wrapper over ICU) to chunk text with locale-aware word segmentation, which suits CJK languages.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L22)
