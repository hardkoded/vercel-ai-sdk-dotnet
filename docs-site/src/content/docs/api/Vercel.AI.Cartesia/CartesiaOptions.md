---
title: "CartesiaOptions"
description: "Cartesia settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaOptions"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaOptions"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public class CartesiaOptions : OpenAICompatibleOptions
```


Cartesia settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAICompatibleOptions](/api/Vercel.AI.OpenAICompatible/OpenAICompatibleOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L18)

## Properties

<a id="Vercel_AI_Cartesia_CartesiaOptions_Version_"></a>
<a id="Vercel_AI_Cartesia_CartesiaOptions_Version"></a>

### Version


```csharp
public string? Version { get; set; }
```


The Cartesia API version, sent as the `Cartesia-Version` header. Defaults to `2026-03-01`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L21)

<a id="Vercel_AI_Cartesia_CartesiaOptions_WebSocket_"></a>
<a id="Vercel_AI_Cartesia_CartesiaOptions_WebSocket"></a>

### WebSocket


```csharp
public WebSocketFactory? WebSocket { get; set; }
```


Custom WebSocket implementation for Ink 2 streaming transcription. Defaults to `ClientWebSocket`.

**Value:** [WebSocketFactory](/api/Vercel.AI.ProviderUtils/WebSocketFactory/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaProvider.cs#L24)
