---
title: "SerialJobExecutor"
description: "Runs async jobs one at a time, in submission order."
editUrl: false
prev: false
next: false
sidebar:
  label: "SerialJobExecutor"
  hidden: true
---

<a id="Vercel_AI_Util_SerialJobExecutor"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class SerialJobExecutor
```


Runs async jobs one at a time, in submission order.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/SerialJobExecutor.cs#L8)

## Methods

<a id="Vercel_AI_Util_SerialJobExecutor_RunAsync_"></a>
<a id="Vercel_AI_Util_SerialJobExecutor_RunAsync_System_Func_System_Threading_Tasks_Task__"></a>

### `RunAsync(Func<Task>)`


```csharp
public Task RunAsync(Func<Task> job)
```


Queues &lt;code class="paramref"&gt;job&lt;/code&gt; and runs it after earlier jobs finish.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `job` | [Func&lt;Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/SerialJobExecutor.cs#L15)
