---
title: "PrepareRetries.Result"
description: "Result of Int32)."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareRetries.Result"
  hidden: true
---

<a id="Vercel_AI_Util_PrepareRetries_Result"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public sealed class PrepareRetries.Result
```


Result of `Int32)`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L235)

## Constructors

<a id="Vercel_AI_Util_PrepareRetries_Result__ctor_"></a>
<a id="Vercel_AI_Util_PrepareRetries_Result__ctor_System_Int32_Vercel_AI_Util_RetryWithExponentialBackoffRespectingRetryHeaders_"></a>

### Result(int, RetryWithExponentialBackoffRespectingRetryHeaders)


```csharp
public Result(int maxRetries, RetryWithExponentialBackoffRespectingRetryHeaders retry)
```


Creates the prepared retry settings.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `maxRetries` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `retry` | [RetryWithExponentialBackoffRespectingRetryHeaders](/api/Vercel.AI.Util/RetryWithExponentialBackoffRespectingRetryHeaders/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L238)

## Properties

<a id="Vercel_AI_Util_PrepareRetries_Result_MaxRetries_"></a>
<a id="Vercel_AI_Util_PrepareRetries_Result_MaxRetries"></a>

### MaxRetries


```csharp
public int MaxRetries { get; }
```


Retry count after validation, or the default.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L245)

<a id="Vercel_AI_Util_PrepareRetries_Result_Retry_"></a>
<a id="Vercel_AI_Util_PrepareRetries_Result_Retry"></a>

### Retry


```csharp
public RetryWithExponentialBackoffRespectingRetryHeaders Retry { get; }
```


Retry strategy bound to that count.

**Value:** [RetryWithExponentialBackoffRespectingRetryHeaders](/api/Vercel.AI.Util/RetryWithExponentialBackoffRespectingRetryHeaders/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/RetryBackoff.cs#L248)
