---
title: "PartQueue<T>"
description: "Async queue used to hand stream parts to one consumer."
editUrl: false
prev: false
next: false
sidebar:
  label: "PartQueue<T>"
  hidden: true
---

<a id="Vercel_AI_Operations_PartQueue_1"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class PartQueue<T>
```


Async queue used to hand stream parts to one consumer.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L253)

## Methods

<a id="Vercel_AI_Operations_PartQueue_1_Complete_"></a>
<a id="Vercel_AI_Operations_PartQueue_1_Complete_System_Exception_"></a>

### Complete(Exception?)


```csharp
public void Complete(Exception? error)
```


Completes the queue, optionally with an error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L282)

<a id="Vercel_AI_Operations_PartQueue_1_Dequeue_"></a>
<a id="Vercel_AI_Operations_PartQueue_1_Dequeue"></a>

### Dequeue()


```csharp
public T Dequeue()
```


Removes the next part. Call after `CancellationToken)` returns true.

**Returns:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L352)

<a id="Vercel_AI_Operations_PartQueue_1_Enqueue_"></a>
<a id="Vercel_AI_Operations_PartQueue_1_Enqueue__0_"></a>

### Enqueue(T)


```csharp
public void Enqueue(T item)
```


Adds a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `item` | `T` |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L261)

<a id="Vercel_AI_Operations_PartQueue_1_WaitAsync_"></a>
<a id="Vercel_AI_Operations_PartQueue_1_WaitAsync_System_Threading_CancellationToken_"></a>

### WaitAsync(CancellationToken)


```csharp
public Task<bool> WaitAsync(CancellationToken cancellationToken)
```


Reads the next part. Returns false when the queue is complete.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L305)
