---
title: "XaiOptions"
description: "xAI settings. Maps to XaiProviderSettings."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public class XaiOptions : OpenAICompatibleOptions
```


xAI settings. Maps to `XaiProviderSettings`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L14)

## Properties

<a id="Vercel_AI_Xai_XaiOptions_WebSocket_"></a>
<a id="Vercel_AI_Xai_XaiOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation. Required in runtimes whose default WebSocket does not support headers for xAI
streaming STT. Maps to `webSocket`.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiProvider.cs#L20)
