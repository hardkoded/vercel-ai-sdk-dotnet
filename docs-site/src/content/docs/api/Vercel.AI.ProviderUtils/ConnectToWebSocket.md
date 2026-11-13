---
title: "ConnectToWebSocket"
description: "Opens a WebSocket for a provider model, owning the transport-generic layer: factory resolution, header hygiene, abort wiring, and message decoding. Maps to c..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ConnectToWebSocket"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocket"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ConnectToWebSocket
```


Opens a WebSocket for a provider model, owning the transport-generic layer: factory resolution, header hygiene,
abort wiring, and message decoding. Maps to `connectToWebSocket`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L69)

## Methods

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocket_Connect_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocket_Connect_Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_"></a>

### Connect(ConnectToWebSocketOptions)


```csharp
public static WebSocketConnection Connect(ConnectToWebSocketOptions options)
```


Opens the socket.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [ConnectToWebSocketOptions](/api/Vercel.AI.ProviderUtils/ConnectToWebSocketOptions/) |  |

**Returns:** [WebSocketConnection](/api/Vercel.AI.ProviderUtils/WebSocketConnection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L72)
