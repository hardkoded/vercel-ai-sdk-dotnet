---
title: "TranscribeOptions"
description: "Transcription options."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscribeOptions"
  hidden: true
---

<a id="Vercel_AI_TranscribeOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class TranscribeOptions
```


Transcription options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L400)

## Properties

<a id="Vercel_AI_TranscribeOptions_Audio_"></a>
<a id="Vercel_AI_TranscribeOptions_Audio"></a>

### Audio


```csharp
public AudioInput Audio { get; set; }
```


Audio.

**Value:** [AudioInput](/api/Vercel.AI.Provider/AudioInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L406)

<a id="Vercel_AI_TranscribeOptions_Model_"></a>
<a id="Vercel_AI_TranscribeOptions_Model"></a>

### Model


```csharp
public ITranscriptionModel? Model { get; set; }
```


Transcription model.

**Value:** [ITranscriptionModel](/api/Vercel.AI.Provider/ITranscriptionModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L403)
