---
title: "RealtimeModelV4MediaStream"
description: "Media owned by the host application and handed to a provider transport. Maps to RealtimeModelV4MediaStream (the browser MediaStream). The host defines the co..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4MediaStream"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4MediaStream"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class RealtimeModelV4MediaStream
```


Media owned by the host application and handed to a provider transport. Maps to `RealtimeModelV4MediaStream`
(the browser `MediaStream`). The host defines the concrete type; a transport must never stop the tracks of a
caller-owned stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [LiveKitMediaStream](/api/Vercel.AI.Mistral/LiveKitMediaStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Transport.cs#L12)
