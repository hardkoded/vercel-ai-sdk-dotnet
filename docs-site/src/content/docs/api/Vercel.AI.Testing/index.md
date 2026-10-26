---
title: "Vercel.AI.Testing"
description: "Test doubles for language models."
editUrl: false
sidebar:
  label: "Vercel.AI.Testing"
---

[API reference](/api/)

Test doubles for language models.

## Classes

| Type | Summary |
| --- | --- |
| [MockEmbeddingModelV3](/api/Vercel.AI.Testing/MockEmbeddingModelV3/) | Embedding model v3 test double. Array-backed results are returned in order, starting at the first entry. |
| [MockEmbeddingModelV4](/api/Vercel.AI.Testing/MockEmbeddingModelV4/) | Embedding model v4 test double. Array-backed results are returned in order, starting at the first entry. |
| [MockEmbedOptions](/api/Vercel.AI.Testing/MockEmbedOptions/) | Embedding values passed to a mock embedding model. |
| [MockEmbedResult](/api/Vercel.AI.Testing/MockEmbedResult/) | Embedding result stored in a scripted list. |
| [MockGenerateResult](/api/Vercel.AI.Testing/MockGenerateResult/) | Generate result stored in a scripted list. |
| [MockLanguageModelCallOptions](/api/Vercel.AI.Testing/MockLanguageModelCallOptions/) | Call options recorded by the mock language models. |
| [MockLanguageModelV2](/api/Vercel.AI.Testing/MockLanguageModelV2/) | Language model v2 test double. Array-backed results are returned in order, starting at the first entry. |
| [MockLanguageModelV3](/api/Vercel.AI.Testing/MockLanguageModelV3/) | Language model v3 test double. Array-backed results are returned in order, starting at the first entry. |
| [MockLanguageModelV4](/api/Vercel.AI.Testing/MockLanguageModelV4/) | Language model v4 test double. Array-backed results are returned in order, starting at the first entry. |
| [MockStreamChunk](/api/Vercel.AI.Testing/MockStreamChunk/) | One chunk of a mock language-model stream. |
| [MockStreamResult](/api/Vercel.AI.Testing/MockStreamResult/) | Stream result stored in a scripted list. |
| [MockTextContent](/api/Vercel.AI.Testing/MockTextContent/) | Text content returned by a mock generate result. |
| [TestLanguageModel](/api/Vercel.AI.Testing/TestLanguageModel/) | Language model that returns scripted generate and stream results. |
