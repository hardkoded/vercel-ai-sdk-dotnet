---
title: "DecisionRounding"
description: "Declared rounding precision."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionRounding"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionRounding"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionRounding
```


Declared rounding precision.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluationRounding](/api/Vercel.AI.Operations/EvaluationRounding/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L109)

## Constructors

<a id="Vercel_AI_Operations_DecisionRounding__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionRounding__ctor_System_Nullable_System_Int32__System_Nullable_System_Int32__"></a>

### DecisionRounding(int?, int?)


```csharp
public DecisionRounding(int? probabilityDecimals = null, int? scoreDecimals = null)
```


Creates rounding limits.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `probabilityDecimals` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `scoreDecimals` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L112)

## Properties

<a id="Vercel_AI_Operations_DecisionRounding_ProbabilityDecimals_"></a>
<a id="Vercel_AI_Operations_DecisionRounding_ProbabilityDecimals"></a>

### ProbabilityDecimals


```csharp
public int? ProbabilityDecimals { get; }
```


Probability decimal places, from 0 through 15.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L119)

<a id="Vercel_AI_Operations_DecisionRounding_ScoreDecimals_"></a>
<a id="Vercel_AI_Operations_DecisionRounding_ScoreDecimals"></a>

### ScoreDecimals


```csharp
public int? ScoreDecimals { get; }
```


Score decimal places, from 0 through 15.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L122)
