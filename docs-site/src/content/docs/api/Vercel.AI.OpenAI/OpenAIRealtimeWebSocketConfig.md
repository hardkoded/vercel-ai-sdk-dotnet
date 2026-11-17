---
title: "OpenAIRealtimeWebSocketConfig"
description: "URL and subprotocols a client opens a WebSocket with. Maps to the result of getWebSocketConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeWebSocketConfig"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeWebSocketConfig"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeWebSocketConfig
```


URL and subprotocols a client opens a WebSocket with. Maps to the result of `getWebSocketConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L33)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeWebSocketConfig_Protocols_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeWebSocketConfig_Protocols"></a>

### Protocols


```csharp
public IReadOnlyList<string>? Protocols { get; }
```


Subprotocols to offer.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L45)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeWebSocketConfig_Url_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeWebSocketConfig_Url"></a>

### Url


```csharp
public string Url { get; }
```


The WebSocket URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Realtime/OpenAIRealtimeModel.cs#L42)
