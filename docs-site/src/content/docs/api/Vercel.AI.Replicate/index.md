---
title: "Vercel.AI.Replicate"
description: "Public types in the `Vercel.AI.Replicate` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Replicate"
---

[API reference](/api/)

Public types in the `Vercel.AI.Replicate` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ReplicateImageModel](/api/Vercel.AI.Replicate/ReplicateImageModel/) | Replicate image model. It creates a prediction, polls it when the sync wait expires, and downloads each output image. A model:version id posts to /predictions; other ids post to /models/{model}/predictions. |
| [ReplicateProvider](/api/Vercel.AI.Replicate/ReplicateProvider/) | Replicate provider. |
| [ReplicateServiceCollectionExtensions](/api/Vercel.AI.Replicate/ReplicateServiceCollectionExtensions/) | Registers Replicate. |
| [ReplicateVideoModel](/api/Vercel.AI.Replicate/ReplicateVideoModel/) | Replicate video model. CancellationToken) creates a prediction and CancellationToken) reads it. A model:version id posts to /predictions; other ids post to /models/{model}/predictions. The operation is a JSON object with the getUrl of the prediction. |
| [ReplicateVideoModelOptions](/api/Vercel.AI.Replicate/ReplicateVideoModelOptions/) | Replicate video provider options, read from providerOptions.replicate. A null property is the same as an omitted one. Options this class does not model are sent as they are. |
