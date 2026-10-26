---
title: "RetryErrorReason"
description: "Why RetryException was thrown."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryErrorReason"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_RetryErrorReason"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public enum RetryErrorReason
```


Why [RetryException](/api/Vercel.AI.ProviderUtils/RetryException/) was thrown.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L8)

## Fields

<a id="Vercel_AI_ProviderUtils_RetryErrorReason_ErrorNotRetryable"></a>

### ErrorNotRetryable


```csharp
ErrorNotRetryable = 1
```


A later attempt failed with an error that must not be retried.

**Value:** [RetryErrorReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L14)

<a id="Vercel_AI_ProviderUtils_RetryErrorReason_MaxRetriesExceeded"></a>

### MaxRetriesExceeded


```csharp
MaxRetriesExceeded = 0
```


The retry budget was used up.

**Value:** [RetryErrorReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L11)
