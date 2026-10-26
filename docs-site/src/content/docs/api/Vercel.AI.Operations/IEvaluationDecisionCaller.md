---
title: "IEvaluationDecisionCaller"
description: "A decision model that also has the deprecated CancellationToken). Returned by the deprecated EvaluationModel accessors of ProviderRegistry and CustomProvider..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IEvaluationDecisionCaller"
  hidden: true
---

<a id="Vercel_AI_Operations_IEvaluationDecisionCaller"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Use IDecisionCaller instead.")]
public interface IEvaluationDecisionCaller : IDecisionCaller, IEvaluationCaller
```


A decision model that also has the deprecated `CancellationToken)`. Returned by the deprecated
`EvaluationModel` accessors of [ProviderRegistry](/api/Vercel.AI/ProviderRegistry/) and [CustomProvider](/api/Vercel.AI/CustomProvider/). Both methods decide
the same way.

**Derived:** [GatewayDecisionModel](/api/Vercel.AI.Gateway/GatewayDecisionModel/), [OpenAIDecisionModel](/api/Vercel.AI.OpenAI/OpenAIDecisionModel/), [DecisionLanguageModel](/api/Vercel.AI.Operations/DecisionLanguageModel/), [TypeSafeAiDecisionModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiDecisionModel/), [TypeSafeAiEvaluationModel](/api/Vercel.AI.TypeSafeAi/TypeSafeAiEvaluationModel/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L110)
