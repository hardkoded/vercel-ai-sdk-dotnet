---
title: "OpenAIRealtimeModelLive"
description: "OpenAI Live realtime model: a continuous conversation over a server WebSocket or WebRTC. Maps to OpenAIRealtimeModelLive."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModelLive"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModelLive : IServerWebSocketRealtimeModel, IRealtimeModel
```


OpenAI Live realtime model: a continuous conversation over a server WebSocket or WebRTC. Maps to
`OpenAIRealtimeModelLive`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L45)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIRealtimeModelLive(OpenAIProvider, string)


```csharp
public OpenAIRealtimeModelLive(OpenAIProvider provider, string modelId)
```


Creates a Live realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L50)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Capabilities_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Capabilities"></a>

### Capabilities


```csharp
public RealtimeCapabilities Capabilities { get; }
```


The conversation mode and the connection kinds this model supports. Maps to `capabilities`.

**Value:** [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L66)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Finalization_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Finalization"></a>

### Finalization


```csharp
public string Finalization { get; }
```


How a session ends. Maps to `capabilities.finalization`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L77)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L63)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L60)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


The realtime model specification version. Maps to `specificationVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L57)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Startup_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_Startup"></a>

### Startup


```csharp
public string Startup { get; }
```


How a session starts. Maps to `capabilities.startup`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L74)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_BuildSessionConfig_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig, RealtimeModelV4SessionIntent?)


```csharp
public JsonObject BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
```


Builds the provider session configuration from a normalized config. Maps to `buildSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L147)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_BuildUri_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L80)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ConnectAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ConnectAsync_System_Threading_CancellationToken_"></a>

### ConnectAsync(CancellationToken)


```csharp
public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
```


Opens a realtime session.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L154)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_CreateServerEventParser_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_CreateServerEventParser"></a>

### CreateServerEventParser()


```csharp
public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CreateServerEventParser()
```


Returns a server event parser. Maps to `createServerEventParser`.

**Returns:** [Func&lt;JsonElement, IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L134)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_CreateWebRTCSessionAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_CreateWebRTCSessionAsync_System_String_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Threading_CancellationToken_"></a>

### CreateWebRTCSessionAsync(string, RealtimeModelV4SessionConfig?, CancellationToken)


```csharp
public Task<OpenAIRealtimeModelLiveWebRTCSession> CreateWebRTCSessionAsync(string sdp, RealtimeModelV4SessionConfig? sessionConfig, CancellationToken cancellationToken)
```


Exchanges a WebRTC offer for an answer. Maps to `doCreateWebRTCSession`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sdp` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sessionConfig` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIRealtimeModelLiveWebRTCSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L99)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L92)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_GetWebRTCConfig_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_GetWebRTCConfig"></a>

### GetWebRTCConfig()


```csharp
public OpenAIRealtimeModelLiveWebRTCConfig GetWebRTCConfig()
```


Maps to `getWebRTCConfig`.

**Returns:** [OpenAIRealtimeModelLiveWebRTCConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLiveWebRTCConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L86)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ParseServerEvent_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
```


Parses a raw provider event into normalized events. Maps to `parseServerEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L128)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_SerializeClientEvent_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLive_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public JsonNode SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
```


Serializes a normalized client event into the provider wire format. Maps to `serializeClientEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLive.cs#L140)
