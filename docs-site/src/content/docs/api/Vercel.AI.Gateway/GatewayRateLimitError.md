---
title: "GatewayRateLimitError"
description: "A Gateway rate limit."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRateLimitError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRateLimitError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRateLimitError : GatewayError, ISerializable
```


A Gateway rate limit.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L105)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRateLimitError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRateLimitError__ctor_System_String_System_Int32_System_Exception_System_String_"></a>

### GatewayRateLimitError(string?, int, Exception?, string?)


```csharp
public GatewayRateLimitError(string? message = null, int statusCode = 429, Exception? cause = null, string? generationId = null)
```


Creates a rate-limit error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L108)

## Properties

<a id="Vercel_AI_Gateway_GatewayRateLimitError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayRateLimitError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L114)

## Methods

<a id="Vercel_AI_Gateway_GatewayRateLimitError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayRateLimitError_IsInstance_System_Exception_"></a>

### IsInstance(Exception?)


```csharp
public static bool IsInstance(Exception? error)
```


True when &lt;code class="paramref"&gt;error&lt;/code&gt; is this error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L117)
