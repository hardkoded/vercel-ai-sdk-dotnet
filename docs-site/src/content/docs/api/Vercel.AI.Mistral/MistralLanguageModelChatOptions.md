---
title: "MistralLanguageModelChatOptions"
description: "Mistral language model provider options. Maps to MistralLanguageModelChatOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "MistralLanguageModelChatOptions"
  hidden: true
---

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions"></a>

Namespace: [Vercel.AI.Mistral](/api/Vercel.AI.Mistral/)


```csharp
public sealed class MistralLanguageModelChatOptions
```


Mistral language model provider options. Maps to `MistralLanguageModelChatOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_DocumentImageLimit_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_DocumentImageLimit"></a>

### DocumentImageLimit


```csharp
public double? DocumentImageLimit { get; init; }
```


Maximum number of document images.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L17)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_DocumentPageLimit_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_DocumentPageLimit"></a>

### DocumentPageLimit


```csharp
public double? DocumentPageLimit { get; init; }
```


Maximum number of document pages.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L20)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_ParallelToolCalls_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_ParallelToolCalls"></a>

### ParallelToolCalls


```csharp
public bool? ParallelToolCalls { get; init; }
```


Whether to enable parallel function calling during tool use. When set to `false`, the model uses at most one
tool per response. Defaults to `true`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L32)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_PromptCacheKey_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_PromptCacheKey"></a>

### PromptCacheKey


```csharp
public string? PromptCacheKey { get; init; }
```


A stable identifier used to route requests with shared prompt prefixes to the same cache.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L35)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public string? ReasoningEffort { get; init; }
```


Reasoning effort for models that support adjustable reasoning: `high`, `none`, `low`, `medium`,
or `max`. Which values a model accepts depends on the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L41)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_SafePrompt_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_SafePrompt"></a>

### SafePrompt


```csharp
public bool? SafePrompt { get; init; }
```


Whether to inject a safety prompt before all conversations. Defaults to `false`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L14)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_StrictJsonSchema_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_StrictJsonSchema"></a>

### StrictJsonSchema


```csharp
public bool? StrictJsonSchema { get; init; }
```


Whether to use strict JSON schema validation. Defaults to `false`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L26)

<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_StructuredOutputs_"></a>
<a id="Vercel_AI_Mistral_MistralLanguageModelChatOptions_StructuredOutputs"></a>

### StructuredOutputs


```csharp
public bool? StructuredOutputs { get; init; }
```


Whether to use structured outputs. Defaults to `true`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mistral/MistralChatLanguageModelOptions.cs#L23)
