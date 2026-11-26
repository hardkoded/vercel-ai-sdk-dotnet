---
title: "QuiverAIImageModelEntry"
description: "A catalog entry of a QuiverAI image model. Maps to the result of getQuiverAIImageModelEntry."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageModelEntry"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIImageModelEntry
```


A catalog entry of a QuiverAI image model. Maps to the result of `getQuiverAIImageModelEntry`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L113)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_Capabilities_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_Capabilities"></a>

### Capabilities


```csharp
public QuiverAIImageCapabilities Capabilities { get; init; }
```


Maps to `capabilities.image`.

**Value:** [QuiverAIImageCapabilities](/api/Vercel.AI.QuiverAI/QuiverAIImageCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L116)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; init; }
```


Maps to `providerOptions.image`.

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L119)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_RequestConfig_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelEntry_RequestConfig"></a>

### RequestConfig


```csharp
public QuiverAIImageRequestConfig RequestConfig { get; init; }
```


Maps to `requestConfig.image`.

**Value:** [QuiverAIImageRequestConfig](/api/Vercel.AI.QuiverAI/QuiverAIImageRequestConfig/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIModelCatalog.cs#L123)
