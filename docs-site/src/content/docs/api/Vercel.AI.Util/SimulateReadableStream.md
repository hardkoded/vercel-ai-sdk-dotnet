---
title: "SimulateReadableStream"
description: "Emits scripted chunks, optionally delaying each pull."
editUrl: false
prev: false
next: false
sidebar:
  label: "SimulateReadableStream"
  hidden: true
---

<a id="Vercel_AI_Util_SimulateReadableStream"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class SimulateReadableStream
```


Emits scripted chunks, optionally delaying each pull.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L300)

## Methods

<a id="Vercel_AI_Util_SimulateReadableStream_Create_"></a>
<a id="Vercel_AI_Util_SimulateReadableStream_Create__1_System_Collections_Generic_IReadOnlyList___0__System_Nullable_System_Int32__System_Nullable_System_Int32__System_Func_System_Nullable_System_Int32__System_Threading_Tasks_Task__"></a>

### `Create<T>(IReadOnlyList<T>, int?, int?, Func<int?, Task>?)`


```csharp
public static ReadableStream<T> Create<T>(IReadOnlyList<T> chunks, int? initialDelayInMs = 0, int? chunkDelayInMs = 0, Func<int?, Task>? delay = null)
```


Creates a stream of &lt;code class="paramref"&gt;chunks&lt;/code&gt;.
A null delay skips the wait. `0` still invokes the delay with zero.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunks` | [IReadOnlyList&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `initialDelayInMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `chunkDelayInMs` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `delay` | [Func&lt;int?, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** `ReadableStream&lt;T&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/StitchableStream.cs#L306)
