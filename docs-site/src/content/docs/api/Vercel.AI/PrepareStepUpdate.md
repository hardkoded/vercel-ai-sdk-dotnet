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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1372)

## Properties

<a id="Vercel_AI_PrepareStepUpdate_ActiveTools_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ActiveTools"></a>

### ActiveTools


```csharp
public IReadOnlyList<string>? ActiveTools { get; set; }
```


Overrides the active tools for this step. See [ActiveTools](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ActiveTools). Maps to `activeTools`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1387)

<a id="Vercel_AI_PrepareStepUpdate_ExperimentalSandbox_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ExperimentalSandbox"></a>

### ExperimentalSandbox


```csharp
public ISandboxSession? ExperimentalSandbox { get; set; }
```


The sandbox environment that the step is operating in. Changing it affects tool execution in this step only. Maps to `experimental_sandbox`.

**Value:** [ISandboxSession](/api/Vercel.AI.ProviderUtils/ISandboxSession/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1427)

<a id="Vercel_AI_PrepareStepUpdate_FrequencyPenalty_"></a>
<a id="Vercel_AI_PrepareStepUpdate_FrequencyPenalty"></a>

### FrequencyPenalty


```csharp
public double? FrequencyPenalty { get; set; }
```


Overrides the frequency penalty for this step. Maps to `frequencyPenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1405)

<a id="Vercel_AI_PrepareStepUpdate_Instructions_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; set; }
```


Replacement instructions. The override carries forward to later steps.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1375)

<a id="Vercel_AI_PrepareStepUpdate_MaxOutputTokens_"></a>
<a id="Vercel_AI_PrepareStepUpdate_MaxOutputTokens"></a>

### MaxOutputTokens


```csharp
public int? MaxOutputTokens { get; set; }
```


Overrides the maximum output tokens for this step. Maps to `maxOutputTokens`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1390)

<a id="Vercel_AI_PrepareStepUpdate_Messages_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; set; }
```


Replacement prompt, without the instructions. The override carries forward to later steps.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1378)

<a id="Vercel_AI_PrepareStepUpdate_Model_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Replacement model.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1381)

<a id="Vercel_AI_PrepareStepUpdate_PresencePenalty_"></a>
<a id="Vercel_AI_PrepareStepUpdate_PresencePenalty"></a>

### PresencePenalty


```csharp
public double? PresencePenalty { get; set; }
```


Overrides the presence penalty for this step. Maps to `presencePenalty`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1402)

<a id="Vercel_AI_PrepareStepUpdate_ProviderOptions_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options for this step, such as an Anthropic container id. They are deep-merged onto the call's provider options.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1422)

<a id="Vercel_AI_PrepareStepUpdate_Reasoning_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; set; }
```


Overrides the reasoning effort for this step. Maps to `reasoning`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1414)

<a id="Vercel_AI_PrepareStepUpdate_RuntimeContext_"></a>
<a id="Vercel_AI_PrepareStepUpdate_RuntimeContext"></a>

### RuntimeContext


```csharp
public IReadOnlyDictionary<string, object?>? RuntimeContext { get; set; }
```


Replacement runtime context. It affects this step and all later steps. Maps to `runtimeContext`.

**Value:** [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1437)

<a id="Vercel_AI_PrepareStepUpdate_Seed_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Seed"></a>

### Seed


```csharp
public int? Seed { get; set; }
```


Overrides the seed for this step. Maps to `seed`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1411)

<a id="Vercel_AI_PrepareStepUpdate_StopSequences_"></a>
<a id="Vercel_AI_PrepareStepUpdate_StopSequences"></a>

### StopSequences


```csharp
public IReadOnlyList<string>? StopSequences { get; set; }
```


Overrides the stop sequences for this step. An empty list clears them. Maps to `stopSequences`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1408)

<a id="Vercel_AI_PrepareStepUpdate_Temperature_"></a>
<a id="Vercel_AI_PrepareStepUpdate_Temperature"></a>

### Temperature


```csharp
public double? Temperature { get; set; }
```


Overrides the temperature for this step. Maps to `temperature`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1393)

<a id="Vercel_AI_PrepareStepUpdate_ToolChoice_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ToolChoice"></a>

### ToolChoice


```csharp
public ToolChoice? ToolChoice { get; set; }
```


Replacement tool choice for this step.

**Value:** [ToolChoice](/api/Vercel.AI.Provider/ToolChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1384)

<a id="Vercel_AI_PrepareStepUpdate_ToolOrder_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ToolOrder"></a>

### ToolOrder


```csharp
public IReadOnlyList<string>? ToolOrder { get; set; }
```


Overrides the order in which tools are sent to the provider for this step. See [ToolOrder](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ToolOrder). Maps to `toolOrder`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1417)

<a id="Vercel_AI_PrepareStepUpdate_ToolsContext_"></a>
<a id="Vercel_AI_PrepareStepUpdate_ToolsContext"></a>

### ToolsContext


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolsContext { get; set; }
```


Replacement tools context. It affects this step and all later steps, and it is passed into tool execution. Maps to `toolsContext`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1432)

<a id="Vercel_AI_PrepareStepUpdate_TopK_"></a>
<a id="Vercel_AI_PrepareStepUpdate_TopK"></a>

### TopK


```csharp
public int? TopK { get; set; }
```


Overrides top-k for this step. Maps to `topK`.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1399)

<a id="Vercel_AI_PrepareStepUpdate_TopP_"></a>
<a id="Vercel_AI_PrepareStepUpdate_TopP"></a>

### TopP


```csharp
public double? TopP { get; set; }
```


Overrides top-p for this step. Maps to `topP`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1396)
