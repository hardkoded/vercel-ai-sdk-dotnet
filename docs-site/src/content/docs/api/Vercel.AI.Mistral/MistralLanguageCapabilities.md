---
title: "MistralLanguageCapabilities"
description: "Language capabilities of a Mistral model. Maps to the catalog's LanguageCapabilities."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralLanguageCapabilities"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralLanguageCapabilities
```


Language capabilities of a Mistral model. Maps to the catalog's `LanguageCapabilities`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L94)

## Properties

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Endpoints_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Endpoints"></a>

### Endpoints


```csharp
public MistralLanguageEndpointCapabilities Endpoints { get; init; }
```


Upstream `endpoints`.

**Value:** [MistralLanguageEndpointCapabilities](/api/Vercel.AI.Mistral/MistralLanguageEndpointCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L112)

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Inputs_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Inputs"></a>

### Inputs


```csharp
public LanguageInputCapabilities Inputs { get; init; }
```


Upstream `inputs`.

**Value:** [LanguageInputCapabilities](/api/Vercel.AI/LanguageInputCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L97)

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_ObjectGeneration_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_ObjectGeneration"></a>

### ObjectGeneration


```csharp
public ModelCapability ObjectGeneration { get; init; }
```


Upstream `objectGeneration`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L100)

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Reasoning_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Reasoning"></a>

### Reasoning


```csharp
public MistralReasoningCapabilities Reasoning { get; init; }
```


Upstream `reasoning`, the Chat API.

**Value:** [MistralReasoningCapabilities](/api/Vercel.AI.Mistral/MistralReasoningCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L106)

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Sampling_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Sampling"></a>

### Sampling


```csharp
public LanguageSamplingCapabilities Sampling { get; init; }
```


Upstream `sampling`.

**Value:** [LanguageSamplingCapabilities](/api/Vercel.AI/LanguageSamplingCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L109)

<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Tools_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageCapabilities_Tools"></a>

### Tools


```csharp
public LanguageToolCapabilities Tools { get; init; }
```


Upstream `tools`.

**Value:** [LanguageToolCapabilities](/api/Vercel.AI/LanguageToolCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralModelCatalog.cs#L103)
