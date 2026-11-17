---
title: "DecisionQuestion"
description: "One decision question. Maps to DecisionModelV4Question."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionQuestion"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionQuestion"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public class DecisionQuestion
```


One decision question. Maps to `DecisionModelV4Question`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [EvaluationQuestion](/api/Vercel.AI.Operations/EvaluationQuestion/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L13)

## Constructors

<a id="Vercel_AI_Operations_DecisionQuestion__ctor_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion__ctor_System_String_Vercel_AI_Operations_DecisionInput_Vercel_AI_Operations_DecisionCriteria_"></a>

### DecisionQuestion(string, DecisionInput?, DecisionCriteria?)


```csharp
public DecisionQuestion(string type, DecisionInput? instructions, DecisionCriteria? criteria = null)
```


Creates a question.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) | `choice`, `score`, `boolean`, or `box`. |
| `instructions` | [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/) | Instructions as a string, JSON object, or JSON array. Every question type may leave them null when the provider supports it. |
| `criteria` | [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/) | [DecisionChoiceCriteria](/api/Vercel.AI.Operations/DecisionChoiceCriteria/) for a choice, [DecisionScoreCriteria](/api/Vercel.AI.Operations/DecisionScoreCriteria/) for a score, and an optional [DecisionBooleanCriteria](/api/Vercel.AI.Operations/DecisionBooleanCriteria/) for a boolean. A box question has no criteria. |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L25)

## Properties

<a id="Vercel_AI_Operations_DecisionQuestion_Criteria_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion_Criteria"></a>

### Criteria


```csharp
public DecisionCriteria? Criteria { get; }
```


Choice options, ordered score levels, or optional boolean descriptions. A box question has no criteria.

**Value:** [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L52)

<a id="Vercel_AI_Operations_DecisionQuestion_HasCriteria_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion_HasCriteria"></a>

### HasCriteria


```csharp
public bool HasCriteria { get; init; }
```


True when `criteria` is present. It is true whenever [Criteria](#Vercel_AI_Operations_DecisionQuestion_Criteria) is set. Set it to true with null criteria to
send an explicit `null`, as upstream does for a boolean question with `criteria: null`. A question that leaves it
false omits the field.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L59)

<a id="Vercel_AI_Operations_DecisionQuestion_HasInstructions_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion_HasInstructions"></a>

### HasInstructions


```csharp
public bool HasInstructions { get; init; }
```


True when `instructions` is present. It is true whenever [Instructions](#Vercel_AI_Operations_DecisionQuestion_Instructions) is set. Set it to true with null
instructions to send an explicit `null`, as upstream does for `instructions: null`. A question that leaves it false omits
the field, as upstream does for an absent or `undefined` value.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L49)

<a id="Vercel_AI_Operations_DecisionQuestion_Instructions_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion_Instructions"></a>

### Instructions


```csharp
public DecisionInput? Instructions { get; }
```


Instructions. A string, JSON object, or JSON array. May be null for every question type.

**Value:** [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L42)

<a id="Vercel_AI_Operations_DecisionQuestion_Type_"></a>
<a id="Vercel_AI_Operations_DecisionQuestion_Type"></a>

### Type


```csharp
public string Type { get; }
```


`choice`, `score`, `boolean`, or `box`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L39)
