---
title: "CartesiaTranscriptionCapabilities"
description: "Capabilities of a Cartesia transcription model id. Maps to getCartesiaTranscriptionModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "CartesiaTranscriptionCapabilities"
  hidden: true
---

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionCapabilities"></a>

Namespace: [Vercel.AI.Cartesia](/api/Vercel.AI.Cartesia/)


```csharp
public sealed class CartesiaTranscriptionCapabilities
```


Capabilities of a Cartesia transcription model id. Maps to `getCartesiaTranscriptionModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaStreamingTranscription.cs#L14)

## Properties

<a id="Vercel_AI_Cartesia_CartesiaTranscriptionCapabilities_Streaming_"></a>
<a id="Vercel_AI_Cartesia_CartesiaTranscriptionCapabilities_Streaming"></a>

### Streaming


```csharp
public bool Streaming { get; }
```


True when the model streams over WebSocket. False for the batch-only Ink Whisper family.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cartesia/CartesiaStreamingTranscription.cs#L22)
