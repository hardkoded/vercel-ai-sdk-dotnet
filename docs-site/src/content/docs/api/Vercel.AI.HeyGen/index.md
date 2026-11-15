---
title: "Vercel.AI.HeyGen"
description: "Public types in the `Vercel.AI.HeyGen` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.HeyGen"
---

[API reference](/api/)

Public types in the `Vercel.AI.HeyGen` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [DecisionHeyGenModel](/api/Vercel.AI.HeyGen/DecisionHeyGenModel/) | Answers questions through the HeyGen Decision API (POST {base}/v3/models/decisions). Boolean questions are sent as native noul questions. Image and video file parts are sent as HeyGen assets. Maps to DecisionHeyGenModel. |
| [HeyGenModelCatalog](/api/Vercel.AI.HeyGen/HeyGenModelCatalog/) | Known HeyGen model ids and the adapter limits. Upstream heygen-model-catalog.ts. |
| [HeyGenProvider](/api/Vercel.AI.HeyGen/HeyGenProvider/) | HeyGen provider. It supports video generation and experimental decision models. |
| [HeyGenServiceCollectionExtensions](/api/Vercel.AI.HeyGen/HeyGenServiceCollectionExtensions/) | Registers HeyGen. |
| [HeyGenVideoModel](/api/Vercel.AI.HeyGen/HeyGenVideoModel/) | HeyGen video model. CancellationToken) submits a job and CancellationToken) reads it. The operation is a JSON object with videoId, mode and resolution. The result is the signed video URL. The bytes are not downloaded. |
