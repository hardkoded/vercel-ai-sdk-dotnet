---
title: "DecisionScoreCriteria"
description: "Criteria of a score question: at least two ordered levels, indexed from zero. A null description means none."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionScoreCriteria"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionScoreCriteria"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionScoreCriteria : DecisionCriteria
```


Criteria of a score question: at least two ordered levels, indexed from zero. A null description means none.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L118)

## Constructors

<a id="Vercel_AI_Operations_DecisionScoreCriteria__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionScoreCriteria__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionInput__"></a>

### `DecisionScoreCriteria(IReadOnlyList<DecisionInput?>)`


```csharp
public DecisionScoreCriteria(IReadOnlyList<DecisionInput?> levels)
```


Creates score criteria.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `levels` | [IReadOnlyList&lt;DecisionInput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L121)

## Properties

<a id="Vercel_AI_Operations_DecisionScoreCriteria_Count_"></a>
<a id="Vercel_AI_Operations_DecisionScoreCriteria_Count"></a>

### Count


```csharp
public override int Count { get; }
```


Number of options or levels. A boolean has none.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L130)

<a id="Vercel_AI_Operations_DecisionScoreCriteria_Levels_"></a>
<a id="Vercel_AI_Operations_DecisionScoreCriteria_Levels"></a>

### Levels


```csharp
public IReadOnlyList<DecisionInput?> Levels { get; }
```


Level descriptions, lowest first.

**Value:** [IReadOnlyList&lt;DecisionInput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L127)
