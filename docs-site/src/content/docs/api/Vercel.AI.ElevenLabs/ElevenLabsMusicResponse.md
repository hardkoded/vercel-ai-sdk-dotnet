---
title: "ElevenLabsMusicResponse"
description: "Reads the multipart/mixed response of /v1/music/detailed. Maps to elevenlabs-music-response.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsMusicResponse"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicResponse"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public static class ElevenLabsMusicResponse
```


Reads the `multipart/mixed` response of `/v1/music/detailed`. Maps to `elevenlabs-music-response.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L56)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicResponse_CreateMusicTimestampsResponseHandler_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsMusicResponse_CreateMusicTimestampsResponseHandler"></a>

### CreateMusicTimestampsResponseHandler()


```csharp
public static JsonResponseHandler<ElevenLabsMusicTimestampsResponse> CreateMusicTimestampsResponseHandler()
```


Creates a handler for the detailed music response: JSON metadata and binary audio as `multipart/mixed`.
Maps to `createMusicTimestampsResponseHandler`.

**Returns:** `JsonResponseHandler&lt;ElevenLabsMusicTimestampsResponse&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsMusicResponse.cs#L82)
