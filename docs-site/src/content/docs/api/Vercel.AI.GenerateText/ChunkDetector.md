---
title: "ChunkDetector"
description: "Detects the first chunk in a buffer. Maps to ChunkDetector. Returns the first detected chunk, or null when no chunk was detected."
editUrl: false
prev: false
next: false
sidebar:
  label: "ChunkDetector"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ChunkDetector"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public delegate string? ChunkDetector(string buffer)
```


Detects the first chunk in a buffer. Maps to `ChunkDetector`.
Returns the first detected chunk, or `null` when no chunk was detected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `buffer` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L16)
