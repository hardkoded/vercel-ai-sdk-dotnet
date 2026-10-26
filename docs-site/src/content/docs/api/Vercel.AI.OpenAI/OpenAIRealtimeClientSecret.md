---
title: "OpenAIRealtimeClientSecret"
description: "A realtime client secret."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIRealtimeClientSecret"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIRealtimeClientSecret
```


A realtime client secret.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L305)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_ExpiresAt_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Expiry as unix seconds, when the provider sent one.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L321)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_Token_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_Token"></a>

### Token


```csharp
public string Token { get; }
```


Client secret value.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L315)

<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_Url_"></a>
<a id="Vercel_AI_OpenAI_OpenAIRealtimeClientSecret_Url"></a>

### Url


```csharp
public string Url { get; }
```


WebSocket URL that includes the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIProvider.cs#L318)
