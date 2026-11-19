---
title: "LiquidModelCatalog"
description: "Known Liquid model ids, the adapter limits and the capability catalog. Upstream liquid-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "LiquidModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Liquid_LiquidModelCatalog"></a>

Namespace: [Vercel.AI.Liquid](/api/Vercel.AI.Liquid/)


```csharp
public static class LiquidModelCatalog
```


Known Liquid model ids, the adapter limits and the capability catalog. Upstream `liquid-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L10)

## Fields

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMaxAudioClips"></a>

### DecisionMaxAudioClips


```csharp
public const int DecisionMaxAudioClips = 8
```


Most audio clips per request. Upstream `liquidDecisionModelLimits.maxAudioClips`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L43)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMaxChoiceOptions"></a>

### DecisionMaxChoiceOptions


```csharp
public const int DecisionMaxChoiceOptions = 255
```


Most options of a choice question. Upstream `liquidDecisionModelLimits.maxChoiceOptions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L31)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMaxImages"></a>

### DecisionMaxImages


```csharp
public const int DecisionMaxImages = 8
```


Most images per request. Upstream `liquidDecisionModelLimits.maxImages`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L40)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMaxQuestions"></a>

### DecisionMaxQuestions


```csharp
public const int DecisionMaxQuestions = 128
```


Most questions per request. Upstream `liquidDecisionModelLimits.maxQuestions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L25)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMaxScoreLevels"></a>

### DecisionMaxScoreLevels


```csharp
public const int DecisionMaxScoreLevels = 10
```


Most levels of a score question. Upstream `liquidDecisionModelLimits.maxScoreLevels`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L37)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMinChoiceOptions"></a>

### DecisionMinChoiceOptions


```csharp
public const int DecisionMinChoiceOptions = 2
```


Fewest options of a choice question. Upstream `liquidDecisionModelLimits.minChoiceOptions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L28)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionMinScoreLevels"></a>

### DecisionMinScoreLevels


```csharp
public const int DecisionMinScoreLevels = 2
```


Fewest levels of a score question. Upstream `liquidDecisionModelLimits.minScoreLevels`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L34)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionModelIds"></a>

### DecisionModelIds


```csharp
public static readonly IReadOnlyList<string> DecisionModelIds
```


Known decision model ids. Other ids are passed through unchanged. Upstream `decisionModelIds`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L13)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_DecisionSupportedQuestionTypes"></a>

### DecisionSupportedQuestionTypes


```csharp
public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes
```


Question types Liquid Decisions supports. Upstream `liquidDecisionModelLimits.supportedQuestionTypes`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L16)

<a id="Vercel_AI_Liquid_LiquidModelCatalog_Entries"></a>

### Entries


```csharp
public static readonly IReadOnlyDictionary<string, ModelEntry<object>> Entries
```


Entries of every known model id. Upstream `liquidModelCatalog`; renamed because C# does not allow a member named like its class.

**Value:** [IReadOnlyDictionary&lt;string, ModelEntry&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L48)

## Methods

<a id="Vercel_AI_Liquid_LiquidModelCatalog_GetLiquidDecisionModelEntry_"></a>
<a id="Vercel_AI_Liquid_LiquidModelCatalog_GetLiquidDecisionModelEntry_System_String_"></a>

### GetLiquidDecisionModelEntry(string)


```csharp
public static ModelEntry<object> GetLiquidDecisionModelEntry(string modelId)
```


Returns the catalog entry of a decision model. Upstream `getLiquidDecisionModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `ModelEntry&lt;object&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Liquid/LiquidModelCatalog.cs#L51)
