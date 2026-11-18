---
title: "DeepSeekLanguageModelEntry"
description: "A catalog entry of a DeepSeek language model. Upstream DeepSeekLanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekLanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelEntry"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekLanguageModelEntry
```


A catalog entry of a DeepSeek language model. Upstream `DeepSeekLanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L143)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelEntry_Language_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelEntry_Language"></a>

### Language


```csharp
public DeepSeekLanguageCapabilities Language { get; init; }
```


Upstream `capabilities.language`.

**Value:** [DeepSeekLanguageCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekLanguageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L146)

<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekLanguageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.language`: support of each option in [Language](/api/Vercel.AI.DeepSeek/DeepSeekProviderOptions/#Vercel_AI_DeepSeek_DeepSeekProviderOptions_Language).

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L149)
