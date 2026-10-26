---
title: "ExponentialBackoff"
description: "Retries a failed operation with exponential backoff. Abort errors are rethrown. The first non-retryable error is rethrown without wrapping."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExponentialBackoff"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ExponentialBackoff"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ExponentialBackoff
```


Retries a failed operation with exponential backoff.
Abort errors are rethrown. The first non-retryable error is rethrown without wrapping.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L55)

## Methods

<a id="Vercel_AI_ProviderUtils_ExponentialBackoff_RetryAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ExponentialBackoff_RetryAsync__1_System_Func_System_Threading_Tasks_Task___0___System_Func_System_Exception_System_Threading_Tasks_Task_System_Boolean___Vercel_AI_ProviderUtils_RetryOptions_System_Threading_CancellationToken_"></a>

### `RetryAsync<T>(Func<Task<T>>, Func<Exception, Task<bool>>, RetryOptions?, CancellationToken)`


```csharp
public static Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, Task<bool>> shouldRetry, RetryOptions? options = null, CancellationToken cancellationToken = default)
```


Runs &lt;code class="paramref"&gt;action&lt;/code&gt; until it succeeds or retries are exhausted.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `action` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |
| `shouldRetry` | [Func&lt;Exception, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `options` | [RetryOptions](/api/Vercel.AI.ProviderUtils/RetryOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L58)
