---
title: "AudioChunkStream"
description: "Audio the model may take ownership of."
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


Audio the model may take ownership of.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L89)

## Properties

<a id="Vercel_AI_Operations_AudioChunkStream_CancelError_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_CancelError"></a>

### CancelError


```csharp
public Exception? CancelError { get; }
```


Reason from a failed setup cancel.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L95)

<a id="Vercel_AI_Operations_AudioChunkStream_Locked_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Locked"></a>

### Locked


```csharp
public bool Locked { get; }
```


True after the model takes a reader.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L92)

## Methods

<a id="Vercel_AI_Operations_AudioChunkStream_Cancel_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Cancel_System_Exception_"></a>

### Cancel(Exception)


```csharp
public void Cancel(Exception error)
```


Cancels an unowned stream. A locked stream throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L104)

<a id="Vercel_AI_Operations_AudioChunkStream_Lock_"></a>
<a id="Vercel_AI_Operations_AudioChunkStream_Lock"></a>

### Lock()


```csharp
public void Lock()
```


Marks the stream as owned by the model.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L98)
