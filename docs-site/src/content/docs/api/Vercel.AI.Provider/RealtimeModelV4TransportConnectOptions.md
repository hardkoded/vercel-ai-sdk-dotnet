---
title: "RealtimeModelV4TransportConnectOptions"
description: "Arguments of ConnectAsync(RealtimeModelV4TransportConnectOptions). Maps to the options of connect."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4TransportConnectOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4TransportConnectOptions
```


Arguments of [ConnectAsync(RealtimeModelV4TransportConnectOptions)](/api/Vercel.AI.Provider/IRealtimeModelV4Transport/#Vercel_AI_Provider_IRealtimeModelV4Transport_ConnectAsync_Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_). Maps to the options of `connect`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L55)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_AbortSignal_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; init; }
```


Cancels the connection. Maps to `abortSignal`.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L70)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Api_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Api"></a>

### Api


```csharp
public string Api { get; init; }
```


Application endpoint for creating and closing provider sessions. Maps to `api`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L58)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Capture_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Capture"></a>

### Capture


```csharp
public bool? Capture { get; init; }
```


Whether to capture local media on connect. Maps to `capture`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L67)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_SessionConfig_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_SessionConfig"></a>

### SessionConfig


```csharp
public RealtimeModelV4SessionConfig? SessionConfig { get; init; }
```


Session configuration. Maps to `sessionConfig`.

**Value:** [RealtimeModelV4SessionConfig](/api/Vercel.AI.Provider/RealtimeModelV4SessionConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L61)

<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Stream_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4TransportConnectOptions_Stream"></a>

### Stream


```csharp
public RealtimeModelV4MediaStream? Stream { get; init; }
```


Caller-owned media. The transport must never stop its tracks. Maps to `stream`.

**Value:** [RealtimeModelV4MediaStream](/api/Vercel.AI.Provider/RealtimeModelV4MediaStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L64)
