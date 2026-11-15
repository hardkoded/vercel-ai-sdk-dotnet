---
title: "Vercel.AI.Tencent"
description: "Public types in the `Vercel.AI.Tencent` namespace."
editUrl: false
sidebar:
  label: "Vercel.AI.Tencent"
---

[API reference](/api/)

Public types in the `Vercel.AI.Tencent` namespace.

## Classes

| Type | Summary |
| --- | --- |
| [ParsedTencentVideoModelId](/api/Vercel.AI.Tencent/ParsedTencentVideoModelId/) | A parsed video model id. Upstream ParsedTencentVideoModelId. |
| [TencentCloudApi](/api/Vercel.AI.Tencent/TencentCloudApi/) | Tencent Cloud API 3.0 client with TC3-HMAC-SHA256 signing. Upstream tencent-cloud-api.ts. |
| [TencentCloudCredentials](/api/Vercel.AI.Tencent/TencentCloudCredentials/) | Credentials for Tencent Cloud API 3.0. Upstream TencentCloudCredentials. |
| [TencentLanguageModelEntry](/api/Vercel.AI.Tencent/TencentLanguageModelEntry/) | The catalog entry of a language model. Upstream getTencentLanguageModelEntry result. |
| [TencentModelCatalog](/api/Vercel.AI.Tencent/TencentModelCatalog/) | Known Tencent model ids and the per-model policy. Upstream tencent-model-catalog.ts. |
| [TencentModelEntry](/api/Vercel.AI.Tencent/TencentModelEntry/) | One catalog record of a model id, holding the operations it declares. Upstream ModelEntry in tencentModelCatalog. |
| [TencentOptions](/api/Vercel.AI.Tencent/TencentOptions/) | Tencent provider settings. Adds the Tencent Cloud credentials for VOD video models to OpenAICompatibleOptions. ApiKey is the TokenHub API key, used by language models and TokenHub video models. It defaults to the TENCENTTOKENHUBAPIKEY environment variable. BaseUrl is the TokenHub base URL and defaults to https://tokenhub-intl.tencentcloudmaas.com/v1. |
| [TencentProvider](/api/Vercel.AI.Tencent/TencentProvider/) | Tencent Cloud provider: TokenHub chat models (OpenAI-compatible) and Kling (TokenHub) and VOD AIGC video models. |
| [TencentProviderOptions](/api/Vercel.AI.Tencent/TencentProviderOptions/) | Provider option descriptions. Upstream tencentProviderOptions. |
| [TencentServiceCollectionExtensions](/api/Vercel.AI.Tencent/TencentServiceCollectionExtensions/) | Registers Tencent. |
| [TencentVideoAsset](/api/Vercel.AI.Tencent/TencentVideoAsset/) | A VOD material registered with CreateAigcMaterial. Upstream assets entries. |
| [TencentVideoCapabilities](/api/Vercel.AI.Tencent/TencentVideoCapabilities/) | Video capabilities of a model. Upstream ModelCapabilities.video. |
| [TencentVideoElement](/api/Vercel.AI.Tencent/TencentVideoElement/) | A custom TokenHub Kling element. Upstream elements entries. |
| [TencentVideoModel](/api/Vercel.AI.Tencent/TencentVideoModel/) | Tencent video model. CancellationToken) creates a task and CancellationToken) reads it. TokenHub Kling models use their TokenHub id (for example kling-video-v3); VOD AIGC models use vod/<ModelName>/<ModelVersion>. The operation is a JSON object with the taskId. |
| [TencentVideoModelEntry](/api/Vercel.AI.Tencent/TencentVideoModelEntry/) | The catalog entry of a video model. Upstream getTencentVideoModelEntry result. |
| [TencentVideoModelOptions](/api/Vercel.AI.Tencent/TencentVideoModelOptions/) | Provider-specific options for Tencent video generation, passed as providerOptions.tencent. Options apply to TokenHub Kling models (kling-video-), VOD AIGC models (vod/<ModelName>/<ModelVersion>), or both. An option that does not apply to the selected model is ignored with a warning. |
| [TencentVideoRequestConfig](/api/Vercel.AI.Tencent/TencentVideoRequestConfig/) | What a video model id implies for request building. |
| [TencentVideoVoice](/api/Vercel.AI.Tencent/TencentVideoVoice/) | A TokenHub Kling omni voice. Upstream voices entries. |
| [TencentVodCredentials](/api/Vercel.AI.Tencent/TencentVodCredentials/) | Credentials and application id for VOD. Upstream TencentVodCredentials. |

## Enums

| Type | Summary |
| --- | --- |
| [TencentKlingVideoKind](/api/Vercel.AI.Tencent/TencentKlingVideoKind/) | The kind of a TokenHub Kling video model. Upstream TencentKlingVideoKind. |
