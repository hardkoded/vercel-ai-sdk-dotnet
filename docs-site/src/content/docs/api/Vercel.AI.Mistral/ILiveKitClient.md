---
title: "ILiveKitClient"
description: "The part of the LiveKit client that MistralRealtimeTransport uses. Upstream loads the livekit-client module on connection; the port has no LiveKit client, so..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ILiveKitClient"
  hidden: true
---

<a id="Vercel_AI_Mistral_ILiveKitClient"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public interface ILiveKitClient
```


The part of the LiveKit client that [MistralRealtimeTransport](/api/Vercel.AI.Mistral/MistralRealtimeTransport/) uses. Upstream loads the
`livekit-client` module on connection; the port has no LiveKit client, so the host supplies one through this
interface. Maps to the `livekit-client` module (`Room` and `createLocalAudioTrack`).

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/LiveKitClient.cs#L14)
