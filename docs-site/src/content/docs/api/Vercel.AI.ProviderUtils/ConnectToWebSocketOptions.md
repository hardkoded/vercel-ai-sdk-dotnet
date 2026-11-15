---
title: "ConnectToWebSocketOptions"
description: "Options for Connect(ConnectToWebSocketOptions)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ConnectToWebSocketOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ConnectToWebSocketOptions
```


Options for [Connect(ConnectToWebSocketOptions)](/api/Vercel.AI.ProviderUtils/ConnectToWebSocket/#Vercel_AI_ProviderUtils_ConnectToWebSocket_Connect_Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L29)

## Properties

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_AbortSignal_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; set; }
```


Aborts the connection.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L44)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Headers_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Request headers. Null values are removed.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L38)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnAbort_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnAbort"></a>

### OnAbort


```csharp
public Action<Exception>? OnAbort { get; set; }
```


Also called, without opening a socket, when the token is already cancelled.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L62)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnClose_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnClose"></a>

### OnClose


```csharp
public Action<int?, string?>? OnClose { get; set; }
```


Receives the close code and reason when the transport provides them.

**Value:** [Action&lt;int?, string&gt;](https://learn.microsoft.com/dotnet/api/system.action-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L59)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnMessageText_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnMessageText"></a>

### OnMessageText


```csharp
public Func<string, Task> OnMessageText { get; set; }
```


One decoded message. A throw goes to [OnProcessingError](#Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnProcessingError).

**Value:** [Func&lt;string, Task&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L50)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnOpen_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnOpen"></a>

### OnOpen


```csharp
public Action<IWebSocketLike>? OnOpen { get; set; }
```


Called when the socket opens.

**Value:** [Action&lt;IWebSocketLike&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L47)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnProcessingError_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnProcessingError"></a>

### OnProcessingError


```csharp
public Action<Exception> OnProcessingError { get; set; }
```


Factory throws and message decoding or processing failures.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L53)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnSocketError_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_OnSocketError"></a>

### OnSocketError


```csharp
public Action? OnSocketError { get; set; }
```


Called on a socket error.

**Value:** [Action](https://learn.microsoft.com/dotnet/api/system.action)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L56)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Protocols_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Protocols"></a>

### Protocols


```csharp
public IReadOnlyList<string>? Protocols { get; set; }
```


Requested subprotocols.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L35)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Url_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_Url"></a>

### Url


```csharp
public Uri Url { get; set; }
```


The URL to open.

**Value:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L32)

<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_WebSocket_"></a>
<a id="Vercel_AI_ProviderUtils_ConnectToWebSocketOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ConnectToWebSocket.cs#L41)
