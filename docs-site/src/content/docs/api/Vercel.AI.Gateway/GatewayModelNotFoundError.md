---
title: "GatewayModelNotFoundError"
description: "The requested model is not available."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayModelNotFoundError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayModelNotFoundError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayModelNotFoundError : GatewayError, ISerializable
```


The requested model is not available.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L124)

## Constructors

<a id="Vercel_AI_Gateway_GatewayModelNotFoundError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayModelNotFoundError__ctor_System_String_System_Int32_System_String_System_Exception_System_String_"></a>

### GatewayModelNotFoundError(string?, int, string?, Exception?, string?)


```csharp
public GatewayModelNotFoundError(string? message = null, int statusCode = 404, string? modelId = null, Exception? cause = null, string? generationId = null)
```


Creates a model-not-found error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L127)

## Properties

<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; }
```


Model id from the error `param`, when it was a `{ modelId }` object.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L137)

<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L134)

## Methods

<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayModelNotFoundError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L140)
