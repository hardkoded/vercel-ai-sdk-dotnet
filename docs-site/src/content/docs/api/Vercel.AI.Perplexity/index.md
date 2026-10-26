---
title: "Vercel.AI.Perplexity"
description: "Public types in the `Vercel.AI.Perplexity` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Perplexity"
---

[API reference](/api/)

Public types in the `Vercel.AI.Perplexity` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [PerplexityDecisionModel](/api/Vercel.AI.Perplexity/PerplexityDecisionModel/) | Answers questions through the Perplexity Decisions API (POST {base}/v1/decisions). Boolean questions are sent as native noul questions. Probabilities and scores keep full precision. Provider options are not supported. |
| [PerplexityEmbeddingModel](/api/Vercel.AI.Perplexity/PerplexityEmbeddingModel/) | Perplexity embeddings at {base}/v1/embeddings. Perplexity returns quantized vectors as base64, so the model decodes base64int8 (the default) or base64binary into numbers. |
| [PerplexityLanguageModel](/api/Vercel.AI.Perplexity/PerplexityLanguageModel/) | Perplexity Agent API language model. Generate and stream requests post to /v1/agent. Sonar PDF input, video input, and image or video results have no Agent API equivalent and are not mapped. |
| [PerplexityProvider](/api/Vercel.AI.Perplexity/PerplexityProvider/) | Perplexity provider. Language generation uses the Agent API at {base}/v1/agent. Embeddings use {base}/v1/embeddings. Preset ids are fast, low, medium, high, and xhigh; any other id is sent as a model id. Legacy Sonar model ids are not aliased and Sonar provider options are not translated. Sonar PDF input, video input, and image or video results have no Agent API equivalent. |
| [PerplexityServiceCollectionExtensions](/api/Vercel.AI.Perplexity/PerplexityServiceCollectionExtensions/) | Registers PerplexityProvider. |
