---
title: "GatewayForbiddenError"
description: "The request was rejected by policy."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayForbiddenError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayForbiddenError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayForbiddenError : GatewayError, ISerializable
```


The request was rejected by policy.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L204)

## Constructors

<a id="Vercel_AI_Gateway_GatewayForbiddenError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayForbiddenError__ctor_System_String_System_Int32_System_Exception_System_String_System_String_"></a>

### GatewayForbiddenError(string?, int, Exception?, string?, string?)


```csharp
public GatewayForbiddenError(string? message = null, int statusCode = 403, Exception? cause = null, string? generationId = null, string? ruleId = null)
```


Creates a forbidden error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `ruleId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L207)

## Properties

<a id="Vercel_AI_Gateway_GatewayForbiddenError_RuleId_"></a>
<a id="Vercel_AI_Gateway_GatewayForbiddenError_RuleId"></a>

### RuleId


```csharp
public string? RuleId { get; }
```


Routing rule id, when `param.ruleId` was a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L217)

<a id="Vercel_AI_Gateway_GatewayForbiddenError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayForbiddenError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L214)

## Methods

<a id="Vercel_AI_Gateway_GatewayForbiddenError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayForbiddenError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L220)
