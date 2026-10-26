---
title: "OperationCallbacks"
description: "Invokes callbacks and swallows their exceptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationCallbacks"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationCallbacks"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class OperationCallbacks
```


Invokes callbacks and swallows their exceptions.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L670)

## Methods

<a id="Vercel_AI_Operations_OperationCallbacks_NotifyAsync_"></a>
<a id="Vercel_AI_Operations_OperationCallbacks_NotifyAsync__1___0_System_Func___0_System_Threading_Tasks_Task____"></a>

### `NotifyAsync<T>(T, params Func<T, Task>?[])`


```csharp
public static Task NotifyAsync<T>(T payload, params Func<T, Task>?[] callbacks)
```


Invokes each callback. A thrown callback does not fail the operation.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `payload` | `T` |  |
| `callbacks` | [Func&lt;T, Task&gt;\[\]](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L673)
