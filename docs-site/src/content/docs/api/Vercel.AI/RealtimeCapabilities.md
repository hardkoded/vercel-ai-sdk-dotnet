---
title: "RealtimeCapabilities"
description: "Upstream ModelCapabilities.realtime."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeCapabilities"
  hidden: true
---

<a id="Vercel_AI_RealtimeCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class RealtimeCapabilities
```


Upstream `ModelCapabilities.realtime`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L340)

## Properties

<a id="Vercel_AI_RealtimeCapabilities_Connections_"></a>
<a id="Vercel_AI_RealtimeCapabilities_Connections"></a>

### Connections


```csharp
public IReadOnlyList<string> Connections { get; init; }
```


Upstream `connections`: `client-secret-websocket`, `server-websocket`, `webrtc` or `provider`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L349)

<a id="Vercel_AI_RealtimeCapabilities_Conversation_"></a>
<a id="Vercel_AI_RealtimeCapabilities_Conversation"></a>

### Conversation


```csharp
public string Conversation { get; init; }
```


Upstream `conversation`: `continuous` or `turn-based`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L343)

<a id="Vercel_AI_RealtimeCapabilities_Finalization_"></a>
<a id="Vercel_AI_RealtimeCapabilities_Finalization"></a>

### Finalization


```csharp
public string? Finalization { get; init; }
```


Upstream `finalization`: `session-close` or `transport-close`. Null preserves transport-close finalization.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L355)

<a id="Vercel_AI_RealtimeCapabilities_Startup_"></a>
<a id="Vercel_AI_RealtimeCapabilities_Startup"></a>

### Startup


```csharp
public string? Startup { get; init; }
```


Upstream `startup`: `session-start` or `session-update`. Null preserves session-update startup.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L352)

<a id="Vercel_AI_RealtimeCapabilities_Transports_"></a>
<a id="Vercel_AI_RealtimeCapabilities_Transports"></a>

### Transports


```csharp
public IReadOnlyList<string> Transports { get; init; }
```


Upstream `transports`: `websocket` or `webrtc`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L346)
