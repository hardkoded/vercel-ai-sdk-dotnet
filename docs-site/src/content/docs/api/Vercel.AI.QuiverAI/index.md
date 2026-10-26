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
| [QuiverAIImageModel](/api/Vercel.AI.QuiverAI/QuiverAIImageModel/) | QuiverAI SVG model. providerOptions.quiverai.operation selects generate (default), vectorize, animate, or edit. Invalid requests throw before any HTTP call. |
| [QuiverAIImageReference](/api/Vercel.AI.QuiverAI/QuiverAIImageReference/) | A reference image accepted by providerOptions.quiverai.referenceImages: { "url" } or { "base64" }. Exactly one of Url and Base64 is set. Serializes to that JSON shape. |
| [QuiverAIProvider](/api/Vercel.AI.QuiverAI/QuiverAIProvider/) | QuiverAI provider. |
| [QuiverAIServiceCollectionExtensions](/api/Vercel.AI.QuiverAI/QuiverAIServiceCollectionExtensions/) | Registers QuiverAI. |
