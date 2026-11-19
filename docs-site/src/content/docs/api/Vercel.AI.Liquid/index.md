---
title: "Vercel.AI.Liquid"
description: "Public types in the `Vercel.AI.Liquid` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Liquid"
---

[API reference](/api/)

Public types in the `Vercel.AI.Liquid` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [DecisionLiquidModel](/api/Vercel.AI.Liquid/DecisionLiquidModel/) | Answers questions through the Liquid Decisions API (POST {base}/decisions/v1/systemone). Boolean questions are sent as native noul questions. Image and audio file parts are sent as base64 data URLs. Maps to DecisionLiquidModel. |
| [LiquidModelCatalog](/api/Vercel.AI.Liquid/LiquidModelCatalog/) | Known Liquid model ids, the adapter limits and the capability catalog. Upstream liquid-model-catalog.ts. |
| [LiquidProvider](/api/Vercel.AI.Liquid/LiquidProvider/) | Liquid provider. It supports experimental decision models. |
| [LiquidServiceCollectionExtensions](/api/Vercel.AI.Liquid/LiquidServiceCollectionExtensions/) | Registers Liquid. |
