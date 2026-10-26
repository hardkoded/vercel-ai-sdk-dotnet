---
title: "GatewayInternalServerError"
description: "The Gateway failed internally."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayInternalServerError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayInternalServerError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayInternalServerError : GatewayError, ISerializable
```


The Gateway failed internally.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L166)

## Constructors

<a id="Vercel_AI_Gateway_GatewayInternalServerError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayInternalServerError__ctor_System_String_System_Int32_System_Exception_System_String_"></a>

### GatewayInternalServerError(string?, int, Exception?, string?)


```csharp
public GatewayInternalServerError(string? message = null, int statusCode = 500, Exception? cause = null, string? generationId = null)
```


Creates an internal-server error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L169)

## Properties

<a id="Vercel_AI_Gateway_GatewayInternalServerError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayInternalServerError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L175)

## Methods

<a id="Vercel_AI_Gateway_GatewayInternalServerError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayInternalServerError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L178)
