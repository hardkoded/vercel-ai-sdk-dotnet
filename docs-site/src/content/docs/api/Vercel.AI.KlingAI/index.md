---
title: "Vercel.AI.KlingAI"
description: "Public types in the `Vercel.AI.KlingAI` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.KlingAI"
---

[API reference](/api/)

Public types in the `Vercel.AI.KlingAI` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [KlingAIAuth](/api/Vercel.AI.KlingAI/KlingAIAuth/) | Resolves the bearer token for KlingAI API requests. KlingAI accepts a single API key sent as a bearer token, or a legacy access key / secret key pair that signs a short-lived JWT. |
| [KlingAIOptions](/api/Vercel.AI.KlingAI/KlingAIOptions/) | KlingAI provider settings. Adds the legacy access key and secret key to OpenAICompatibleOptions. |
| [KlingAIProvider](/api/Vercel.AI.KlingAI/KlingAIProvider/) | KlingAI provider. It supports video generation only. |
| [KlingAIServiceCollectionExtensions](/api/Vercel.AI.KlingAI/KlingAIServiceCollectionExtensions/) | Registers KlingAI. |
| [KlingAIVideoModel](/api/Vercel.AI.KlingAI/KlingAIVideoModel/) | KlingAI video model. CancellationToken) creates a task and CancellationToken) reads it. The mode comes from the model id suffix: -t2v, -i2v or -motion-control. The operation is a JSON object with the taskId and the endpointPath. |
