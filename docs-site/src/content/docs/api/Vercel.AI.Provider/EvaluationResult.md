---
title: "EvaluationResult"
description: "Deprecated name of RubricResult."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluationResult"
  hidden: true
---

<a id="Vercel_AI_Provider_EvaluationResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
[Obsolete("Use RubricResult instead.")]
public sealed class EvaluationResult
```


Deprecated name of [RubricResult](/api/Vercel.AI.Provider/RubricResult/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L515)

## Constructors

<a id="Vercel_AI_Provider_EvaluationResult__ctor_"></a>
<a id="Vercel_AI_Provider_EvaluationResult__ctor_System_Double_System_String_"></a>

### EvaluationResult(double, string?)


```csharp
public EvaluationResult(double score, string? reason)
```


Creates an evaluation result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `score` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L519)

## Properties

<a id="Vercel_AI_Provider_EvaluationResult_Reason_"></a>
<a id="Vercel_AI_Provider_EvaluationResult_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Explanation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L529)

<a id="Vercel_AI_Provider_EvaluationResult_Score_"></a>
<a id="Vercel_AI_Provider_EvaluationResult_Score"></a>

### Score


```csharp
public double Score { get; }
```


Score assigned by the evaluator.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L526)
