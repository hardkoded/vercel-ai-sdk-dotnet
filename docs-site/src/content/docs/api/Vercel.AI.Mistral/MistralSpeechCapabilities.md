---
title: "MistralSpeechCapabilities"
description: "Speech capabilities of a Mistral model. Maps to the catalog's SpeechCapabilities."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralSpeechCapabilities"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralSpeechCapabilities
```


Speech capabilities of a Mistral model. Maps to the catalog's `SpeechCapabilities`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L147)

## Properties

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_Instructions_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_Instructions"></a>

### Instructions


```csharp
public ModelCapability Instructions { get; init; }
```


Upstream `instructions`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L150)

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_NonStreaming_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_NonStreaming"></a>

### NonStreaming


```csharp
public bool NonStreaming { get; init; }
```


Upstream `nonStreaming`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L159)

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_OutputFormats_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_OutputFormats"></a>

### OutputFormats


```csharp
public IReadOnlyList<string> OutputFormats { get; init; }
```


Upstream `outputFormats`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L162)

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_ReferenceAudio_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_ReferenceAudio"></a>

### ReferenceAudio


```csharp
public ModelCapability ReferenceAudio { get; init; }
```


Upstream `referenceAudio`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L156)

<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_SavedVoices_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechCapabilities_SavedVoices"></a>

### SavedVoices


```csharp
public ModelCapability SavedVoices { get; init; }
```


Upstream `savedVoices`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L153)
