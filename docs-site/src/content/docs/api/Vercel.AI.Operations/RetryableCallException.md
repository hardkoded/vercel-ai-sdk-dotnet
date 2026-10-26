---
title: "RetryableCallException"
description: "A provider HTTP failure that may be retried."
editUrl: false
prev: false
next: false
sidebar:
  label: "RetryableCallException"
  hidden: true
---

<a id="Vercel_AI_Operations_RetryableCallException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class RetryableCallException : AiSdkException, ISerializable
```


A provider HTTP failure that may be retried.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L203)

## Constructors

<a id="Vercel_AI_Operations_RetryableCallException__ctor_"></a>
<a id="Vercel_AI_Operations_RetryableCallException__ctor_System_String_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Boolean__"></a>

### `RetryableCallException(string, int?, IReadOnlyDictionary<string, string>?, bool?)`


```csharp
public RetryableCallException(string message, int? statusCode, IReadOnlyDictionary<string, string>? responseHeaders, bool? isRetryable = null)
```


Creates a retryable provider error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L206)

## Properties

<a id="Vercel_AI_Operations_RetryableCallException_IsRetryable_"></a>
<a id="Vercel_AI_Operations_RetryableCallException_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; }
```


Whether the operation retry loop should try again.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L221)

<a id="Vercel_AI_Operations_RetryableCallException_ResponseHeaders_"></a>
<a id="Vercel_AI_Operations_RetryableCallException_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }
```


Response headers, including `retry-after-ms` when present.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L218)

<a id="Vercel_AI_Operations_RetryableCallException_StatusCode_"></a>
<a id="Vercel_AI_Operations_RetryableCallException_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status, when the failure came from HTTP.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L215)
