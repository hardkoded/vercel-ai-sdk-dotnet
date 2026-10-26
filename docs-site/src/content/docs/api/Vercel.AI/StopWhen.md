---
title: "StopWhen"
description: "Stop-condition factories. Maps to isStepCount, hasToolCall, and isLoopFinished."
editUrl: false
prev: false
next: false
sidebar:
  label: "StopWhen"
  hidden: true
---

<a id="Vercel_AI_StopWhen"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public static class StopWhen
```


Stop-condition factories. Maps to `isStepCount`, `hasToolCall`, and `isLoopFinished`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L334)

## Methods

<a id="Vercel_AI_StopWhen_HasToolCall_"></a>
<a id="Vercel_AI_StopWhen_HasToolCall_System_String___"></a>

### HasToolCall(params string[])


```csharp
public static StopCondition HasToolCall(params string[] toolNames)
```


Stops when any of &lt;code class="paramref"&gt;toolNames&lt;/code&gt; was called on the latest step.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolNames` | [string\[\]](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L348)

<a id="Vercel_AI_StopWhen_IsLoopFinished_"></a>
<a id="Vercel_AI_StopWhen_IsLoopFinished"></a>

### IsLoopFinished()


```csharp
public static StopCondition IsLoopFinished()
```


A stop condition that never stops the loop. The loop still ends when a step does not call a tool.

**Returns:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L363)

<a id="Vercel_AI_StopWhen_IsStepCount_"></a>
<a id="Vercel_AI_StopWhen_IsStepCount_System_Int32_"></a>

### IsStepCount(int)


```csharp
public static StopCondition IsStepCount(int count)
```


Stops when the number of completed steps equals &lt;code class="paramref"&gt;count&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `count` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [StopCondition](/api/Vercel.AI/StopCondition/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L337)

<a id="Vercel_AI_StopWhen_IsStopConditionMet_"></a>
<a id="Vercel_AI_StopWhen_IsStopConditionMet_System_Collections_Generic_IReadOnlyList_System_Func_System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__System_Threading_Tasks_Task_System_Boolean____System_Collections_Generic_IReadOnlyList_Vercel_AI_StepResult__"></a>

### `IsStopConditionMet(IReadOnlyList<Func<IReadOnlyList<StepResult>, Task<bool>>>, IReadOnlyList<StepResult>)`


```csharp
public static Task<bool> IsStopConditionMet(IReadOnlyList<Func<IReadOnlyList<StepResult>, Task<bool>>> stopConditions, IReadOnlyList<StepResult> steps)
```


Returns true when any stop condition returns true. Maps to `isStopConditionMet`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stopConditions` | [IReadOnlyList&lt;Func&lt;IReadOnlyList&lt;StepResult&gt;, Task&lt;bool&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `steps` | [IReadOnlyList&lt;StepResult&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [Task&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L369)
