---
title: "XaiRealtimeModel"
description: "xAI realtime voice model. Maps to XaiRealtimeModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiRealtimeModel"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiRealtimeModel : IServerWebSocketRealtimeModel, IRealtimeModel
```


xAI realtime voice model. Maps to `XaiRealtimeModel`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L66)

## Constructors

<a id="Vercel_AI_Xai_XaiRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel__ctor_System_String_Vercel_AI_Xai_XaiRealtimeModelConfig_"></a>

### XaiRealtimeModel(string, XaiRealtimeModelConfig)


```csharp
public XaiRealtimeModel(string modelId, XaiRealtimeModelConfig config)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [XaiRealtimeModelConfig](/api/Vercel.AI.Xai/XaiRealtimeModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L73)

## Properties

<a id="Vercel_AI_Xai_XaiRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L86)

<a id="Vercel_AI_Xai_XaiRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L80)

## Methods

<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildSessionConfig_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig, RealtimeModelV4SessionIntent?)


```csharp
public JsonObject BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
```


Builds the xAI `session` object for a normalized session config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L199)

<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L89)

<a id="Vercel_AI_Xai_XaiRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

### ConnectAsync(CancellationToken)


```csharp
public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
```


WebSocket sessions are opened by the caller with [Url](/api/Vercel.AI.Xai/XaiClientSecret/#Vercel_AI_Xai_XaiClientSecret_Url).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L95)

<a id="Vercel_AI_Xai_XaiRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_CreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<XaiClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Posts to `/realtime/client_secrets` and returns a WebSocket URL that includes the model. Maps to `doCreateClientSecret`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;XaiClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L101)

<a id="Vercel_AI_Xai_XaiRealtimeModel_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L158)

<a id="Vercel_AI_Xai_XaiRealtimeModel_GetWebSocketConfig_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_GetWebSocketConfig_System_String_System_String_"></a>

### GetWebSocketConfig(string, string)


```csharp
public RealtimeModelV4WebSocketConfig GetWebSocketConfig(string token, string url)
```


The URL and subprotocol a client opens with a client secret: the token travels as the
`xai-client-secret.` subprotocol. Maps to `getWebSocketConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [RealtimeModelV4WebSocketConfig](/api/Vercel.AI.Provider/RealtimeModelV4WebSocketConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L152)

<a id="Vercel_AI_Xai_XaiRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public RealtimeModelV4ServerEvent ParseServerEvent(JsonElement raw)
```


Maps one xAI server event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [RealtimeModelV4ServerEvent](/api/Vercel.AI.Provider/RealtimeModelV4ServerEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L180)

<a id="Vercel_AI_Xai_XaiRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Xai_XaiRealtimeModel_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public JsonObject? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
```


Serializes a client event. `conversation-item-truncate` is dropped (null) because xAI ignores it over
WebSocket.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiRealtimeModel.cs#L189)
