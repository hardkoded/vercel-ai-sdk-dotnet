---
title: "Vercel.AI.HuggingFace"
description: "Public types in the `Vercel.AI.HuggingFace` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.HuggingFace"
---

[API reference](/api/)

Public types in the `Vercel.AI.HuggingFace` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [HuggingFaceProvider](/api/Vercel.AI.HuggingFace/HuggingFaceProvider/) | HuggingFace provider. OpenAI Chat Completions compatible at https://router.huggingface.co/v1. |
| [HuggingFaceReasoning](/api/Vercel.AI.HuggingFace/HuggingFaceReasoning/) | Reasoning text that carries the Responses output item id. |
| [HuggingFaceResponsesLanguageModel](/api/Vercel.AI.HuggingFace/HuggingFaceResponsesLanguageModel/) | Hugging Face Responses model. LanguageModel(string) stays on Chat Completions so existing chat clients keep working. Call ResponsesModel(string) for /responses. |
| [HuggingFaceServiceCollectionExtensions](/api/Vercel.AI.HuggingFace/HuggingFaceServiceCollectionExtensions/) | Registers HuggingFaceProvider. |
| [HuggingFaceText](/api/Vercel.AI.HuggingFace/HuggingFaceText/) | Generated text that carries the Responses output item id. |
| [HuggingFaceToolCall](/api/Vercel.AI.HuggingFace/HuggingFaceToolCall/) | A tool call that Hugging Face ran itself, such as an MCP call. |
| [HuggingFaceToolResult](/api/Vercel.AI.HuggingFace/HuggingFaceToolResult/) | A tool result returned in the response output. |
