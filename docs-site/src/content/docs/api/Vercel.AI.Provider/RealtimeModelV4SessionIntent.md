---
title: "RealtimeModelV4SessionIntent"
description: "The realtime endpoint variant to open. Maps to RealtimeModelV4SessionIntent. Conversation is the conversational voice endpoint; Transcription is a dedicated..."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4SessionIntent"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4SessionIntent"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public enum RealtimeModelV4SessionIntent
```


The realtime endpoint variant to open. Maps to `RealtimeModelV4SessionIntent`.
[Conversation](#Vercel_AI_Provider_RealtimeModelV4SessionIntent_Conversation) is the conversational voice endpoint; [Transcription](#Vercel_AI_Provider_RealtimeModelV4SessionIntent_Transcription) is a dedicated
transcription endpoint. Providers fail fast when an intent is not supported by their endpoint and codec.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L12)

## Fields

<a id="Vercel_AI_Provider_RealtimeModelV4SessionIntent_Conversation"></a>

### Conversation


```csharp
Conversation = 0
```


The conversational voice endpoint.

**Value:** [RealtimeModelV4SessionIntent](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L15)

<a id="Vercel_AI_Provider_RealtimeModelV4SessionIntent_Transcription"></a>

### Transcription


```csharp
Transcription = 1
```


A dedicated transcription endpoint.

**Value:** [RealtimeModelV4SessionIntent](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4ServerConnection.cs#L18)
