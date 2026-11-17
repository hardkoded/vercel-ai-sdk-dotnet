---
title: "OpenAIRealtimeModel"
description: "OpenAI Realtime session URI builder and WebSocket client."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModel : IServerWebSocketRealtimeModel, IRealtimeModel
```


OpenAI Realtime session URI builder and WebSocket client.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L83)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel__ctor_System_String_Vercel_AI_OpenAI_OpenAIRealtimeModelConfig_"></a>

### OpenAIRealtimeModel(string, OpenAIRealtimeModelConfig)


```csharp
public OpenAIRealtimeModel(string modelId, OpenAIRealtimeModelConfig config)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [OpenAIRealtimeModelConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L108)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel__ctor_Vercel_AI_OpenAI_OpenAIProvider_System_String_"></a>

### OpenAIRealtimeModel(OpenAIProvider, string)


```csharp
public OpenAIRealtimeModel(OpenAIProvider provider, string modelId)
```


Creates a realtime model from a provider.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L91)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Capabilities_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Capabilities"></a>

### Capabilities


```csharp
public RealtimeCapabilities Capabilities { get; }
```


The conversation mode and the connection kinds this model supports. Maps to `capabilities`.

**Value:** [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L130)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L125)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Provider_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L122)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_SpecificationVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


The realtime model specification version. Maps to `specificationVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L119)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildSessionConfig_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L258)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L139)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L271)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_CreateClientSecretAsync_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(int?, CancellationToken)


```csharp
public Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Requests a realtime client secret. &lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; is omitted when null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L151)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_CreateClientSecretAsync_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(RealtimeModelV4SessionConfig?, int?, CancellationToken)


```csharp
public Task<OpenAIRealtimeClientSecret> CreateClientSecretAsync(RealtimeModelV4SessionConfig? sessionConfig, int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Requests a realtime client secret for a session built from &lt;code class="paramref"&gt;sessionConfig&lt;/code&gt;.
Maps to `doCreateClientSecret`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sessionConfig` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;OpenAIRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L164)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L224)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_GetWebSocketConfig_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_GetWebSocketConfig_System_String_System_String_"></a>

### GetWebSocketConfig(string, string)


```csharp
public OpenAIRealtimeWebSocketConfig GetWebSocketConfig(string token, string url)
```


Returns the URL and the subprotocols that carry the token (a browser has no header channel). Maps to `getWebSocketConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [OpenAIRealtimeWebSocketConfig](/api/Vercel.AI.OpenAI/OpenAIRealtimeWebSocketConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L215)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public RealtimeModelV4ServerEvent ParseServerEvent(JsonElement raw)
```


Parses a raw provider event into a normalized event. Maps to `parseServerEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [RealtimeModelV4ServerEvent](/api/Vercel.AI.Provider/RealtimeModelV4ServerEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L239)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModel_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public JsonNode? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
```


Serializes a normalized client event into the provider wire format. Maps to `serializeClientEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L245)
