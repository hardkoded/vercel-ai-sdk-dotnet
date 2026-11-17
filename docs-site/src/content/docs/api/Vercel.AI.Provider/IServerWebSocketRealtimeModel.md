---
title: "IServerWebSocketRealtimeModel"
description: "A realtime model that can open a server-owned WebSocket. Maps to the optional RealtimeModelV4.getServerWebSocketConfig. It is a separate interface because ne..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IServerWebSocketRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Provider_IServerWebSocketRealtimeModel"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public interface IServerWebSocketRealtimeModel : IRealtimeModel
```


A realtime model that can open a server-owned WebSocket. Maps to the optional
`RealtimeModelV4.getServerWebSocketConfig`. It is a separate interface because
`netstandard2.0` has no default interface members.

**Derived:** [GatewayRealtimeModel](/api/Vercel.AI.Gateway/GatewayRealtimeModel/), [GoogleRealtimeModel](/api/Vercel.AI.Google/GoogleRealtimeModel/), [OpenAIRealtimeModel](/api/Vercel.AI.OpenAI/OpenAIRealtimeModel/), [OpenAIRealtimeModelLive](/api/Vercel.AI.OpenAI/OpenAIRealtimeModelLive/), [XaiRealtimeModel](/api/Vercel.AI.Xai/XaiRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L55)
