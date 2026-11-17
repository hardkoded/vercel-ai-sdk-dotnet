---
title: "GatewayToolError"
description: "Error returned as the output of a Gateway provider-executed tool instead of a result. Maps to GatewayToolError."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayToolError"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayToolError"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayToolError
```


Error returned as the output of a Gateway provider-executed tool instead of a result. Maps to `GatewayToolError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L13)

## Properties

<a id="Vercel_AI_Gateway_GatewayToolError_AdditionalProperties_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_AdditionalProperties"></a>

### AdditionalProperties


```csharp
[JsonExtensionData]
public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
```


Fields the schema does not declare.

**Value:** [IDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L42)

<a id="Vercel_AI_Gateway_GatewayToolError_Error_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_Error"></a>

### Error


```csharp
[JsonPropertyName("error")]
public string Error { get; set; }
```


Error code: `api_error`, `configuration_error`, `execution_error`, `gateway_rate_limit`,
`invalid_input`, `rate_limit`, `timeout`, `unknown` or `unknown_tool`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L19)

<a id="Vercel_AI_Gateway_GatewayToolError_Message_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_Message"></a>

### Message


```csharp
[JsonPropertyName("message")]
public string Message { get; set; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L23)

<a id="Vercel_AI_Gateway_GatewayToolError_RetryAfter_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_RetryAfter"></a>

### RetryAfter


```csharp
[JsonPropertyName("retryAfter")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? RetryAfter { get; set; }
```


Retry-After value of a rate limit response.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L32)

<a id="Vercel_AI_Gateway_GatewayToolError_StatusCode_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_StatusCode"></a>

### StatusCode


```csharp
[JsonPropertyName("statusCode")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public double? StatusCode { get; set; }
```


HTTP status code, when the error came from an HTTP response.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L27)

<a id="Vercel_AI_Gateway_GatewayToolError_StatusText_"></a>
<a id="Vercel_AI_Gateway_GatewayToolError_StatusText"></a>

### StatusText


```csharp
[JsonPropertyName("statusText")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? StatusText { get; set; }
```


HTTP status text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayToolError.cs#L37)
