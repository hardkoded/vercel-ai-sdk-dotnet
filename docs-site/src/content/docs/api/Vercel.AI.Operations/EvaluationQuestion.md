---
title: "EvaluationQuestion"
description: "Deprecated alias of DecisionQuestion."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationQuestion"
  hidden: true
---

<a id="Vercel_AI_Operations_EvaluationQuestion"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use DecisionQuestion instead.")]
public sealed class EvaluationQuestion : DecisionQuestion
```


Deprecated alias of [DecisionQuestion](/api/Vercel.AI.Operations/DecisionQuestion/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [DecisionQuestion](/api/Vercel.AI.Operations/DecisionQuestion/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L11)

## Constructors

<a id="Vercel_AI_Operations_EvaluationQuestion__ctor_"></a>
<a id="Vercel_AI_Operations_EvaluationQuestion__ctor_System_String_Vercel_AI_Operations_DecisionInput_Vercel_AI_Operations_DecisionCriteria_"></a>

### EvaluationQuestion(string, DecisionInput?, DecisionCriteria?)


```csharp
public EvaluationQuestion(string type, DecisionInput? instructions, DecisionCriteria? criteria = null)
```


Creates a question.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `instructions` | [DecisionInput](/api/Vercel.AI.Operations/DecisionInput/) |  |
| `criteria` | [DecisionCriteria](/api/Vercel.AI.Operations/DecisionCriteria/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L15)
