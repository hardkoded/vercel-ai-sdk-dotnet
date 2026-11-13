---
title: "AudioChunkStream"
description: "Audio the model may take ownership of. Chunks are written with Byte%5b%5d) or Write(string) (base64) and read through GetReader()."
editUrl: false
prev: false
next: false
sidebar:
  label: "AudioChunkStream"
  hidden: true
---

<a id="Vercel_AI_Operations_AudioChunkStream"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class AudioChunkStream
```


Audio the model may take ownership of. Chunks are written with `Byte%5b%5d)` or
[Write(string)](#Vercel_AI_Operations_AudioChunkStream_Write_System_String_) (base64) and read through [GetReader()](#Vercel_AI_Operations_AudioChunkStream_GetReader).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L124)

## Constructors

<a id="Vercel_AI_Operations_AudioChunkStream__ctor_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream__ctor_System_Action_System_Exception__"></a>

### `AudioChunkStream(Action<Exception?>?)`


```csharp
public AudioChunkStream(Action<Exception?>? onCancel = null)
```


Creates an audio stream. &lt;code class="paramref"&gt;onCancel&lt;/code&gt; runs once when the stream or its reader is cancelled.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `onCancel` | [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L133)

## Properties

<a id="Vercel_AI_Operations_AudioChunkStream_CancelError_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_CancelError"></a>

### CancelError


```csharp
public Exception? CancelError { get; }
```


Reason from a failed setup cancel.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L142)

<a id="Vercel_AI_Operations_AudioChunkStream_Locked_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Locked"></a>

### Locked


```csharp
public bool Locked { get; }
```


True after the model takes a reader.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L139)

## Methods

<a id="Vercel_AI_Operations_AudioChunkStream_Cancel_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Cancel_System_Exception_"></a>

### Cancel(Exception?)


```csharp
public void Cancel(Exception? error = null)
```


Cancels an unowned stream. A locked stream throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L180)

<a id="Vercel_AI_Operations_AudioChunkStream_Complete_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Complete"></a>

### Complete()


```csharp
public void Complete()
```


Ends the stream after the written chunks.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L163)

<a id="Vercel_AI_Operations_AudioChunkStream_Fail_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Fail_System_Exception_"></a>

### Fail(Exception)


```csharp
public void Fail(Exception error)
```


Fails the stream after the written chunks.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L174)

<a id="Vercel_AI_Operations_AudioChunkStream_GetReader_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_GetReader"></a>

### GetReader()


```csharp
public AudioChunkReader GetReader()
```


Locks the stream and returns its only reader.

**Returns:** [AudioChunkReader](/api/Vercel.AI.Operations/AudioChunkReader/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L192)

<a id="Vercel_AI_Operations_AudioChunkStream_Lock_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Lock"></a>

### Lock()


```csharp
public void Lock()
```


Marks the stream as owned by the model.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L145)

<a id="Vercel_AI_Operations_AudioChunkStream_Write_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Write_System_Byte___"></a>

### Write(byte[])


```csharp
public void Write(byte[] chunk)
```


Writes a binary chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L151)

<a id="Vercel_AI_Operations_AudioChunkStream_Write_System_String_"></a>

### Write(string)


```csharp
public void Write(string base64Chunk)
```


Writes a base64 chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `base64Chunk` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L157)
