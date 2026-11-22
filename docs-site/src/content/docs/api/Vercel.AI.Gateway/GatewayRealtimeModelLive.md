---
title: "GatewayRealtimeModelLive"
description: "Live model backed by the AI Gateway. Live is a continuous conversation API: one session-start opens the session, PCM audio streams in both directions, and se..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeModelLive"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeModelLive : IServerWebSocketRealtimeModel, IRealtimeModel
```


Live model backed by the AI Gateway. Live is a continuous conversation API: one `session-start` opens the
session, PCM audio streams in both directions, and `session-close` ends it with final usage. The client speaks
the Gateway's normalized Live protocol on `/live-model`, and the Gateway runs the provider mapping server-side,
so server events arrive already normalized. Maps to `GatewayRealtimeModelLive`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L70)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive__ctor_System_String_Vercel_AI_Gateway_GatewayRealtimeModelLiveConfig_"></a>

### GatewayRealtimeModelLive(string, GatewayRealtimeModelLiveConfig)


```csharp
public GatewayRealtimeModelLive(string modelId, GatewayRealtimeModelLiveConfig config)
```


Creates a Live model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [GatewayRealtimeModelLiveConfig](/api/Vercel.AI.Gateway/GatewayRealtimeModelLiveConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L90)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_Capabilities_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_Capabilities"></a>

### Capabilities


```csharp
public RealtimeCapabilities Capabilities { get; }
```


The conversation mode and the connection kinds this model supports. Maps to `capabilities`.

**Value:** [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L106)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L103)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L100)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L97)

## Methods

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_BuildSessionConfig_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig, RealtimeModelV4SessionIntent?)


```csharp
public RealtimeModelV4SessionConfig BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
```


Validates a normalized session config against what the Gateway Live route accepts and returns it. Maps to
`buildSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L293)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_BuildUri_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


The Gateway Live WebSocket URL for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L117)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ConnectAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ConnectAsync_System_Threading_CancellationToken_"></a>

### ConnectAsync(CancellationToken)


```csharp
public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
```


The Gateway connects through the client-secret or server WebSocket config, not through a session.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L123)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_DoCreateClientSecretAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_DoCreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### DoCreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<GatewayRealtimeClientSecret> DoCreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Mints a short-lived client secret bound to this Live model. The browser opens the Live WebSocket with it through
the `ai-gateway-auth.<token>` subprotocol; the session configuration is sent later in
`session-start`. Maps to `doCreateClientSecret`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L135)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

### GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent?, CancellationToken)


```csharp
public Task<RealtimeModelV4ServerConnection> GetServerWebSocketConfigAsync(RealtimeModelV4SessionIntent? intent, CancellationToken cancellationToken)
```


Returns the upstream WebSocket URL and auth headers for a server-initiated connection, using the
credential the model was constructed with. Unsupported intents throw before any connection is made.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `intent` | `RealtimeModelV4SessionIntent?` |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;RealtimeModelV4ServerConnection&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L160)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_GetWebSocketConfig_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_GetWebSocketConfig_System_String_System_String_"></a>

### GetWebSocketConfig(string, string)


```csharp
public GatewayRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
```


Returns the URL and the subprotocols that carry the token. Maps to `getWebSocketConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayRealtimeWebSocketConfig](/api/Vercel.AI.Gateway/GatewayRealtimeWebSocketConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L151)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ParseServerEvent_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_ParseServerEvent_System_Object_"></a>

### ParseServerEvent(object?)


```csharp
public RealtimeModelV4ServerEvent ParseServerEvent(object? raw)
```


Passes a normalized server event through. A Gateway-originated `error` frame carries no provider payload,
so the frame is kept as `Raw`. A value that is not an event with a string `type` becomes an
`invalid_server_event` error. Maps to `parseServerEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [RealtimeModelV4ServerEvent](/api/Vercel.AI.Provider/RealtimeModelV4ServerEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L188)

<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_SerializeClientEvent_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModelLive_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public JsonNode SerializeClientEvent(RealtimeModelV4ClientEvent @event, RealtimeModelV4SessionIntent? intent = null)
```


Maps a normalized client event to the Gateway Live wire format. Maps to `serializeClientEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `event` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelLive.cs#L227)
