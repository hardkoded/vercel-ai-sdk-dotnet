---
title: "TencentLanguageModelEntry"
description: "The catalog entry of a language model. Upstream getTencentLanguageModelEntry result."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentLanguageModelEntry"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentLanguageModelEntry
```


The catalog entry of a language model. Upstream `getTencentLanguageModelEntry` result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L262)

## Properties

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_AudioInput_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_AudioInput"></a>

### AudioInput


```csharp
public ModelCapability AudioInput { get; }
```


Audio input.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L273)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ChatCompletions_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ChatCompletions"></a>

### ChatCompletions


```csharp
public bool ChatCompletions { get; }
```


The model uses the chat completions API.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L297)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ImageInput_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ImageInput"></a>

### ImageInput


```csharp
public ModelCapability ImageInput { get; }
```


Image input. Unknown: the hosted model decides.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L270)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_Logprobs_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_Logprobs"></a>

### Logprobs


```csharp
public ModelCapability Logprobs { get; }
```


Logprobs.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L294)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ObjectGeneration_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ObjectGeneration"></a>

### ObjectGeneration


```csharp
public ModelCapability ObjectGeneration { get; }
```


Object generation.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L276)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ProviderOptions_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, ModelCapability> ProviderOptions { get; }
```


Support per language option in [Language](/api/Vercel.AI.Tencent/TencentProviderOptions/#Vercel_AI_Tencent_TencentProviderOptions_Language).

**Value:** [IReadOnlyDictionary&lt;string, ModelCapability&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L300)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ReasoningSupport_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ReasoningSupport"></a>

### ReasoningSupport


```csharp
public ModelCapability ReasoningSupport { get; }
```


Reasoning support.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L285)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_Temperature_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_Temperature"></a>

### Temperature


```csharp
public ModelCapability Temperature { get; }
```


Temperature sampling.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L288)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ToolStreaming_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ToolStreaming"></a>

### ToolStreaming


```csharp
public ModelCapability ToolStreaming { get; }
```


Tool streaming.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L282)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ToolUsage_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_ToolUsage"></a>

### ToolUsage


```csharp
public ModelCapability ToolUsage { get; }
```


Tool usage.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L279)

<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_TopP_"></a>
<a id="Vercel_AI_Tencent_TencentLanguageModelEntry_TopP"></a>

### TopP


```csharp
public ModelCapability TopP { get; }
```


Top-p sampling.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L291)
