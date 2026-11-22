---
title: "LiveKitMediaStream"
description: "A caller-owned stream. Maps to the browser MediaStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "LiveKitMediaStream"
  hidden: true
---

<a id="Vercel_AI_Mistral_LiveKitMediaStream"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public abstract class LiveKitMediaStream : RealtimeModelV4MediaStream
```


A caller-owned stream. Maps to the browser `MediaStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [RealtimeModelV4MediaStream](/api/Vercel.AI.Provider/RealtimeModelV4MediaStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/LiveKitClient.cs#L40)

## Methods

<a id="Vercel_AI_Mistral_LiveKitMediaStream_GetAudioTracks_"></a>
<a id="Vercel_AI_Mistral_LiveKitMediaStream_GetAudioTracks"></a>

### GetAudioTracks()


```csharp
public abstract IReadOnlyList<ILiveKitMediaTrack> GetAudioTracks()
```


Maps to `getAudioTracks`.

**Returns:** [IReadOnlyList&lt;ILiveKitMediaTrack&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/LiveKitClient.cs#L43)
