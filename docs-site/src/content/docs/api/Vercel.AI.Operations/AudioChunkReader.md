---
title: "AudioChunkReader"
description: "The reader of an AudioChunkStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "AudioChunkReader"
  hidden: true
---

<a id="Vercel_AI_Operations_AudioChunkReader"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class AudioChunkReader
```


The reader of an [AudioChunkStream](/api/Vercel.AI.Operations/AudioChunkStream/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L243)

## Methods

<a id="Vercel_AI_Operations_AudioChunkReader_Cancel_"></a>
<a id="Vercel_AI_Operations_AudioChunkReader_Cancel"></a>

### Cancel()


```csharp
public void Cancel()
```


Cancels the underlying stream and any pending read.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L274)

<a id="Vercel_AI_Operations_AudioChunkReader_ReadAsync_"></a>
<a id="Vercel_AI_Operations_AudioChunkReader_ReadAsync_System_Threading_CancellationToken_"></a>

### ReadAsync(CancellationToken)


```csharp
public Task<byte[]?> ReadAsync(CancellationToken cancellationToken = default)
```


Reads the next chunk. Returns null when the stream ends or the reader is cancelled. Base64 chunks are decoded.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L254)

<a id="Vercel_AI_Operations_AudioChunkReader_ReleaseLock_"></a>
<a id="Vercel_AI_Operations_AudioChunkReader_ReleaseLock"></a>

### ReleaseLock()


```csharp
public void ReleaseLock()
```


Releases the lock so the stream can be cancelled directly.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L281)
