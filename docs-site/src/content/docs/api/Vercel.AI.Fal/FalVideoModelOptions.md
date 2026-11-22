---
title: "FalVideoModelOptions"
description: "Provider options for Fal video generation, read from providerOptions.fal. Upstream FalVideoModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "FalVideoModelOptions"
  hidden: true
---

<a id="Vercel_AI_Fal_FalVideoModelOptions"></a>

Namespace: [Vercel.AI.Fal](/api/Vercel.AI.Fal/)


```csharp
public sealed class FalVideoModelOptions
```


Provider options for Fal video generation, read from `providerOptions.fal`. Upstream `FalVideoModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Fal_FalVideoModelOptions_AdditionalProperties_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; }
```


Every other key, sent to the API as is. Upstream's index signature.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L29)

<a id="Vercel_AI_Fal_FalVideoModelOptions_Loop_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_Loop"></a>

### Loop


```csharp
public bool? Loop { get; set; }
```


Video loop. Only for Luma models. Sent as `loop`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L14)

<a id="Vercel_AI_Fal_FalVideoModelOptions_MotionStrength_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_MotionStrength"></a>

### MotionStrength


```csharp
public double? MotionStrength { get; set; }
```


Motion strength between 0 and 1. Sent as `motion_strength`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L17)

<a id="Vercel_AI_Fal_FalVideoModelOptions_NegativePrompt_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_NegativePrompt"></a>

### NegativePrompt


```csharp
public string? NegativePrompt { get; set; }
```


Sent as `negative_prompt`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L23)

<a id="Vercel_AI_Fal_FalVideoModelOptions_PromptOptimizer_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_PromptOptimizer"></a>

### PromptOptimizer


```csharp
public bool? PromptOptimizer { get; set; }
```


Sent as `prompt_optimizer`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L26)

<a id="Vercel_AI_Fal_FalVideoModelOptions_Resolution_"></a>
<a id="Vercel_AI_Fal_FalVideoModelOptions_Resolution"></a>

### Resolution


```csharp
public string? Resolution { get; set; }
```


Resolution, such as `480p`, `720p` or `1080p`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Fal/FalVideoModelOptions.cs#L20)
