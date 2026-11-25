---
title: "AssemblyAIRedactPiiAudioOptions"
description: "Options for PII-redacted audio files. Maps to redactPiiAudioOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIRedactPiiAudioOptions"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIRedactPiiAudioOptions"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAIRedactPiiAudioOptions
```


Options for PII-redacted audio files. Maps to `redactPiiAudioOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L38)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIRedactPiiAudioOptions_OverrideAudioRedactionMethod_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIRedactPiiAudioOptions_OverrideAudioRedactionMethod"></a>

### OverrideAudioRedactionMethod


```csharp
public string? OverrideAudioRedactionMethod { get; init; }
```


Redaction method; set to `silence` to replace PII with silence. Maps to `overrideAudioRedactionMethod`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L44)

<a id="Vercel_AI_AssemblyAI_AssemblyAIRedactPiiAudioOptions_ReturnRedactedNoSpeechAudio_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIRedactPiiAudioOptions_ReturnRedactedNoSpeechAudio"></a>

### ReturnRedactedNoSpeechAudio


```csharp
public bool? ReturnRedactedNoSpeechAudio { get; init; }
```


Return redacted audio even for files without detected speech. Maps to `returnRedactedNoSpeechAudio`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAITranscriptionModelOptions.cs#L41)
