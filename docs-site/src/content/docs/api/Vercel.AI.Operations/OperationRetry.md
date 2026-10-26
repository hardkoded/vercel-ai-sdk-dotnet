---
title: "OperationRetry"
description: "Retries a provider call the way prepareRetries does."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationRetry"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationRetry"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class OperationRetry
```


Retries a provider call the way `prepareRetries` does.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L534)

## Methods

<a id="Vercel_AI_Operations_OperationRetry_ExecuteAsync_"></a>
<a id="Vercel_AI_Operations_OperationRetry_ExecuteAsync__1_System_Nullable_System_Int32__System_Threading_CancellationToken_System_Exception_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_Task___0___System_Func_System_Exception_System_Boolean__"></a>

### `ExecuteAsync<T>(int?, CancellationToken, Exception?, Func<CancellationToken, Task<T>>, Func<Exception, bool>?)`


```csharp
public static Task<T> ExecuteAsync<T>(int? maxRetries, CancellationToken cancellationToken, Exception? abortReason, Func<CancellationToken, Task<T>> action, Func<Exception, bool>? additionalRetryable)
```


Validates &lt;code class="paramref"&gt;maxRetries&lt;/code&gt; and runs &lt;code class="paramref"&gt;action&lt;/code&gt;.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `maxRetries` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `abortReason` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `action` | [Func&lt;CancellationToken, Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `additionalRetryable` | [Func&lt;Exception, bool&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L537)

<a id="Vercel_AI_Operations_OperationRetry_ResolveMaxRetries_"></a>
<a id="Vercel_AI_Operations_OperationRetry_ResolveMaxRetries_System_Nullable_System_Int32__System_String_"></a>

### ResolveMaxRetries(int?, string)


```csharp
public static int ResolveMaxRetries(int? maxRetries, string parameter = "maxRetries")
```


Returns the retry count after validation. Null becomes 2.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `maxRetries` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `parameter` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L554)

<a id="Vercel_AI_Operations_OperationRetry_ThrowIfAborted_"></a>
<a id="Vercel_AI_Operations_OperationRetry_ThrowIfAborted_System_Threading_CancellationToken_System_Exception_"></a>

### ThrowIfAborted(CancellationToken, Exception?)


```csharp
public static void ThrowIfAborted(CancellationToken cancellationToken, Exception? reason)
```


Throws &lt;code class="paramref"&gt;reason&lt;/code&gt; when the token is cancelled.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |
| `reason` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L634)
