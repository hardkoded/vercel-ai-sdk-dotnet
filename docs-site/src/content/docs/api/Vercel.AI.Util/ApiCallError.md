---
title: "ApiCallError"
description: "A failed provider HTTP call, including retry metadata and response headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiCallError"
  hidden: true
---

<a id="Vercel_AI_Util_ApiCallError"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public class ApiCallError : AiSdkError, ISerializable, IErrorMarkers
```


A failed provider HTTP call, including retry metadata and response headers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkError](/api/Vercel.AI.Util/AiSdkError/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable), [IErrorMarkers](/api/Vercel.AI.Util/IErrorMarkers/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L8)

## Constructors

<a id="Vercel_AI_Util_ApiCallError__ctor_"></a>
<a id="Vercel_AI_Util_ApiCallError__ctor_System_String_System_String_System_Object_System_Nullable_System_Int32__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_String_System_Exception_System_Nullable_System_Boolean__System_Object_"></a>

### `ApiCallError(string, string, object, int?, IReadOnlyDictionary<string, string>?, string?, Exception?, bool?, object?)`


```csharp
public ApiCallError(string message, string url, object requestBodyValues, int? statusCode = null, IReadOnlyDictionary<string, string>? responseHeaders = null, string? responseBody = null, Exception? cause = null, bool? isRetryable = null, object? data = null)
```


Creates the error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `isRetryable` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L14)

## Fields

<a id="Vercel_AI_Util_ApiCallError_ErrorMarker"></a>

### ErrorMarker


```csharp
public const string ErrorMarker = "vercel.ai.error.AI_APICallError"
```


Marker for [IsInstance(object)](#Vercel_AI_Util_ApiCallError_IsInstance_System_Object_).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L11)

## Properties

<a id="Vercel_AI_Util_ApiCallError_Data_"></a>
<a id="Vercel_AI_Util_ApiCallError_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Parsed provider payload.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L55)

<a id="Vercel_AI_Util_ApiCallError_IsRetryable_"></a>
<a id="Vercel_AI_Util_ApiCallError_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; }
```


Whether the call may be retried.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L52)

<a id="Vercel_AI_Util_ApiCallError_RequestBodyValues_"></a>
<a id="Vercel_AI_Util_ApiCallError_RequestBodyValues"></a>

### RequestBodyValues


```csharp
public object RequestBodyValues { get; }
```


Request body that was sent.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L40)

<a id="Vercel_AI_Util_ApiCallError_ResponseBody_"></a>
<a id="Vercel_AI_Util_ApiCallError_ResponseBody"></a>

### ResponseBody


```csharp
public string? ResponseBody { get; }
```


Raw response body.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L49)

<a id="Vercel_AI_Util_ApiCallError_ResponseHeaders_"></a>
<a id="Vercel_AI_Util_ApiCallError_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }
```


Response headers. Lookup is case-insensitive.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L46)

<a id="Vercel_AI_Util_ApiCallError_StatusCode_"></a>
<a id="Vercel_AI_Util_ApiCallError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status, when the provider responded.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L43)

<a id="Vercel_AI_Util_ApiCallError_Url_"></a>
<a id="Vercel_AI_Util_ApiCallError_Url"></a>

### Url


```csharp
public string Url { get; }
```


Request URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L37)

## Methods

<a id="Vercel_AI_Util_ApiCallError_GetHeader_"></a>
<a id="Vercel_AI_Util_ApiCallError_GetHeader_System_String_"></a>

### GetHeader(string?)


```csharp
public string? GetHeader(string? name)
```


Reads a response header, ignoring case.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L64)

<a id="Vercel_AI_Util_ApiCallError_IsInstance_"></a>
<a id="Vercel_AI_Util_ApiCallError_IsInstance_System_Object_"></a>

### IsInstance(object?)


```csharp
public static bool IsInstance(object? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; carries this error's marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/ApiCallError.cs#L58)
