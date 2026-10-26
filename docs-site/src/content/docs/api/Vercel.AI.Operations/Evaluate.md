---
title: "Evaluate"
description: "Deprecated alias of Decide."
editUrl: false
prev: false
next: false
sidebar:
  label: "Evaluate"
  hidden: true
---

<a id="Vercel_AI_Operations_Evaluate"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use Decide instead.")]
public static class Evaluate
```


Deprecated alias of [Decide](/api/Vercel.AI.Operations/Decide/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L215)

## Methods

<a id="Vercel_AI_Operations_Evaluate_EvaluateAsync_"></a>
<a id="Vercel_AI_Operations_Evaluate_EvaluateAsync_Vercel_AI_Operations_EvaluateRequest_System_Threading_CancellationToken_"></a>

### EvaluateAsync(EvaluateRequest, CancellationToken)


```csharp
public static Task<EvaluateResult> EvaluateAsync(EvaluateRequest request, CancellationToken cancellationToken = default)
```


Deprecated alias of `CancellationToken)`. Both run the same operation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [EvaluateRequest](/api/Vercel.AI.Operations/EvaluateRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;EvaluateResult&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L219)

<a id="Vercel_AI_Operations_Evaluate_ValidateAnswers_"></a>
<a id="Vercel_AI_Operations_Evaluate_ValidateAnswers_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_EvaluationQuestion__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_EvaluationAnswer__Vercel_AI_Operations_EvaluationRounding_"></a>

### `ValidateAnswers(IReadOnlyDictionary<string, EvaluationQuestion>, IReadOnlyDictionary<string, EvaluationAnswer>?, EvaluationRounding?)`


```csharp
public static void ValidateAnswers(IReadOnlyDictionary<string, EvaluationQuestion> questions, IReadOnlyDictionary<string, EvaluationAnswer>? answers, EvaluationRounding? rounding)
```


Deprecated alias of `DecisionRounding)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `questions` | [IReadOnlyDictionary&lt;string, EvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `answers` | [IReadOnlyDictionary&lt;string, EvaluationAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `rounding` | [EvaluationRounding](/api/Vercel.AI.Operations/EvaluationRounding/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L257)

<a id="Vercel_AI_Operations_Evaluate_ValidateInput_"></a>
<a id="Vercel_AI_Operations_Evaluate_ValidateInput_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_EvaluationQuestion__"></a>

### `ValidateInput(DecisionState?, IReadOnlyDictionary<string, EvaluationQuestion>?)`


```csharp
public static void ValidateInput(DecisionState? state, IReadOnlyDictionary<string, EvaluationQuestion>? questions)
```


Deprecated alias of `DecisionQuestion%7d)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `questions` | [IReadOnlyDictionary&lt;string, EvaluationQuestion&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L251)
