---
title: "Vercel.AI.QuiverAI"
description: "Public types in the `Vercel.AI.QuiverAI` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.QuiverAI"
---

[API reference](/api/)

Public types in the `Vercel.AI.QuiverAI` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [QuiverAIImageAttributes](/api/Vercel.AI.QuiverAI/QuiverAIImageAttributes/) | SVG root attributes. Maps to attributes of QuiverAIImageModelOptions. |
| [QuiverAIImageCapabilities](/api/Vercel.AI.QuiverAI/QuiverAIImageCapabilities/) | The image capabilities of a QuiverAI model: the shared ImageCapabilities plus animation and editing. Maps to capabilities.image of getQuiverAIImageModelEntry. |
| [QuiverAIImageModel](/api/Vercel.AI.QuiverAI/QuiverAIImageModel/) | QuiverAI SVG model. providerOptions.quiverai.operation selects generate (default), vectorize, animate, or edit. Invalid requests throw before any HTTP call. |
| [QuiverAIImageModelEntry](/api/Vercel.AI.QuiverAI/QuiverAIImageModelEntry/) | A catalog entry of a QuiverAI image model. Maps to the result of getQuiverAIImageModelEntry. |
| [QuiverAIImageModelOptions](/api/Vercel.AI.QuiverAI/QuiverAIImageModelOptions/) | The quiverai provider options of an image call. Maps to QuiverAIImageModelOptions. |
| [QuiverAIImageReference](/api/Vercel.AI.QuiverAI/QuiverAIImageReference/) | A reference image accepted by providerOptions.quiverai.referenceImages: { "url" } or { "base64" }. Exactly one of Url and Base64 is set. Serializes to that JSON shape. |
| [QuiverAIImageRequestConfig](/api/Vercel.AI.QuiverAI/QuiverAIImageRequestConfig/) | Request configuration of an image model. Maps to requestConfig.image of getQuiverAIImageModelEntry. |
| [QuiverAILanguageModel](/api/Vercel.AI.QuiverAI/QuiverAILanguageModel/) | Applies QuiverAI's request policy while reusing the Open Responses transport. Maps to QuiverAILanguageModel. |
| [QuiverAILanguageModelEntry](/api/Vercel.AI.QuiverAI/QuiverAILanguageModelEntry/) | A catalog entry of a QuiverAI language model. Maps to the result of getQuiverAILanguageModelEntry. |
| [QuiverAILanguageModelOptions](/api/Vercel.AI.QuiverAI/QuiverAILanguageModelOptions/) | The quiverai provider options of a language model call. Maps to QuiverAILanguageModelOptions. |
| [QuiverAILanguageRequestConfig](/api/Vercel.AI.QuiverAI/QuiverAILanguageRequestConfig/) | Request defaults of the language models. Maps to quiveraiRequestDefaults.language. |
| [QuiverAIModelCatalog](/api/Vercel.AI.QuiverAI/QuiverAIModelCatalog/) | QuiverAI model catalog. Maps to quiverai-model-catalog.ts. |
| [QuiverAIProvider](/api/Vercel.AI.QuiverAI/QuiverAIProvider/) | QuiverAI provider. |
| [QuiverAIProviderOptions](/api/Vercel.AI.QuiverAI/QuiverAIProviderOptions/) | Known options accepted under providerOptions.quiverai. Maps to quiveraiProviderOptions. |
| [QuiverAIServiceCollectionExtensions](/api/Vercel.AI.QuiverAI/QuiverAIServiceCollectionExtensions/) | Registers QuiverAI. |
| [QuiverAIViewBox](/api/Vercel.AI.QuiverAI/QuiverAIViewBox/) | The SVG viewBox of QuiverAIImageModelOptions.attributes. |

## Enums

| Type | Summary |
| --- | --- |
| [QuiverAIImageOperation](/api/Vercel.AI.QuiverAI/QuiverAIImageOperation/) | The operation of an image call. Maps to the operation union of QuiverAIImageModelOptions. |
| [QuiverAIReasoningEffort](/api/Vercel.AI.QuiverAI/QuiverAIReasoningEffort/) | Reasoning effort. Maps to the reasoningEffort union of QuiverAILanguageModelOptions. |
| [QuiverAIReasoningSummary](/api/Vercel.AI.QuiverAI/QuiverAIReasoningSummary/) | Reasoning summary. Maps to the reasoningSummary union of QuiverAILanguageModelOptions. |
