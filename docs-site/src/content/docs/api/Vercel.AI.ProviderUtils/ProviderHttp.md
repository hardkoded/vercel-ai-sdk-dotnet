---
title: "ProviderHttp"
description: "Sends JSON and SSE requests with the SDK retry policy."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderHttp"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderHttp"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ProviderHttp
```


Sends JSON and SSE requests with the SDK retry policy.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L14)

## Constructors

<a id="Vercel_AI_ProviderUtils_ProviderHttp__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp__ctor_System_Net_Http_HttpClient_Vercel_AI_ProviderUtils_RetryPolicy_System_Nullable_System_TimeSpan__"></a>

### ProviderHttp(HttpClient, RetryPolicy?, TimeSpan?)


```csharp
public ProviderHttp(HttpClient httpClient, RetryPolicy? retry = null, TimeSpan? timeout = null)
```


Creates a provider HTTP client.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `retry` | [RetryPolicy](/api/Vercel.AI.ProviderUtils/RetryPolicy/) |  |
| `timeout` | [TimeSpan?](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L21)

## Methods

<a id="Vercel_AI_ProviderUtils_ProviderHttp_IsRetryable_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_IsRetryable_System_Int32_"></a>

### IsRetryable(int)


```csharp
public static bool IsRetryable(int statusCode)
```


True for 408, 429, and 500–599.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L322)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_MapStatus_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_MapStatus_System_Int32_System_String_"></a>

### MapStatus(int, string?)


```csharp
public static ApiException MapStatus(int statusCode, string? body)
```


Maps an HTTP status onto the SDK exception hierarchy.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L294)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_MapStatus_System_Int32_System_String_System_String_"></a>

### MapStatus(int, string, string?)


```csharp
public static ApiException MapStatus(int statusCode, string message, string? body)
```


Maps an HTTP status onto the SDK exception hierarchy with a provider-specific &lt;code class="paramref"&gt;message&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L300)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenJsonLinesAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenJsonLinesAsync_System_Net_Http_HttpMethod_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### `OpenJsonLinesAsync(HttpMethod, Uri, string?, IReadOnlyDictionary<string, string?>?, int?, CancellationToken)`


```csharp
public Task<IAsyncEnumerable<JsonElement>> OpenJsonLinesAsync(HttpMethod method, Uri uri, string? jsonBody, IReadOnlyDictionary<string, string?>? headers, int? maxLineBytes, CancellationToken cancellationToken)
```


Sends a request and returns once the response status is known. Failed responses throw here.
The returned sequence reads one JSON value per line of the body. Enumerate it once: it disposes the response when it ends.
A row above &lt;code class="paramref"&gt;maxLineBytes&lt;/code&gt; UTF-8 bytes throws a [DownloadError](/api/Vercel.AI.Util/DownloadError/) and cancels the download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [HttpMethod](https://learn.microsoft.com/dotnet/api/system.net.http.httpmethod) |  |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `maxLineBytes` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IAsyncEnumerable&lt;JsonElement&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L243)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenSseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenSseAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `OpenSseAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<IAsyncEnumerable<string>> OpenSseAsync(Uri uri, string jsonBody, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and returns once the response status is known. A failed response throws here.
The returned sequence yields SSE data payloads as they arrive. Enumerate it once: it disposes the response when it ends or the consumer stops.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IAsyncEnumerable&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L142)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenSseWithResourceAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_OpenSseWithResourceAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `OpenSseWithResourceAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<(IAsyncEnumerable<string> Events, IDisposable Resource)> OpenSseWithResourceAsync(Uri uri, string jsonBody, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Like `CancellationToken)`, and also returns the response so a caller that never enumerates the events can release it.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;(IAsyncEnumerable&lt;string&gt; Events, IDisposable Resource)&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L154)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendBytesAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendBytesAsync_System_Net_Http_HttpMethod_System_Uri_System_Net_Http_HttpContent_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendBytesAsync(HttpMethod, Uri, HttpContent, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<byte[]> SendBytesAsync(HttpMethod method, Uri uri, HttpContent content, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Sends a request and returns the raw bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [HttpMethod](https://learn.microsoft.com/dotnet/api/system.net.http.httpmethod) |  |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `content` | [HttpContent](https://learn.microsoft.com/dotnet/api/system.net.http.httpcontent) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L77)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonAsync_System_Net_Http_HttpMethod_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendJsonAsync(HttpMethod, Uri, string?, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<JsonDocument> SendJsonAsync(HttpMethod method, Uri uri, string? jsonBody, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and returns the parsed body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [HttpMethod](https://learn.microsoft.com/dotnet/api/system.net.http.httpmethod) |  |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;JsonDocument&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L29)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonLinesAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonLinesAsync_System_Net_Http_HttpMethod_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### `SendJsonLinesAsync(HttpMethod, Uri, string?, IReadOnlyDictionary<string, string?>?, int?, CancellationToken)`


```csharp
public IAsyncEnumerable<JsonElement> SendJsonLinesAsync(HttpMethod method, Uri uri, string? jsonBody, IReadOnlyDictionary<string, string?>? headers, int? maxLineBytes, CancellationToken cancellationToken)
```


Sends a request and yields one JSON value per line of the response body.
A row above &lt;code class="paramref"&gt;maxLineBytes&lt;/code&gt; UTF-8 bytes throws a [DownloadError](/api/Vercel.AI.Util/DownloadError/) and cancels the download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [HttpMethod](https://learn.microsoft.com/dotnet/api/system.net.http.httpmethod) |  |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `maxLineBytes` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L223)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonStringAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendJsonStringAsync_System_Net_Http_HttpMethod_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendJsonStringAsync(HttpMethod, Uri, string?, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public Task<ProviderTextResponse> SendJsonStringAsync(HttpMethod method, Uri uri, string? jsonBody, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and returns the response body text and headers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [HttpMethod](https://learn.microsoft.com/dotnet/api/system.net.http.httpmethod) |  |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;ProviderTextResponse&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L52)

<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendSseAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderHttp_SendSseAsync_System_Uri_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SendSseAsync(Uri, string, IReadOnlyDictionary<string, string?>?, CancellationToken)`


```csharp
public IAsyncEnumerable<string> SendSseAsync(Uri uri, string jsonBody, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken)
```


Posts JSON and yields SSE data payloads.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `uri` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `jsonBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [IAsyncEnumerable&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderHttp.cs#L125)
