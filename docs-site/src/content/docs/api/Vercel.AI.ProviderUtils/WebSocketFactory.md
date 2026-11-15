---
title: "WebSocketFactory"
description: "Creates a socket. Maps to WebSocketConstructor."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebSocketFactory"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_WebSocketFactory"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public delegate IWebSocketLike WebSocketFactory(Uri url, IReadOnlyList<string>? protocols, IReadOnlyDictionary<string, string>? headers)
```


Creates a socket. Maps to `WebSocketConstructor`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) | The WebSocket URL. |
| `protocols` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) | Requested subprotocols. |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) | Request headers, for implementations that can send them. |

**Returns:** [IWebSocketLike](/api/Vercel.AI.ProviderUtils/IWebSocketLike/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/WebSocket.cs#L62)
