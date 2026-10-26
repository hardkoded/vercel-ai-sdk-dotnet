---
title: "OperationFullStream<T>"
description: "A single-consumer stream with an explicit Cancel."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationFullStream<T>"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationFullStream_1"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class OperationFullStream<T> : IAsyncEnumerable<T>
```


A single-consumer stream with an explicit `Cancel`.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L178)

## Constructors

<a id="Vercel_AI_Operations_OperationFullStream_1__ctor_"></a>
<a id="Vercel_AI_Operations_OperationFullStream_1__ctor_System_Func_System_Collections_Generic_IAsyncEnumerator__0___System_Action_"></a>

### `OperationFullStream(Func<IAsyncEnumerator<T>>, Action)`


```csharp
public OperationFullStream(Func<IAsyncEnumerator<T>> open, Action cancel)
```


Creates a stream.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `open` | [Func&lt;IAsyncEnumerator&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `cancel` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L184)

<a id="Vercel_AI_Operations_OperationFullStream_1__ctor_System_Func_System_Threading_CancellationToken_System_Collections_Generic_IAsyncEnumerator__0___System_Action_"></a>

### `OperationFullStream(Func<CancellationToken, IAsyncEnumerator<T>>, Action)`


```csharp
public OperationFullStream(Func<CancellationToken, IAsyncEnumerator<T>> open, Action cancel)
```


Creates a stream whose enumerator observes the token passed to `CancellationToken)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `open` | [Func&lt;CancellationToken, IAsyncEnumerator&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `cancel` | [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L190)

## Methods

<a id="Vercel_AI_Operations_OperationFullStream_1_Cancel_"></a>
<a id="Vercel_AI_Operations_OperationFullStream_1_Cancel"></a>

### Cancel()


```csharp
public void Cancel()
```


Cancels the stream and aborts a pending model call.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L197)

<a id="Vercel_AI_Operations_OperationFullStream_1_GetAsyncEnumerator_"></a>
<a id="Vercel_AI_Operations_OperationFullStream_1_GetAsyncEnumerator_System_Threading_CancellationToken_"></a>

### GetAsyncEnumerator(CancellationToken)


```csharp
public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
```


Returns the single enumerator.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerator&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerator-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L203)
