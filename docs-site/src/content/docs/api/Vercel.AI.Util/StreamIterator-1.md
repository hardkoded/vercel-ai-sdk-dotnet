---
title: "StreamIterator<T>"
description: "Async iterator over a ReadableStream%601."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamIterator<T>"
  hidden: true
---

<a id="Vercel_AI_Util_StreamIterator_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class StreamIterator<T>
```


Async iterator over a `ReadableStream%601`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L478)

## Methods

<a id="Vercel_AI_Util_StreamIterator_1_NextAsync_"></a>
<a id="Vercel_AI_Util_StreamIterator_1_NextAsync"></a>

### NextAsync()


```csharp
public Task<IteratorResult<T>> NextAsync()
```


Reads the next chunk.

**Returns:** [Task&lt;IteratorResult&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L489)

<a id="Vercel_AI_Util_StreamIterator_1_ReturnAsync_"></a>
<a id="Vercel_AI_Util_StreamIterator_1_ReturnAsync"></a>

### ReturnAsync()


```csharp
public Task<IteratorResult<T>> ReturnAsync()
```


Stops iteration and cancels the stream.

**Returns:** [Task&lt;IteratorResult&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L519)

<a id="Vercel_AI_Util_StreamIterator_1_ThrowAsync_"></a>
<a id="Vercel_AI_Util_StreamIterator_1_ThrowAsync_System_Exception_"></a>

### ThrowAsync(Exception)


```csharp
public Task ThrowAsync(Exception error)
```


Stops iteration, cancels the stream, and throws &lt;code class="paramref"&gt;error&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L526)
