---
title: "IProviderTransportRealtimeModel"
description: "A realtime model that creates a provider-owned transport. Maps to the optional RealtimeModelV4.createTransport. It is a separate interface because netstandar..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IProviderTransportRealtimeModel"
  hidden: true
---

<a id="Vercel_AI_Provider_IProviderTransportRealtimeModel"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public interface IProviderTransportRealtimeModel : IRealtimeModel
```


A realtime model that creates a provider-owned transport. Maps to the optional `RealtimeModelV4.createTransport`.
It is a separate interface because `netstandard2.0` has no default interface members.

**Derived:** [MistralRealtimeModel](/api/Vercel.AI.Mistral/MistralRealtimeModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L109)
