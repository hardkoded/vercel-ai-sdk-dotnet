---
title: "RetryOptions"
description: "Options for RetryWithExponentialBackoffRespectingRetryHeaders."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryOptions"
  hidden: true
---

<a id="Vercel_AI_Util_RetryOptions"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class RetryOptions
```


Options for [RetryWithExponentialBackoffRespectingRetryHeaders](/api/Vercel.AI.Util/RetryWithExponentialBackoffRespectingRetryHeaders/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L11)

## Properties

<a id="Vercel_AI_Util_RetryOptions_AbortSignal_"></a>
<a id="Vercel_AI_Util_RetryOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; set; }
```


Aborts the wait between attempts.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L23)

<a id="Vercel_AI_Util_RetryOptions_AdditionalRetryableError_"></a>
<a id="Vercel_AI_Util_RetryOptions_AdditionalRetryableError"></a>

### AdditionalRetryableError


```csharp
public Func<object, Task<bool>>? AdditionalRetryableError { get; set; }
```


Extra predicate consulted when the error is not an API or gateway retry.

**Value:** [Func&lt;object, Task&lt;bool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L26)

<a id="Vercel_AI_Util_RetryOptions_BackoffFactor_"></a>
<a id="Vercel_AI_Util_RetryOptions_BackoffFactor"></a>

### BackoffFactor


```csharp
public double BackoffFactor { get; set; }
```


Multiplier applied after each retry. Default is 2.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L20)

<a id="Vercel_AI_Util_RetryOptions_Delay_"></a>
<a id="Vercel_AI_Util_RetryOptions_Delay"></a>

### Delay


```csharp
public Func<int, CancellationToken, Task>? Delay { get; set; }
```


Waits `milliseconds` before the next attempt.
The default is `CancellationToken)`.

**Value:** [Func&lt;int, CancellationToken, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L32)

<a id="Vercel_AI_Util_RetryOptions_InitialDelayInMs_"></a>
<a id="Vercel_AI_Util_RetryOptions_InitialDelayInMs"></a>

### InitialDelayInMs


```csharp
public int InitialDelayInMs { get; set; }
```


Delay before the first retry, in milliseconds. Default is 2000.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L17)

<a id="Vercel_AI_Util_RetryOptions_MaxRetries_"></a>
<a id="Vercel_AI_Util_RetryOptions_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; set; }
```


Maximum retries after the first attempt. Default is 2.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L14)

<a id="Vercel_AI_Util_RetryOptions_UtcNow_"></a>
<a id="Vercel_AI_Util_RetryOptions_UtcNow"></a>

### UtcNow


```csharp
public Func<DateTimeOffset>? UtcNow { get; set; }
```


Clock used to parse HTTP-date `Retry-After` values. Default is UTC now.

**Value:** [Func&lt;DateTimeOffset&gt;](https://learn.microsoft.com/dotnet/api/system.func-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L35)
