---
title: "DeepgramTranscriptionCapabilities"
description: "Capabilities of a Deepgram transcription model."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramTranscriptionCapabilities"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionCapabilities"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramTranscriptionCapabilities
```


Capabilities of a Deepgram transcription model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramModelCatalog.cs#L8)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramTranscriptionCapabilities_Streaming_"></a>
<a id="Vercel_AI_Deepgram_DeepgramTranscriptionCapabilities_Streaming"></a>

### Streaming


```csharp
public bool Streaming { get; }
```


True when the model streams over `/v1/listen`. Flux models use `/v2/listen` and are not supported.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramModelCatalog.cs#L16)
