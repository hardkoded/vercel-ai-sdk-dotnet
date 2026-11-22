---
title: "MistralRealtimeTransport"
description: "LiveKit stays inside the Mistral provider and is loaded only on connection. Maps to MistralRealtimeTransport."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeTransport"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeTransport"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeTransport : IRealtimeModelV4Transport, IDisposable
```


LiveKit stays inside the Mistral provider and is loaded only on connection. Maps to `MistralRealtimeTransport`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IRealtimeModelV4Transport](/api/Vercel.AI.Provider/IRealtimeModelV4Transport/), [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L13)

## Constructors

<a id="Vercel_AI_Mistral_MistralRealtimeTransport__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport__ctor_Vercel_AI_Provider_RealtimeModelV4TransportOptions_Vercel_AI_Mistral_ILiveKitClient_System_Net_Http_HttpClient_"></a>

### MistralRealtimeTransport(RealtimeModelV4TransportOptions, ILiveKitClient?, HttpClient?)


```csharp
public MistralRealtimeTransport(RealtimeModelV4TransportOptions options, ILiveKitClient? liveKit = null, HttpClient? httpClient = null)
```


Creates a transport. It opens no room, makes no request and acquires no media until
[ConnectAsync(RealtimeModelV4TransportConnectOptions)](#Vercel_AI_Mistral_MistralRealtimeTransport_ConnectAsync_Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_). &lt;code class="paramref"&gt;liveKit&lt;/code&gt; is the host's LiveKit client (upstream loads
`livekit-client`) and &lt;code class="paramref"&gt;httpClient&lt;/code&gt; calls the application's session endpoint (upstream
`fetch`).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [RealtimeModelV4TransportOptions](/api/Vercel.AI.Provider/RealtimeModelV4TransportOptions/) |  |
| `liveKit` | [ILiveKitClient](/api/Vercel.AI.Mistral/ILiveKitClient/) |  |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L61)

## Methods

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_CloseAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_CloseAsync"></a>

### CloseAsync()


```csharp
public Task CloseAsync()
```


Releases local media and awaits remote session cleanup. Must be idempotent. Maps to `close`.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L340)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_ConnectAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_ConnectAsync_Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_"></a>

### ConnectAsync(RealtimeModelV4TransportConnectOptions)


```csharp
public Task ConnectAsync(RealtimeModelV4TransportConnectOptions options)
```


Opens the connection. Maps to `connect`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [RealtimeModelV4TransportConnectOptions](/api/Vercel.AI.Provider/RealtimeModelV4TransportConnectOptions/) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L73)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_Dispose_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Closes the transport without waiting. Errors go to `OnError`. Maps to `dispose`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L423)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_FinishAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_FinishAsync"></a>

### FinishAsync()


```csharp
public Task FinishAsync()
```


Drains already accepted incoming events. Maps to `finish`.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L320)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_ResumePlaybackAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_ResumePlaybackAsync"></a>

### ResumePlaybackAsync()


```csharp
public Task ResumePlaybackAsync()
```


Resumes remote playback. Maps to `resumePlayback`.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L302)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_SendEventAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_SendEventAsync_Vercel_AI_Provider_RealtimeModelV4ClientEvent_System_Func_System_Boolean__"></a>

### `SendEventAsync(RealtimeModelV4ClientEvent, Func<bool>?)`


```csharp
public Task SendEventAsync(RealtimeModelV4ClientEvent clientEvent, Func<bool>? shouldSend = null)
```


Sends an event. &lt;code class="paramref"&gt;shouldSend&lt;/code&gt; is checked right before sending. Maps to `sendEvent`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `clientEvent` | [RealtimeModelV4ClientEvent](/api/Vercel.AI.Provider/RealtimeModelV4ClientEvent/) |  |
| `shouldSend` | [Func&lt;bool&gt;](https://learn.microsoft.com/dotnet/api/system.func-1) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L117)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StartCaptureAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StartCaptureAsync_Vercel_AI_Provider_RealtimeModelV4MediaStream_"></a>

### StartCaptureAsync(RealtimeModelV4MediaStream?)


```csharp
public Task StartCaptureAsync(RealtimeModelV4MediaStream? stream = null)
```


Starts capturing local media. Maps to `startCapture`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [RealtimeModelV4MediaStream](/api/Vercel.AI.Provider/RealtimeModelV4MediaStream/) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L176)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StopCaptureAsync_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StopCaptureAsync"></a>

### StopCaptureAsync()


```csharp
public Task StopCaptureAsync()
```


Stops capturing local media. Maps to `stopCapture`.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L278)

<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StopPlayback_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTransport_StopPlayback"></a>

### StopPlayback()


```csharp
public void StopPlayback()
```


Stops remote playback. Maps to `stopPlayback`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeTransport.cs#L290)
