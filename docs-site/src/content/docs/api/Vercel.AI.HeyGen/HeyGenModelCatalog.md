---
title: "HeyGenModelCatalog"
description: "Known HeyGen model ids, the adapter limits and the capability catalog. Upstream heygen-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "HeyGenModelCatalog"
  hidden: true
---

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog"></a>

Namespace: [Vercel.AI.HeyGen](/api/Vercel.AI.HeyGen/)


```csharp
public static class HeyGenModelCatalog
```


Known HeyGen model ids, the adapter limits and the capability catalog. Upstream `heygen-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L33)

## Fields

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxChoiceOptions"></a>

### DecisionMaxChoiceOptions


```csharp
public const int DecisionMaxChoiceOptions = 255
```


Most options of a choice question. Upstream `heyGenDecisionModelLimits.maxChoiceOptions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L57)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxImages"></a>

### DecisionMaxImages


```csharp
public const int DecisionMaxImages = 4
```


Most images per request. Upstream `heyGenDecisionModelLimits.maxImages`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L63)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxQuestions"></a>

### DecisionMaxQuestions


```csharp
public const int DecisionMaxQuestions = 400
```


Most questions per request. Upstream `heyGenDecisionModelLimits.maxQuestions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L54)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxScoreLevels"></a>

### DecisionMaxScoreLevels


```csharp
public const int DecisionMaxScoreLevels = 10
```


Most levels of a score question. Upstream `heyGenDecisionModelLimits.maxScoreLevels`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L60)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxVideos"></a>

### DecisionMaxVideos


```csharp
public const int DecisionMaxVideos = 1
```


Most videos per request. Upstream `heyGenDecisionModelLimits.maxVideos`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L66)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionModelIds"></a>

### DecisionModelIds


```csharp
public static readonly IReadOnlyList<string> DecisionModelIds
```


Known decision model ids. Other ids are passed through unchanged.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L39)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionSupportedQuestionTypes"></a>

### DecisionSupportedQuestionTypes


```csharp
public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes
```


Question types HeyGen Decision supports. Upstream `heyGenDecisionModelLimits.supportedQuestionTypes`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L46)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_Entries"></a>

### Entries


```csharp
public static readonly IReadOnlyDictionary<string, ModelEntry<object>> Entries
```


Entries of every known model id. Upstream `heyGenModelCatalog`; renamed because C# does not allow a member named like its class.

**Value:** [IReadOnlyDictionary&lt;string, ModelEntry&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L71)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_VideoModelIds"></a>

### VideoModelIds


```csharp
public static readonly IReadOnlyList<string> VideoModelIds
```


Known video model ids.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L36)

## Methods

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_GetHeyGenDecisionModelEntry_"></a>
<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_GetHeyGenDecisionModelEntry_System_String_"></a>

### GetHeyGenDecisionModelEntry(string)


```csharp
public static ModelEntry<object> GetHeyGenDecisionModelEntry(string modelId)
```


Returns the catalog entry of a decision model. Upstream `getHeyGenDecisionModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `ModelEntry&lt;object&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L115)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_GetHeyGenVideoModelEntry_"></a>
<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_GetHeyGenVideoModelEntry_System_String_"></a>

### GetHeyGenVideoModelEntry(string)


```csharp
public static ModelEntry<object> GetHeyGenVideoModelEntry(string modelId)
```


Returns the catalog entry of a video model. Upstream `getHeyGenVideoModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `ModelEntry&lt;object&gt;`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L74)
