---
title: "GoogleRealtimeClientSecret"
description: "An ephemeral Gemini Live auth token and the WebSocket URL it opens."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeClientSecret"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeClientSecret"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeClientSecret
```


An ephemeral Gemini Live auth token and the WebSocket URL it opens.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L204)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_ExpiresAt_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_ExpiresAt"></a>

### ExpiresAt


```csharp
public long? ExpiresAt { get; }
```


Token expiry as unix seconds, when the provider sent one.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L220)

<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_Token_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_Token"></a>

### Token


```csharp
public string Token { get; }
```


Auth token name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L214)

<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_Url_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeClientSecret_Url"></a>

### Url


```csharp
public string Url { get; }
```


Constrained Live API WebSocket URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleProvider.cs#L217)
