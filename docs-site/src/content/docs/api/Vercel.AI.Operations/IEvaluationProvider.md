---
title: "IEvaluationProvider"
description: "Deprecated provider factory for decision models. Implement IDecisionProvider instead. A provider that implements both is asked through IDecisionProvider only."
editUrl: false
prev: false
next: false
sidebar:
  label: "IEvaluationProvider"
  hidden: true
---

<a id="Vercel_AI_Operations_IEvaluationProvider"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
[Obsolete("Implement IDecisionProvider instead.")]
public interface IEvaluationProvider
```


Deprecated provider factory for decision models. Implement [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/) instead. A provider that
implements both is asked through [IDecisionProvider](/api/Vercel.AI.Operations/IDecisionProvider/) only.

**Derived:** [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/), [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/), [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/), [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/EvaluateAliases.cs#L119)
