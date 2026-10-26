---
title: "Callbacks"
description: "Invokes callbacks and ignores individual failures."
editUrl: false
prev: false
next: false
sidebar:
  label: "Callbacks"
  hidden: true
---

<a id="Vercel_AI_Util_Callbacks"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class Callbacks
```


Invokes callbacks and ignores individual failures.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Callbacks.cs#L8)

## Methods

<a id="Vercel_AI_Util_Callbacks_MergeCallbacks_"></a>
<a id="Vercel_AI_Util_Callbacks_MergeCallbacks__1_System_Func___0_System_Threading_Tasks_Task____"></a>

### `MergeCallbacks<TEvent>(params Func<TEvent, Task>?[])`


```csharp
public static Func<TEvent, Task> MergeCallbacks<TEvent>(params Func<TEvent, Task>?[] callbacks)
```


Returns a callback that forwards each event to &lt;code class="paramref"&gt;callbacks&lt;/code&gt; in parallel
and waits until they settle. Failures are ignored.

**Type parameters**

| Name | Description |
| --- | --- |
| `TEvent` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `callbacks` | [Func&lt;TEvent, Task&gt;\[\]](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [Func&lt;TEvent, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Callbacks.cs#L48)

<a id="Vercel_AI_Util_Callbacks_NotifyAsync_"></a>
<a id="Vercel_AI_Util_Callbacks_NotifyAsync__1___0_System_Collections_Generic_IEnumerable_System_Func___0_System_Threading_Tasks_Task___"></a>

### `NotifyAsync<T>(T, IEnumerable<Func<T, Task>?>?)`


```csharp
public static Task NotifyAsync<T>(T eventValue, IEnumerable<Func<T, Task>?>? callbacks)
```


Invokes &lt;code class="paramref"&gt;callbacks&lt;/code&gt; in parallel with &lt;code class="paramref"&gt;eventValue&lt;/code&gt; and waits for them to settle.
Null callbacks are skipped. Thrown and rejected callbacks are ignored.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `eventValue` | `T` |  |
| `callbacks` | [IEnumerable&lt;Func&lt;T, Task&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Callbacks.cs#L14)

<a id="Vercel_AI_Util_Callbacks_NotifyAsync__1___0_System_Func___0_System_Threading_Tasks_Task__"></a>

### `NotifyAsync<T>(T, Func<T, Task>?)`


```csharp
public static Task NotifyAsync<T>(T eventValue, Func<T, Task>? callback)
```


Notifies one callback. A null callback is ignored.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `eventValue` | `T` |  |
| `callback` | [Func&lt;T, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/Callbacks.cs#L34)
