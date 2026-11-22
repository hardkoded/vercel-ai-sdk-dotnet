---
title: "MistralRealtimeModel"
description: "A managed Mistral voice agent with a provider-owned LiveKit transport. Maps to MistralRealtimeModel. It has no WebSocket: BuildUri() and ConnectAsync(Cancell..."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeModel"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeModel : IProviderTransportRealtimeModel, IRealtimeModel
```


A managed Mistral voice agent with a provider-owned LiveKit transport. Maps to `MistralRealtimeModel`.
It has no WebSocket: [BuildUri()](#Vercel_AI_Mistral_MistralRealtimeModel_BuildUri) and [ConnectAsync(CancellationToken)](#Vercel_AI_Mistral_MistralRealtimeModel_ConnectAsync_System_Threading_CancellationToken_) are unsupported.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IProviderTransportRealtimeModel](/api/Vercel.AI.Provider/IProviderTransportRealtimeModel/), [IRealtimeModel](/api/Vercel.AI.Provider/IRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L15)

## Constructors

<a id="Vercel_AI_Mistral_MistralRealtimeModel__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel__ctor_System_String_Vercel_AI_Mistral_ILiveKitClient_System_Net_Http_HttpClient_"></a>

### MistralRealtimeModel(string, ILiveKitClient?, HttpClient?)


```csharp
public MistralRealtimeModel(string modelId, ILiveKitClient? liveKit = null, HttpClient? httpClient = null)
```


Creates a model. &lt;code class="paramref"&gt;liveKit&lt;/code&gt; is the host's LiveKit client, which its transports use to join
the voice room (upstream loads `livekit-client`). &lt;code class="paramref"&gt;httpClient&lt;/code&gt; calls the application's
session endpoint (upstream `fetch`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `liveKit` | [ILiveKitClient](/api/Vercel.AI.Mistral/ILiveKitClient/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L25)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeModel_Capabilities_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_Capabilities"></a>

### Capabilities


```csharp
public RealtimeCapabilities Capabilities { get; }
```


Conversation semantics and supported transports. Maps to `capabilities`.

**Value:** [RealtimeCapabilities](/api/Vercel.AI/RealtimeCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L46)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_ModelId_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L43)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_Provider_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L40)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_SpecificationVersion_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_SpecificationVersion"></a>

### SpecificationVersion


```csharp
public string SpecificationVersion { get; }
```


Specification version. Maps to `specificationVersion`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L37)

## Methods

<a id="Vercel_AI_Mistral_MistralRealtimeModel_BuildSessionConfig_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_BuildSessionConfig_Vercel_AI_Provider_RealtimeModelV4SessionConfig_"></a>

### BuildSessionConfig(RealtimeModelV4SessionConfig)


```csharp
public JsonElement BuildSessionConfig(RealtimeModelV4SessionConfig config)
```


Builds the session configuration. Maps to `buildSessionConfig`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `config` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L89)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_BuildUri_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_BuildUri"></a>

### BuildUri()


```csharp
public Uri BuildUri()
```


WebSocket URI for this model.

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L63)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_ConnectAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_ConnectAsync_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L69)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_CreateTransport_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_CreateTransport_Vercel_AI_Provider_RealtimeModelV4TransportOptions_"></a>

### CreateTransport(RealtimeModelV4TransportOptions)


```csharp
public IRealtimeModelV4Transport CreateTransport(RealtimeModelV4TransportOptions options)
```


Creates a provider-owned LiveKit transport without opening a connection. Maps to `createTransport`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [RealtimeModelV4TransportOptions](/api/Vercel.AI.Provider/RealtimeModelV4TransportOptions/) |  |

**Returns:** [IRealtimeModelV4Transport](/api/Vercel.AI.Provider/IRealtimeModelV4Transport/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L57)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_ParseServerEvent_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_ParseServerEvent_System_Text_Json_JsonElement_"></a>

### ParseServerEvent(JsonElement)


```csharp
public IReadOnlyList<RealtimeModelV4ServerEvent> ParseServerEvent(JsonElement raw)
```


Normalizes a provider event. Maps to `parseServerEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyList&lt;RealtimeModelV4ServerEvent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L75)

<a id="Vercel_AI_Mistral_MistralRealtimeModel_SerializeClientEvent_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeModel_SerializeClientEvent_Vercel_AI_Provider_RealtimeModelV4ClientEvent_"></a>

### SerializeClientEvent(RealtimeModelV4ClientEvent)


```csharp
public JsonElement SerializeClientEvent(RealtimeModelV4ClientEvent clientEvent)
```


Always throws: client events go through the provider transport only. Maps to `serializeClientEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

**Exceptions**

| Type | Condition |
| --- | --- |
| [UnsupportedFunctionalityException](/api/Vercel.AI.Operations/UnsupportedFunctionalityException/) | Always. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModel.cs#L82)
