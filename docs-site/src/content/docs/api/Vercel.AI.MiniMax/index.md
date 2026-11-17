---
title: "Vercel.AI.MiniMax"
description: "Public types in the `Vercel.AI.MiniMax` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.MiniMax"
---

[API reference](/api/)

Public types in the `Vercel.AI.MiniMax` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [MiniMaxChatLanguageModel](/api/Vercel.AI.MiniMax/MiniMaxChatLanguageModel/) | MiniMax chat language model. Reuses the Anthropic Messages adapter and adds the M3.1 and later capabilities. |
| [MiniMaxImageModel](/api/Vercel.AI.MiniMax/MiniMaxImageModel/) | MiniMax image model. It posts to {baseURL}/images/generations, or /images/edits when input images are given, asks for URL output, and downloads each image. |
| [MiniMaxImageModelConfig](/api/Vercel.AI.MiniMax/MiniMaxImageModelConfig/) | Configuration of a MiniMaxImageModel. |
| [MiniMaxImageModelOptions](/api/Vercel.AI.MiniMax/MiniMaxImageModelOptions/) | Provider options for MiniMax image generation and editing. Pass them under minimax (upstream MiniMaxImageModelOptions). |
| [MiniMaxOptions](/api/Vercel.AI.MiniMax/MiniMaxOptions/) | MiniMax provider settings. |
| [MiniMaxProvider](/api/Vercel.AI.MiniMax/MiniMaxProvider/) | MiniMax provider. Speaks the Anthropic Messages protocol at https://api.minimax.io/anthropic/v1. |
| [MiniMaxServiceCollectionExtensions](/api/Vercel.AI.MiniMax/MiniMaxServiceCollectionExtensions/) | Registers MiniMaxProvider. |
| [MiniMaxVideoModel](/api/Vercel.AI.MiniMax/MiniMaxVideoModel/) | MiniMax video model. DoStartAsync posts to /v2/videogeneration and DoStatusAsync reads the task. The operation is a JSON object with the taskId and the resolvedInputs that were sent. |
| [MiniMaxVideoModelConfig](/api/Vercel.AI.MiniMax/MiniMaxVideoModelConfig/) | Configuration of a MiniMaxVideoModel. |
| [MiniMaxVideoModelOptions](/api/Vercel.AI.MiniMax/MiniMaxVideoModelOptions/) | Provider options for MiniMax video generation. Pass them under minimax. |
