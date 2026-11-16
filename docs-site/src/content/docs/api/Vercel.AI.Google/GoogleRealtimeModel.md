---
title: "GoogleRealtimeModel"
description: "Realtime model for Gemini Live. Maps to GoogleRealtimeModel. Event mapping is local; opening the Live socket is left to the caller, which uses CancellationTo..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeModel"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeModel : IServerWebSocketRealtimeModel, IRealtimeModel
```


Realtime model for Gemini Live. Maps to `GoogleRealtimeModel`. Event mapping is local; opening the Live
socket is left to the caller, which uses `CancellationToken)` or a client secret.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IServerWebSocketRealtimeModel](/api/Vercel.AI.Provider/IServerWebSocketRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L307)

## Constructors

<a id="Vercel_AI_Google_GoogleRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel__ctor_System_String_Vercel_AI_Google_GoogleRealtimeModelConfig_"></a>

### GoogleRealtimeModel(string, GoogleRealtimeModelConfig)


```csharp
public GoogleRealtimeModel(string modelId, GoogleRealtimeModelConfig config)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [GoogleRealtimeModelConfig](/api/Vercel.AI.Google/GoogleRealtimeModelConfig/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L323)

<a id="Vercel_AI_Google_GoogleRealtimeModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleRealtimeModel(GoogleProvider, string)


```csharp
public GoogleRealtimeModel(GoogleProvider provider, string modelId)
```


Creates a realtime model that takes its base URL, headers and HTTP client from &lt;code class="paramref"&gt;provider&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L330)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L351)

<a id="Vercel_AI_Google_GoogleRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L345)

## Methods

<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildSessionConfig_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig, RealtimeModelV4SessionIntent?)


```csharp
public JsonObject BuildSessionConfig(RealtimeModelV4SessionConfig config, RealtimeModelV4SessionIntent? intent = null)
```


Builds the Live `setup` object for a normalized session config.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L451)

<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L354)

<a id="Vercel_AI_Google_GoogleRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L360)

<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateClientSecretAsync_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(RealtimeModelV4SessionConfig?, int?, CancellationToken)


```csharp
public Task<GoogleRealtimeClientSecret> CreateClientSecretAsync(RealtimeModelV4SessionConfig? sessionConfig, int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Creates an ephemeral auth token that a client uses to open a Live session.
&lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; is the window to open a session and defaults to 60 seconds.
The token itself lives 30 minutes longer, so the opened session has room to run.
Maps to `doCreateClientSecret`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sessionConfig` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GoogleRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L371)

<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateServerEventParser_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateServerEventParser"></a>

### CreateServerEventParser()


```csharp
public Func<JsonElement, IReadOnlyList<RealtimeModelV4ServerEvent>> CreateServerEventParser()
```


Creates a parser with its own turn and usage state, so concurrent server connections do not share it.
Maps to `createServerEventParser`.

**Returns:** [Func&lt;JsonElement, IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L437)

<a id="Vercel_AI_Google_GoogleRealtimeModel_GetServerWebSocketConfigAsync_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_GetServerWebSocketConfigAsync_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L419)

<a id="Vercel_AI_Google_GoogleRealtimeModel_GetWebSocketUrl_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_GetWebSocketUrl_System_String_System_String_"></a>

### GetWebSocketUrl(string, string)


```csharp
public string GetWebSocketUrl(string token, string url)
```


The URL a client opens with a token from `CancellationToken)`. Maps to `getWebSocketConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L413)

<a id="Vercel_AI_Google_GoogleRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
```


Maps one server message with this model's shared event mapper.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L428)

<a id="Vercel_AI_Google_GoogleRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Nullable_Vercel_AI_Provider_RealtimeModelV4SessionIntent__"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent, RealtimeModelV4SessionIntent?)


```csharp
public JsonObject? SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent, RealtimeModelV4SessionIntent? intent = null)
```


Serializes a client event with this model's event mapper.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `intent` | `RealtimeModelV4SessionIntent?` |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L444)
