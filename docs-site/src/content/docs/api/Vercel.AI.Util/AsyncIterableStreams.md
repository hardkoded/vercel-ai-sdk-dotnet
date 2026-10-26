---
title: "AsyncIterableStreams"
description: "Wraps readable streams so they can be consumed with async iteration."
editUrl: false
prev: false
next: false
sidebar:
  label: "AsyncIterableStreams"
  hidden: true
---

<a id="Vercel_AI_Util_AsyncIterableStreams"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class AsyncIterableStreams
```


Wraps readable streams so they can be consumed with async iteration.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L662)

## Methods

<a id="Vercel_AI_Util_AsyncIterableStreams_AsAsyncIterableStream_"></a>
<a id="Vercel_AI_Util_AsyncIterableStreams_AsAsyncIterableStream__1_Vercel_AI_Util_ReadableStream___0__"></a>

### `AsAsyncIterableStream<T>(ReadableStream<T>)`


```csharp
public static AsyncIterableStream<T> AsAsyncIterableStream<T>(ReadableStream<T> stream)
```


Attaches an async iterator to &lt;code class="paramref"&gt;stream&lt;/code&gt; without inserting another transform.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;T&gt;` |  |

**Returns:** `AsyncIterableStream&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L665)

<a id="Vercel_AI_Util_AsyncIterableStreams_CreateAsyncIterableStream_"></a>
<a id="Vercel_AI_Util_AsyncIterableStreams_CreateAsyncIterableStream__1_Vercel_AI_Util_ReadableStream___0__"></a>

### `CreateAsyncIterableStream<T>(ReadableStream<T>)`


```csharp
public static AsyncIterableStream<T> CreateAsyncIterableStream<T>(ReadableStream<T> source)
```


Pipes &lt;code class="paramref"&gt;source&lt;/code&gt; through an identity transform and returns an async iterable.
Cancelling the result cancels &lt;code class="paramref"&gt;source&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `source` | `ReadableStream&lt;T&gt;` |  |

**Returns:** `AsyncIterableStream&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ReadableStream.cs#L674)
