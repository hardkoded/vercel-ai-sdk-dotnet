---
title: "ApiException"
description: "The provider returned a non-success HTTP status after retries."
editUrl: false
prev: false
next: false
sidebar:
  label: "ApiException"
  hidden: true
---

<a id="Vercel_AI_Provider_ApiException"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class ApiException : AiSdkException, ISerializable
```


The provider returned a non-success HTTP status after retries.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

**Derived:** [AuthenticationException](/api/Vercel.AI.Provider/AuthenticationException/), [BadRequestException](/api/Vercel.AI.Provider/BadRequestException/), [InternalServerException](/api/Vercel.AI.Provider/InternalServerException/), [NotFoundException](/api/Vercel.AI.Provider/NotFoundException/), [PermissionDeniedException](/api/Vercel.AI.Provider/PermissionDeniedException/), [RateLimitException](/api/Vercel.AI.Provider/RateLimitException/), [UnprocessableEntityException](/api/Vercel.AI.Provider/UnprocessableEntityException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L24)

## Constructors

<a id="Vercel_AI_Provider_ApiException__ctor_"></a>
<a id="Vercel_AI_Provider_ApiException__ctor_System_String_System_Int32_System_String_"></a>

### ApiException(string, int, string?)


```csharp
public ApiException(string message, int statusCode, string? responseBody)
```


Creates an API exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L27)

<a id="Vercel_AI_Provider_ApiException__ctor_System_String_System_Int32_System_String_System_Exception_"></a>

### ApiException(string, int, string?, Exception)


```csharp
public ApiException(string message, int statusCode, string? responseBody, Exception innerException)
```


Creates an API exception caused by &lt;code class="paramref"&gt;innerException&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L35)

## Properties

<a id="Vercel_AI_Provider_ApiException_ErrorData_"></a>
<a id="Vercel_AI_Provider_ApiException_ErrorData"></a>

### ErrorData


```csharp
public object? ErrorData { get; init; }
```


The provider error parsed from the response body, when the handler could parse it.
Maps to `APICallError.data`. Named `ErrorData` because [Data](https://learn.microsoft.com/dotnet/api/system.exception.data) already exists.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L61)

<a id="Vercel_AI_Provider_ApiException_IsRetryable_"></a>
<a id="Vercel_AI_Provider_ApiException_IsRetryable"></a>

### IsRetryable


```csharp
public bool IsRetryable { get; init; }
```


True when the call can be retried. Defaults to 408, 429, and 500 and above.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L64)

<a id="Vercel_AI_Provider_ApiException_RequestBodyValues_"></a>
<a id="Vercel_AI_Provider_ApiException_RequestBodyValues"></a>

### RequestBodyValues


```csharp
public object? RequestBodyValues { get; init; }
```


Request body values the call was made with. Set by the response handlers.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L52)

<a id="Vercel_AI_Provider_ApiException_ResponseBody_"></a>
<a id="Vercel_AI_Provider_ApiException_ResponseBody"></a>

### ResponseBody


```csharp
public string? ResponseBody { get; }
```


Raw response body, when the provider sent one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L46)

<a id="Vercel_AI_Provider_ApiException_ResponseHeaders_"></a>
<a id="Vercel_AI_Provider_ApiException_ResponseHeaders"></a>

### ResponseHeaders


```csharp
public IReadOnlyDictionary<string, string>? ResponseHeaders { get; init; }
```


Response headers, when the handler had them.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L55)

<a id="Vercel_AI_Provider_ApiException_StatusCode_"></a>
<a id="Vercel_AI_Provider_ApiException_StatusCode"></a>

### StatusCode


```csharp
public int StatusCode { get; }
```


HTTP status code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L43)

<a id="Vercel_AI_Provider_ApiException_Url_"></a>
<a id="Vercel_AI_Provider_ApiException_Url"></a>

### Url


```csharp
public string? Url { get; init; }
```


URL of the failed request. Set by the response handlers.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/Errors.cs#L49)
