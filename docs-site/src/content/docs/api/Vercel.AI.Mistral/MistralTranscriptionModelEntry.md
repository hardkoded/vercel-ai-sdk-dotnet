---
title: "MistralTranscriptionModelEntry"
description: "Transcription model entry of the Mistral catalog. Maps to getMistralTranscriptionModelEntry's result."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralTranscriptionModelEntry"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralTranscriptionModelEntry"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralTranscriptionModelEntry
```


Transcription model entry of the Mistral catalog. Maps to `getMistralTranscriptionModelEntry`'s result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L177)

## Properties

<a id="Vercel_AI_Mistral_MistralTranscriptionModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.transcription`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L183)

<a id="Vercel_AI_Mistral_MistralTranscriptionModelEntry_Transcription_"></a>
<a id="Vercel_AI_Mistral_MistralTranscriptionModelEntry_Transcription"></a>

### Transcription


```csharp
public MistralTranscriptionCapabilities Transcription { get; init; }
```


Upstream `capabilities.transcription`.

**Value:** [MistralTranscriptionCapabilities](/api/Vercel.AI.Mistral/MistralTranscriptionCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L180)
