---
title: "GatewayAuthenticationError"
description: "Authentication failed."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayAuthenticationError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayAuthenticationError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayAuthenticationError : GatewayError, ISerializable
```


Authentication failed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L47)

## Constructors

<a id="Vercel_AI_Gateway_GatewayAuthenticationError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayAuthenticationError__ctor_System_String_System_Int32_System_Exception_System_String_"></a>

### GatewayAuthenticationError(string?, int, Exception?, string?)


```csharp
public GatewayAuthenticationError(string? message = null, int statusCode = 401, Exception? cause = null, string? generationId = null)
```


Creates an authentication error. The default message is `Authentication failed`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L50)

## Properties

<a id="Vercel_AI_Gateway_GatewayAuthenticationError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayAuthenticationError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L56)

## Methods

<a id="Vercel_AI_Gateway_GatewayAuthenticationError_CreateContextual_"></a>
<a id="Vercel_AI_Gateway_GatewayAuthenticationError_CreateContextual_System_Boolean_System_Boolean_System_Int32_System_Exception_System_String_"></a>

### CreateContextual(bool, bool, int, Exception?, string?)


```csharp
public static GatewayAuthenticationError CreateContextual(bool apiKeyProvided, bool oidcTokenProvided, int statusCode = 401, Exception? cause = null, string? generationId = null)
```


Builds the message for an API key, an OIDC token, or missing credentials.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `apiKeyProvided` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `oidcTokenProvided` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayAuthenticationError](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L65)

<a id="Vercel_AI_Gateway_GatewayAuthenticationError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayAuthenticationError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L59)
