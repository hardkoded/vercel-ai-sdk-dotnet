---
title: "SmoothStreamOptions"
description: "Options of Create(SmoothStreamOptions). Maps to the options of smoothStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "SmoothStreamOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SmoothStreamOptions"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class SmoothStreamOptions
```


Options of [Create(SmoothStreamOptions)](/api/Vercel.AI.GenerateText/SmoothStream/#Vercel_AI_GenerateText_SmoothStream_Create_Vercel_AI_GenerateText_SmoothStreamOptions_). Maps to the options of `smoothStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L39)

## Properties

<a id="Vercel_AI_GenerateText_SmoothStreamOptions_ChunkDetector_"></a>
<a id="Vercel_AI_GenerateText_SmoothStreamOptions_ChunkDetector"></a>

### ChunkDetector


```csharp
public ChunkDetector? ChunkDetector { get; set; }
```


A custom chunk detector. Takes precedence over [ChunkingRegex](#Vercel_AI_GenerateText_SmoothStreamOptions_ChunkingRegex) and [Chunking](#Vercel_AI_GenerateText_SmoothStreamOptions_Chunking).

**Value:** [ChunkDetector](/api/Vercel.AI.GenerateText/ChunkDetector/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L54)

<a id="Vercel_AI_GenerateText_SmoothStreamOptions_Chunking_"></a>
<a id="Vercel_AI_GenerateText_SmoothStreamOptions_Chunking"></a>

### Chunking


```csharp
public SmoothStreamChunking Chunking { get; set; }
```


Controls how the text is chunked for streaming. Defaults to [Word](/api/Vercel.AI.GenerateText/SmoothStreamChunking/#Vercel_AI_GenerateText_SmoothStreamChunking_Word).

**Value:** [SmoothStreamChunking](/api/Vercel.AI.GenerateText/SmoothStreamChunking/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L45)

<a id="Vercel_AI_GenerateText_SmoothStreamOptions_ChunkingRegex_"></a>
<a id="Vercel_AI_GenerateText_SmoothStreamOptions_ChunkingRegex"></a>

### ChunkingRegex


```csharp
public Regex? ChunkingRegex { get; set; }
```


A custom pattern that does not match the empty string. Takes precedence over [Chunking](#Vercel_AI_GenerateText_SmoothStreamOptions_Chunking).

**Value:** [Regex](https://learn.microsoft.com/dotnet/api/system.text.regularexpressions.regex)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L51)

<a id="Vercel_AI_GenerateText_SmoothStreamOptions_DelayInMs_"></a>
<a id="Vercel_AI_GenerateText_SmoothStreamOptions_DelayInMs"></a>

### DelayInMs


```csharp
public int? DelayInMs { get; set; }
```


The delay in milliseconds between each chunk. Defaults to 10ms. Set to `null` to skip the delay.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L42)

<a id="Vercel_AI_GenerateText_SmoothStreamOptions_Segmenter_"></a>
<a id="Vercel_AI_GenerateText_SmoothStreamOptions_Segmenter"></a>

### Segmenter


```csharp
public ISegmenter? Segmenter { get; set; }
```


A segmenter that chunks the text. Takes precedence over [ChunkDetector](#Vercel_AI_GenerateText_SmoothStreamOptions_ChunkDetector), [ChunkingRegex](#Vercel_AI_GenerateText_SmoothStreamOptions_ChunkingRegex) and [Chunking](#Vercel_AI_GenerateText_SmoothStreamOptions_Chunking).

**Value:** [ISegmenter](/api/Vercel.AI.GenerateText/ISegmenter/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L48)
