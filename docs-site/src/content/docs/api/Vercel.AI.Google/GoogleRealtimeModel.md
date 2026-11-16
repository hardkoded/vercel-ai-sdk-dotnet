---
title: "GoogleRealtimeModel"
description: "Realtime model. Event mapping is local; opening the Live socket is not available on this runtime."
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
public sealed class GoogleRealtimeModel : IRealtimeModel
```


Realtime model. Event mapping is local; opening the Live socket is not available on this runtime.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L224)

## Constructors

<a id="Vercel_AI_Google_GoogleRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel__ctor_Vercel_AI_Google_GoogleProvider_System_String_"></a>

### GoogleRealtimeModel(GoogleProvider, string)


```csharp
public GoogleRealtimeModel(GoogleProvider provider, string modelId)
```


Creates a realtime model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L232)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L245)

<a id="Vercel_AI_Google_GoogleRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L239)

## Methods

<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L248)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L255)

<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateClientSecretAsync_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_CreateClientSecretAsync_Vercel_AI_Google_GoogleRealtimeSession_System_Nullable_System_Int32__System_Threading_CancellationToken_"></a>

### CreateClientSecretAsync(GoogleRealtimeSession?, int?, CancellationToken)


```csharp
public Task<GoogleRealtimeClientSecret> CreateClientSecretAsync(GoogleRealtimeSession? session, int? expiresAfterSeconds, CancellationToken cancellationToken)
```


Creates an ephemeral auth token that a client uses to open a Live session.
&lt;code class="paramref"&gt;expiresAfterSeconds&lt;/code&gt; is the window to open a session and defaults to 60 seconds.
The token itself lives 30 minutes longer, so the opened session has room to run.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `session` | [GoogleRealtimeSession](/api/Vercel.AI.Google/GoogleRealtimeSession/) |  |
| `expiresAfterSeconds` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;GoogleRealtimeClientSecret&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L265)

<a id="Vercel_AI_Google_GoogleRealtimeModel_GetWebSocketUrl_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_GetWebSocketUrl_System_String_System_String_"></a>

### GetWebSocketUrl(string, string)


```csharp
public string GetWebSocketUrl(string token, string url)
```


The URL a client opens with a token from `CancellationToken)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `token` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L301)

<a id="Vercel_AI_Google_GoogleRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<GoogleRealtimeEvent> ParseServerEvent(JsonElement raw)
```


Maps one server message with this model's event mapper.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;GoogleRealtimeEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L307)

<a id="Vercel_AI_Google_GoogleRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeModel_SerializeClientEvent_System_String_Vercel_AI_Google_GoogleRealtimeSession_System_String_System_String_System_String_System_String_System_String_"></a>

### SerializeClientEvent(string, GoogleRealtimeSession?, string?, string?, string?, string?, string?)


```csharp
public JsonObject? SerializeClientEvent(string type, GoogleRealtimeSession? session, string? audio, string? text, string? callId, string? callName, string? callOutput)
```


Serializes a client event with this model's event mapper. A session update uses this model.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `session` | [GoogleRealtimeSession](/api/Vercel.AI.Google/GoogleRealtimeSession/) |  |
| `audio` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `callOutput` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L313)
