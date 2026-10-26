---
title: "ReadableStreamReader<T>"
description: "Default reader for a ReadableStream%601."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReadableStreamReader<T>"
  hidden: true
---

<a id="Vercel_AI_Util_ReadableStreamReader_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class ReadableStreamReader<T>
```


Default reader for a `ReadableStream%601`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L115)

## Methods

<a id="Vercel_AI_Util_ReadableStreamReader_1_CancelAsync_"></a>
<a id="Vercel_AI_Util_ReadableStreamReader_1_CancelAsync"></a>

### CancelAsync()


```csharp
public Task CancelAsync()
```


Cancels the stream and releases this reader.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L131)

<a id="Vercel_AI_Util_ReadableStreamReader_1_ReadAsync_"></a>
<a id="Vercel_AI_Util_ReadableStreamReader_1_ReadAsync"></a>

### ReadAsync()


```csharp
public Task<StreamRead<T>> ReadAsync()
```


Reads the next chunk, a completion, or a failure.

**Returns:** [Task&lt;StreamRead&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L125)

<a id="Vercel_AI_Util_ReadableStreamReader_1_ReleaseLock_"></a>
<a id="Vercel_AI_Util_ReadableStreamReader_1_ReleaseLock"></a>

### ReleaseLock()


```csharp
public void ReleaseLock()
```


Releases the lock without cancelling.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L137)
