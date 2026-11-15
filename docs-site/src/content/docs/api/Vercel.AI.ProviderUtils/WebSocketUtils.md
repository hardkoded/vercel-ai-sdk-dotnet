---
title: "WebSocketUtils"
description: "WebSocket helpers. Maps to provider-utils/src/websocket.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebSocketUtils"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_WebSocketUtils"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class WebSocketUtils
```


WebSocket helpers. Maps to `provider-utils/src/websocket.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L65)

## Methods

<a id="Vercel_AI_ProviderUtils_WebSocketUtils_GetWebSocketConstructor_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketUtils_GetWebSocketConstructor_Vercel_AI_ProviderUtils_WebSocketFactory_"></a>

### GetWebSocketConstructor(WebSocketFactory?)


```csharp
public static WebSocketFactory GetWebSocketConstructor(WebSocketFactory? webSocket)
```


Returns &lt;code class="paramref"&gt;webSocket&lt;/code&gt;, or the [ClientWebSocket](https://learn.microsoft.com/dotnet/api/system.net.websockets.clientwebsocket) factory.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `webSocket` | [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/) |  |

**Returns:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L70)

<a id="Vercel_AI_ProviderUtils_WebSocketUtils_ReadWebSocketMessageTextAsync_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketUtils_ReadWebSocketMessageTextAsync_System_Object_"></a>

### ReadWebSocketMessageTextAsync(object?)


```csharp
public static Task<string> ReadWebSocketMessageTextAsync(object? data)
```


Reads WebSocket message data as text, handling string and binary payloads.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [Task&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L103)

<a id="Vercel_AI_ProviderUtils_WebSocketUtils_ToWebSocketUrl_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketUtils_ToWebSocketUrl_System_String_"></a>

### ToWebSocketUrl(string)


```csharp
public static Uri ToWebSocketUrl(string url)
```


Converts an http(s) URL to the corresponding ws(s) URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L76)

<a id="Vercel_AI_ProviderUtils_WebSocketUtils_ToWebSocketUrl_System_Uri_"></a>

### ToWebSocketUrl(Uri)


```csharp
public static Uri ToWebSocketUrl(Uri url)
```


Converts an http(s) URL to the corresponding ws(s) URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L82)

<a id="Vercel_AI_ProviderUtils_WebSocketUtils_WaitForWebSocketBufferDrainAsync_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketUtils_WaitForWebSocketBufferDrainAsync_Vercel_AI_ProviderUtils_IWebSocketLike_System_Int64_System_Int32_System_Threading_CancellationToken_"></a>

### WaitForWebSocketBufferDrainAsync(IWebSocketLike, long, int, CancellationToken)


```csharp
public static Task WaitForWebSocketBufferDrainAsync(IWebSocketLike socket, long highWaterMark = 1048576, int pollIntervalMs = 20, CancellationToken cancellationToken = default)
```


Waits until the socket's send buffer drains below &lt;code class="paramref"&gt;highWaterMark&lt;/code&gt; bytes. It does nothing for
implementations that do not expose [BufferedAmount](/api/Vercel.AI.ProviderUtils/IWebSocketLike/#Vercel_AI_ProviderUtils_IWebSocketLike_BufferedAmount). It returns as soon as the
socket is no longer open or the token is cancelled.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `socket` | [IWebSocketLike](/api/Vercel.AI.ProviderUtils/IWebSocketLike/) |  |
| `highWaterMark` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `pollIntervalMs` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L125)
