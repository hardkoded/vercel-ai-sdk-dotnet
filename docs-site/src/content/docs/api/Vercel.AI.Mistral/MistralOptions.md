---
title: "MistralOptions"
description: "Mistral settings. Maps to MistralProviderSettings."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public class MistralOptions : OpenAICompatibleOptions
```


Mistral settings. Maps to `MistralProviderSettings`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L14)

## Properties

<a id="Vercel_AI_Mistral_MistralOptions_WebSocket_"></a>
<a id="Vercel_AI_Mistral_MistralOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation. Required in runtimes whose default WebSocket does not support headers for
realtime transcription. Maps to `webSocket`.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralProvider.cs#L20)
