---
title: "GatewayRealtimeAuth"
description: "Shared WebSocket subprotocol contract for AI Gateway realtime, streaming transcription, and streaming speech auth. Maps to gateway-realtime-auth.ts. The Gate..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeAuth"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayRealtimeAuth
```


Shared WebSocket subprotocol contract for AI Gateway realtime, streaming transcription, and streaming speech
auth. Maps to `gateway-realtime-auth.ts`. The Gateway auth (bearer) token travels through the
`Sec-WebSocket-Protocol` handshake because a browser `WebSocket` cannot set request headers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L14)

## Fields

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GatewayAuthSubprotocolPrefix"></a>

### GatewayAuthSubprotocolPrefix


```csharp
public const string GatewayAuthSubprotocolPrefix = "ai-gateway-auth."
```


Subprotocol prefix that carries the Gateway auth (bearer) token. Maps to `GATEWAY_AUTH_SUBPROTOCOL_PREFIX`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L26)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GatewayRealtimeSubprotocol"></a>

### GatewayRealtimeSubprotocol


```csharp
public const string GatewayRealtimeSubprotocol = "ai-gateway-realtime.v1"
```


Marker subprotocol offered on every realtime handshake. Maps to `GATEWAY_REALTIME_SUBPROTOCOL`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L17)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GatewaySpeechSubprotocol"></a>

### GatewaySpeechSubprotocol


```csharp
public const string GatewaySpeechSubprotocol = "ai-gateway-speech.v1"
```


Marker subprotocol for streaming speech handshakes. Maps to `GATEWAY_SPEECH_SUBPROTOCOL`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L23)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GatewayTeamSubprotocolPrefix"></a>

### GatewayTeamSubprotocolPrefix


```csharp
public const string GatewayTeamSubprotocolPrefix = "ai-gateway-team."
```


Subprotocol prefix that carries optional Vercel team scoping. Maps to `GATEWAY_TEAM_SUBPROTOCOL_PREFIX`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L29)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GatewayTranscriptionSubprotocol"></a>

### GatewayTranscriptionSubprotocol


```csharp
public const string GatewayTranscriptionSubprotocol = "ai-gateway-transcription.v1"
```


Marker subprotocol for streaming transcription handshakes. Maps to `GATEWAY_TRANSCRIPTION_SUBPROTOCOL`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L20)

## Methods

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeAuthToken_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeAuthToken_System_String_"></a>

### GetGatewayRealtimeAuthToken(string?)


```csharp
public static string? GetGatewayRealtimeAuthToken(string? secWebSocketProtocol)
```


Server side: extracts the Gateway auth (bearer) token from a `Sec-WebSocket-Protocol` header value, or null
when it is absent or empty.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secWebSocketProtocol` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L53)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeProtocols_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeProtocols_System_String_System_String_"></a>

### GetGatewayRealtimeProtocols(string, string?)


```csharp
public static IReadOnlyList<string> GetGatewayRealtimeProtocols(string token, string? teamIdOrSlug = null)
```


Builds the WebSocket subprotocols that carry &lt;code class="paramref"&gt;token&lt;/code&gt; to the Gateway.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `teamIdOrSlug` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L32)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeTeamIdOrSlug_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayRealtimeTeamIdOrSlug_System_String_"></a>

### GetGatewayRealtimeTeamIdOrSlug(string?)


```csharp
public static string? GetGatewayRealtimeTeamIdOrSlug(string? secWebSocketProtocol)
```


Server side: extracts the optional Vercel team ID or slug from a `Sec-WebSocket-Protocol` header value. The
team is base64url-encoded.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `secWebSocketProtocol` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L63)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewaySpeechProtocols_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewaySpeechProtocols_System_String_System_String_"></a>

### GetGatewaySpeechProtocols(string, string?)


```csharp
public static IReadOnlyList<string> GetGatewaySpeechProtocols(string token, string? teamIdOrSlug = null)
```


Like `String)`, with the streaming speech marker subprotocol.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `teamIdOrSlug` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L44)

<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayTranscriptionProtocols_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeAuth_GetGatewayTranscriptionProtocols_System_String_System_String_"></a>

### GetGatewayTranscriptionProtocols(string, string?)


```csharp
public static IReadOnlyList<string> GetGatewayTranscriptionProtocols(string token, string? teamIdOrSlug = null)
```


Like `String)`, with the streaming transcription marker subprotocol.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `teamIdOrSlug` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeAuth.cs#L38)
