---
title: "RetryWithExponentialBackoffRespectingRetryHeaders"
description: "Retries a failed operation with exponential backoff, honoring retry-after-ms and retry-after when the delay is between 0 and 60 seconds, or shorter than the..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryWithExponentialBackoffRespectingRetryHeaders"
  hidden: true
---

<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class RetryWithExponentialBackoffRespectingRetryHeaders
```


Retries a failed operation with exponential backoff, honoring `retry-after-ms` and `retry-after`
when the delay is between 0 and 60 seconds, or shorter than the backoff delay.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L42)

## Constructors

<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders__ctor_"></a>
<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders__ctor_Vercel_AI_Util_RetryOptions_"></a>

### RetryWithExponentialBackoffRespectingRetryHeaders(RetryOptions?)


```csharp
public RetryWithExponentialBackoffRespectingRetryHeaders(RetryOptions? options = null)
```


Creates a retry strategy.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [RetryOptions](/api/Vercel.AI.Util/RetryOptions/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L47)

## Methods

<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders_ExecuteAsync_"></a>
<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders_ExecuteAsync__1_System_Func_System_Threading_Tasks_Task___0___"></a>

### `ExecuteAsync<T>(Func<Task<T>>)`


```csharp
public Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
```


Runs &lt;code class="paramref"&gt;operation&lt;/code&gt; until it succeeds or retries stop.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `operation` | [Func&lt;Task&lt;T&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [Task&lt;T&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L53)

<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders_GetRetryDelayInMs_"></a>
<a id="Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders_GetRetryDelayInMs_System_Object_System_Double_System_Nullable_System_DateTimeOffset__"></a>

### GetRetryDelayInMs(object, double, DateTimeOffset?)


```csharp
public static double GetRetryDelayInMs(object error, double exponentialBackoffDelay, DateTimeOffset? utcNow = null)
```


Delay in milliseconds for &lt;code class="paramref"&gt;error&lt;/code&gt; given the current exponential delay.
Header delays are used when they are at least 0 and either under 60 seconds or shorter than the backoff.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `exponentialBackoffDelay` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `utcNow` | [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |

**Returns:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L67)
