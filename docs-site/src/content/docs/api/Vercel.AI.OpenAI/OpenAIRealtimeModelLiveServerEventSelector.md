---
title: "OpenAIRealtimeModelLiveServerEventSelector"
description: "A server event the Live data channel forwards to the client."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModelLiveServerEventSelector"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveServerEventSelector"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModelLiveServerEventSelector
```


A server event the Live data channel forwards to the client.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L12)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveServerEventSelector_ResponseEvent_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveServerEventSelector_ResponseEvent"></a>

### ResponseEvent


```csharp
public string? ResponseEvent { get; set; }
```


Required for `response.event` and forbidden for other event types.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L18)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveServerEventSelector_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveServerEventSelector_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Event type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L15)
