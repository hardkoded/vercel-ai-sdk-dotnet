---
title: "Vercel.AI.AmazonBedrock"
description: "Amazon Bedrock provider."
editUrl: false
sidebar:
  label: "Vercel.AI.AmazonBedrock"
---

[API reference](/api/)

Amazon Bedrock provider.

## Classes

| Type | Summary |
| --- | --- |
| [AmazonBedrockAnthropicFetch](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicFetch/) | Wraps a handler so the Anthropic Messages model can read Bedrock responses. Failed responses become Anthropic error bodies, and application/vnd.amazon.eventstream bodies become server-sent events. Maps to createAmazonBedrockAnthropicFetch. |
| [AmazonBedrockAnthropicLanguageModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicLanguageModel/) | Anthropic language model invoked through Bedrock. Delegates to AnthropicLanguageModel. |
| [AmazonBedrockAnthropicOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicOptions/) | Settings for Anthropic Messages on Amazon Bedrock InvokeModel. |
| [AmazonBedrockAnthropicProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicProvider/) | Anthropic Messages provider hosted on Amazon Bedrock. |
| [AmazonBedrockAnthropicRequests](/api/Vercel.AI.AmazonBedrock/AmazonBedrockAnthropicRequests/) | Builds InvokeModel URLs and the Bedrock Anthropic request body. |
| [AmazonBedrockApiKeyFetch](/api/Vercel.AI.AmazonBedrock/AmazonBedrockApiKeyFetch/) | API-key fetch wrapper. The transport is read when the call is sent, not when the wrapper is created. |
| [AmazonBedrockCredentials](/api/Vercel.AI.AmazonBedrock/AmazonBedrockCredentials/) | AWS credentials used to sign Amazon Bedrock requests. |
| [AmazonBedrockEmbeddingModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModel/) | Amazon Bedrock InvokeModel embedding model for Titan, Cohere, and Nova. |
| [AmazonBedrockEmbeddingModelSettings](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModelSettings/) | Settings for AmazonBedrockEmbeddingModel. |
| [AmazonBedrockEndpoints](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEndpoints/) | Builds Bedrock service hosts. Partition prefixes select the DNS suffix. |
| [AmazonBedrockErrors](/api/Vercel.AI.AmazonBedrock/AmazonBedrockErrors/) | Maps a Bedrock JSON error body onto the SDK exception hierarchy. |
| [AmazonBedrockFetch](/api/Vercel.AI.AmazonBedrock/AmazonBedrockFetch/) | Amazon Bedrock fetch behavior. GET requests and POST requests without a body skip signing. POST bodies are signed with SigV4. Caller headers are signed, except values with non-ASCII characters. |
| [AmazonBedrockLanguageModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockLanguageModel/) | Bedrock Converse language model. |
| [AmazonBedrockMessages](/api/Vercel.AI.AmazonBedrock/AmazonBedrockMessages/) | Converts V4 prompt messages into Converse system and messages arrays. |
| [AmazonBedrockOptions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockOptions/) | Amazon Bedrock settings. |
| [AmazonBedrockPreparedCall](/api/Vercel.AI.AmazonBedrock/AmazonBedrockPreparedCall/) | A request after the Bedrock fetch wrapper has applied user-agent and authentication. |
| [AmazonBedrockProvider](/api/Vercel.AI.AmazonBedrock/AmazonBedrockProvider/) | Amazon Bedrock Converse provider. |
| [AmazonBedrockRerankingModel](/api/Vercel.AI.AmazonBedrock/AmazonBedrockRerankingModel/) | Amazon Bedrock Agent Runtime rerank model. |
| [AmazonBedrockServiceCollectionExtensions](/api/Vercel.AI.AmazonBedrock/AmazonBedrockServiceCollectionExtensions/) | Registers Bedrock. |
| [AmazonBedrockSigV4Fetch](/api/Vercel.AI.AmazonBedrock/AmazonBedrockSigV4Fetch/) | SigV4 fetch wrapper. The credentials are read when each call is sent. |
| [AmazonBedrockToolIds](/api/Vercel.AI.AmazonBedrock/AmazonBedrockToolIds/) | Mistral tool-call ids on Bedrock must be exactly nine alphanumeric characters. |
| [AmazonBedrockTools](/api/Vercel.AI.AmazonBedrock/AmazonBedrockTools/) | Builds a Converse toolConfig from V4 function tools. |
| [AmazonBedrockUsage](/api/Vercel.AI.AmazonBedrock/AmazonBedrockUsage/) | Converts a Converse usage object. Bedrock inputTokens excludes cache tokens; the V4 input total adds cache read and cache write. |
| [AwsSigV4](/api/Vercel.AI.AmazonBedrock/AwsSigV4/) | Signs an HTTP request with AWS Signature Version 4. |
| [BedrockMantleLanguageModel](/api/Vercel.AI.AmazonBedrock/BedrockMantleLanguageModel/) | Mantle language model. Delegates to the OpenAI Chat Completions or Responses model, with the request signed for bedrock-mantle. |
| [BedrockMantleOptions](/api/Vercel.AI.AmazonBedrock/BedrockMantleOptions/) | Settings for the OpenAI-compatible Bedrock Mantle endpoint. |
| [BedrockMantleProvider](/api/Vercel.AI.AmazonBedrock/BedrockMantleProvider/) | Bedrock Mantle provider. Chat calls use /chat/completions and are signed for bedrock-mantle. |
| [BedrockMantleRoutes](/api/Vercel.AI.AmazonBedrock/BedrockMantleRoutes/) | Mantle URL rules. Some model ids are served under /openai/v1. |

## Enums

| Type | Summary |
| --- | --- |
| [AmazonBedrockEmbeddingModelFamily](/api/Vercel.AI.AmazonBedrock/AmazonBedrockEmbeddingModelFamily/) | Amazon Bedrock embedding model family. It selects the request format and the batch size. |
