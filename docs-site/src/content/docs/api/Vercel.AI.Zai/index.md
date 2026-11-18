---
title: "Vercel.AI.Zai"
description: "Public types in the `Vercel.AI.Zai` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Zai"
---

[API reference](/api/)

Public types in the `Vercel.AI.Zai` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ZaiChatLanguageModel](/api/Vercel.AI.Zai/ZaiChatLanguageModel/) | Z.AI chat completions over the OpenAI-compatible protocol. Serializes its own config because the URL is rebuilt from the base URL when the model is restored. |
| [ZaiLanguageModelChatOptions](/api/Vercel.AI.Zai/ZaiLanguageModelChatOptions/) | The zai provider options of a chat call. Maps to ZaiLanguageModelChatOptions. |
| [ZaiProvider](/api/Vercel.AI.Zai/ZaiProvider/) | Zai provider. OpenAI Chat Completions compatible at https://api.z.ai/api/paas/v4. |
| [ZaiServiceCollectionExtensions](/api/Vercel.AI.Zai/ZaiServiceCollectionExtensions/) | Registers ZaiProvider. |
| [ZaiThinking](/api/Vercel.AI.Zai/ZaiThinking/) | Controls model thinking. Maps to the thinking option of ZaiLanguageModelChatOptions. |

## Enums

| Type | Summary |
| --- | --- |
| [ZaiReasoningEffort](/api/Vercel.AI.Zai/ZaiReasoningEffort/) | Reasoning effort. Maps to the reasoningEffort enum of ZaiLanguageModelChatOptions. |
| [ZaiThinkingType](/api/Vercel.AI.Zai/ZaiThinkingType/) | Thinking mode. Maps to the thinking.type enum of ZaiLanguageModelChatOptions. |
