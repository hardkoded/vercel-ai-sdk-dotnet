---
title: "DecisionChoiceCriteria"
description: "Criteria of a choice question: a nonempty map of option names to descriptions. A null description means none."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionChoiceCriteria"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionChoiceCriteria"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class DecisionChoiceCriteria : DecisionCriteria
```


Criteria of a choice question: a nonempty map of option names to descriptions. A null description means none.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L91)

## Constructors

<a id="Vercel_AI_Operations_DecisionChoiceCriteria__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionChoiceCriteria__ctor_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_DecisionInput__"></a>

### `DecisionChoiceCriteria(IReadOnlyDictionary<string, DecisionInput?>)`


```csharp
public DecisionChoiceCriteria(IReadOnlyDictionary<string, DecisionInput?> options)
```


Creates choice criteria.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [IReadOnlyDictionary&lt;string, DecisionInput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L94)

## Properties

<a id="Vercel_AI_Operations_DecisionChoiceCriteria_Count_"></a>
<a id="Vercel_AI_Operations_DecisionChoiceCriteria_Count"></a>

### Count


```csharp
public override int Count { get; }
```


Number of options or levels. A boolean has none.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L103)

<a id="Vercel_AI_Operations_DecisionChoiceCriteria_Options_"></a>
<a id="Vercel_AI_Operations_DecisionChoiceCriteria_Options"></a>

### Options


```csharp
public IReadOnlyDictionary<string, DecisionInput?> Options { get; }
```


Option names with their descriptions, in order.

**Value:** [IReadOnlyDictionary&lt;string, DecisionInput&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/DecisionInput.cs#L100)
