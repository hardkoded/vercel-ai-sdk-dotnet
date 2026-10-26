---
title: "GatewayTimeoutError"
description: "The client timed out before the Gateway responded."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayTimeoutError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayTimeoutError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayTimeoutError : GatewayError, ISerializable
```


The client timed out before the Gateway responded.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L254)

## Constructors

<a id="Vercel_AI_Gateway_GatewayTimeoutError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayTimeoutError__ctor_System_String_System_Int32_System_Exception_System_String_"></a>

### GatewayTimeoutError(string?, int, Exception?, string?)


```csharp
public GatewayTimeoutError(string? message = null, int statusCode = 408, Exception? cause = null, string? generationId = null)
```


Creates a timeout error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L257)

## Properties

<a id="Vercel_AI_Gateway_GatewayTimeoutError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayTimeoutError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L263)

## Methods

<a id="Vercel_AI_Gateway_GatewayTimeoutError_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayTimeoutError_Create_System_String_System_Exception_System_Int32_System_String_"></a>

### Create(string, Exception?, int, string?)


```csharp
public static GatewayTimeoutError Create(string originalMessage, Exception? cause = null, int statusCode = 408, string? generationId = null)
```


Wraps a client timeout with the Gateway troubleshooting text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `originalMessage` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayTimeoutError](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L272)

<a id="Vercel_AI_Gateway_GatewayTimeoutError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayTimeoutError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L266)
