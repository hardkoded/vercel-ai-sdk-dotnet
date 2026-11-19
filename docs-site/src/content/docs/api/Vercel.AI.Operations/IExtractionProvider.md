---
title: "IExtractionProvider"
description: "A provider that returns extraction models. ProviderRegistry and CustomProvider resolve extraction models through it. Maps to ExtractionProvider, the optional..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IExtractionProvider"
  hidden: true
---

<a id="Vercel_AI_Operations_IExtractionProvider"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public interface IExtractionProvider
```


A provider that returns extraction models. [ProviderRegistry](/api/Vercel.AI/ProviderRegistry/) and [CustomProvider](/api/Vercel.AI/CustomProvider/) resolve
extraction models through it. Maps to `ExtractionProvider`, the optional `extractionModel` factory of a provider.

**Derived:** [AnthropicAwsProvider](/api/Vercel.AI.Anthropic/AnthropicAwsProvider/), [AnthropicProvider](/api/Vercel.AI.Anthropic/AnthropicProvider/), [CustomProvider](/api/Vercel.AI/CustomProvider/), [GoogleProvider](/api/Vercel.AI.Google/GoogleProvider/), [GoogleVertexAnthropicProvider](/api/Vercel.AI.Google/GoogleVertexAnthropicProvider/), [GoogleVertexProvider](/api/Vercel.AI.Google/GoogleVertexProvider/), [OpenAIProvider](/api/Vercel.AI.OpenAI/OpenAIProvider/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ExtractionProvider.cs#L11)
