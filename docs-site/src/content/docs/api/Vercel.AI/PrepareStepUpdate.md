---
title: "PrepareStepUpdate"
description: "Optional changes applied before a step."
editUrl: false
prev: false
next: false
sidebar:
  label: "PrepareStepUpdate"
  hidden: true
---

<a id="Vercel_AI_PrepareStepUpdate"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class PrepareStepUpdate
```


Optional changes applied before a step.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L602)

## Properties

<a id="Vercel_AI_PrepareStepUpdate_Instructions_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


Replacement instructions. The override carries forward to later steps.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L605)

<a id="Vercel_AI_PrepareStepUpdate_Messages_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Replacement prompt, without the instructions. The override carries forward to later steps.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L608)

<a id="Vercel_AI_PrepareStepUpdate_Model_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Replacement model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L611)

<a id="Vercel_AI_PrepareStepUpdate_ProviderOptions_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options for this step, such as an Anthropic container id. They are deep-merged onto the call's provider options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L619)

<a id="Vercel_AI_PrepareStepUpdate_ToolChoice_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; set; }
```


Replacement tool choice for this step.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L614)
