---
title: "HeyGenModelCatalog"
description: "Known HeyGen model ids and the adapter limits. Upstream heygen-model-catalog.ts."
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


Known HeyGen model ids and the adapter limits. Upstream `heygen-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L8)

## Fields

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxChoiceOptions"></a>

### DecisionMaxChoiceOptions


```csharp
public const int DecisionMaxChoiceOptions = 255
```


Most options of a choice question. Upstream `heyGenDecisionModelLimits.maxChoiceOptions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L20)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxImages"></a>

### DecisionMaxImages


```csharp
public const int DecisionMaxImages = 4
```


Most images per request. Upstream `heyGenDecisionModelLimits.maxImages`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L26)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxQuestions"></a>

### DecisionMaxQuestions


```csharp
public const int DecisionMaxQuestions = 400
```


Most questions per request. Upstream `heyGenDecisionModelLimits.maxQuestions`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L17)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxScoreLevels"></a>

### DecisionMaxScoreLevels


```csharp
public const int DecisionMaxScoreLevels = 10
```


Most levels of a score question. Upstream `heyGenDecisionModelLimits.maxScoreLevels`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L23)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionMaxVideos"></a>

### DecisionMaxVideos


```csharp
public const int DecisionMaxVideos = 1
```


Most videos per request. Upstream `heyGenDecisionModelLimits.maxVideos`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L29)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionModelIds"></a>

### DecisionModelIds


```csharp
public static readonly IReadOnlyList<string> DecisionModelIds
```


Known decision model ids. Other ids are passed through unchanged.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L11)

<a id="Vercel_AI_HeyGen_HeyGenModelCatalog_DecisionSupportedQuestionTypes"></a>

### DecisionSupportedQuestionTypes


```csharp
public static readonly IReadOnlyList<string> DecisionSupportedQuestionTypes
```


Question types HeyGen Decision supports. Upstream `heyGenDecisionModelLimits.supportedQuestionTypes`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/HeyGen/HeyGenModelCatalog.cs#L14)
