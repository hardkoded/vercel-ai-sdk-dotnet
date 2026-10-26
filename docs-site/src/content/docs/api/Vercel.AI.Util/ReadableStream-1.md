---
title: "ReadableStream<T>"
description: "Pull-based readable stream. Supports enqueue, close, error, backpressure via pull, and a single reader."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReadableStream<T>"
  hidden: true
---

<a id="Vercel_AI_Util_ReadableStream_1"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public class ReadableStream<T>
```


Pull-based readable stream. Supports enqueue, close, error, backpressure via pull, and a single reader.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L146)

## Constructors

<a id="Vercel_AI_Util_ReadableStream_1__ctor_"></a>
<a id="Vercel_AI_Util_ReadableStream_1__ctor_System_Action_Vercel_AI_Util_ReadableStreamController__0___System_Func_Vercel_AI_Util_ReadableStreamController__0__System_Threading_Tasks_Task__System_Func_System_Threading_Tasks_Task__"></a>

### `ReadableStream(Action<ReadableStreamController<T>>?, Func<ReadableStreamController<T>, Task>?, Func<Task>?)`


```csharp
public ReadableStream(Action<ReadableStreamController<T>>? start = null, Func<ReadableStreamController<T>, Task>? pull = null, Func<Task>? cancel = null)
```


Creates a stream. &lt;code class="paramref"&gt;start&lt;/code&gt; runs before the constructor returns.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `start` | [Action&lt;ReadableStreamController&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) |  |
| `pull` | [Func&lt;ReadableStreamController&lt;T&gt;, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `cancel` | [Func&lt;Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L163)

## Properties

<a id="Vercel_AI_Util_ReadableStream_1_Locked_"></a>
<a id="Vercel_AI_Util_ReadableStream_1_Locked"></a>

### Locked


```csharp
public bool Locked { get; }
```


Whether a reader currently holds the lock.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L175)

## Methods

<a id="Vercel_AI_Util_ReadableStream_1_CancelAsync_"></a>
<a id="Vercel_AI_Util_ReadableStream_1_CancelAsync"></a>

### CancelAsync()


```csharp
public Task CancelAsync()
```


Cancels the stream. Throws when a reader holds the lock.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L202)

<a id="Vercel_AI_Util_ReadableStream_1_FromArray_"></a>
<a id="Vercel_AI_Util_ReadableStream_1_FromArray_System_Collections_Generic_IReadOnlyList__0__"></a>

### `FromArray(IReadOnlyList<T>?)`


```csharp
public static ReadableStream<T> FromArray(IReadOnlyList<T>? values)
```


A stream that emits &lt;code class="paramref"&gt;values&lt;/code&gt; and then closes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `values` | [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [ReadableStream&lt;T&gt;](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L246)

<a id="Vercel_AI_Util_ReadableStream_1_GetReader_"></a>
<a id="Vercel_AI_Util_ReadableStream_1_GetReader"></a>

### GetReader()


```csharp
public ReadableStreamReader<T> GetReader()
```


Locks the stream and returns its reader.

**Returns:** `ReadableStreamReader&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L187)

<a id="Vercel_AI_Util_ReadableStream_1_ToArrayAsync_"></a>
<a id="Vercel_AI_Util_ReadableStream_1_ToArrayAsync"></a>

### ToArrayAsync()


```csharp
public Task<List<T>> ToArrayAsync()
```


Reads every chunk into a list.

**Returns:** [Task&lt;List&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L216)
