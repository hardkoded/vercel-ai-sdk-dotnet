---
title: "ToolCallConcurrencyScheduler"
description: "Tool call concurrency helpers. Maps to tool-call-concurrency.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolCallConcurrencyScheduler"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolCallConcurrencyScheduler"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ToolCallConcurrencyScheduler
```


Tool call concurrency helpers. Maps to `tool-call-concurrency.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolCallConcurrency.cs#L10)

## Methods

<a id="Vercel_AI_GenerateText_ToolCallConcurrencyScheduler_MapWithConcurrency_"></a>
<a id="Vercel_AI_GenerateText_ToolCallConcurrencyScheduler_MapWithConcurrency__2_System_Collections_Generic_IReadOnlyList___0__System_Nullable_System_Int32__System_Func___0_System_Int32_System_Threading_Tasks_Task___1___System_Threading_CancellationToken_"></a>

### `MapWithConcurrency<TInput, TOutput>(IReadOnlyList<TInput>, int?, Func<TInput, int, Task<TOutput>>, CancellationToken)`


```csharp
public static Task<IReadOnlyList<TOutput>> MapWithConcurrency<TInput, TOutput>(IReadOnlyList<TInput> items, int? concurrency, Func<TInput, int, Task<TOutput>> execute, CancellationToken cancellationToken = default)
```


Maps items with an optional concurrency limit while preserving result order.
Workers claim items in input order. When &lt;code class="paramref"&gt;cancellationToken&lt;/code&gt; is canceled or an execution throws,
no more queued items start. Maps to `mapWithConcurrency`.

**Type parameters**

| Name | Description |
| --- | --- |
| `TInput` |  |
| `TOutput` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `items` | [IReadOnlyList&lt;TInput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `concurrency` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `execute` | [Func&lt;TInput, int, Task&lt;TOutput&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;TOutput&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolCallConcurrency.cs#L37)

<a id="Vercel_AI_GenerateText_ToolCallConcurrencyScheduler_PrepareToolCallConcurrency_"></a>
<a id="Vercel_AI_GenerateText_ToolCallConcurrencyScheduler_PrepareToolCallConcurrency_System_Nullable_System_Int32__"></a>

### PrepareToolCallConcurrency(int?)


```csharp
public static int? PrepareToolCallConcurrency(int? toolCallConcurrency)
```


Validates the maximum number of tool calls that may execute concurrently.
Null passes through. Maps to `prepareToolCallConcurrency`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallConcurrency` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidArgumentError](/api/Vercel.AI.Util/InvalidArgumentError/) | Thrown when &lt;code class="paramref"&gt;toolCallConcurrency&lt;/code&gt; is below 1. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolCallConcurrency.cs#L17)
