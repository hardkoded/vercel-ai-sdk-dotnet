---
title: "GatewayRealtimeModel"
description: "Realtime model backed by the AI Gateway. The Gateway normalizes realtime like every other modality, so this model is a thin identity codec over the normalize..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewayRealtimeModel : IServerWebSocketRealtimeModel, IRealtimeModel
```


Realtime model backed by the AI Gateway. The Gateway normalizes realtime like every other modality, so this model is
a thin identity codec over the normalized protocol; only the connection and authentication are Gateway-specific.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L115)

## Constructors

<a id="Vercel_AI_Gateway_GatewayRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel__ctor_System_String_Vercel_AI_Gateway_GatewayRealtimeModelConfig_"></a>

### GatewayRealtimeModel(string, GatewayRealtimeModelConfig)


```csharp
public GatewayRealtimeModel(string modelId, GatewayRealtimeModelConfig config)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [GatewayRealtimeModelConfig](/api/Vercel.AI.Gateway/GatewayRealtimeModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L120)

## Properties

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L133)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L130)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Always `v4`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L127)

## Methods

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_BuildSessionConfig_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig, RealtimeModelV4SessionIntent?)


```csharp
public object BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
```


The session config is already normalized; the Gateway maps it to the upstream payload server-side.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L218)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


The Gateway realtime WebSocket URL for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L136)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L142)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_DoCreateClientSecretAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_DoCreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### DoCreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<GatewayRealtimeClientSecret> DoCreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Mints a single-use, short-lived client secret (`vcst_`) the browser uses to open the realtime WebSocket without
holding the long-lived Gateway credential. &lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; is forwarded to the mint endpoint.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GatewayRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L153)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L178)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_GetWebSocketConfig_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_GetWebSocketConfig_System_String_System_String_"></a>

### GetWebSocketConfig(string, string)


```csharp
public GatewayRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
```


Returns the URL and the subprotocols that carry the token (a browser has no header channel).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [GatewayRealtimeWebSocketConfig](/api/Vercel.AI.Gateway/GatewayRealtimeWebSocketConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L169)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_ParseServerEvent_System_Object_"></a>

### ParseServerEvent(object)


```csharp
public RealtimeModelV4ServerEvent ParseServerEvent(object raw)
```


The Gateway emits normalized events, so no provider-specific mapping is needed.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [RealtimeModelV4ServerEvent](/api/Vercel.AI.Provider/RealtimeModelV4ServerEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L202)

<a id="Vercel_AI_Gateway_GatewayRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Gateway_GatewayRealtimeModel_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public object SerializeClientEvent(RealtimeModelV4ClientEvent @event, RealtimeModelV4SessionIntent? intent = null)
```


The Gateway accepts normalized client events directly.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `event` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModel.cs#L208)
