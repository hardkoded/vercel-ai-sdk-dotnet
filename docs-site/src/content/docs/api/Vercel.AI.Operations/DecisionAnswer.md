---
title: "DecisionAnswer"
description: "One model answer. Numbers are returned unchanged."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionAnswer"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionAnswer"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionAnswer
```


One model answer. Numbers are returned unchanged.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluationAnswer](/api/Vercel.AI.Operations/EvaluationAnswer/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L51)

## Constructors

<a id="Vercel_AI_Operations_DecisionAnswer__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer__ctor_System_String_System_String_System_Nullable_System_Double__System_Nullable_System_Double__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Double__Vercel_AI_Operations_DecisionBox_"></a>

### `DecisionAnswer(string, string?, double?, double?, IReadOnlyDictionary<string, double>?, DecisionBox?)`


```csharp
public DecisionAnswer(string type, string? choice = null, double? score = null, double? probability = null, IReadOnlyDictionary<string, double>? probabilities = null, DecisionBox? box = null)
```


Creates an answer.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `choice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `score` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `probability` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `probabilities` | [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `box` | [DecisionBox](/api/Vercel.AI.Operations/DecisionBox/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L54)

## Properties

<a id="Vercel_AI_Operations_DecisionAnswer_Box_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Box"></a>

### Box


```csharp
public DecisionBox? Box { get; }
```


Normalized box for a box question.

**Value:** [DecisionBox](/api/Vercel.AI.Operations/DecisionBox/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L80)

<a id="Vercel_AI_Operations_DecisionAnswer_Choice_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Choice"></a>

### Choice


```csharp
public string? Choice { get; }
```


Selected choice.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L68)

<a id="Vercel_AI_Operations_DecisionAnswer_Probabilities_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Probabilities"></a>

### Probabilities


```csharp
public IReadOnlyDictionary<string, double>? Probabilities { get; }
```


Distribution. Keys are option ids or score indexes.

**Value:** [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L77)

<a id="Vercel_AI_Operations_DecisionAnswer_Probability_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Probability"></a>

### Probability


```csharp
public double? Probability { get; }
```


P(true) for a boolean question.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L74)

<a id="Vercel_AI_Operations_DecisionAnswer_Score_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Score"></a>

### Score


```csharp
public double? Score { get; }
```


Score.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L71)

<a id="Vercel_AI_Operations_DecisionAnswer_Type_"></a>
<a id="Vercel_AI_Operations_DecisionAnswer_Type"></a>

### Type


```csharp
public string Type { get; }
```


Answer type. It is the question type, or `refusal` when the model declined the question.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L65)
