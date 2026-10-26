---
title: "GatewayErrors"
description: "Parses Gateway auth headers and error bodies."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayErrors"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayErrors"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayErrors
```


Parses Gateway auth headers and error bodies.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L280)

## Fields

<a id="Vercel_AI_Gateway_GatewayErrors_AuthMethodHeader"></a>

### AuthMethodHeader


```csharp
public const string AuthMethodHeader = "ai-gateway-auth-method"
```


Header that carries `api-key` or `oidc`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L285)

<a id="Vercel_AI_Gateway_GatewayErrors_TeamHeader"></a>

### TeamHeader


```csharp
public const string TeamHeader = "x-vercel-ai-gateway-team"
```


Header that carries the Vercel AI Gateway team.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L288)

## Methods

<a id="Vercel_AI_Gateway_GatewayErrors_AsGatewayError_"></a>
<a id="Vercel_AI_Gateway_GatewayErrors_AsGatewayError_System_Exception_System_String_"></a>

### AsGatewayError(Exception?, string?)


```csharp
public static GatewayError AsGatewayError(Exception? error, string? authMethod = null)
```


Maps any failure to a Gateway error. Gateway errors pass through, transport timeouts become [GatewayTimeoutError](/api/Vercel.AI.Gateway/GatewayTimeoutError/),
and API call errors are read as Gateway error bodies. Maps to `asGatewayError`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `authMethod` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L310)

<a id="Vercel_AI_Gateway_GatewayErrors_Create_"></a>
<a id="Vercel_AI_Gateway_GatewayErrors_Create_System_Text_Json_JsonElement_System_Int32_System_String_System_String_System_Exception_System_Nullable_System_Boolean__"></a>

### Create(JsonElement, int, string?, string, Exception?, bool?)


```csharp
public static GatewayError Create(JsonElement response, int statusCode, string? authMethod = null, string defaultMessage = "Gateway request failed", Exception? cause = null, bool? isRetryable = null)
```


Maps a Gateway JSON body to a typed error. Invalid bodies become [GatewayResponseError](/api/Vercel.AI.Gateway/GatewayResponseError/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `authMethod` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `defaultMessage` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L373)

<a id="Vercel_AI_Gateway_GatewayErrors_ExtractApiCallResponse_"></a>
<a id="Vercel_AI_Gateway_GatewayErrors_ExtractApiCallResponse_Vercel_AI_Util_ApiCallError_"></a>

### ExtractApiCallResponse(ApiCallError)


```csharp
public static JsonElement ExtractApiCallResponse(ApiCallError error)
```


Returns [Data](/api/Vercel.AI.Util/ApiCallError/#Vercel_AI_Util_ApiCallError_Data) when set, else the response body parsed as JSON, else the raw body as a JSON string.
A missing body yields an empty object. Maps to `extractApiCallResponse`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [ApiCallError](/api/Vercel.AI.Util/ApiCallError/) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L345)

<a id="Vercel_AI_Gateway_GatewayErrors_FromResponseBody_"></a>
<a id="Vercel_AI_Gateway_GatewayErrors_FromResponseBody_System_String_System_Int32_System_String_System_Exception_"></a>

### FromResponseBody(string?, int, string?, Exception?)


```csharp
public static GatewayError FromResponseBody(string? body, int statusCode, string? authMethod, Exception? cause)
```


Maps an HTTP error body. Non-JSON bodies become [GatewayResponseError](/api/Vercel.AI.Gateway/GatewayResponseError/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `authMethod` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [GatewayError](/api/Vercel.AI.Gateway/GatewayError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L410)

<a id="Vercel_AI_Gateway_GatewayErrors_ParseAuthMethod_"></a>
<a id="Vercel_AI_Gateway_GatewayErrors_ParseAuthMethod_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `ParseAuthMethod(IReadOnlyDictionary<string, string?>?)`


```csharp
public static string? ParseAuthMethod(IReadOnlyDictionary<string, string?>? headers)
```


Reads [AuthMethodHeader](#Vercel_AI_Gateway_GatewayErrors_AuthMethodHeader). Any other value, including a different case, is ignored.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayErrors.cs#L291)
