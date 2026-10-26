---
title: "ResponseHandlers"
description: "Response handlers for binary, event stream, and error responses. Maps to createBinaryResponseHandler, createBinaryStreamResponseHandler, createEventSourceRes..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResponseHandlers"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ResponseHandlers"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ResponseHandlers
```


Response handlers for binary, event stream, and error responses.
Maps to `createBinaryResponseHandler`, `createBinaryStreamResponseHandler`,
`createEventSourceResponseHandler`, `createJsonErrorResponseHandler`,
and `createStatusCodeErrorResponseHandler`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L36)

## Methods

<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateBinaryResponseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateBinaryResponseAsync_System_Net_Http_HttpResponseMessage_System_String_System_Object_System_Threading_CancellationToken_"></a>

### CreateBinaryResponseAsync(HttpResponseMessage, string, object?, CancellationToken)


```csharp
public static Task<ResponseHandlerResult<byte[]>> CreateBinaryResponseAsync(HttpResponseMessage response, string url, object? requestBodyValues = null, CancellationToken cancellationToken = default)
```


Reads the body as bytes. Maps to `createBinaryResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ResponseHandlerResult&lt;byte\[\]&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [ApiException](/api/Vercel.AI.Provider/ApiException/) | The response has no body. |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The body is larger than [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes). |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L41)

<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateBinaryStreamResponseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateBinaryStreamResponseAsync_System_Net_Http_HttpResponseMessage_"></a>

### CreateBinaryStreamResponseAsync(HttpResponseMessage)


```csharp
public static Task<ResponseHandlerResult<Stream>> CreateBinaryStreamResponseAsync(HttpResponseMessage response)
```


Passes the body through as a stream. Maps to `createBinaryStreamResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |

**Returns:** [Task&lt;ResponseHandlerResult&lt;Stream&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) | The response has no body. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L69)

<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateEventSourceResponseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateEventSourceResponseAsync_System_Net_Http_HttpResponseMessage_System_String_System_Object_System_Text_Json_Nodes_JsonNode_System_Threading_CancellationToken_"></a>

### CreateEventSourceResponseAsync(HttpResponseMessage, string, object?, JsonNode?, CancellationToken)


```csharp
public static Task<ResponseHandlerResult<IAsyncEnumerable<JsonParseResult>>> CreateEventSourceResponseAsync(HttpResponseMessage response, string url, object? requestBodyValues, JsonNode? schema, CancellationToken cancellationToken = default)
```


Parses a Server-Sent Event body into JSON values. A read failure on the successful response is
rethrown as a retryable [ApiException](/api/Vercel.AI.Provider/ApiException/) that keeps the status, URL, headers, and cause.
Maps to `createEventSourceResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `schema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ResponseHandlerResult&lt;IAsyncEnumerable&lt;JsonParseResult&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) | The response has no body. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L91)

<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateJsonErrorResponseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateJsonErrorResponseAsync_System_Net_Http_HttpResponseMessage_System_String_System_Object_System_Text_Json_Nodes_JsonNode_System_Func_System_Text_Json_JsonElement_System_String__System_Threading_CancellationToken_"></a>

### `CreateJsonErrorResponseAsync(HttpResponseMessage, string, object?, JsonNode?, Func<JsonElement, string>, CancellationToken)`


```csharp
public static Task<ApiException> CreateJsonErrorResponseAsync(HttpResponseMessage response, string url, object? requestBodyValues, JsonNode? errorSchema, Func<JsonElement, string> errorToMessage, CancellationToken cancellationToken = default)
```


Builds an [ApiException](/api/Vercel.AI.Provider/ApiException/) from an error response whose body is JSON.
The message comes from &lt;code class="paramref"&gt;errorToMessage&lt;/code&gt;. When the body is not valid for the schema, the status text is used.
Maps to `createJsonErrorResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `errorSchema` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |
| `errorToMessage` | [Func&lt;JsonElement, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ApiException&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The body is larger than [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes). |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L121)

<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateStatusCodeErrorResponseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseHandlers_CreateStatusCodeErrorResponseAsync_System_Net_Http_HttpResponseMessage_System_String_System_Object_System_Threading_CancellationToken_"></a>

### CreateStatusCodeErrorResponseAsync(HttpResponseMessage, string, object?, CancellationToken)


```csharp
public static Task<ApiException> CreateStatusCodeErrorResponseAsync(HttpResponseMessage response, string url, object? requestBodyValues = null, CancellationToken cancellationToken = default)
```


Builds an [ApiException](/api/Vercel.AI.Provider/ApiException/) whose message is the status text.
Maps to `createStatusCodeErrorResponseHandler`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ApiException&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The body is larger than [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes). |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseHandlers.cs#L162)
