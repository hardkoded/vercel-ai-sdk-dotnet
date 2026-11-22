---
title: "GatewayRealtimeApi"
description: "The Gateway API a realtime model id is routed to. Maps to GatewayRealtimeOptions.api."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayRealtimeApi"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayRealtimeApi"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public enum GatewayRealtimeApi
```


The Gateway API a realtime model id is routed to. Maps to `GatewayRealtimeOptions.api`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelSettings.cs#L57)

## Fields

<a id="Vercel_AI_Gateway_GatewayRealtimeApi_Live"></a>

### Live


```csharp
Live = 0
```


`'live'`: the continuous Live API on `/live-model`.

**Value:** [GatewayRealtimeApi](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelSettings.cs#L60)

<a id="Vercel_AI_Gateway_GatewayRealtimeApi_Realtime"></a>

### Realtime


```csharp
Realtime = 1
```


`'realtime'`: the turn-based realtime API on `/realtime-model`.

**Value:** [GatewayRealtimeApi](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelSettings.cs#L63)
