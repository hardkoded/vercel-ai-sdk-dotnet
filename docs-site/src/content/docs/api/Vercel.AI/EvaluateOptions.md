---
title: "EvaluateOptions"
description: "Evaluation options."
editUrl: false
prev: false
next: false
sidebar:
  label: "EvaluateOptions"
  hidden: true
---

<a id="Vercel_AI_EvaluateOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class EvaluateOptions
```


Evaluation options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L433)

## Properties

<a id="Vercel_AI_EvaluateOptions_Candidate_"></a>
<a id="Vercel_AI_EvaluateOptions_Candidate"></a>

### Candidate


```csharp
public string Candidate { get; set; }
```


Candidate text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L456)

<a id="Vercel_AI_EvaluateOptions_EvaluationModel_"></a>
<a id="Vercel_AI_EvaluateOptions_EvaluationModel"></a>

### EvaluationModel


```csharp
[Obsolete("Use RubricModel instead.")]
public IEvaluationModel? EvaluationModel { get; set; }
```


Deprecated name of [RubricModel](#Vercel_AI_EvaluateOptions_RubricModel). Setting it sets [RubricModel](#Vercel_AI_EvaluateOptions_RubricModel).

**Value:** [IEvaluationModel](/api/Vercel.AI.Provider/IEvaluationModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L439)

<a id="Vercel_AI_EvaluateOptions_Model_"></a>
<a id="Vercel_AI_EvaluateOptions_Model"></a>

### Model


```csharp
public ILanguageModel? Model { get; set; }
```


Language model used when [RubricModel](#Vercel_AI_EvaluateOptions_RubricModel) is null.

**Value:** [ILanguageModel](/api/Vercel.AI.Provider/ILanguageModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L447)

<a id="Vercel_AI_EvaluateOptions_ModelId_"></a>
<a id="Vercel_AI_EvaluateOptions_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Gateway model id used when no model instance is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L450)

<a id="Vercel_AI_EvaluateOptions_Rubric_"></a>
<a id="Vercel_AI_EvaluateOptions_Rubric"></a>

### Rubric


```csharp
public string Rubric { get; set; }
```


Rubric.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L453)

<a id="Vercel_AI_EvaluateOptions_RubricModel_"></a>
<a id="Vercel_AI_EvaluateOptions_RubricModel"></a>

### RubricModel


```csharp
public IRubricModel? RubricModel { get; set; }
```


Dedicated rubric model.

**Value:** [IRubricModel](/api/Vercel.AI.Provider/IRubricModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L436)
