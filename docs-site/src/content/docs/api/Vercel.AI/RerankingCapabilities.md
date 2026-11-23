---
title: "RerankingCapabilities"
description: "Upstream ModelCapabilities.reranking."
editUrl: false
prev: false
next: false
sidebar:
  label: "RerankingCapabilities"
  hidden: true
---

<a id="Vercel_AI_RerankingCapabilities"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class RerankingCapabilities
```


Upstream `ModelCapabilities.reranking`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L242)

## Properties

<a id="Vercel_AI_RerankingCapabilities_Documents_"></a>
<a id="Vercel_AI_RerankingCapabilities_Documents"></a>

### Documents


```csharp
public RerankingDocumentCapabilities Documents { get; init; }
```


Input support exposed by the provider, including compatibility conversions.

**Value:** [RerankingDocumentCapabilities](/api/Vercel.AI/RerankingDocumentCapabilities/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L245)

<a id="Vercel_AI_RerankingCapabilities_TopN_"></a>
<a id="Vercel_AI_RerankingCapabilities_TopN"></a>

### TopN


```csharp
public ModelCapability TopN { get; init; }
```


Upstream `topN`.

**Value:** [ModelCapability](/api/Vercel.AI/ModelCapability/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapabilities.cs#L248)
