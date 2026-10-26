---
title: "RetryOptions"
description: "Options for ExponentialBackoff."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_RetryOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class RetryOptions
```


Options for [ExponentialBackoff](/api/Vercel.AI.ProviderUtils/ExponentialBackoff/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L36)

## Properties

<a id="Vercel_AI_ProviderUtils_RetryOptions_BackoffFactor_"></a>
<a id="Vercel_AI_ProviderUtils_RetryOptions_BackoffFactor"></a>

### BackoffFactor


```csharp
public double BackoffFactor { get; set; }
```


Multiplier applied after each retry.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L45)

<a id="Vercel_AI_ProviderUtils_RetryOptions_GetDelayInMs_"></a>
<a id="Vercel_AI_ProviderUtils_RetryOptions_GetDelayInMs"></a>

### GetDelayInMs


```csharp
public Func<Exception, int, int>? GetDelayInMs { get; set; }
```


Replaces the computed delay. The second argument is the current backoff delay.

**Value:** [Func&lt;Exception, int, int&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L48)

<a id="Vercel_AI_ProviderUtils_RetryOptions_InitialDelayInMs_"></a>
<a id="Vercel_AI_ProviderUtils_RetryOptions_InitialDelayInMs"></a>

### InitialDelayInMs


```csharp
public int InitialDelayInMs { get; set; }
```


Delay before the first retry, in milliseconds.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L42)

<a id="Vercel_AI_ProviderUtils_RetryOptions_MaxRetries_"></a>
<a id="Vercel_AI_ProviderUtils_RetryOptions_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; set; }
```


Retries after the first attempt. `0` disables wrapping and retry.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L39)
