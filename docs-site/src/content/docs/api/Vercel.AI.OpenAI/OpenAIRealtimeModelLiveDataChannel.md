---
title: "OpenAIRealtimeModelLiveDataChannel"
description: "Data channel permissions of a Live WebRTC client."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeModelLiveDataChannel"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeModelLiveDataChannel
```


Data channel permissions of a Live WebRTC client.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L22)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowAllClientEvents_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowAllClientEvents"></a>

### AllowAllClientEvents


```csharp
public bool AllowAllClientEvents { get; set; }
```


True for `allowedClientEvents: "all"`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L25)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowAllServerEvents_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowAllServerEvents"></a>

### AllowAllServerEvents


```csharp
public bool AllowAllServerEvents { get; set; }
```


True for `allowedServerEvents: "all"`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowedClientEvents_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowedClientEvents"></a>

### AllowedClientEvents


```csharp
public IReadOnlyList<string>? AllowedClientEvents { get; set; }
```


The client event types the data channel accepts.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L28)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowedServerEvents_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeModelLiveDataChannel_AllowedServerEvents"></a>

### AllowedServerEvents


```csharp
public IReadOnlyList<OpenAIRealtimeModelLiveServerEventSelector>? AllowedServerEvents { get; set; }
```


The server events the data channel forwards.

**Value:** [IReadOnlyList&lt;OpenAIRealtimeModelLiveServerEventSelector&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Live/OpenAIRealtimeModelLiveOptions.cs#L34)
