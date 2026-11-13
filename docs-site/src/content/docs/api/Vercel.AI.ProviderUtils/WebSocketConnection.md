---
title: "WebSocketConnection"
description: "A WebSocket opened by ConnectToWebSocket."
editUrl: false
prev: false
next: false
sidebar:
  label: "WebSocketConnection"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_WebSocketConnection"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class WebSocketConnection
```


A WebSocket opened by [ConnectToWebSocket](/api/Vercel.AI.ProviderUtils/ConnectToWebSocket/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L8)

## Properties

<a id="Vercel_AI_ProviderUtils_WebSocketConnection_Socket_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketConnection_Socket"></a>

### Socket


```csharp
public IWebSocketLike? Socket { get; }
```


Null when the factory threw or the token was already cancelled.

**Value:** [IWebSocketLike](/api/Vercel.AI.ProviderUtils/IWebSocketLike/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L19)

## Methods

<a id="Vercel_AI_ProviderUtils_WebSocketConnection_Close_"></a>
<a id="Vercel_AI_ProviderUtils_WebSocketConnection_Close_System_Nullable_System_Int32__"></a>

### Close(int?)


```csharp
public void Close(int? code = null)
```


Unregisters the abort listener and closes the socket. It never throws.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `code` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L22)
