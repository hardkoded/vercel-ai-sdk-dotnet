---
title: "MistralSpeechModelEntry"
description: "Speech model entry of the Mistral catalog. Maps to getMistralSpeechModelEntry's result."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralSpeechModelEntry"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralSpeechModelEntry"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralSpeechModelEntry
```


Speech model entry of the Mistral catalog. Maps to `getMistralSpeechModelEntry`'s result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L188)

## Properties

<a id="Vercel_AI_Mistral_MistralSpeechModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.speech`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L194)

<a id="Vercel_AI_Mistral_MistralSpeechModelEntry_Speech_"></a>
<a id="Vercel_AI_Mistral_MistralSpeechModelEntry_Speech"></a>

### Speech


```csharp
public MistralSpeechCapabilities Speech { get; init; }
```


Upstream `capabilities.speech`.

**Value:** [MistralSpeechCapabilities](/api/Vercel.AI.Mistral/MistralSpeechCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L191)
