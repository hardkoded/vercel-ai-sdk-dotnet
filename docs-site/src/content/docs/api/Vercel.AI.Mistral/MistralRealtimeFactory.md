---
title: "MistralRealtimeFactory"
description: "Creates Mistral voice agent models and manages their sessions. Maps to MistralRealtimeFactory, built by createMistralRealtimeFactory. Reached through Experim..."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeFactory"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeFactory"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeFactory
```


Creates Mistral voice agent models and manages their sessions. Maps to `MistralRealtimeFactory`, built by
`createMistralRealtimeFactory`. Reached through [ExperimentalRealtime](/api/Vercel.AI.Mistral/MistralProvider/#Vercel_AI_Mistral_MistralProvider_ExperimentalRealtime).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L38)

## Methods

<a id="Vercel_AI_Mistral_MistralRealtimeFactory_CloseSessionAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeFactory_CloseSessionAsync_System_String_System_Threading_CancellationToken_"></a>

### CloseSessionAsync(string, CancellationToken)


```csharp
public Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
```


Server-only: releases a voice session after the browser disconnects. Maps to `closeSession`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sessionId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L118)

<a id="Vercel_AI_Mistral_MistralRealtimeFactory_CreateSessionAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeFactory_CreateSessionAsync_System_String_Vercel_AI_Provider_RealtimeModelV4SessionConfig_System_Threading_CancellationToken_"></a>

### CreateSessionAsync(string, RealtimeModelV4SessionConfig?, CancellationToken)


```csharp
public Task<MistralRealtimeSession> CreateSessionAsync(string model, RealtimeModelV4SessionConfig? sessionConfig = null, CancellationToken cancellationToken = default)
```


Server-only: creates a voice session using the Mistral API key. Maps to `createSession`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `sessionConfig` | [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;MistralRealtimeSession&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L63)

<a id="Vercel_AI_Mistral_MistralRealtimeFactory_Model_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeFactory_Model_System_String_Vercel_AI_Mistral_ILiveKitClient_System_Net_Http_HttpClient_"></a>

### Model(string, ILiveKitClient?, HttpClient?)


```csharp
public MistralRealtimeModel Model(string modelId, ILiveKitClient? liveKit = null, HttpClient? httpClient = null)
```


Creates a browser model. It does not load an API key. Maps to calling the factory with a model id.
&lt;code class="paramref"&gt;liveKit&lt;/code&gt; is the host's LiveKit client (upstream loads `livekit-client`), for example the
`LiveKitRtcClient` of the `Vercel.AI.Mistral.LiveKit` package; &lt;code class="paramref"&gt;httpClient&lt;/code&gt; calls
the application's session endpoint (upstream `fetch`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `liveKit` | [ILiveKitClient](/api/Vercel.AI.Mistral/ILiveKitClient/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

**Returns:** [MistralRealtimeModel](/api/Vercel.AI.Mistral/MistralRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L53)
