---
title: "Vercel.AI.Gateway"
description: "Vercel AI Gateway provider."
editUrl: false
sidebar:
  label: "Vercel.AI.Gateway"
---

[API reference](/api/)

Vercel AI Gateway provider.

## Classes

| Type | Summary |
| --- | --- |
| [GatewayAuthenticationError](/api/Vercel.AI.Gateway/GatewayAuthenticationError/) | Authentication failed. |
| [GatewayBatchApi](/api/Vercel.AI.Gateway/GatewayBatchApi/) | AI Gateway batch API. Downloads the JSONL results of a batch. |
| [GatewayDecisionModel](/api/Vercel.AI.Gateway/GatewayDecisionModel/) | Answers questions through the Gateway /decision-model route. |
| [GatewayEmbeddingModel](/api/Vercel.AI.Gateway/GatewayEmbeddingModel/) | Gateway embedding model. |
| [GatewayError](/api/Vercel.AI.Gateway/GatewayError/) | Base class for AI Gateway errors. The message includes [generationId] when one was returned. |
| [GatewayErrors](/api/Vercel.AI.Gateway/GatewayErrors/) | Parses Gateway auth headers and error bodies. |
| [GatewayFailedDependencyError](/api/Vercel.AI.Gateway/GatewayFailedDependencyError/) | A dependency required by the Gateway request was unavailable. Status 424 is not retried. |
| [GatewayForbiddenError](/api/Vercel.AI.Gateway/GatewayForbiddenError/) | The request was rejected by policy. |
| [GatewayImageModel](/api/Vercel.AI.Gateway/GatewayImageModel/) | Gateway image model. |
| [GatewayInternalServerError](/api/Vercel.AI.Gateway/GatewayInternalServerError/) | The Gateway failed internally. |
| [GatewayInvalidRequestError](/api/Vercel.AI.Gateway/GatewayInvalidRequestError/) | The Gateway rejected the request. |
| [GatewayLanguageModel](/api/Vercel.AI.Gateway/GatewayLanguageModel/) | Gateway language model. The request body is the V4 call options. |
| [GatewayModelNotFoundError](/api/Vercel.AI.Gateway/GatewayModelNotFoundError/) | The requested model is not available. |
| [GatewayNotFoundError](/api/Vercel.AI.Gateway/GatewayNotFoundError/) | A Gateway resource was not found. |
| [GatewayOptions](/api/Vercel.AI.Gateway/GatewayOptions/) | AI Gateway settings. The default base URL is https://ai-gateway.vercel.sh/v4/ai. |
| [GatewayProvider](/api/Vercel.AI.Gateway/GatewayProvider/) | Vercel AI Gateway provider. Language, embedding, and image calls use the Gateway V4 routes (/language-model, /embedding-model, /image-model). |
| [GatewayRateLimitError](/api/Vercel.AI.Gateway/GatewayRateLimitError/) | A Gateway rate limit. |
| [GatewayResponseError](/api/Vercel.AI.Gateway/GatewayResponseError/) | The Gateway error body could not be read. |
| [GatewayServiceCollectionExtensions](/api/Vercel.AI.Gateway/GatewayServiceCollectionExtensions/) | Registers the Gateway provider. |
| [GatewayTakoSearch](/api/Vercel.AI.Gateway/GatewayTakoSearch/) | JSON schemas for the Gateway Tako search tool. |
| [GatewayTimeoutError](/api/Vercel.AI.Gateway/GatewayTimeoutError/) | The client timed out before the Gateway responded. |
| [V4Json](/api/Vercel.AI.Gateway/V4Json/) | Serializes call options and parses results in the Language Model V4 JSON shape the Gateway expects. |
