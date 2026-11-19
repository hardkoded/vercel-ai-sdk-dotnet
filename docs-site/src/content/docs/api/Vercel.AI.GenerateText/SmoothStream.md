---
title: "SmoothStream"
description: "Smooths text and reasoning streaming output. Maps to smoothStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "SmoothStream"
  hidden: true
---

<a id="Vercel_AI_GenerateText_SmoothStream"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class SmoothStream
```


Smooths text and reasoning streaming output. Maps to `smoothStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L67)

## Methods

<a id="Vercel_AI_GenerateText_SmoothStream_Create_"></a>
<a id="Vercel_AI_GenerateText_SmoothStream_Create_Vercel_AI_GenerateText_SmoothStreamOptions_"></a>

### Create(SmoothStreamOptions?)


```csharp
public static StreamTextTransform Create(SmoothStreamOptions? options = null)
```


Creates the transform. Pass it to [ExperimentalTransform](/api/Vercel.AI/StreamTextOptions/#Vercel_AI_StreamTextOptions_ExperimentalTransform), or call it with [StreamTextTransformOptions](/api/Vercel.AI/StreamTextTransformOptions/) to get a transform of the full stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SmoothStreamOptions](/api/Vercel.AI.GenerateText/SmoothStreamOptions/) |  |

**Returns:** [StreamTextTransform](/api/Vercel.AI/StreamTextTransform/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L87)
