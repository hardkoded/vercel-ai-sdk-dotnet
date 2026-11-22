---
title: "MistralRealtimeSession"
description: "Credentials for one browser voice conversation. Maps to MistralRealtimeSession."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeSession"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeSession"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeSession
```


Credentials for one browser voice conversation. Maps to `MistralRealtimeSession`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L14)

## Constructors

<a id="Vercel_AI_Mistral_MistralRealtimeSession__ctor_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSession__ctor_System_String_System_String_System_String_"></a>

### MistralRealtimeSession(string, string, string)


```csharp
public MistralRealtimeSession(string id, string participantToken, string serverUrl)
```


Creates a session.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `participantToken` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `serverUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L17)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeSession_Id_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSession_Id"></a>

### Id


```csharp
public string Id { get; }
```


Session id. Maps to `id`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L25)

<a id="Vercel_AI_Mistral_MistralRealtimeSession_ParticipantToken_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSession_ParticipantToken"></a>

### ParticipantToken


```csharp
public string ParticipantToken { get; }
```


Token the browser joins with. Maps to `participantToken`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L28)

<a id="Vercel_AI_Mistral_MistralRealtimeSession_ServerUrl_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeSession_ServerUrl"></a>

### ServerUrl


```csharp
public string ServerUrl { get; }
```


LiveKit server URL. Maps to `serverUrl`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeFactory.cs#L31)
