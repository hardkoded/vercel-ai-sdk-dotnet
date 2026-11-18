---
title: "DeepSeekSamplingCapabilities"
description: "Sampling support of a DeepSeek language model. Upstream LanguageCapabilities.sampling."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekSamplingCapabilities"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public sealed class DeepSeekSamplingCapabilities
```


Sampling support of a DeepSeek language model. Upstream `LanguageCapabilities.sampling`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L79)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Logprobs_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Logprobs"></a>

### Logprobs


```csharp
public ModelCapability Logprobs { get; init; }
```


Upstream `logprobs`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L88)

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Penalties_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Penalties"></a>

### Penalties


```csharp
public bool Penalties { get; init; }
```


Upstream `penalties`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L91)

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Seed_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Seed"></a>

### Seed


```csharp
public bool Seed { get; init; }
```


Upstream `seed`. Always false.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L97)

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Temperature_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_Temperature"></a>

### Temperature


```csharp
public ModelCapability Temperature { get; init; }
```


Upstream `temperature`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L82)

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_TopK_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_TopK"></a>

### TopK


```csharp
public bool TopK { get; init; }
```


Upstream `topK`. Always false.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L94)

<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_TopP_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekSamplingCapabilities_TopP"></a>

### TopP


```csharp
public ModelCapability TopP { get; init; }
```


Upstream `topP`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L85)
