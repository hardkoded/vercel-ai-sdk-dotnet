---
title: "AsyncIterableStream<T>"
description: "A readable stream that can also be enumerated with await foreach."
editUrl: false
prev: false
next: false
sidebar:
  label: "AsyncIterableStream<T>"
  hidden: true
---

<a id="Vercel_AI_Util_AsyncIterableStream_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class AsyncIterableStream<T> : IAsyncEnumerable<T>
```


A readable stream that can also be enumerated with `await foreach`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L555)

## Constructors

<a id="Vercel_AI_Util_AsyncIterableStream_1__ctor_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1__ctor_Vercel_AI_Util_ReadableStream__0__"></a>

### `AsyncIterableStream(ReadableStream<T>)`


```csharp
public AsyncIterableStream(ReadableStream<T> stream)
```


Creates an iterable view of &lt;code class="paramref"&gt;stream&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;T&gt;` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L558)

## Properties

<a id="Vercel_AI_Util_AsyncIterableStream_1_Locked_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_Locked"></a>

### Locked


```csharp
public bool Locked { get; }
```


Whether a reader holds the lock.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L567)

<a id="Vercel_AI_Util_AsyncIterableStream_1_Stream_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_Stream"></a>

### Stream


```csharp
public ReadableStream<T> Stream { get; }
```


Underlying stream.

**Value:** `ReadableStream&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L564)

## Methods

<a id="Vercel_AI_Util_AsyncIterableStream_1_CancelAsync_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_CancelAsync"></a>

### CancelAsync()


```csharp
public Task CancelAsync()
```


Cancels the stream.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L579)

<a id="Vercel_AI_Util_AsyncIterableStream_1_GetAsyncEnumerator_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_GetAsyncEnumerator_System_Threading_CancellationToken_"></a>

### GetAsyncEnumerator(CancellationToken)


```csharp
public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
```


Returns an enumerator that iterates asynchronously through the collection.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | A [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) that may be used to cancel the asynchronous iteration. |

**Returns:** [IAsyncEnumerator&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerator-1) — An enumerator that can be used to iterate asynchronously through the collection.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L591)

<a id="Vercel_AI_Util_AsyncIterableStream_1_GetIterator_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_GetIterator"></a>

### GetIterator()


```csharp
public StreamIterator<T> GetIterator()
```


Starts an iterator that cancels the stream when exited early.

**Returns:** `StreamIterator&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L585)

<a id="Vercel_AI_Util_AsyncIterableStream_1_GetReader_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_GetReader"></a>

### GetReader()


```csharp
public ReadableStreamReader<T> GetReader()
```


Locks the stream.

**Returns:** `ReadableStreamReader&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L573)

<a id="Vercel_AI_Util_AsyncIterableStream_1_ToArrayAsync_"></a>
<a id="Vercel_AI_Util_AsyncIterableStream_1_ToArrayAsync"></a>

### ToArrayAsync()


```csharp
public Task<List<T>> ToArrayAsync()
```


Reads every value.

**Returns:** [Task&lt;List&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L597)
