---
title: "OperationRetryException"
description: "Retries were exhausted. LastError is the final attempt."
editUrl: false
prev: false
next: false
sidebar:
  label: "OperationRetryException"
  hidden: true
---

<a id="Vercel_AI_Operations_OperationRetryException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class OperationRetryException : AiSdkException, ISerializable
```


Retries were exhausted. [LastError](#Vercel_AI_Operations_OperationRetryException_LastError) is the final attempt.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L225)

## Constructors

<a id="Vercel_AI_Operations_OperationRetryException__ctor_"></a>
<a id="Vercel_AI_Operations_OperationRetryException__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_Exception__"></a>

### `OperationRetryException(string, string, IReadOnlyList<Exception>)`


```csharp
public OperationRetryException(string message, string reason, IReadOnlyList<Exception> errors)
```


Creates a retry failure.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `errors` | [IReadOnlyList&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L228)

## Properties

<a id="Vercel_AI_Operations_OperationRetryException_Errors_"></a>
<a id="Vercel_AI_Operations_OperationRetryException_Errors"></a>

### Errors


```csharp
public IReadOnlyList<Exception> Errors { get; }
```


Errors from each attempt.

**Value:** [IReadOnlyList&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L240)

<a id="Vercel_AI_Operations_OperationRetryException_LastError_"></a>
<a id="Vercel_AI_Operations_OperationRetryException_LastError"></a>

### LastError


```csharp
public Exception? LastError { get; }
```


The last attempt's error.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L243)

<a id="Vercel_AI_Operations_OperationRetryException_Reason_"></a>
<a id="Vercel_AI_Operations_OperationRetryException_Reason"></a>

### Reason


```csharp
public string Reason { get; }
```


`maxRetriesExceeded` or `errorNotRetryable`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L237)
