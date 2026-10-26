---
title: "RetryErrorReason"
description: "Why RetryError stopped retrying."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryErrorReason"
  hidden: true
---

<a id="Vercel_AI_Util_RetryErrorReason"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public enum RetryErrorReason
```


Why [RetryError](/api/Vercel.AI.Util/RetryError/) stopped retrying.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L212)

## Fields

<a id="Vercel_AI_Util_RetryErrorReason_Abort"></a>

### Abort


```csharp
Abort = 2
```


The call was aborted.

**Value:** [RetryErrorReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L221)

<a id="Vercel_AI_Util_RetryErrorReason_ErrorNotRetryable"></a>

### ErrorNotRetryable


```csharp
ErrorNotRetryable = 1
```


A later attempt failed with an error that must not be retried.

**Value:** [RetryErrorReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L218)

<a id="Vercel_AI_Util_RetryErrorReason_MaxRetriesExceeded"></a>

### MaxRetriesExceeded


```csharp
MaxRetriesExceeded = 0
```


The retry budget was spent.

**Value:** [RetryErrorReason](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L215)
