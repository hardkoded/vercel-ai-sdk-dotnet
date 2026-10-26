---
title: "Vercel.AI.Prodia"
description: "Public types in the `Vercel.AI.Prodia` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Prodia"
---

[API reference](/api/)

Public types in the `Vercel.AI.Prodia` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ProdiaImageModel](/api/Vercel.AI.Prodia/ProdiaImageModel/) | Prodia text-to-image model. It submits one job per image and reads the multipart result. |
| [ProdiaLanguageModel](/api/Vercel.AI.Prodia/ProdiaLanguageModel/) | Prodia image-to-image job exposed as a language model. The last user message supplies the text and the optional input image. The result holds the returned text and images. |
| [ProdiaProvider](/api/Vercel.AI.Prodia/ProdiaProvider/) | Prodia provider. |
| [ProdiaServiceCollectionExtensions](/api/Vercel.AI.Prodia/ProdiaServiceCollectionExtensions/) | Registers Prodia. |
| [ProdiaVideoModel](/api/Vercel.AI.Prodia/ProdiaVideoModel/) | Prodia video model. A prompt alone sends a JSON job (text to video). An input image sends a multipart job (image to video). Image URLs are checked against private addresses before they are downloaded. |
