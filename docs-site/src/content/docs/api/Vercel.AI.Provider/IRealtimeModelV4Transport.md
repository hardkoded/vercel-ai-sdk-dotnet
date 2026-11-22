---
title: "IRealtimeModelV4Transport"
description: "Transport for providers whose media protocol requires its own client. Maps to RealtimeModelV4Transport. Construction must not acquire media or open a connect..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IRealtimeModelV4Transport"
  hidden: true
---

<a id="Vercel_AI_Provider_IRealtimeModelV4Transport"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public interface IRealtimeModelV4Transport : IDisposable
```


Transport for providers whose media protocol requires its own client. Maps to `RealtimeModelV4Transport`.
Construction must not acquire media or open a connection. Provider-specific dependencies and session
setup and cleanup belong to the implementation.

**Derived:** [MistralRealtimeTransport](/api/Vercel.AI.Mistral/MistralRealtimeTransport/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L78)
