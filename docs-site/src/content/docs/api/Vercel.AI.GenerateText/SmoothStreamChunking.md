---
title: "SmoothStreamChunking"
description: "How SmoothStream chunks the text. Maps to the 'word' | 'line' values of chunking."
editUrl: false
prev: false
next: false
sidebar:
  label: "SmoothStreamChunking"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SmoothStreamChunking"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public enum SmoothStreamChunking
```


How [SmoothStream](/api/Vercel.AI.GenerateText/SmoothStream/) chunks the text. Maps to the `'word' | 'line'` values of `chunking`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L29)

## Fields

<a id="Vercel_AI_GenerateText_SmoothStreamChunking_Line"></a>

### Line


```csharp
Line = 1
```


Stream line by line.

**Value:** [SmoothStreamChunking](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L35)

<a id="Vercel_AI_GenerateText_SmoothStreamChunking_Word"></a>

### Word


```csharp
Word = 0
```


Stream word by word. This is the default.

**Value:** [SmoothStreamChunking](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L32)
