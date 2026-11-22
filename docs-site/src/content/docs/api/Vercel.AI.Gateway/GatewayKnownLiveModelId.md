---
title: "GatewayKnownLiveModelId"
description: "Realtime model ids that use the continuous Live API by default. Maps to gatewayLiveModelIds and GatewayKnownLiveModelId. Other ids use the realtime API unles..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayKnownLiveModelId"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewayKnownLiveModelId"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class GatewayKnownLiveModelId
```


Realtime model ids that use the continuous Live API by default. Maps to `gatewayLiveModelIds` and
`GatewayKnownLiveModelId`. Other ids use the realtime API unless [Live](/api/Vercel.AI.Gateway/GatewayRealtimeApi/#Vercel_AI_Gateway_GatewayRealtimeApi_Live) is passed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelSettings.cs#L49)

## Properties

<a id="Vercel_AI_Gateway_GatewayKnownLiveModelId_GatewayLiveModelIds_"></a>
<a id="Vercel_AI_Gateway_GatewayKnownLiveModelId_GatewayLiveModelIds"></a>

### GatewayLiveModelIds


```csharp
public static IReadOnlyList<string> GatewayLiveModelIds { get; }
```


Maps to `gatewayLiveModelIds`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayRealtimeModelSettings.cs#L52)
