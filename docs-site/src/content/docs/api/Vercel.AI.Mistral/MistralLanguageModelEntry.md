---
title: "MistralLanguageModelEntry"
description: "Language model entry of the Mistral catalog. Maps to MistralLanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralLanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralLanguageModelEntry"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralLanguageModelEntry
```


Language model entry of the Mistral catalog. Maps to `MistralLanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L166)

## Properties

<a id="Vercel_AI_Mistral_MistralLanguageModelEntry_Language_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelEntry_Language"></a>

### Language


```csharp
public MistralLanguageCapabilities Language { get; init; }
```


Upstream `capabilities.language`.

**Value:** [MistralLanguageCapabilities](/api/Vercel.AI.Mistral/MistralLanguageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L169)

<a id="Vercel_AI_Mistral_MistralLanguageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.language`: support of each option in [Language](/api/Vercel.AI.Mistral/MistralProviderOptions/#Vercel_AI_Mistral_MistralProviderOptions_Language).

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L172)
