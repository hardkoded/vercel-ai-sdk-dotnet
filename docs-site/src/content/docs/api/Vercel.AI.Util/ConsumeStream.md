---
title: "ConsumeStream"
description: "Reads a stream to the end. Maps to consumeStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "ConsumeStream"
  hidden: true
---

<a id="Vercel_AI_Util_ConsumeStream"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class ConsumeStream
```


Reads a stream to the end. Maps to `consumeStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ConsumeStream.cs#L8)

## Methods

<a id="Vercel_AI_Util_ConsumeStream_ConsumeAsync_"></a>
<a id="Vercel_AI_Util_ConsumeStream_ConsumeAsync__1_Vercel_AI_Util_ReadableStream___0__System_Action_System_Exception__Vercel_AI_Util_AbortSignal_"></a>

### `ConsumeAsync<T>(ReadableStream<T>, Action<Exception>?, AbortSignal?)`


```csharp
public static Task ConsumeAsync<T>(ReadableStream<T> stream, Action<Exception>? onError = null, AbortSignal? abortSignal = null)
```


Consumes &lt;code class="paramref"&gt;stream&lt;/code&gt; until it is fully read. It does not process or return the data.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | `ReadableStream&lt;T&gt;` | The stream to read. |
| `onError` | [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1) | Called when reading fails. The failure is not rethrown. |
| `abortSignal` | [AbortSignal](/api/Vercel.AI.Util/AbortSignal/) | Cancels the stream when it aborts. |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) — A task that completes when the stream is fully consumed.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ConsumeStream.cs#L17)
