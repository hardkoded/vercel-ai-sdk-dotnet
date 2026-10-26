---
title: "ToolExecution"
description: "Runs a tool delegate and reports preliminary and final outputs."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolExecution"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ToolExecution"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ToolExecution
```


Runs a tool delegate and reports preliminary and final outputs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L533)

## Methods

<a id="Vercel_AI_ProviderUtils_ToolExecution_ExecuteAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_ExecuteAsync__1_System_Func_System_Threading_Tasks_Task___0___"></a>

### `ExecuteAsync<T>(Func<Task<T>>)`


```csharp
public static IAsyncEnumerable<ToolExecution.ToolStep<T>> ExecuteAsync<T>(Func<Task<T>> execute)
```


Yields one final step from a single result.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `execute` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [IAsyncEnumerable&lt;ToolExecution.ToolStep&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L559)

<a id="Vercel_AI_ProviderUtils_ToolExecution_ExecuteStreamAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_ExecuteStreamAsync__1_System_Collections_Generic_IAsyncEnumerable___0__"></a>

### `ExecuteStreamAsync<T>(IAsyncEnumerable<T>)`


```csharp
public static IAsyncEnumerable<ToolExecution.ToolStep<T>> ExecuteStreamAsync<T>(IAsyncEnumerable<T> outputs)
```


Yields each streamed value as preliminary, then repeats the last value as final.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `outputs` | [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1) |  |

**Returns:** [IAsyncEnumerable&lt;ToolExecution.ToolStep&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L571)

<a id="Vercel_AI_ProviderUtils_ToolExecution_IsExecutable_"></a>
<a id="Vercel_AI_ProviderUtils_ToolExecution_IsExecutable_System_Delegate_"></a>

### IsExecutable(Delegate?)


```csharp
public static bool IsExecutable(Delegate? execute)
```


True when &lt;code class="paramref"&gt;execute&lt;/code&gt; is present.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `execute` | [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L553)
