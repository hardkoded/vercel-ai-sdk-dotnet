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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L61)

## Methods

<a id="Vercel_AI_GenerateText_SmoothStream_Create_"></a>
<a id="Vercel_AI_GenerateText_SmoothStream_Create_Vercel_AI_GenerateText_SmoothStreamOptions_"></a>

### Create(SmoothStreamOptions?)


```csharp
public static Func<IReadOnlyList<Tool>?, Func<IAsyncEnumerable<TextStreamPart>, CancellationToken, IAsyncEnumerable<TextStreamPart>>> Create(SmoothStreamOptions? options = null)
```


Creates the transform. Call the returned function with the tools to get a transform of the full stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [SmoothStreamOptions](/api/Vercel.AI.GenerateText/SmoothStreamOptions/) |  |

**Returns:** [Func&lt;IReadOnlyList&lt;Tool&gt;, Func&lt;IAsyncEnumerable&lt;TextStreamPart&gt;, CancellationToken, IAsyncEnumerable&lt;TextStreamPart&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/SmoothStream.cs#L77)
