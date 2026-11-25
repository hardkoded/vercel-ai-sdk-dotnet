---
title: "QuiverAILanguageModelEntry"
description: "A catalog entry of a QuiverAI language model. Maps to the result of getQuiverAILanguageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAILanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAILanguageModelEntry
```


A catalog entry of a QuiverAI language model. Maps to the result of `getQuiverAILanguageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L99)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_Capabilities_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_Capabilities"></a>

### Capabilities


```csharp
public LanguageCapabilities Capabilities { get; init; }
```


Maps to `capabilities.language`.

**Value:** [LanguageCapabilities](/api/Vercel.AI/LanguageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L102)

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Maps to `providerOptions.language`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L105)

<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_RequestConfig_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAILanguageModelEntry_RequestConfig"></a>

### RequestConfig


```csharp
public QuiverAILanguageRequestConfig RequestConfig { get; init; }
```


Maps to `requestConfig.language`.

**Value:** [QuiverAILanguageRequestConfig](/api/Vercel.AI.QuiverAI/QuiverAILanguageRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L109)
