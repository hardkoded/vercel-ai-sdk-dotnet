---
title: "QuiverAIImageModelOptions"
description: "The quiverai provider options of an image call. Maps to QuiverAIImageModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageModelOptions"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIImageModelOptions
```


The `quiverai` provider options of an image call. Maps to `QuiverAIImageModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L50)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Attributes_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Attributes"></a>

### Attributes


```csharp
public QuiverAIImageAttributes? Attributes { get; set; }
```


SVG root attributes requested for generation or vectorization.

**Value:** [QuiverAIImageAttributes](/api/Vercel.AI.QuiverAI/QuiverAIImageAttributes/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L71)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_AutoCrop_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_AutoCrop"></a>

### AutoCrop


```csharp
public bool? AutoCrop { get; set; }
```


Whether to auto-crop the input image before vectorization. Only used when the operation is `vectorize`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L95)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Instructions_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Extra style guidance for prompt-based generation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L56)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_MaxOutputTokens_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Maximum number of output tokens (1 - 65536 for Arrow 2 models, 131072 otherwise).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L86)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_MaxReviewSteps_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_MaxReviewSteps"></a>

### MaxReviewSteps


```csharp
public int? MaxReviewSteps { get; set; }
```


Maximum number of edit review and redo steps (0-5).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L68)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Operation_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Operation"></a>

### Operation


```csharp
public QuiverAIImageOperation? Operation { get; set; }
```


The operation to perform. Defaults to `generate`.

**Value:** `QuiverAIImageOperation?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L53)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_OrchestratorMaxOutputTokens_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_OrchestratorMaxOutputTokens"></a>

### OrchestratorMaxOutputTokens


```csharp
public int? OrchestratorMaxOutputTokens { get; set; }
```


Provider orchestrator token budget for SVG editing (1-65536).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L89)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenalty_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Presence penalty (-2 to 2). Set [PresencePenaltyIsNull](#Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenaltyIsNull) to send an explicit `null`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L80)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenaltyIsNull_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenaltyIsNull"></a>

### PresencePenaltyIsNull


```csharp
public bool PresencePenaltyIsNull { get; set; }
```


Sends `presencePenalty: null` (upstream's nullable) when [PresencePenalty](#Vercel_AI_QuiverAI_QuiverAIImageModelOptions_PresencePenalty) is not set.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L83)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ReasoningEffort_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ReasoningEffort"></a>

### ReasoningEffort


```csharp
public QuiverAIReasoningEffort? ReasoningEffort { get; set; }
```


Reasoning effort applied to generation or vectorization.

**Value:** `QuiverAIReasoningEffort?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L59)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ReferenceImages_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ReferenceImages"></a>

### ReferenceImages


```csharp
public IReadOnlyList<QuiverAIImageReference>? ReferenceImages { get; set; }
```


Optional reference images for SVG editing (at most 4). Use
`Byte%5b%5d)` to convert binary inputs.

**Value:** [IReadOnlyList&lt;QuiverAIImageReference&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L65)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ShallowMaxOutputTokens_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ShallowMaxOutputTokens"></a>

### ShallowMaxOutputTokens


```csharp
public int? ShallowMaxOutputTokens { get; set; }
```


Provider shallow edit token budget for SVG editing (1-65536).

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L92)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_TargetSize_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_TargetSize"></a>

### TargetSize


```csharp
public int? TargetSize { get; set; }
```


Target canvas size in pixels for vectorization (128 - 4096). Only used when the operation is `vectorize`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L98)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Temperature_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Sampling temperature (0-2).

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L74)

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_TopP_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Nucleus sampling top-p (0-1).

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L77)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageModelOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(string providerOptionsName = "quiverai")
```


Returns the options as call provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt;
(`quiverai` by default), ready for `ImageModelCallOptions.ProviderOptions`.
The model validates the values when it is called.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageModelOptions.cs#L105)
