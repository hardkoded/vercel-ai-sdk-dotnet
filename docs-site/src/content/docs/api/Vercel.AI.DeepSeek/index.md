---
title: "Vercel.AI.DeepSeek"
description: "Public types in the `Vercel.AI.DeepSeek` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.DeepSeek"
---

[API reference](/api/)

Public types in the `Vercel.AI.DeepSeek` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [DeepSeekAssistantMessageProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekAssistantMessageProviderOptions/) | The deepseek provider options of an assistant message. Maps to DeepSeekAssistantMessageProviderOptions. |
| [DeepSeekChatConfig](/api/Vercel.AI.DeepSeek/DeepSeekChatConfig/) | The configuration of a DeepSeekChatLanguageModel. Maps to DeepSeekChatConfig. Transport is .NET-only. It replaces upstream's headers and fetch. |
| [DeepSeekChatLanguageModel](/api/Vercel.AI.DeepSeek/DeepSeekChatLanguageModel/) | DeepSeek Chat Completions language model. Maps to DeepSeekChatLanguageModel. |
| [DeepSeekEndpointCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekEndpointCapabilities/) | Endpoint support of a DeepSeek language model. Upstream LanguageCapabilities.endpoints. |
| [DeepSeekEndpointOptions](/api/Vercel.AI.DeepSeek/DeepSeekEndpointOptions/) | What the serving endpoint supports. Upstream DeepSeekEndpointOptions. |
| [DeepSeekFilePartProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekFilePartProviderOptions/) | The deepseek provider options of an image part. Maps to DeepSeekFilePartProviderOptions. |
| [DeepSeekFileStore](/api/Vercel.AI.DeepSeek/DeepSeekFileStore/) | Uploads images to POST /files with purpose userdata. |
| [DeepSeekLanguageCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekLanguageCapabilities/) | The language capabilities of a DeepSeek model. Upstream LanguageCapabilities of deepseek-model-catalog.ts. |
| [DeepSeekLanguageModelChatOptions](/api/Vercel.AI.DeepSeek/DeepSeekLanguageModelChatOptions/) | The deepseek provider options of a chat call. Maps to DeepSeekLanguageModelChatOptions. |
| [DeepSeekLanguageModelEntry](/api/Vercel.AI.DeepSeek/DeepSeekLanguageModelEntry/) | A catalog entry of a DeepSeek language model. Upstream DeepSeekLanguageModelEntry. |
| [DeepSeekLanguageModelResponsesOptions](/api/Vercel.AI.DeepSeek/DeepSeekLanguageModelResponsesOptions/) | The deepseek provider options of a Responses call. Maps to DeepSeekLanguageModelResponsesOptions. |
| [DeepSeekMessageProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekMessageProviderOptions/) | The deepseek provider options of a chat message. Maps to DeepSeekMessageProviderOptions. |
| [DeepSeekModelCatalog](/api/Vercel.AI.DeepSeek/DeepSeekModelCatalog/) | DeepSeek model catalog. Upstream deepseek-model-catalog.ts. |
| [DeepSeekModelIds](/api/Vercel.AI.DeepSeek/DeepSeekModelIds/) | Known DeepSeek model ids per endpoint. Upstream DeepSeekModelIds. |
| [DeepSeekProvider](/api/Vercel.AI.DeepSeek/DeepSeekProvider/) | DeepSeek provider. OpenAI Chat Completions compatible at https://api.deepseek.com. |
| [DeepSeekProviderOptions](/api/Vercel.AI.DeepSeek/DeepSeekProviderOptions/) | Known options accepted under providerOptions.deepseek. Upstream deepseekProviderOptions. |
| [DeepSeekReasoningCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekReasoningCapabilities/) | Reasoning support of a DeepSeek language model. Upstream LanguageCapabilities.reasoning. |
| [DeepSeekResponsesLanguageModel](/api/Vercel.AI.DeepSeek/DeepSeekResponsesLanguageModel/) | DeepSeek Responses API language model. Generate and stream requests post to /responses. It supports the server-side WebSearch(string) tool. Maps to DeepSeekResponsesLanguageModel. |
| [DeepSeekSamplingCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekSamplingCapabilities/) | Sampling support of a DeepSeek language model. Upstream LanguageCapabilities.sampling. |
| [DeepSeekServiceCollectionExtensions](/api/Vercel.AI.DeepSeek/DeepSeekServiceCollectionExtensions/) | Registers DeepSeekProvider. |
| [DeepSeekStructuredOutputCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekStructuredOutputCapabilities/) | Structured output support of a DeepSeek language model. Upstream LanguageCapabilities.structuredOutput. |
| [DeepSeekThinkingOptions](/api/Vercel.AI.DeepSeek/DeepSeekThinkingOptions/) | The thinking option of DeepSeekLanguageModelResponsesOptions. |
| [DeepSeekToolCapabilities](/api/Vercel.AI.DeepSeek/DeepSeekToolCapabilities/) | Tool support of a DeepSeek language model. Upstream LanguageCapabilities.tools. |
| [DeepSeekTools](/api/Vercel.AI.DeepSeek/DeepSeekTools/) | DeepSeek server-side tools for the Responses API. Maps to deepseekTools. |
