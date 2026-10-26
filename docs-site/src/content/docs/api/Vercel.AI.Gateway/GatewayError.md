---
title: "GatewayError"
description: "Base class for AI Gateway errors. The message includes [generationId] when one was returned."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public abstract class GatewayError : AiSdkException, ISerializable
```


Base class for AI Gateway errors. The message includes `[generationId]` when one was returned.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

**Derived:** [GatewayAuthenticationError](/api/Vercel.AI.Gateway/GatewayAuthenticationError/), [GatewayFailedDependencyError](/api/Vercel.AI.Gateway/GatewayFailedDependencyError/), [GatewayForbiddenError](/api/Vercel.AI.Gateway/GatewayForbiddenError/), [GatewayInternalServerError](/api/Vercel.AI.Gateway/GatewayInternalServerError/), [GatewayInvalidRequestError](/api/Vercel.AI.Gateway/GatewayInvalidRequestError/), [GatewayModelNotFoundError](/api/Vercel.AI.Gateway/GatewayModelNotFoundError/), [GatewayNotFoundError](/api/Vercel.AI.Gateway/GatewayNotFoundError/), [GatewayRateLimitError](/api/Vercel.AI.Gateway/GatewayRateLimitError/), [GatewayResponseError](/api/Vercel.AI.Gateway/GatewayResponseError/), [GatewayTimeoutError](/api/Vercel.AI.Gateway/GatewayTimeoutError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L12)

## Constructors

<a id="Vercel_AI_Gateway_GatewayError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayError__ctor_System_String_System_Int32_System_Exception_System_String_System_Nullable_System_Boolean__"></a>

### GatewayError(string, int, Exception?, string?, bool?)


```csharp
protected GatewayError(string message, int statusCode, Exception? cause, string? generationId, bool? isRetryable)
```


Creates a Gateway error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L15)

## Properties

<a id="Vercel_AI_Gateway_GatewayError_Cause_"></a>
<a id="Vercel_AI_Gateway_GatewayError_Cause"></a>

### Cause


```csharp
public Exception? Cause { get; }
```


Underlying failure, when one was supplied.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L31)

<a id="Vercel_AI_Gateway_GatewayError_GenerationId_"></a>
<a id="Vercel_AI_Gateway_GatewayError_GenerationId"></a>

### GenerationId


```csharp
public string? GenerationId { get; }
```


Gateway generation id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L34)

<a id="Vercel_AI_Gateway_GatewayError_IsRetryable_"></a>
<a id="Vercel_AI_Gateway_GatewayError_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; }
```


Whether the caller should retry this status.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L37)

<a id="Vercel_AI_Gateway_GatewayError_StatusCode_"></a>
<a id="Vercel_AI_Gateway_GatewayError_StatusCode"></a>

### StatusCode


```csharp
public int StatusCode { get; }
```


HTTP status.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L28)

<a id="Vercel_AI_Gateway_GatewayError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayError_Type"></a>

### Type


```csharp
public abstract string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L25)

## Methods

<a id="Vercel_AI_Gateway_GatewayError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayError_IsInstance_System_Exception_"></a>

### IsInstance(Exception?)


```csharp
public static bool IsInstance(Exception? error)
```


True when &lt;code class="paramref"&gt;error&lt;/code&gt; is a Gateway error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L40)
