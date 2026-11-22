---
title: "Vercel.AI.Fal"
description: "Public types in the `Vercel.AI.Fal` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Fal"
---

[API reference](/api/)

Public types in the `Vercel.AI.Fal` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [FalImageModel](/api/Vercel.AI.Fal/FalImageModel/) | Fal image model. Posts to {baseURL}/{modelId} and downloads each returned image. |
| [FalProvider](/api/Vercel.AI.Fal/FalProvider/) | Fal provider. |
| [FalServiceCollectionExtensions](/api/Vercel.AI.Fal/FalServiceCollectionExtensions/) | Registers Fal. |
| [FalSpeechModel](/api/Vercel.AI.Fal/FalSpeechModel/) | Fal text-to-speech model. Posts to https://fal.run/{modelId} and downloads the returned audio. |
| [FalTranscriptionModel](/api/Vercel.AI.Fal/FalTranscriptionModel/) | Fal transcription model. Submits the audio to the Fal queue and polls until the transcript is ready. |
| [FalVideoModel](/api/Vercel.AI.Fal/FalVideoModel/) | Fal video model. CancellationToken) submits a job to the Fal queue and CancellationToken) reads it. The operation is a JSON object with the responseUrl and the submitUrl. |
| [FalVideoModelOptions](/api/Vercel.AI.Fal/FalVideoModelOptions/) | Provider options for Fal video generation, read from providerOptions.fal. Upstream FalVideoModelOptions. |
