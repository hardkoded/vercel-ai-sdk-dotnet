---
title: "GatewayFailedDependencyError"
description: "A dependency required by the Gateway request was unavailable. Status 424 is not retried."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayFailedDependencyError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayFailedDependencyError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayFailedDependencyError : GatewayError, ISerializable
```


A dependency required by the Gateway request was unavailable. Status 424 is not retried.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L185)

## Constructors

<a id="Vercel_AI_Gateway_GatewayFailedDependencyError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayFailedDependencyError__ctor_System_String_System_Int32_System_Exception_System_String_"></a>

### GatewayFailedDependencyError(string?, int, Exception?, string?)


```csharp
public GatewayFailedDependencyError(string? message = null, int statusCode = 424, Exception? cause = null, string? generationId = null)
```


Creates a failed-dependency error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L188)

## Properties

<a id="Vercel_AI_Gateway_GatewayFailedDependencyError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayFailedDependencyError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L194)

## Methods

<a id="Vercel_AI_Gateway_GatewayFailedDependencyError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayFailedDependencyError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L197)
