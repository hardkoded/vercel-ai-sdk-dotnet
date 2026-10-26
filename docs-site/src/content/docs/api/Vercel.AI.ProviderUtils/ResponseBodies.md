---
title: "ResponseBodies"
description: "Reads and releases HTTP response bodies."
editUrl: false
prev: false
next: false
sidebar:
  label: "ResponseBodies"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ResponseBodies"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ResponseBodies
```


Reads and releases HTTP response bodies.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseBodies.cs#L11)

## Methods

<a id="Vercel_AI_ProviderUtils_ResponseBodies_CancelResponseBody_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseBodies_CancelResponseBody_System_Net_Http_HttpResponseMessage_"></a>

### CancelResponseBody(HttpResponseMessage)


```csharp
public static void CancelResponseBody(HttpResponseMessage response)
```


Releases the response body so the connection can be reused. Errors are swallowed so they do not hide the original failure.
Maps to `cancelResponseBody`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseBodies.cs#L17)

<a id="Vercel_AI_ProviderUtils_ResponseBodies_ReadResponseWithSizeLimitAsync_"></a>
<a id="Vercel_AI_ProviderUtils_ResponseBodies_ReadResponseWithSizeLimitAsync_System_Net_Http_HttpResponseMessage_System_String_System_Nullable_System_Int64__System_Threading_CancellationToken_"></a>

### ReadResponseWithSizeLimitAsync(HttpResponseMessage, string, long?, CancellationToken)


```csharp
public static Task<byte[]> ReadResponseWithSizeLimitAsync(HttpResponseMessage response, string url, long? maxBytes = null, CancellationToken cancellationToken = default)
```


Reads the body, rejecting a declared Content-Length or a streamed size above &lt;code class="paramref"&gt;maxBytes&lt;/code&gt;.
The default limit is [DefaultMaxBytes](/api/Vercel.AI.Util/Download/#Vercel_AI_Util_Download_DefaultMaxBytes). Maps to `readResponseWithSizeLimit`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `response` | [HttpResponseMessage](https://learn.microsoft.com/dotnet/api/system.net.http.httpresponsemessage) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxBytes` | [long?](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;byte\[\]&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

**Exceptions**

| Type | Condition |
| --- | --- |
| [DownloadError](/api/Vercel.AI.Util/DownloadError/) | The body is larger than the limit. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ResponseBodies.cs#L34)
