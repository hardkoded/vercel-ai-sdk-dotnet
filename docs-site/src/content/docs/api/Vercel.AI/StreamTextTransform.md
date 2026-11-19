---
title: "StreamTextTransform"
description: "A transformation that is applied to the stream of parts. It receives the stream and returns the transformed stream. The transformation must maintain the stre..."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTextTransform"
  hidden: true
---

<a id="Vercel_AI_StreamTextTransform"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public delegate Func<IAsyncEnumerable<TextStreamPart>, CancellationToken, IAsyncEnumerable<TextStreamPart>> StreamTextTransform(StreamTextTransformOptions options)
```


A transformation that is applied to the stream of parts. It receives the stream and returns the transformed stream.
The transformation must maintain the stream structure for `StreamTextAsync` to work correctly. Maps to `StreamTextTransform`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StreamTextTransformOptions](/api/Vercel.AI/StreamTextTransformOptions/) |  |

**Returns:** [Func&lt;IAsyncEnumerable&lt;TextStreamPart&gt;, CancellationToken, IAsyncEnumerable&lt;TextStreamPart&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1824)
