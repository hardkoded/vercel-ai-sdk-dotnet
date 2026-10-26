---
title: "Vercel.AI.BlackForestLabs"
description: "Public types in the `Vercel.AI.BlackForestLabs` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.BlackForestLabs"
---

[API reference](/api/)

Public types in the `Vercel.AI.BlackForestLabs` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [BlackForestLabsImageModel](/api/Vercel.AI.BlackForestLabs/BlackForestLabsImageModel/) | Black Forest Labs image model. It submits the request, polls pollingurl until the task is Ready, then downloads the sample. Model ids pick the legacy, FLUX 3, or latest image API. |
| [BlackForestLabsProvider](/api/Vercel.AI.BlackForestLabs/BlackForestLabsProvider/) | BlackForestLabs provider. |
| [BlackForestLabsServiceCollectionExtensions](/api/Vercel.AI.BlackForestLabs/BlackForestLabsServiceCollectionExtensions/) | Registers BlackForestLabs. |
| [BlackForestLabsVideoModel](/api/Vercel.AI.BlackForestLabs/BlackForestLabsVideoModel/) | Black Forest Labs FLUX 3 video model. CancellationToken) submits a job and CancellationToken) reads it. The video is returned as a signed URL and is never downloaded. The operation is a JSON object with the requestId, the pollingUrl, and the estimates of the submit response. |
