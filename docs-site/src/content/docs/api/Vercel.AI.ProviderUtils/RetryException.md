---
title: "RetryException"
description: "Failure of ExponentialBackoff after at least one retry."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryException"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_RetryException"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class RetryException : Exception, ISerializable
```


Failure of [ExponentialBackoff](/api/Vercel.AI.ProviderUtils/ExponentialBackoff/) after at least one retry.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L18)

## Constructors

<a id="Vercel_AI_ProviderUtils_RetryException__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_RetryException__ctor_System_String_Vercel_AI_ProviderUtils_RetryErrorReason_System_Collections_Generic_IReadOnlyList_System_Exception__"></a>

### `RetryException(string, RetryErrorReason, IReadOnlyList<Exception>)`


```csharp
public RetryException(string message, RetryErrorReason reason, IReadOnlyList<Exception> errors)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reason` | [RetryErrorReason](/api/Vercel.AI.ProviderUtils/RetryErrorReason/) |  |
| `errors` | [IReadOnlyList&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L21)

## Properties

<a id="Vercel_AI_ProviderUtils_RetryException_Errors_"></a>
<a id="Vercel_AI_ProviderUtils_RetryException_Errors"></a>

### Errors


```csharp
public IReadOnlyList<Exception> Errors { get; }
```


Errors from each attempt, in order.

**Value:** [IReadOnlyList&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L32)

<a id="Vercel_AI_ProviderUtils_RetryException_Reason_"></a>
<a id="Vercel_AI_ProviderUtils_RetryException_Reason"></a>

### Reason


```csharp
public RetryErrorReason Reason { get; }
```


Why retries stopped.

**Value:** [RetryErrorReason](/api/Vercel.AI.ProviderUtils/RetryErrorReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ExponentialBackoff.cs#L29)
