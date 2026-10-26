---
title: "IDecisionProvider"
description: "A provider that returns decision models. ProviderRegistry and CustomProvider resolve decision models through it. Maps to the optional decisionModel factory o..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IDecisionProvider"
  hidden: true
---

<a id="Vercel_AI_Operations_IDecisionProvider"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public interface IDecisionProvider
```


A provider that returns decision models. [ProviderRegistry](/api/Vercel.AI/ProviderRegistry/) and [CustomProvider](/api/Vercel.AI/CustomProvider/) resolve
decision models through it. Maps to the optional `decisionModel` factory of a provider.

**Derived:** [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/), [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/), [CustomProvider](/api/Vercel.AI/CustomProvider/), [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/), [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/), [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Decide.cs#L221)
