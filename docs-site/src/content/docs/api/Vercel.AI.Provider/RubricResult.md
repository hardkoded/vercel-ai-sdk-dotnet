---
title: "RubricResult"
description: "Score of a candidate against a rubric. Returned by IRubricModel."
editUrl: false
prev: false
next: false
sidebar:
  label: "RubricResult"
  hidden: true
---

<a id="Vercel_AI_Provider_RubricResult"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RubricResult
```


Score of a candidate against a rubric. Returned by [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L485)

## Constructors

<a id="Vercel_AI_Provider_RubricResult__ctor_"></a>
<a id="Vercel_AI_Provider_RubricResult__ctor_System_Double_System_String_"></a>

### RubricResult(double, string?)


```csharp
public RubricResult(double score, string? reason)
```


Creates a rubric result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `score` | [double](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `reason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L488)

## Properties

<a id="Vercel_AI_Provider_RubricResult_Reason_"></a>
<a id="Vercel_AI_Provider_RubricResult_Reason"></a>

### Reason


```csharp
public string? Reason { get; }
```


Explanation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L498)

<a id="Vercel_AI_Provider_RubricResult_Score_"></a>
<a id="Vercel_AI_Provider_RubricResult_Score"></a>

### Score


```csharp
public double Score { get; }
```


Score assigned by the evaluator.

**Value:** [double](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L495)
