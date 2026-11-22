---
title: "MistralRealtimeTtsOptions"
description: "Speech synthesis settings. sessionConfig.voice overrides VoiceId. Maps to tts."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralRealtimeTtsOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralRealtimeTtsOptions
```


Speech synthesis settings. `sessionConfig.voice` overrides [VoiceId](#Vercel_AI_Mistral_MistralRealtimeTtsOptions_VoiceId). Maps to `tts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L51)

## Properties

<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_Model_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_Model"></a>

### Model


```csharp
public string? Model { get; init; }
```


Upstream `model`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L54)

<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_PhonemeHints_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_PhonemeHints"></a>

### PhonemeHints


```csharp
public IReadOnlyDictionary<string, IReadOnlyList<string>>? PhonemeHints { get; init; }
```


Upstream `phonemeHints`.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L60)

<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_VoiceId_"></a>
<a id="Vercel_AI_Mistral_MistralRealtimeTtsOptions_VoiceId"></a>

### VoiceId


```csharp
public string? VoiceId { get; init; }
```


Upstream `voiceId`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralRealtimeModelOptions.cs#L57)
