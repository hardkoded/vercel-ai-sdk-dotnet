---
title: "FetchErrors"
description: "Turns transport failures into retryable ApiCallErrors. Maps to handleFetchError."
editUrl: false
prev: false
next: false
sidebar:
  label: "FetchErrors"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_FetchErrors"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class FetchErrors
```


Turns transport failures into retryable [ApiCallError](/api/Vercel.AI.Util/ApiCallError/)s. Maps to `handleFetchError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/HandleFetchError.cs#L12)

## Methods

<a id="Vercel_AI_ProviderUtils_FetchErrors_HandleFetchError_"></a>
<a id="Vercel_AI_ProviderUtils_FetchErrors_HandleFetchError_System_Exception_System_String_System_Object_"></a>

### HandleFetchError(Exception, string, object)


```csharp
public static Exception HandleFetchError(Exception error, string url, object requestBodyValues)
```


Returns abort errors unchanged. An [HttpRequestException](https://learn.microsoft.com/dotnet/api/system.net.http.httprequestexception) with an inner cause, or any error caused by a
connection-level [SocketException](https://learn.microsoft.com/dotnet/api/system.net.sockets.socketexception), becomes a retryable [ApiCallError](/api/Vercel.AI.Util/ApiCallError/). Other errors are returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/HandleFetchError.cs#L29)
