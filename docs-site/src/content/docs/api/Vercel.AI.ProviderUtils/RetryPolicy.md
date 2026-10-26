---
title: "RetryPolicy"
description: "Retry policy aligned with the AI SDK defaults: 2 retries, 500–5000ms backoff, 25% jitter. Retryable statuses are 408, 429, and 500–599. Retry-After is honore..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryPolicy"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_RetryPolicy"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class RetryPolicy
```


Retry policy aligned with the AI SDK defaults: 2 retries, 500–5000ms backoff, 25% jitter.
Retryable statuses are 408, 429, and 500–599. `Retry-After` is honored up to 60 seconds.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L11)

## Properties

<a id="Vercel_AI_ProviderUtils_RetryPolicy_Default_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_Default"></a>

### Default


```csharp
public static RetryPolicy Default { get; }
```


SDK default policy.

**Value:** [RetryPolicy](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L26)

<a id="Vercel_AI_ProviderUtils_RetryPolicy_InitialDelay_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_InitialDelay"></a>

### InitialDelay


```csharp
public TimeSpan InitialDelay { get; set; }
```


Initial delay.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L17)

<a id="Vercel_AI_ProviderUtils_RetryPolicy_Jitter_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_Jitter"></a>

### Jitter


```csharp
public double Jitter { get; set; }
```


Jitter fraction applied to the delay. `0.25` is 25%.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L23)

<a id="Vercel_AI_ProviderUtils_RetryPolicy_MaxDelay_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_MaxDelay"></a>

### MaxDelay


```csharp
public TimeSpan MaxDelay { get; set; }
```


Maximum delay.

**Value:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L20)

<a id="Vercel_AI_ProviderUtils_RetryPolicy_MaxRetries_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; set; }
```


Maximum retries after the first attempt.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L14)

## Methods

<a id="Vercel_AI_ProviderUtils_RetryPolicy_GetDelay_"></a>
<a id="Vercel_AI_ProviderUtils_RetryPolicy_GetDelay_System_Int32_System_Nullable_System_TimeSpan__"></a>

### GetDelay(int, TimeSpan?)


```csharp
public TimeSpan GetDelay(int attempt, TimeSpan? retryAfter)
```


Computes the delay before the next attempt. &lt;code class="paramref"&gt;attempt&lt;/code&gt; is zero-based.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `attempt` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `retryAfter` | [TimeSpan?](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

**Returns:** [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/RetryPolicy.cs#L29)
