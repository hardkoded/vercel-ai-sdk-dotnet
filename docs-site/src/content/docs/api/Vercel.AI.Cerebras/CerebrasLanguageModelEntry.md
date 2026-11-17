---
title: "CerebrasLanguageModelEntry"
description: "A catalog entry of a Cerebras language model. Upstream CerebrasLanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasLanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelEntry"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasLanguageModelEntry
```


A catalog entry of a Cerebras language model. Upstream `CerebrasLanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L62)

## Properties

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelEntry_Language_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelEntry_Language"></a>

### Language


```csharp
public CerebrasLanguageCapabilities Language { get; init; }
```


Upstream `capabilities.language`.

**Value:** [CerebrasLanguageCapabilities](/api/Vercel.AI.Cerebras/CerebrasLanguageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L65)

<a id="Vercel_AI_Cerebras_CerebrasLanguageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Upstream `providerOptions.language`: support of each option in [Language](/api/Vercel.AI.Cerebras/CerebrasProviderOptions/#Vercel_AI_Cerebras_CerebrasProviderOptions_Language).

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L68)
