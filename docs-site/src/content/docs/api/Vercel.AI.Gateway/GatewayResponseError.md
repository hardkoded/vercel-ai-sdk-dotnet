---
title: "GatewayResponseError"
description: "The Gateway error body could not be read."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayResponseError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayResponseError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayResponseError : GatewayError, ISerializable
```


The Gateway error body could not be read.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/) → [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L227)

## Constructors

<a id="Vercel_AI_Gateway_GatewayResponseError__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayResponseError__ctor_System_String_System_Int32_System_Nullable_System_Text_Json_JsonElement__System_String_System_Exception_System_String_System_Nullable_System_Boolean__"></a>

### GatewayResponseError(string?, int, JsonElement?, string?, Exception?, string?, bool?)


```csharp
public GatewayResponseError(string? message = null, int statusCode = 502, JsonElement? response = null, string? validationError = null, Exception? cause = null, string? generationId = null, bool? isRetryable = null)
```


Creates a response error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `response` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `validationError` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `generationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L230)

## Properties

<a id="Vercel_AI_Gateway_GatewayResponseError_Response_"></a>
<a id="Vercel_AI_Gateway_GatewayResponseError_Response"></a>

### Response


```csharp
public JsonElement? Response { get; }
```


Raw response body.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L241)

<a id="Vercel_AI_Gateway_GatewayResponseError_Type_"></a>
<a id="Vercel_AI_Gateway_GatewayResponseError_Type"></a>

### Type


```csharp
public override string Type { get; }
```


Gateway error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L238)

<a id="Vercel_AI_Gateway_GatewayResponseError_ValidationError_"></a>
<a id="Vercel_AI_Gateway_GatewayResponseError_ValidationError"></a>

### ValidationError


```csharp
public string? ValidationError { get; }
```


Why the body was rejected.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L244)

## Methods

<a id="Vercel_AI_Gateway_GatewayResponseError_IsInstance_"></a>
<a id="Vercel_AI_Gateway_GatewayResponseError_IsInstance_System_Exception_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L247)
