---
title: "DecisionCaller"
description: "Helpers for IDecisionCaller."
editUrl: false
prev: false
next: false
sidebar:
  label: "DecisionCaller"
  hidden: true
---

<a id="Vercel_AI_Operations_DecisionCaller"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class DecisionCaller
```


Helpers for [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L264)

## Methods

<a id="Vercel_AI_Operations_DecisionCaller_FromLegacy_"></a>
<a id="Vercel_AI_Operations_DecisionCaller_FromLegacy_Vercel_AI_Operations_IEvaluationCaller_"></a>

### FromLegacy(IEvaluationCaller)


```csharp
[Obsolete("Implement IDecisionCaller on the model instead.")]
public static IDecisionCaller FromLegacy(IEvaluationCaller model)
```


Wraps a model that implements only the deprecated [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/) so
`CancellationToken)` can run it. A model that already implements
[IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/) is returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `model` | [IEvaluationCaller](/api/Vercel.AI.Operations/IEvaluationCaller/) |  |

**Returns:** [IDecisionCaller](/api/Vercel.AI.Operations/IDecisionCaller/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L271)
