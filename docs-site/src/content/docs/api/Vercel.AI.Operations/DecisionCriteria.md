---
title: "DecisionCriteria"
description: "Criteria of a decision question. Maps to the criteria member of DecisionModelV4Question."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionCriteria"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionCriteria"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public abstract class DecisionCriteria
```


Criteria of a decision question. Maps to the `criteria` member of `DecisionModelV4Question`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [DecisionBooleanCriteria](/api/Vercel.AI.Operations/DecisionBooleanCriteria/), [DecisionChoiceCriteria](/api/Vercel.AI.Operations/DecisionChoiceCriteria/), [DecisionScoreCriteria](/api/Vercel.AI.Operations/DecisionScoreCriteria/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L73)

## Properties

<a id="Vercel_AI_Operations_DecisionCriteria_Count_"></a>
<a id="Vercel_AI_Operations_DecisionCriteria_Count"></a>

### Count


```csharp
public virtual int Count { get; }
```


Number of options or levels. A boolean has none.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L80)
