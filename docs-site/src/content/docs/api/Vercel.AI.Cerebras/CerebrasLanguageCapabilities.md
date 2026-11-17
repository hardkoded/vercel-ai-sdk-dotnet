---
title: "CerebrasLanguageCapabilities"
description: "The language capabilities of a Cerebras model: the shared LanguageCapabilities plus StructuredOutput. Upstream LanguageCapabilities of cerebras-model-catalog..."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasLanguageCapabilities"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public sealed class CerebrasLanguageCapabilities
```


The language capabilities of a Cerebras model: the shared [LanguageCapabilities](/api/Vercel.AI/LanguageCapabilities/) plus
[StructuredOutput](#Vercel_AI_Cerebras_CerebrasLanguageCapabilities_StructuredOutput). Upstream `LanguageCapabilities` of `cerebras-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L40)

## Properties

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Inputs_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Inputs"></a>

### Inputs


```csharp
public LanguageInputCapabilities Inputs { get; init; }
```


Upstream `inputs`.

**Value:** [LanguageInputCapabilities](/api/Vercel.AI/LanguageInputCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L43)

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_ObjectGeneration_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_ObjectGeneration"></a>

### ObjectGeneration


```csharp
public ModelCapability ObjectGeneration { get; init; }
```


Upstream `objectGeneration`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L46)

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Reasoning_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Reasoning"></a>

### Reasoning


```csharp
public LanguageReasoningCapabilities Reasoning { get; init; }
```


Upstream `reasoning`.

**Value:** [LanguageReasoningCapabilities](/api/Vercel.AI/LanguageReasoningCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L52)

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Sampling_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Sampling"></a>

### Sampling


```csharp
public LanguageSamplingCapabilities Sampling { get; init; }
```


Upstream `sampling`.

**Value:** [LanguageSamplingCapabilities](/api/Vercel.AI/LanguageSamplingCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L55)

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_StructuredOutput_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_StructuredOutput"></a>

### StructuredOutput


```csharp
public CerebrasStructuredOutputCapabilities StructuredOutput { get; init; }
```


Upstream `structuredOutput`.

**Value:** [CerebrasStructuredOutputCapabilities](/api/Vercel.AI.Cerebras/CerebrasStructuredOutputCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L58)

<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Tools_"></a>
<a id="Vercel_AI_Cerebras_CerebrasLanguageCapabilities_Tools"></a>

### Tools


```csharp
public LanguageToolCapabilities Tools { get; init; }
```


Upstream `tools`.

**Value:** [LanguageToolCapabilities](/api/Vercel.AI/LanguageToolCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L49)
